using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.ComponentModel;
using System.IO;

namespace DuplicateFileSearcher
{
    public delegate byte[] CalcHash(string FileName);
    public class HashProviderModel
    {
        public string Description {get; set;}
        public CalcHash Calc {get; set;}
        public HashAlgorithm Algoritm { get; set; }

        public byte[] ComputeHashForFile(string fileName)
        {
            byte[] result;
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                result = Algoritm.ComputeHash(stream);
            }
            //algoritm.Initialize();
            return result;
        }
         public UInt64 ComputeUInt64HashForFile(string fileName)
        {
            return Helper.ComputeUInt64HashForFile(fileName, Algoritm);
        }
    }
}
