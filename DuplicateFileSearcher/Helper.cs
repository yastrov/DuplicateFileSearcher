using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFileSearcher
{
    public static class Helper
    {
#region Hash
        public static byte[] ComputeHashForFile(string fileName, HashAlgorithm algoritm)
        {
            byte[] result;
            algoritm.Initialize();
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                result = algoritm.ComputeHash(stream);
            }
            return result;
        }

        public static UInt64 ComputeUInt64HashForFile(string fileName, HashAlgorithm algoritm)
        {
            byte[] result;
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                result = algoritm.ComputeHash(stream);
            }
            return BitConverter.ToUInt64(result, 0);

        }
#endregion

        #region Work with File list
        // It may be rewritten (to store data in dictionary from start), but I write this in past...
        public static void FilterBySize(LinkedList<FileInfo> FilesBySize, CancellationToken cToken, long FilterFileSize = 0)
        {
            Dictionary<long, LinkedList<FileInfo>> dct = new Dictionary<long, LinkedList<FileInfo>>();
            LinkedList<FileInfo> temp;
            cToken.ThrowIfCancellationRequested();
            foreach (var file in FilesBySize)
            {
                if ((file.Length == 0) || (file.Length < FilterFileSize))
                    continue;
                if (dct.TryGetValue(file.Length, out temp))
                {
                    temp.AddLast(file);
                }
                else
                {
                    temp = new LinkedList<FileInfo>();
                    temp.AddFirst(file);
                    dct[file.Length] = temp;
                }
            }
            cToken.ThrowIfCancellationRequested();
            foreach (var item in dct)
            {
                if (item.Value.Count == 1)
                    FilesBySize.Remove(item.Value.First.Value);
            }
            dct.Clear();
        }

        public static IEnumerable<FileViewModel> FilterByHash(LinkedList<FileInfo> FilesBySize, HashProviderModel CurrentHashProvider, CancellationToken cToken)
        {
            var lst = new LinkedList<FileViewModel>();
            foreach (var file in FilesBySize)
            {
                try
                {
                    lst.AddLast(new FileViewModel
                    {
                        FileName = file.FullName,
                        Size = file.Length,
                        HashSum = CurrentHashProvider.ComputeUInt64HashForFile(file.FullName)
                    });
                }
                catch(Exception ex)
                {//If we can't compute hash (for any reason), it not interesting for us
                    ;
                }
            }
            cToken.ThrowIfCancellationRequested();
            var _dct = new Dictionary<UInt64, LinkedList<FileViewModel>>();
            LinkedList<FileViewModel> _temp;
            foreach (var item in lst)
            {
                if (_dct.TryGetValue(item.HashSum, out _temp))
                {
                    item.Changed = true;
                    _temp.AddLast(item);
                }
                else
                {
                    _temp = new LinkedList<FileViewModel>();
                    _temp.AddFirst(item);
                    _dct[item.HashSum] = _temp;
                }
            }
            cToken.ThrowIfCancellationRequested();
            int group = 0;
            foreach (var item in _dct)
            {
                if (item.Value.Count == 1)
                {
                    lst.Remove(item.Value.First.Value);
                }
                else
                {
                    group += 1;
                    foreach (var i in item.Value)
                        i.Group = group;
                }
            }
            _dct.Clear();
            return lst.Where(i=> i.Size != 0);
        }
        #endregion
    }
}
