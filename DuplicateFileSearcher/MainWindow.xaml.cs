using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Avalon.Windows.Dialogs;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DuplicateFileSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Inotify
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region DataBinding
        private ObservableCollection<FileViewModel> _fileCollection = null;
        public ObservableCollection<FileViewModel> FileCollection
        {
            get { return _fileCollection; }
            set
            {
                _fileCollection = value;
                NotifyPropertyChanged("FileCollection");
            }
        }

        private ObservableCollection<string> _folderCollection = new ObservableCollection<string>();
        public ObservableCollection<string> FolderCollection
        {
            get { return _folderCollection; }
            set
            {
                _folderCollection = value;
                NotifyPropertyChanged("FolderCollection");
            }
        }

        private string _currentFolder = string.Empty;
        public string CurrentFolder
        {
            get { return _currentFolder; }
            set
            {
                _currentFolder = value;
                NotifyPropertyChanged("CurrentFolder");
            }
        }

        private ObservableCollection<HashProviderModel> _hashProviders = null;
        public ObservableCollection<HashProviderModel> HashProviders
        {
            get { return _hashProviders; }
            set
            {
                _hashProviders = value;
                NotifyPropertyChanged("HashProviders");
            }
        }
        private HashProviderModel _currentHashProvider = null;
        public HashProviderModel CurrentHashProvider
        {
            get { return _currentHashProvider; }
            set
            {
                _currentHashProvider = value;
                NotifyPropertyChanged("CurrentHashProvider");
            }
        }

        public string VersionInfo
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            private set { ; }
        }
        private string _filterFileMask = "*.*";
        public string FilterFileMaask
        {
            get { return _filterFileMask; }
            set
            {
                _filterFileMask = value.Trim();
                NotifyPropertyChanged("FilterFileMaask");
            }
        }

        // We no reason to lock some objects, but we have reason to
        // Disable WPF input controls
        private bool _paramsCanBeChanged = true;
        public bool ParamsCanBeChanged
        {
            get { return _paramsCanBeChanged; }
            set
            {
                _paramsCanBeChanged = value;
                NotifyPropertyChanged("ParamsCanBeChanged");
            }
        }

        private double _filterFileSize;
        public double FilterFileSize
        {
            get { return _filterFileSize; }
            set
            {
                _filterFileSize = value;
                NotifyPropertyChanged("FilterFileSize");
            }
        }
        private ObservableCollection<KeyValuePair<string, long>> _SizeProviders = null;
        public ObservableCollection<KeyValuePair<string, long>> SizeProviders
        {
            get { return _SizeProviders; }
            set
            {
                _SizeProviders = value;
                NotifyPropertyChanged("SizeProviders");
            }
        }
        private KeyValuePair<string, long> _CurrentSizeProvider = new KeyValuePair<string,long>();
        public KeyValuePair<string, long> CurrentSizeProvider
        {
            get { return _CurrentSizeProvider; }
            set
            {
                _CurrentSizeProvider = value;
                NotifyPropertyChanged("CurrentSizeProvider");
            }
        }
        #endregion
        private CancellationTokenSource cancelSource = null;
        private const string WindowTitle = "Duplicate File Searcher";
        public MainWindow()
        {
            InitializeComponent();
            Title = WindowTitle;
            #region Init Hash Providers
            var _HashProviders = new ObservableCollection<HashProviderModel>();
            CurrentHashProvider = new HashProviderModel
            {
                Description = "SHA256",
                Algoritm = SHA256Managed.Create(),
            };
            _HashProviders.Add(new HashProviderModel
            {
                Description = "MD5",
                Algoritm = MD5.Create(),
            });
            _HashProviders.Add(CurrentHashProvider);
            _HashProviders.Add(new HashProviderModel
            {
                Description = "SHA512",
                Algoritm = SHA512Managed.Create(),
            });
            HashProviders = _HashProviders;
            #endregion
            var k = new KeyValuePair<string, long>("B", 1);
            CurrentSizeProvider = k;
            ObservableCollection<KeyValuePair<string, long>> _SizeProviders = new ObservableCollection<KeyValuePair<string, long>>();
            _SizeProviders.Add(k);
            _SizeProviders.Add(new KeyValuePair<string, long>("KB", 1024));
            _SizeProviders.Add(new KeyValuePair<string, long>("MB", 1024 * 1024));
            _SizeProviders.Add(new KeyValuePair<string, long>("GB", 1024 * 1024 * 1024));
            SizeProviders = _SizeProviders;
        }

        #region Show dialog
        private FolderBrowserDialog dlg = new FolderBrowserDialog();
        public void ShowFolderBrowserDialog()
        {
            dlg.Title = "Select path";
            dlg.SelectedPath = Directory.GetCurrentDirectory();
            dlg.RootPath = Directory.GetCurrentDirectory();
            if (dlg.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(dlg.SelectedPath))
                    FolderCollection.Add(dlg.SelectedPath);
            }
        }
        #endregion

        private Task<List<FileViewModel>> GoAsync(IEnumerable<string> FoldersCollection, CancellationToken cToken)
        {
            return Task.Run(() =>
            {
                #region Make List of files
                LinkedList<FileInfo> FilesBySize = new LinkedList<FileInfo>();
                foreach (var path in FoldersCollection)
                {
                    try
                    {
                        var l = Directory.EnumerateFiles(path, FilterFileMaask, SearchOption.AllDirectories);
                        foreach (var i in l)
                        {
                            try
                            {
                                FilesBySize.AddLast(new FileInfo(i));
                            }
                            catch (System.NotSupportedException ex)
                            {
                                var s = string.Format("Invalid path: {0}", i);
                                MessageBox.Show(s, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                #endregion
                cToken.ThrowIfCancellationRequested();
                #region Filter by size
                Helper.FilterBySize(FilesBySize, cToken, (long)(CurrentSizeProvider.Value * FilterFileSize));
                #endregion
                cToken.ThrowIfCancellationRequested();
                #region Filter by Hash
                var lst = Helper.FilterByHash(FilesBySize, CurrentHashProvider, cToken);
                #endregion
                return lst.ToList();
            }, cToken);

        }
        #region Button Clicks
        private void AddFolderClick(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowserDialog();
        }

        private void RemoveFolderClick(object sender, RoutedEventArgs e)
        {
            FolderCollection.Remove(CurrentFolder);
        }

        private async void StartClick(object sender, RoutedEventArgs e)
        {
            if (FolderCollection == null)
                return;
            if (FolderCollection.Count == 0)
                return;
            Cursor = Cursors.Wait;
            ParamsCanBeChanged = false;
            cancelSource = new CancellationTokenSource();
            try
            {
                var l = await GoAsync(FolderCollection, cancelSource.Token);
                FileCollection = new ObservableCollection<FileViewModel>(l.OrderBy(i => i.HashSum));
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Cancelled by user!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                ParamsCanBeChanged = true;
            }
        }

        private void RemoveSelectedFilesClick(object sender, RoutedEventArgs e)
        {
            if (FileCollection == null)
                return;
            foreach (var item in FileCollection)
            {
                try
                {
                    if (item.Changed)
                    {
                        System.IO.File.Delete(item.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), WindowTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            FileCollection = new ObservableCollection<FileViewModel>(FileCollection.Where(i => i.Changed != true));
        }

        private void DeSelectAllClick(object sender, RoutedEventArgs e)
        {
            if (FileCollection != null)
            foreach (var item in FileCollection)
            {
                item.Changed = false;
            }
        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            if (FileCollection != null)
                foreach (var item in FileCollection)
                {
                    item.Changed = true;
                }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            if (cancelSource != null)
                cancelSource.Cancel();
        }

        private void SaveFileCollectionClick(object sender, RoutedEventArgs e)
        {
            if (FileCollection == null)
                return;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "XML documents (.xml)|*.xml|Binary|*.bin";
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                var ext = System.IO.Path.GetExtension(dlg.FileName);
                using (var fstream = new FileStream(dlg.FileName, FileMode.Create))
                {
                    switch(ext)
                    {
                        case ".xml":
                            var formatter = new System.Xml.Serialization.XmlSerializer(typeof(FileViewModel));
                            foreach (var item in FileCollection)
                            {
                                formatter.Serialize(fstream, item);
                            }
                            break;
                        case ".bin":
                        case ".data":
                            IFormatter bformatter = new BinaryFormatter();
                            foreach (var item in FileCollection)
                            {
                                bformatter.Serialize(fstream, item);
                            }
                            break;
                    }
                }
            }
        }
        #endregion
        #region Overload Windows funclion
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (cancelSource != null)
            {
                try
                {
                    cancelSource.Cancel();
                }
                catch(OperationCanceledException ex)
                {
                    //We don't do anything here, because it's no reason.
                    ;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            base.OnClosing(e);
        }
        #endregion
    }
}
