using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onix_Gameboy_Cartridge_Reader
{


    internal class Gen2SaveFile
    {
        byte[] data;
        bool Crystal = false;
        bool PrimaryBad = false;

        ushort PokedexAddr = 0;

        public bool IsCrystal
        {  get { return Crystal; } }

        // {From, To, Length},
        public static ushort[,] PrimaryToSecondaryInfoGS = 
        {
            { 0x2009,  0x15C7,  (ushort)(0x17EC - 0x15C7) },
            { 0x222F,  0x3D96,  (ushort)(0x3F3F - 0x3D96) },
            { 0x23D9,  0x0C6B,  (ushort)(0x10E7 - 0x0C6B) },
            { 0x2856,  0x7E39,  (ushort)(0x7E6C - 0x7E39) },
            { 0x288A,  0x10E8,  (ushort)(0x15C6 - 0x10E8) },
            { 0x2D69,  0x7E6D, (ushort)2 }
        };

        public ushort Checksum
        {
            get
            {
                return IsCrystal ? ChecksumC : ChecksumGS;
            }
        }

        public ushort ChecksumSecondary
        {
            get
            {
                return (ushort)((data[0x2D6A] << 8) | data[0x2D69]);
            }
        }

        public ushort PrimarySaveAddress
        {
            get { return (ushort)0x2009; }
        }

        public ushort PokedexAddress
        {
            get { return PokedexAddr; }
        }

        public ushort PokedexSecondaryAddress
        {
            get { return (ushort)(IsCrystal? 0x2A27 - (0x2009 - 0x1209):0x2A4C-(0x288A-0x10E8) ); }
        }

        public ushort ChecksumAddress
        { get { return (ushort)(IsCrystal?0x2D0D:0x2D69); } }

        public ushort ChecksumSecondaryAddress
        { get { return (ushort)(IsCrystal?0x1F0D:0x7E6D); } }

        public ushort ChecksumGS
        { get { return (ushort)((data[0x2D6A] << 8) | data[0x2D69]); } }

        public ushort ChecksumGS2
        { get { return (ushort)((data[0x7E6E] << 8) | data[0x7E6D]); } }

        public ushort ChecksumC
        { get { return (ushort)((data[0x2D0E] << 8) | data[0x2D0D]); } }

        public ushort ChecksumC2
        { get { return (ushort)((data[0x1F0E] << 8) | data[0x1F0D]); } }


        public ushort TrainerID
        {
            get { return BToU16(data, 0x2009); }
        }

        public ushort LottoNumber
        {
            get { return BToU16(data, 0x2851); }
        }

        public bool SecondaryValidGS
        {
            get
            {
                return ChecksumGS2 == GenerateSecondaryChecksumGoldSilver();
            }
        }

        public bool PrimaryValidGS
        {
            get
            {
                return ChecksumGS == GenerateChecksumGoldSilver();
            }
        }

        public bool SecondaryValidC
        {
            get
            {
                return ChecksumC2 == GenerateSecondaryChecksumCrystal();
            }
        }

        public bool PrimaryValidC
        {
            get
            {
                return ChecksumC == GenerateChecksumCrystal();
            }
        }


        public Gen2SaveFile(string filename) : this(File.ReadAllBytes(filename))
        { }


        public Gen2SaveFile(byte[] saveData)
        {
            data = saveData;

            if (PrimaryValidC)
                Crystal = true;
            else if (SecondaryValidC)
            {
                Crystal = true;
                PrimaryBad = true;
            }
            else if (!PrimaryValidGS)
                if (!SecondaryValidGS)
                    throw new Exception("Save file not recognized!");
                else
                    PrimaryBad = true;

            if (PrimaryBad)
                if (IsCrystal)
                    CopySecondaryToPrimaryC();
                else
                    CopySecondaryToPrimaryGS();

            PokedexAddr = (ushort)(IsCrystal ? 0x2A27 : 0x2A4C);
        }

        public static Gen2SaveFile FromPrimaryData(byte[] saveData)
        {
            byte[] mockSave = new byte[0x8000];
            saveData.CopyTo(mockSave, 0x2000);

            mockSave[0x7E6D] = 0xFF;
            mockSave[0x1F0D] = 0xFF;

            return new Gen2SaveFile(mockSave);
        }

        public byte[] Data
        { get { return data; } }

        public void UpdateChecksum()
        {
            ushort checksum = GenerateChecksum();

            if (IsCrystal)
            {
                data[0x2D0D] = (byte)(checksum & 0x00FF);
                data[0x2D0E] = (byte)(checksum >> 8);
            }
            else
            {
                data[0x2D69] = (byte)(checksum & 0x00FF);
                data[0x2D6A] = (byte)(checksum >> 8);
            }
        }

        public ushort GenerateChecksum()
        {
            if (IsCrystal)
                return GenerateChecksumCrystal();
            else
                return GenerateChecksumGoldSilver();
        }

        // Gold/Silver
        // Sum the bytes from 0x2009 to 0x2D68 and store the result at 0x2D69
        // Sum the bytes from 0x0C6B to 0x17EC, 0x3D96 to 0x3F3F and 0x7E39 to 0x7E6C, and store the result at 0x7E6D

        public void CopyPrimaryToSecondaryGS()
        {
            if (PrimaryValidGS)
            {
                for (int i = 0; i != 0x10E8 - 0x0C6B; ++i)
                    data[0x0C6B + i] = data[0x23D9 + i];

                for (int i = 0; i != 0x17ED - 0x15C7; ++i)
                    data[0x15C7 + i] = data[0x2009 + i];

                for (int i = 0; i != 0x15C7 - 0x10E8; ++i)
                    data[0x10E8 + i] = data[0x288A + i];

                for (int i = 0; i != 0x3F40 - 0x3D96; ++i)
                    data[0x3D96 + i] = data[0x222F + i];

                for (int i = 0; i != 0x7E6D - 0x7E39; ++i)
                    data[0x7E39 + i] = data[0x2856 + i];

                //Checksum
                data[0x7E6D] = data[0x2D69];
                data[0x7E6E] = data[0x2D6A];
            }

        }

        public void CopySecondaryToPrimaryGS()
        {
            if (SecondaryValidGS)
            {
                for (int i = 0; i != 0x10E8 - 0x0C6B; ++i)
                    data[0x23D9 + i] = data[0x0C6B + i];

                for (int i = 0; i != 0x17ED - 0x15C7; ++i)
                    data[0x2009 + i] = data[0x15C7 + i];

                for (int i = 0; i != 0x15C7 - 0x10E8; ++i)
                    data[0x288A + i] = data[0x10E8 + i];

                for (int i = 0; i != 0x3F40 - 0x3D96; ++i)
                    data[0x222F + i] = data[0x3D96 + i];

                for (int i = 0; i != 0x7E6D - 0x7E39; ++i)
                    data[0x2856 + i] = data[0x7E39 + i];

                //Checksum
                data[0x2D69] = data[0x7E6D];
                data[0x2D6A] = data[0x7E6E];

            }

        }

        public void CopyPrimaryToSecondaryC()
        {
            if (PrimaryValidC)
            {
                //0x2009	0x2B82	0x1209	0x1D82
                for (int i = 0; i != (0x1D82 - 0x1209); ++i)
                    data[0x1209 + i] = data[0x2009 + i];

                //Checksum
                data[0x1F0D] = data[0x2D0D];
                data[0x1F0E] = data[0x2D0E];
            }
        }

        public void CopySecondaryToPrimaryC()
        {
            if (SecondaryValidC)
            {
                //0x2009	0x2B82	0x1209	0x1D82
                for (int i = 0; i != (0x1D82 - 0x1209); ++i)
                    data[0x2009 + i] = data[0x1209 + i];

                //Checksum
                data[0x2D0D] = data[0x1F0D];
                data[0x2D0E] = data[0x1F0E];
            }
        }

        public ushort GenerateChecksumGoldSilver()
        {
            ushort checksum = 0;

            for (int i = 0x2009; i != 0x2D69; ++i)
                checksum += data[i];

            return checksum;
        }

        public ushort GenerateSecondaryChecksumGoldSilver()
        {
            ushort checksum = 0;

            for (int i = 0x0C6B; i != 0x17ED; ++i)
                checksum += data[i];

            for (int i = 0x3D96; i != 0x3F40; ++i)
                checksum += data[i];

            for (int i = 0x7E39; i != 0x7E6D; ++i)
                checksum += data[i];

            return checksum;
        }

        public ushort GenerateChecksumCrystal()
        {
            ushort checksum = 0;

            for (int i = 0x2009; i != 0x2B83; ++i)
                checksum += data[i];

            return checksum;
        }

        public ushort GenerateSecondaryChecksumCrystal()
        {
            ushort checksum = 0;

            for (int i = 0x1209; i != 0x1D83; ++i)
                checksum += data[i];

            return checksum;
        }

        public void SaveToFile(string filename)
        {
            UpdateChecksum();
            if (IsCrystal)
                CopyPrimaryToSecondaryC();
            else
                CopyPrimaryToSecondaryGS();

            File.WriteAllBytes(filename, data);
        }

        public byte[] GetPokedexData()
        {
            byte[] output = new byte[0x40];
            ;
            for (int i = 0; i != 0x40; ++i)
                output[i] = data[PokedexAddress + i];

            return output;
        }

        public void SetPokedexData(byte[] pokedexData)
        {
            for (int i = 0; i != 0x40; ++i)
                data[PokedexAddress + i] = pokedexData[i];
        }

        public void MergePokedexData(byte[] pokedexData)
        {
            for (int i = 0; i != 0x40; ++i)
                data[PokedexAddress + i] |= pokedexData[i];
        }

        public PokemonDataGen2[] GetPartyPokemon()
        {
            List<PokemonDataGen2> output = new List<PokemonDataGen2>();
            //GS 0x288A	  C 0x2865	
            ushort partyAddr = (ushort)(IsCrystal ? 0x2865 : 0x288A);

            for (int i = 0; i != data[partyAddr]; ++i)
                output.Add(PokemonDataGen2.PokemonDataFromData(data, partyAddr + 8 + (i * 48), false));

            return output.ToArray();
        }

        public PokemonDataGen2[] GetBoxedPokemon(int boxNumber)
        {
            List<PokemonDataGen2> output = new List<PokemonDataGen2>();
            //GS 0x288A	  C 0x2865	
            int currentBoxNumer = IsCrystal ? data[0x2700] : data[0x2724];
            ushort boxAddr = (ushort)(boxNumber == currentBoxNumer ? (IsCrystal?0x2D10:0x2D6C) : 0x4000 + 0x2000*(int)(boxNumber/7) + 0x450*(boxNumber%7));

            for (int i = 0; i != data[boxAddr]; ++i)
                output.Add(PokemonDataGen2.PokemonDataFromData(data, boxAddr + 22 + (i * 32), true));

            return output.ToArray();
        }


        ushort ushortFromByteArray(byte[] data, int index = 0)
        {
            return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(data, index));
        }

        ushort BToU16(byte[] data, int index = 0) => ushortFromByteArray(data, index);
    }

    static class GoldSilverAddressList
    {
        public const ushort Checksum1 = 0x2D69,
            Checksum2 = 0x7E6D,
            TrainerID = 0x2009,
            TimePlayed = 0x2053,
            FirstPokemonDV = 0x288A + 0x08 + 0x15;
    }

    static class CrystalAddressList
    {
        public const ushort Checksum1 = 0x2D0D,
            Checksum2 = 0x1F0D,
            TrainerID = 0x2009,
            TimePlayed = 0x2054,
            FirstPokemonDV = 0x2865 + 0x08 + 0x15;
    }
}
