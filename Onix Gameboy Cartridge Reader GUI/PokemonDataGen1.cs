using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Onix_Gameboy_Cartridge_Reader
{
    internal class PokemonDataGen1
    {
        //Match string from Bulbapedia (0x[0-9A-F]+)\t([0-9A-Za-z '/]*)\t([0-9] bytes?)

        bool isShortFormat = false;

        public byte SpeciesID; //Offset: 0x00   Length: 1 byte
        public ushort CurrentHP; //Offset: 0x01   Length: 2 bytes
        public byte Level; //Offset: 0x03   Length: 1 byte
        public byte StatusCondition; //Offset: 0x04   Length: 1 byte
        public byte Type1; //Offset: 0x05   Length: 1 byte
        public byte Type2; //Offset: 0x06   Length: 1 byte
        public byte CatchRate; //Offset: 0x07   Length: 1 byte        public  Held Item in Gen 2
        public byte Move1; //Offset: 0x08   Length: 1 byte
        public byte Move2; //Offset: 0x09   Length: 1 byte
        public byte Move3; //Offset: 0x0A   Length: 1 byte
        public byte Move4; //Offset: 0x0B   Length: 1 byte
        public ushort OTID; //Offset: 0x0C   Length: 2 bytes
        public int EXP; //Offset: 0x0E   Length: 3 bytes
        public ushort HPEV; //Offset: 0x11   Length: 2 bytes
        public ushort AttackEV; //Offset: 0x13   Length: 2 bytes
        public ushort DefenseEV; //Offset: 0x15   Length: 2 bytes
        public ushort SpeedEV; //Offset: 0x17   Length: 2 bytes
        public ushort SpecialEV; //Offset: 0x19   Length: 2 bytes
        public ushort DV; //Offset: 0x1B   Length: 2 bytes
        public byte Move1PP; //Offset: 0x1D   Length: 1 byte
        public byte Move2PP; //Offset: 0x1E   Length: 1 byte
        public byte Move3PP; //Offset: 0x1F   Length: 1 byte
        public byte Move4PP; //Offset: 0x20   Length: 1 byte
        public byte LevelDuplicate; //Offset: 0x21   Length: 1 byte
        public ushort MaxHP; //Offset: 0x22   Length: 2 bytes
        public ushort Attack; //Offset: 0x24   Length: 2 bytes
        public ushort Defense; //Offset: 0x26   Length: 2 bytes
        public ushort Speed; //Offset: 0x28   Length: 2 bytes
        public ushort Special; //Offset: 0x2A   Length: 2 bytes

        public PokemonDataGen1(byte[] baseData)
        {
            if (baseData.Length == 33 || baseData.Length == 44)
            {
                SpeciesID = baseData[0];
                CurrentHP = BToU16(baseData, 0x01);
                Level = baseData[0x03];
                StatusCondition = baseData[0x04];
                Type1 = baseData[0x05]; //Offset: 0x05   Length: 1 byte
                Type2 = baseData[0x06]; //Offset: 0x06   Length: 1 byte
                CatchRate = baseData[0x07]; //Offset: 0x07   Length: 1 byte        public  Held Item in Gen 2
                Move1 = baseData[0x08]; //Offset: 0x08   Length: 1 byte
                Move2 = baseData[0x09]; //Offset: 0x09   Length: 1 byte
                Move3 = baseData[0x0A]; //Offset: 0x0A   Length: 1 byte
                Move4 = baseData[0x0B]; //Offset: 0x0B   Length: 1 byte
                OTID = BToU16(baseData, 0x0C); //Offset: 0x0C   Length: 2 bytes
                EXP = baseData[0x10] | (baseData[0x0F] << 8) | (baseData[0x0E] << 16); //Offset: 0x0E   Length: 3 bytes
                HPEV = BToU16(baseData, 0x11); //Offset: 0x11   Length: 2 bytes
                AttackEV = BToU16(baseData, 0x13); //Offset: 0x13   Length: 2 bytes
                DefenseEV = BToU16(baseData, 0x15); //Offset: 0x15   Length: 2 bytes
                SpeedEV = BToU16(baseData, 0x17); //Offset: 0x17   Length: 2 bytes
                SpecialEV = BToU16(baseData, 0x19); //Offset: 0x19   Length: 2 bytes
                DV = BToU16(baseData, 0x1B); //Offset: 0x1B   Length: 2 bytes
                Move1PP = baseData[0x1D]; //Offset: 0x1D   Length: 1 byte
                Move2PP = baseData[0x1E]; //Offset: 0x1E   Length: 1 byte
                Move3PP = baseData[0x1F]; //Offset: 0x1F   Length: 1 byte
                Move4PP = baseData[0x20]; //Offset: 0x20   Length: 1 byte

                if (baseData.Length == 44)
                {
                    LevelDuplicate = baseData[0x21]; //Offset: 0x21   Length: 1 byte
                    MaxHP = BToU16(baseData, 0x22); //Offset: 0x22   Length: 2 bytes
                    Attack = BToU16(baseData, 0x24); //Offset: 0x24   Length: 2 bytes
                    Defense = BToU16(baseData, 0x26); //Offset: 0x26   Length: 2 bytes
                    Speed = BToU16(baseData, 0x28); //Offset: 0x28   Length: 2 bytes
                    Special = BToU16(baseData, 0x2A); //Offset: 0x2A   Length: 2 bytes
                }
                else
                    isShortFormat = true;
            }
            else
                throw new Exception("Unknown data");
        }

        public static PokemonDataGen1 PokemonDataFromData(byte[] baseData, int startIndex, bool shortForm = true)
        {
            
            byte[] tmp = new byte[shortForm?33:44];
            Array.Copy(baseData, startIndex, tmp, 0, tmp.Length);

            return new PokemonDataGen1(tmp);

        }


        ushort ushortFromByteArray(byte[] data, int index = 0)
        {
            return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(data, index));
        }

        ushort BToU16(byte[] data, int index = 0) => ushortFromByteArray(data, index);
    }
}

