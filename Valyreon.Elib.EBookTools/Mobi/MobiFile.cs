using System;
using System.Collections.Generic;
using System.Text;

namespace Valyreon.Elib.EbookTools.Mobi
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
            MAppInfoId = pf.AppInfoId;
            MAttributes = pf.MAttributes;
            MCompression = pf.Compression;
            MCreationDate = pf.CreationDate;
            MCreator = pf.Creator;
            MCurrentPosition = pf.CurrentPosition;
            MFileName = pf.FileName;
            MLastBackupDate = pf.LastBackupDate;
            MModificationDate = pf.ModificationDate;
            MModificationNumber = pf.ModificationNumber;
            MName = pf.Name;
            MNextRecordListId = pf.NextRecordListId;
            MNumberOfRecords = pf.NumberOfRecords;
            MRecordList = pf.RecordList;
            MSortInfoId = pf.SortInfoId;
            MTextLength = pf.TextLength;
            MTextRecordCount = pf.TextRecordCount;
            MTextRecordSize = pf.TextRecordSize;
            MType = pf.Type;
            MUniqueIdSeed = pf.UniqueIdSeed;
            MVersion = pf.Version;
            MRecordInfoList = pf.MRecordInfoList;
        }

        public string BookText => MBookText;

        public uint EncryptionType => MEncryptionType;

        public static new MobiFile LoadFile(byte[] file)
        {
            var retval = new MobiFile(PalmFile.LoadFile(file));
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
                var sb = new StringBuilder();
                var a = 1;
                while (a < retval.MTextRecordCount + 1)
                {
                    var blockbuilder = new List<byte>();
                    var datatemp = new List<byte>(retval.MRecordList[a++].Data) { 0 };
                    var pos = 0;
                    var temps = new List<byte>();

                    while (pos < datatemp.Count && blockbuilder.Count < 4096)
                    {
                        var ab = datatemp[pos++];
                        if (ab is 0x00 or (> 0x08 and <= 0x7f))
                        {
                            blockbuilder.Add(ab);
                            //blockbuilder.Add (0);
                        }
                        else if (ab is > 0x00 and <= 0x08)
                        {
                            temps.Clear();
                            temps.Add(0);
                            temps.Add(0);
                            temps.Add(0);
                            temps.Add(ab);
                            var value = BytesToUint(temps.ToArray());
                            for (uint i = 0; i < value; i++)
                            {
                                blockbuilder.Add(datatemp[pos++]);
                            }

                            //	blockbuilder.Add (0);
                        }
                        else if (ab is > 0x7f and <= 0xbf)
                        {
                            temps.Clear();
                            temps.Add(0);
                            temps.Add(0);
                            var bb = (byte)(ab & 63); // do this to drop the first 2 bits
                            temps.Add(bb);
                            temps.Add(pos < datatemp.Count ? datatemp[pos++] : 0);

                            var b = BytesToUint(temps.ToArray());
                            var dist = (b >> 3) * 1;
                            var len = (b << 29) >> 29;
                            var uncompressedpos = blockbuilder.Count - (int)dist;
                            for (var i = 0; i < (len + 3) * 1; i++)
                            {
                                try
                                {
                                    blockbuilder.Add(blockbuilder[uncompressedpos + i]);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        else if (ab is > 0xbf and <= 0xff)
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
                var off1 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(huffdata.GetRange(20, 4));
                var off2 = BytesToUint(temp.ToArray());

                temp.Clear();
                temp.AddRange(cdicdata.GetRange(12, 4));
                var entrybits = BytesToUint(temp.ToArray());

                var huffdict1 = new List<uint>();
                var huffdict2 = new List<uint>();
                var huffdicts = new List<List<byte>>();

                for (var i = 0; i < 256; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off1 + (i * 4)), 4));
                    huffdict1.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }

                for (var i = 0; i < 64; i++)
                {
                    temp.Clear();
                    temp.AddRange(huffdata.GetRange((int)(off2 + (i * 4)), 4));
                    huffdict2.Add(BitConverter.ToUInt32(temp.ToArray(), 0));
                }

                for (var i = 1; i < retval.MHuffCount; i++)
                {
                    huffdicts.Add(new List<byte>(retval.MRecordList[retval.MHuffOffset + i].Data));
                }

                var sb = new StringBuilder();
                for (var i = 0; i < retval.MTextRecordCount; i++)
                {
                    // Remove Trailing Entries
                    var datatemp = new List<byte>(retval.MRecordList[1 + i].Data);
                    var size = GetSizeOfTrailingDataEntries(datatemp.ToArray(), datatemp.Count, retval.MExtraFlags);

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
            var retval = new StringBuilder();

            if (depth > 32)
            {
                throw new Exception("corrupt file");
            }

            while (bits.Left())
            {
                var dw = bits.Peek(32);
                var v = huffdict1[dw >> 24];
                var codelen = v & 0x1F;
                //assert codelen != 0;
                var code = dw >> (int)(32 - codelen);
                ulong r = v >> 8;
                if ((v & 0x80) == 0)
                {
                    while (code < huffdict2[(codelen - 1) * 2])
                    {
                        ++codelen;
                        code = dw >> (int)(32 - codelen);
                    }

                    r = huffdict2[((codelen - 1) * 2) + 1];
                }

                r -= code;
                //assert codelen != 0;
                if (!bits.Eat(codelen))
                {
                    return retval.ToString();
                }

                var dicno = r >> entrybits;
                var off1 = 16 + ((r - (dicno << entrybits)) * 2);
                var dic = huffdicts[(int)(long)dicno];
                var off2 = 16 + ((char)dic[(int)(long)off1] * 256) + (char)dic[(int)(long)off1 + 1];
                var blen = ((char)dic[off2] * 256) + (char)dic[off2 + 1];
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
            var retval = 0;
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
            var bitpos = 0;
            while (true)
            {
                uint v = (char)ptr[size - 1];
                retval |= (v & 0x7F) << bitpos;
                bitpos += 7;
                --size;
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
            mData = new List<byte>(bytes)
            {
                0,
                0,
                0,
                0
            };
            mNbits = (mData.Count - 4) * 8;
        }

        public ulong Peek(ulong n)
        {
            ulong r = 0;
            ulong g = 0;
            while (g < n)
            {
                r = (r << 8) | (char)mData[(int)(long)((mPos + g) >> 3)];
                g = g + 8 - ((mPos + g) & 7);
            }

            return (r >> (int)(long)(g - n)) & (((ulong)1 << (int)n) - 1);
        }

        public bool Eat(uint n)
        {
            mPos += n;
            return mPos <= mNbits;
        }

        public bool Left()
        {
            return mNbits - mPos > 0;
        }
    }
}
