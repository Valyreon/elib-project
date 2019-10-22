using System;
using System.Collections.Generic;
using System.Text;

namespace EbookTools.Mobi
{
    internal class MobiFile : PalmFile
    {
        protected uint m_EncryptionType;
        protected uint m_HuffOffset;
        protected uint m_HuffCount;
        protected uint m_Extra_flags;
        protected string m_BookText;

        public uint EncryptionType
        {
            get { return m_EncryptionType; }
        }

        public string BookText
        {
            get { return m_BookText; }
        }

        public MobiFile() : base()
        {
        }

        public MobiFile(PalmFile pf)
            : base()
        {
            this.m_AppInfoID = pf.AppInfoID;
            this.m_Attributes = pf.m_Attributes;
            this.m_Compression = pf.Compression;
            this.m_CreationDate = pf.CreationDate;
            this.m_Creator = pf.Creator;
            this.m_CurrentPosition = pf.CurrentPosition;
            this.m_FileName = pf.FileName;
            this.m_LastBackupDate = pf.LastBackupDate;
            this.m_ModificationDate = pf.ModificationDate;
            this.m_ModificationNumber = pf.ModificationNumber;
            this.m_Name = pf.Name;
            this.m_NextRecordListID = pf.NextRecordListID;
            this.m_NumberOfRecords = pf.NumberOfRecords;
            this.m_RecordList = pf.RecordList;
            this.m_SortInfoID = pf.SortInfoID;
            this.m_TextLength = pf.TextLength;
            this.m_TextRecordCount = pf.TextRecordCount;
            this.m_TextRecordSize = pf.TextRecordSize;
            this.m_Type = pf.Type;
            this.m_UniqueIDSeed = pf.UniqueIDSeed;
            this.m_Version = pf.Version;
            this.m_RecordInfoList = pf.m_RecordInfoList;
        }

        public new static MobiFile LoadFile(byte[] file)
        {
            MobiFile retval = new MobiFile(PalmFile.LoadFile(file));
            List<byte> empty2 = new List<byte>();
            List<byte> temp = new List<byte>();
            empty2.Add(0);
            empty2.Add(0);
            List<byte> headerdata = new List<byte>();
            headerdata.AddRange(retval.m_RecordList[0].Data);

            temp.AddRange(empty2);
            temp.AddRange(headerdata.GetRange(12, 2));
            retval.m_EncryptionType = BytesToUint(temp.ToArray());

            if (retval.Compression == 2)
            {
                StringBuilder sb = new StringBuilder();
                int pos = 0;
                int a = 1;
                List<byte> datatemp = null;
                while (a < retval.m_TextRecordCount + 1)
                {
                    List<byte> blockbuilder = new List<byte>();
                    datatemp = new List<byte>(retval.m_RecordList[a++].Data);
                    datatemp.Add(0);
                    pos = 0;
                    List<byte> temps = new List<byte>();

                    while (pos < datatemp.Count && blockbuilder.Count < 4096)
                    {
                        byte ab = (byte)(datatemp[pos++]);
                        if (ab == 0x00 || (ab > 0x08 && ab <= 0x7f))
                        {
                            blockbuilder.Add(ab);
                            //blockbuilder.Add (0);
                        }
                        else if (ab > 0x00 && ab <= 0x08)
                        {
                            temps.Clear();
                            temps.Add(0);
                            temps.Add(0);
                            temps.Add(0);
                            temps.Add(ab);
                            uint value = BytesToUint(temps.ToArray());
                            for (uint i = 0; i < value; i++)
                            {
                                blockbuilder.Add((byte)(datatemp[pos++]));
                                //	blockbuilder.Add (0);
                            }
                        }
                        else if (ab > 0x7f && ab <= 0xbf)
                        {
                            temps.Clear();
                            temps.Add(0);
                            temps.Add(0);
                            byte bb = (byte)((ab & 63));  // do this to drop the first 2 bits
                            temps.Add(bb);
                            if (pos < datatemp.Count)
                            {
                                temps.Add((byte)(datatemp[pos++]));
                            }
                            else
                            {
                                temps.Add(0);
                            }

                            uint b = BytesToUint(temps.ToArray());
                            uint dist = (b >> 3) * 1;
                            uint len = ((b << 29) >> 29);
                            int uncompressedpos = blockbuilder.Count - ((int)dist);
                            for (int i = 0; i < (len + 3) * 1; i++)
                            {
                                try
                                {
                                    blockbuilder.Add(blockbuilder[uncompressedpos + i]);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        else if (ab > 0xbf && ab <= 0xff)
                        {
                            blockbuilder.Add(32);
                            //blockbuilder.Add (0);
                            blockbuilder.Add((byte)(ab ^ 0x80));
                            //blockbuilder.Add (0);
                        }
                    }

                    sb.Append(Encoding.UTF8.GetString(blockbuilder.ToArray()));
                }
                retval.m_BookText = sb.ToString();
            }
            else if (retval.Compression == 17480)
            {
                temp.Clear();
                temp.AddRange(headerdata.GetRange(112, 4));
                retval.m_HuffOffset = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(headerdata.GetRange(116, 4));
                retval.m_HuffCount = BytesToUint(temp.ToArray());

                if (headerdata.Count >= 244)
                {
                    temp.Clear();
                    temp.AddRange(headerdata.GetRange(240, 4));
                    retval.m_Extra_flags = BytesToUint(temp.ToArray());
                }

                uint off1;
                uint off2;
                uint entrybits;
                List<byte> huffdata = new List<byte>();
                List<byte> cdicdata = new List<byte>();
                huffdata.AddRange(retval.m_RecordList[retval.m_HuffOffset].Data);
                cdicdata.AddRange(retval.m_RecordList[retval.m_HuffOffset + 1].Data);

                temp.Clear();
                temp.AddRange(huffdata.GetRange(16, 4));
                off1 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(huffdata.GetRange(20, 4));
                off2 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(cdicdata.GetRange(12, 4));
                entrybits = BytesToUint(temp.ToArray());

                List<uint> huffdict1 = new List<uint>();
                List<uint> huffdict2 = new List<uint>();
                List<List<byte>> huffdicts = new List<List<byte>>();

                for (int i = 0; i < 256; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off1 + (i * 4)), 4));
                    huffdict1.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }
                for (int i = 0; i < 64; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off2 + (i * 4)), 4));
                    huffdict2.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }

                for (int i = 1; i < retval.m_HuffCount; i++)
                {
                    huffdicts.Add(new List<byte>(retval.m_RecordList[retval.m_HuffOffset + i].Data));
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retval.m_TextRecordCount; i++)
                {
                    // Remove Trailing Entries
                    List<byte> datatemp = new List<byte>(retval.m_RecordList[1 + i].Data);
                    int size = GetSizeOfTrailingDataEntries(datatemp.ToArray(), datatemp.Count, retval.m_Extra_flags);

                    sb.Append(Unpack(new BitReader(datatemp.GetRange(0, datatemp.Count - size).ToArray()), huffdict1.ToArray(), huffdict2.ToArray(), huffdicts, (int)((long)entrybits)));
                }

                retval.m_BookText = sb.ToString();
            }
            else
            {
                throw new Exception("Compression format is unsupported");
            }
            return retval;
        }

        protected static string ret = "";

        protected static string Unpack(BitReader bits, uint[] huffdict1, uint[] huffdict2, List<List<byte>> huffdicts, int entrybits)
        {
            return Unpack(bits, 0, huffdict1, huffdict2, huffdicts, entrybits);
        }

        protected static string Unpack(BitReader bits, int depth, uint[] huffdict1, uint[] huffdict2, List<List<byte>> huffdicts, int entrybits)
        {
            StringBuilder retval = new StringBuilder();

            if (depth > 32)
            {
                throw new Exception("corrupt file");
            }
            while (bits.Left())
            {
                ulong dw = bits.Peek(32);
                uint v = (huffdict1[dw >> 24]);
                uint codelen = v & 0x1F;
                //assert codelen != 0;
                ulong code = dw >> (int)(32 - codelen);
                ulong r = (v >> 8);
                if ((v & 0x80) == 0)
                {
                    while (code < ((ulong)huffdict2[(codelen - 1) * 2]))
                    {
                        codelen += 1;
                        code = dw >> (int)(32 - codelen);
                    }
                    r = huffdict2[(codelen - 1) * 2 + 1];
                }
                r -= code;
                //assert codelen != 0;
                if (!bits.Eat(codelen))
                {
                    return retval.ToString();
                }
                ulong dicno = r >> entrybits;
                ulong off1 = 16 + (r - (dicno << entrybits)) * 2;
                List<byte> dic = huffdicts[(int)((long)dicno)];
                int off2 = 16 + (char)(dic[(int)((long)off1)]) * 256 + (char)(dic[(int)((long)off1) + 1]);
                int blen = ((char)(dic[off2]) * 256 + (char)(dic[off2 + 1]));
                List<byte> slicelist = dic.GetRange(off2 + 2, (int)(blen & 0x7fff));
                byte[] slice = slicelist.ToArray();
                if ((blen & 0x8000) > 0)
                {
                    retval.Append(Encoding.ASCII.GetString(slice));
                }
                else
                {
                    retval.Append(Unpack(new BitReader(slice), depth + 1, huffdict1, huffdict2, huffdicts, entrybits));
                }
            }
            return retval.ToString();
        }

        protected static int GetSizeOfTrailingDataEntries(byte[] ptr, int size, uint flags)
        {
            int retval = 0;
            flags >>= 1;
            while (flags > 0)
            {
                if ((flags & 1) > 0)
                {
                    retval += (int)((long)GetSizeOfTrailingDataEntry(ptr, size - retval));
                }
                flags >>= 1;
            }
            return retval;
        }

        protected static uint GetSizeOfTrailingDataEntry(byte[] ptr, int size)
        {
            uint retval = 0;
            int bitpos = 0;
            while (true)
            {
                uint v = (char)(ptr[size - 1]);
                retval |= (v & 0x7F) << bitpos;
                bitpos += 7;
                size -= 1;
                if ((v & 0x80) != 0 || (bitpos >= 28) || (size == 0))
                {
                    return retval;
                }
            }
        }
    }

    public class BitReader
    {
        private List<byte> m_data;
        private uint m_pos = 0;
        private int m_nbits;

        public BitReader(byte[] bytes)
        {
            m_data = new List<byte>(bytes)
            {
                0,
                0,
                0,
                0
            };
            m_nbits = (m_data.Count - 4) * 8;
        }

        public ulong Peek(ulong n)
        {
            ulong r = 0;
            ulong g = 0;
            while (g < n)
            {
                r = (r << 8) | (char)(m_data[(int)((long)(m_pos + g >> 3))]);
                g = g + 8 - ((m_pos + g) & 7);
            }
            return (ulong)(r >> (int)((long)(g - n))) & (ulong)(((ulong)(1) << (int)n) - 1);
        }

        public bool Eat(uint n)
        {
            m_pos += n;
            return m_pos <= m_nbits;
        }

        public bool Left()
        {
            return (m_nbits - m_pos) > 0;
        }
    }
}