using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gen_1_Save_Classes
{
    internal class Gen1SaveFile
    {
        public static DataBlockInfo PlayerName = new DataBlockInfo(0x2598, 0x0B),
            MainData = new DataBlockInfo(0x25A3, 0x789),
            SpriteData = new DataBlockInfo(0x2D2C, 0x200),
            PartyData = new DataBlockInfo(0x2F2C, 0x194),
            CurrentBoxData = new DataBlockInfo(0x30C0, 0x462),
            TilesetType = new DataBlockInfo(0x3522, 0x1),
            MainDataChecksum = new DataBlockInfo(0x3523, 0x1);


        byte[] data;

        public byte Checksum
        { get {  return data[0x3523]; } }

        public Gen1SaveFile(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public byte[] Data
            { get { return data; } }

        internal byte GenerateBank1Checksum()
        {
            byte checksum = 0x00;

            for(int i = 0x2598; i!= 0x3523; ++i)
                checksum = (byte)(checksum + data[i]);

            checksum = (byte)~checksum;

            return checksum;
        }

        public void UpdateChecksum()
        {
            data[0x3523] = GenerateBank1Checksum();
        }

    }

    internal struct DataBlockInfo
    {
        UInt16 offset, size;

        public DataBlockInfo(int _offset, int _size)
        {
            offset = (UInt16)_offset;
            size = (UInt16)_size;
        }

        public UInt16 Offset
            { get { return offset; } }
        public UInt16 Size
            { get { return size; } }
    }
}
