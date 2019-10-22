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
        public uint UniqueID;
    }

    internal class PalmFile
    {
        protected byte[] m_FileName;
        protected string m_Name;

        // Database Name
        internal uint m_Attributes;

        // bit field.
        protected uint m_Version;

        // File Version
        protected DateTime m_CreationDate;

        // Creation Date
        protected DateTime m_ModificationDate;

        // Modification Date
        protected DateTime m_LastBackupDate;

        // Last Backup Date
        protected uint m_ModificationNumber;

        protected uint m_AppInfoID;
        protected uint m_SortInfoID;
        protected string m_Type;
        protected string m_Creator;
        protected uint m_UniqueIDSeed;
        protected uint m_NextRecordListID;
        protected uint m_NumberOfRecords;
        protected uint m_Compression;
        protected uint m_TextLength;
        protected uint m_TextRecordCount;
        protected uint m_TextRecordSize;
        protected uint m_CurrentPosition;

        internal PalmRecordInfo[] m_RecordInfoList;
        internal PalmRecord[] m_RecordList;

        public string Name
        {
            get { return m_Name; }
        }

        public byte[] FileName
        {
            get { return m_FileName; }
        }

        public bool ReadOnly
        {
            get { return (m_Attributes & 0x0002) == 0x0002; }
        }

        public bool DirtyAppInfoArea
        {
            get { return (m_Attributes & 0x0004) == 0x0004; }
        }

        public bool BackupThisDatabase
        {
            get { return (m_Attributes & 0x0008) == 0x0008; }
        }

        public bool OKToInstallNewer
        {
            get { return (m_Attributes & 0x0010) == 0x0010; }
        }

        public bool ForceReset
        {
            get { return (m_Attributes & 0x0020) == 0x0020; }
        }

        public bool NoBeam
        {
            get { return (m_Attributes & 0x0040) == 0x0040; }
        }

        public uint Version
        {
            get { return m_Version; }
        }

        public DateTime CreationDate
        {
            get { return m_CreationDate; }
        }

        public DateTime ModificationDate
        {
            get { return m_ModificationDate; }
        }

        public DateTime LastBackupDate
        {
            get { return m_LastBackupDate; }
        }

        public uint ModificationNumber
        {
            get { return m_ModificationNumber; }
        }

        public uint AppInfoID
        {
            get { return m_AppInfoID; }
        }

        public uint SortInfoID
        {
            get { return m_SortInfoID; }
        }

        public string Type
        {
            get { return m_Type; }
        }

        public string Creator
        {
            get { return m_Creator; }
        }

        public uint UniqueIDSeed
        {
            get { return m_UniqueIDSeed; }
        }

        public uint NextRecordListID
        {
            get { return m_NextRecordListID; }
        }

        public uint NumberOfRecords
        {
            get { return m_NumberOfRecords; }
        }

        public PalmRecord[] RecordList
        {
            get { return m_RecordList; }
        }

        public uint Compression
        {
            get { return m_Compression; }
        }

        public uint TextLength
        {
            get { return m_TextLength; }
        }

        public uint TextRecordCount
        {
            get { return m_TextRecordCount; }
        }

        public uint TextRecordSize
        {
            get { return m_TextRecordSize; }
        }

        public uint CurrentPosition
        {
            get { return m_CurrentPosition; }
        }

        public PalmFile()
        {
        }

        public static PalmFile LoadFile(byte[] file)
        {
            PalmFile retval = new PalmFile
            {
                m_FileName = file
            };
            MemoryStream fs = new MemoryStream(file);
            StreamReader sr = null;
            uint seconds = 0;
            DateTime startdate = new DateTime(1904, 1, 1);

            //startdate = new DateTime(1970, 1, 1);
            try
            {
                sr = new StreamReader(fs);
                Char[] buffer = new char[32];
                sr.Read(buffer, 0, 32);
                fs.Seek(32, SeekOrigin.Begin);
                retval.m_Name = new string(buffer);
                byte[] bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);
                retval.m_Attributes = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);
                retval.m_Version = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 0, 4);
                seconds = BytesToUint(bytebuffer);
                TimeSpan ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.m_CreationDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                seconds = BytesToUint(bytebuffer);
                ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.m_ModificationDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                seconds = BytesToUint(bytebuffer);
                ts = new TimeSpan(0, (int)(seconds / 60), 0);
                retval.m_LastBackupDate = startdate + ts;
                fs.Read(bytebuffer, 0, 4);
                retval.m_ModificationNumber = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);
                retval.m_AppInfoID = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);
                retval.m_SortInfoID = BytesToUint(bytebuffer);
                buffer = new char[4];
                sr.DiscardBufferedData();
                sr.Read(buffer, 0, 4);
                retval.m_Type = new string(buffer);
                sr.Read(buffer, 0, 4);
                retval.m_Creator = new string(buffer);
                fs.Seek(68, SeekOrigin.Begin);
                fs.Read(bytebuffer, 0, 4);

                retval.m_UniqueIDSeed = BytesToUint(bytebuffer);
                fs.Read(bytebuffer, 0, 4);

                retval.m_NextRecordListID = BytesToUint(bytebuffer);
                bytebuffer = new byte[4];
                fs.Read(bytebuffer, 2, 2);

                // Load RecordInfo

                retval.m_NumberOfRecords = BytesToUint(bytebuffer);
                retval.m_RecordInfoList = new PalmRecordInfo[retval.m_NumberOfRecords];
                retval.m_RecordList = new PalmRecord[retval.m_NumberOfRecords];
                for (int i = 0; i < retval.m_NumberOfRecords; i++)
                {
                    fs.Read(bytebuffer, 0, 4);
                    retval.m_RecordInfoList[i].DataOffset = BytesToUint(bytebuffer);

                    bytebuffer = new byte[4];
                    fs.Read(bytebuffer, 3, 1);
                    retval.m_RecordInfoList[i].Attributes = BytesToUint(bytebuffer);

                    bytebuffer = new byte[4];
                    fs.Read(bytebuffer, 1, 3);
                    retval.m_RecordInfoList[i].UniqueID = BytesToUint(bytebuffer);
                }

                //Load Records

                uint StartOffset;
                uint EndOffset;
                int RecordLength;
                for (int i = 0; i < retval.m_NumberOfRecords - 1; i++)
                {
                    StartOffset = retval.m_RecordInfoList[i].DataOffset;
                    EndOffset = retval.m_RecordInfoList[i + 1].DataOffset;
                    RecordLength = ((int)(EndOffset - StartOffset));
                    fs.Seek(StartOffset, SeekOrigin.Begin);
                    retval.m_RecordList[i].Data = new byte[RecordLength];
                    fs.Read(retval.m_RecordList[i].Data, 0, RecordLength);
                }

                StartOffset = retval.m_RecordInfoList[retval.m_NumberOfRecords - 1].DataOffset;
                RecordLength = (int)(fs.Length - ((int)StartOffset));
                fs.Seek(StartOffset, SeekOrigin.Begin);
                retval.m_RecordList[retval.m_NumberOfRecords - 1].Data = new byte[RecordLength];
                fs.Read(retval.m_RecordList[retval.m_NumberOfRecords - 1].Data, 0, RecordLength);

                // LoadHeader
                List<byte> empty2 = new List<byte>();
                List<byte> temp = new List<byte>();
                empty2.Add(0);
                empty2.Add(0);

                List<byte> headerdata = new List<byte>();

                headerdata.AddRange(retval.m_RecordList[0].Data);

                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(0, 2));
                retval.m_Compression = BytesToUint(temp.ToArray());
                temp.Clear();
                temp.AddRange(headerdata.GetRange(4, 4));
                retval.m_TextLength = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(8, 2));
                retval.m_TextRecordCount = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(empty2);
                temp.AddRange(headerdata.GetRange(10, 2));
                retval.m_TextRecordSize = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(headerdata.GetRange(12, 4));
                retval.m_CurrentPosition = BytesToUint(temp.ToArray());
                ;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            return retval;
        }

        protected static uint Reversebytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
            (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static uint BytesToUint(byte[] bytes)
        {
            return (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        }

        public static uint BytesToUintR(byte[] bytes)
        {
            return Reversebytes((uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]));
        }
    }
}