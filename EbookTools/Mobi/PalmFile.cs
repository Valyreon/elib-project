using System;
using System.Collections.Generic;
using System.IO;

namespace EbookTools.Mobi
{
    internal struct PalmRecord
    {
        public byte[] Data;
    }

    internal struct PalmRecordInfo
    {
        public uint DataOffset;
        public uint Attributes;
        public uint UniqueId;
    }

    internal class PalmFile
    {
        protected uint MAppInfoId;
        protected uint MCompression;

        // File Version
        protected DateTime MCreationDate;

        protected string MCreator;
        protected uint MCurrentPosition;
        protected byte[] MFileName;

        // Modification Date
        protected DateTime MLastBackupDate;

        // Creation Date
        protected DateTime MModificationDate;

        // Last Backup Date
        protected uint MModificationNumber;

        protected string MName;
        protected uint MNextRecordListId;
        protected uint MNumberOfRecords;
        protected uint MSortInfoId;
        protected uint MTextLength;
        protected uint MTextRecordCount;
        protected uint MTextRecordSize;
        protected string MType;
        protected uint MUniqueIdSeed;

        // bit field.
        protected uint MVersion;

        // Database Name
        internal uint MAttributes;

        internal PalmRecordInfo[] MRecordInfoList;
        internal PalmRecord[] MRecordList;

        public uint AppInfoId => this.MAppInfoId;

        public bool BackupThisDatabase => (this.MAttributes & 0x0008) == 0x0008;

        public uint Compression => this.MCompression;

        public DateTime CreationDate => this.MCreationDate;

        public string Creator => this.MCreator;

        public uint CurrentPosition => this.MCurrentPosition;

        public bool DirtyAppInfoArea => (this.MAttributes & 0x0004) == 0x0004;

        public byte[] FileName => this.MFileName;

        public bool ForceReset => (this.MAttributes & 0x0020) == 0x0020;

        public DateTime LastBackupDate => this.MLastBackupDate;

        public DateTime ModificationDate => this.MModificationDate;

        public uint ModificationNumber => this.MModificationNumber;

        public string Name => this.MName;

        public uint NextRecordListId => this.MNextRecordListId;

        public bool NoBeam => (this.MAttributes & 0x0040) == 0x0040;

        public uint NumberOfRecords => this.MNumberOfRecords;

        public bool OkToInstallNewer => (this.MAttributes & 0x0010) == 0x0010;

        public bool ReadOnly => (this.MAttributes & 0x0002) == 0x0002;

        public PalmRecord[] RecordList => this.MRecordList;

        public uint SortInfoId => this.MSortInfoId;

        public uint TextLength => this.MTextLength;

        public uint TextRecordCount => this.MTextRecordCount;

        public uint TextRecordSize => this.MTextRecordSize;

        public string Type => this.MType;

        public uint UniqueIdSeed => this.MUniqueIdSeed;

        public uint Version => this.MVersion;

        public static PalmFile LoadFile(byte[] file)
        {
            PalmFile retval = new PalmFile
            {
                MFileName = file
            };
            using MemoryStream fs = new MemoryStream(file);
            using StreamReader sr = new StreamReader(fs);
            DateTime startdate = new DateTime(1904, 1, 1);

            //startdate = new DateTime(1970, 1, 1);
            try
            {
                var buffer = new char[32];
                sr.Read(buffer, 0, 32);
                fs.Seek(32, SeekOrigin.Begin);
                retval.MName = new string(buffer);
                var bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);
                retval.MAttributes = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);
                retval.MVersion = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 0, 4);
                uint seconds = BytesToUint(bytebuffer);
                TimeSpan ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.MCreationDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                seconds = BytesToUint(bytebuffer);
                ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.MModificationDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                seconds = BytesToUint(bytebuffer);
                ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.MLastBackupDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                retval.MModificationNumber = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);
                retval.MAppInfoId = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);
                retval.MSortInfoId = BytesToUint(bytebuffer);
                buffer = new char[4];
                sr.DiscardBufferedData();
                sr.Read(buffer, 0, 4);
                retval.MType = new string(buffer);
                sr.Read(buffer, 0, 4);
                retval.MCreator = new string(buffer);
                fs.Seek(68, SeekOrigin.Begin);
                fs.Read(bytebuffer, 0, 4);

                retval.MUniqueIdSeed = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);

                retval.MNextRecordListId = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);

                // Load RecordInfo

                retval.MNumberOfRecords = BytesToUint(bytebuffer);
                retval.MRecordInfoList = new PalmRecordInfo[retval.MNumberOfRecords];
                retval.MRecordList = new PalmRecord[retval.MNumberOfRecords];
                for (int i = 0; i < retval.MNumberOfRecords; i++)
                {
                    fs.Read(bytebuffer, 0, 4);
                    retval.MRecordInfoList[i].DataOffset = BytesToUint(bytebuffer);

                    bytebuffer = new byte[4];
                    fs.Read(bytebuffer, 3, 1);
                    retval.MRecordInfoList[i].Attributes = BytesToUint(bytebuffer);

                    bytebuffer = new byte[4];
                    fs.Read(bytebuffer, 1, 3);
                    retval.MRecordInfoList[i].UniqueId = BytesToUint(bytebuffer);
                }

                //Load Records

                uint startOffset;
                int recordLength;
                for (int i = 0; i < retval.MNumberOfRecords - 1; i++)
                {
                    startOffset = retval.MRecordInfoList[i].DataOffset;
                    uint endOffset = retval.MRecordInfoList[i + 1].DataOffset;
                    recordLength = (int)(endOffset - startOffset);
                    fs.Seek(startOffset, SeekOrigin.Begin);
                    retval.MRecordList[i].Data = new byte[recordLength];
                    fs.Read(retval.MRecordList[i].Data, 0, recordLength);
                }

                startOffset = retval.MRecordInfoList[retval.MNumberOfRecords - 1].DataOffset;
                recordLength = (int)(fs.Length - (int)startOffset);
                fs.Seek(startOffset, SeekOrigin.Begin);
                retval.MRecordList[retval.MNumberOfRecords - 1].Data = new byte[recordLength];
                fs.Read(retval.MRecordList[retval.MNumberOfRecords - 1].Data, 0, recordLength);

                // LoadHeader
                var empty2 = new List<byte>();
                var temp = new List<byte>();
                empty2.Add(0);
                empty2.Add(0);

                var headerdata = new List<byte>();

                headerdata.AddRange(retval.MRecordList[0].Data);

                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(0, 2));
                retval.MCompression = BytesToUint(temp.ToArray());
                temp.Clear();
                temp.AddRange(headerdata.GetRange(4, 4));
                retval.MTextLength = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(8, 2));
                retval.MTextRecordCount = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(10, 2));
                retval.MTextRecordSize = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(headerdata.GetRange(12, 4));
                retval.MCurrentPosition = BytesToUint(temp.ToArray());
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }

                fs.Close();
            }

            return retval;
        }

        public static uint BytesToUint(byte[] bytes)
        {
            return (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        }
    }
}