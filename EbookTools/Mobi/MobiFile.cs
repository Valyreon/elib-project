using System;
using System.Collections.Generic;
using System.Text;

namespace EbookTools.Mobi
{
    internal class MobiFile : PalmFile
    {
        protected static string Ret = "";
        protected string MBookText;
        protected uint MEncryptionType;
        protected uint MExtraFlags;
        protected uint MHuffCount;
        protected uint MHuffOffset;

        public MobiFile(PalmFile pf)
        {
            this.MAppInfoId = pf.AppInfoId;
            this.MAttributes = pf.MAttributes;
            this.MCompression = pf.Compression;
            this.MCreationDate = pf.CreationDate;
            this.MCreator = pf.Creator;
            this.MCurrentPosition = pf.CurrentPosition;
            this.MFileName = pf.FileName;
            this.MLastBackupDate = pf.LastBackupDate;
            this.MModificationDate = pf.ModificationDate;
            this.MModificationNumber = pf.ModificationNumber;
            this.MName = pf.Name;
            this.MNextRecordListId = pf.NextRecordListId;
            this.MNumberOfRecords = pf.NumberOfRecords;
            this.MRecordList = pf.RecordList;
            this.MSortInfoId = pf.SortInfoId;
            this.MTextLength = pf.TextLength;
            this.MTextRecordCount = pf.TextRecordCount;
            this.MTextRecordSize = pf.TextRecordSize;
            this.MType = pf.Type;
            this.MUniqueIdSeed = pf.UniqueIdSeed;
            this.MVersion = pf.Version;
            this.MRecordInfoList = pf.MRecordInfoList;
        }

        public string BookText => this.MBookText;

        public uint EncryptionType => this.MEncryptionType;

        public new static MobiFile LoadFile(byte[] file)
        {
            MobiFile retval = new MobiFile(PalmFile.LoadFile(file));
            var empty2 = new List<byte>();
            var temp = new List<byte>();
            empty2.Add(0);
            empty2.Add(0);
            var headerdata = new List<byte>();
            headerdata.AddRange(retval.MRecordList[0].Data);

            temp.AddRange(empty2);
            temp.AddRange(headerdata.GetRange(12, 2));
            retval.MEncryptionType = BytesToUint(temp.ToArray());

            if (retval.Compression == 2)
            {
                StringBuilder sb = new StringBuilder();
                int a = 1;
                while (a < retval.MTextRecordCount + 1)
                {
                    var blockbuilder = new List<byte>();
                    var datatemp = new List<byte>(retval.MRecordList[a++].Data) { 0 };
                    int pos = 0;
                    var temps = new List<byte>();

                    while (pos < datatemp.Count && blockbuilder.Count < 4096)
                    {
                        byte ab = datatemp[pos++];
                        if (ab == 0x00 || ab > 0x08 && ab <= 0x7f)
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
                                blockbuilder.Add(datatemp[pos++]);
                            }

                            //	blockbuilder.Add (0);
                        }
                        else if (ab > 0x7f && ab <= 0xbf)
                        {
                            temps.Clear();
                            temps.Add(0);
                            temps.Add(0);
                            byte bb = (byte)(ab & 63); // do this to drop the first 2 bits
                            temps.Add(bb);
                            temps.Add(pos < datatemp.Count ? datatemp[pos++] : (byte)0);

                            uint b = BytesToUint(temps.ToArray());
                            uint dist = (b >> 3) * 1;
                            uint len = (b << 29) >> 29;
                            int uncompressedpos = blockbuilder.Count - (int)dist;
                            for (int i = 0; i < (len + 3) * 1; i++)
                            {
                                try
                                {
                                    blockbuilder.Add(blockbuilder[uncompressedpos + i]);
                                }
                                catch (Exception)
                                {
                                    // ignored
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

                retval.MBookText = sb.ToString();
            }
            else if (retval.Compression == 17480)
            {
                temp.Clear();
                temp.AddRange(headerdata.GetRange(112, 4));
                retval.MHuffOffset = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(headerdata.GetRange(116, 4));
                retval.MHuffCount = BytesToUint(temp.ToArray());

                if (headerdata.Count >= 244)
                {
                    temp.Clear();
                    temp.AddRange(headerdata.GetRange(240, 4));
                    retval.MExtraFlags = BytesToUint(temp.ToArray());
                }

                var huffdata = new List<byte>();
                var cdicdata = new List<byte>();
                huffdata.AddRange(retval.MRecordList[retval.MHuffOffset].Data);
                cdicdata.AddRange(retval.MRecordList[retval.MHuffOffset + 1].Data);

                temp.Clear();
                temp.AddRange(huffdata.GetRange(16, 4));
                uint off1 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(huffdata.GetRange(20, 4));
                uint off2 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(cdicdata.GetRange(12, 4));
                uint entrybits = BytesToUint(temp.ToArray());

                var huffdict1 = new List<uint>();
                var huffdict2 = new List<uint>();
                var huffdicts = new List<List<byte>>();

                for (int i = 0; i < 256; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off1 + i * 4), 4));
                    huffdict1.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }

                for (int i = 0; i < 64; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off2 + i * 4), 4));
                    huffdict2.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }

                for (int i = 1; i < retval.MHuffCount; i++)
                {
                    huffdicts.Add(new List<byte>(retval.MRecordList[retval.MHuffOffset + i].Data));
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retval.MTextRecordCount; i++)
                {
                    // Remove Trailing Entries
                    var datatemp = new List<byte>(retval.MRecordList[1 + i].Data);
                    int size = GetSizeOfTrailingDataEntries(datatemp.ToArray(), datatemp.Count, retval.MExtraFlags);

                    sb.Append(Unpack(new BitReader(datatemp.GetRange(0, datatemp.Count - size).ToArray()),
                        huffdict1.ToArray(), huffdict2.ToArray(), huffdicts, (int)entrybits));
                }

                retval.MBookText = sb.ToString();
            }
            else
            {
                throw new Exception("Compression format is unsupported");
            }

            return retval;
        }

        protected static string Unpack(BitReader bits, uint[] huffdict1, uint[] huffdict2, List<List<byte>> huffdicts,
            int entrybits)
        {
            return Unpack(bits, 0, huffdict1, huffdict2, huffdicts, entrybits);
        }

        protected static string Unpack(BitReader bits, int depth, uint[] huffdict1, uint[] huffdict2,
            List<List<byte>> huffdicts, int entrybits)
        {
            StringBuilder retval = new StringBuilder();

            if (depth > 32)
            {
                throw new Exception("corrupt file");
            }

            while (bits.Left())
            {
                ulong dw = bits.Peek(32);
                uint v = huffdict1[dw >> 24];
                uint codelen = v & 0x1F;
                //assert codelen != 0;
                ulong code = dw >> (int)(32 - codelen);
                ulong r = v >> 8;
                if ((v & 0x80) == 0)
                {
                    while (code < huffdict2[(codelen - 1) * 2])
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
                var dic = huffdicts[(int)(long)dicno];
                int off2 = 16 + (char)dic[(int)(long)off1] * 256 + (char)dic[(int)(long)off1 + 1];
                int blen = (char)dic[off2] * 256 + (char)dic[off2 + 1];
                var slicelist = dic.GetRange(off2 + 2, blen & 0x7fff);
                var slice = slicelist.ToArray();
                retval.Append((blen & 0x8000) > 0
                    ? Encoding.ASCII.GetString(slice)
                    : Unpack(new BitReader(slice), depth + 1, huffdict1, huffdict2, huffdicts, entrybits));
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
                    retval += (int)GetSizeOfTrailingDataEntry(ptr, size - retval);
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
                uint v = (char)ptr[size - 1];
                retval |= (v & 0x7F) << bitpos;
                bitpos += 7;
                size -= 1;
                if ((v & 0x80) != 0 || bitpos >= 28 || size == 0)
                {
                    return retval;
                }
            }
        }
    }

    public class BitReader
    {
        private readonly List<byte> mData;
        private readonly int mNbits;
        private uint mPos;

        public BitReader(IEnumerable<byte> bytes)
        {
            this.mData = new List<byte>(bytes)
            {
                0,
                0,
                0,
                0
            };
            this.mNbits = (this.mData.Count - 4) * 8;
        }

        public ulong Peek(ulong n)
        {
            ulong r = 0;
            ulong g = 0;
            while (g < n)
            {
                r = (r << 8) | (char)this.mData[(int)(long)((this.mPos + g) >> 3)];
                g = g + 8 - ((this.mPos + g) & 7);
            }

            return (r >> (int)(long)(g - n)) & (((ulong)1 << (int)n) - 1);
        }

        public bool Eat(uint n)
        {
            this.mPos += n;
            return this.mPos <= this.mNbits;
        }

        public bool Left()
        {
            return this.mNbits - this.mPos > 0;
        }
    }
}