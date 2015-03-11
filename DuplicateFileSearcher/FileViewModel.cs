using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DuplicateFileSearcher
{
    [Serializable]
    public class FileViewModel : INotifyPropertyChanged, ISerializable
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
        [DisplayName("File name")]
        public string FileName { get; set; }
    [DisplayName("Size of file")]
        public long Size { get; set; }

        private bool changed = false;
        public bool Changed {
            get { return changed; }
            set { changed = value;
            NotifyPropertyChanged("Changed");
            }
        }
        [DisplayName("Hash")]
        public UInt64 HashSum { get; set; }
        [DisplayName("Group")]
        public int Group { get; set; }

        //public fileviewmodel(fileinfo file)
        //{
        //    this.filename = file.fullname;
        //    this.size = file.length;
        //}
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Group", Group, typeof(int));
            info.AddValue("HashSum", Group, typeof(UInt64));
            info.AddValue("Size", Group, typeof(long));
            info.AddValue("FileName", FileName, typeof(string));
        }
    }
}
