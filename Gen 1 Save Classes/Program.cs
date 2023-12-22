using System.IO;

using Onix_Gameboy_Cartridge_Reader;

namespace Pokemon_Save_Classes
{
    internal class Program
    {
        static string[]? PokemonNames;

        static void Main(string[] args)
        {
            PokemonNames = File.ReadAllLines("Gen1 Pokemon Names.txt");

            byte[] data = new byte[0x1000];
            Array.Copy(File.ReadAllBytes("PM_CRYSTAL.sav"), 0x2000, data, 0, 0x1000);


            Gen2SaveFile PKMNSILVER = new Gen2SaveFile("POKEMON_SLVAAXE.sav");
            Gen2SaveFile PKMNGOLD = new Gen2SaveFile("POKEMON_GLDAAUE.sav");
            Gen2SaveFile PKMNCRYSTAL = Gen2SaveFile.FromPrimaryData(data);
            //Gen2SaveFile PKMNSILVER = new Gen2SaveFile("POKEMON SILVER test.sav");
            //Gen1SaveFile PKMNBLUE = new Gen1SaveFile("POKEMON BLUE.sav");
            //Gen1SaveFile PKMNRED = new Gen1SaveFile("POKEMON RED.sav");

            Console.WriteLine(" Original: {0:X}\r\nGenerated: {1:X}", PKMNCRYSTAL.Checksum, PKMNCRYSTAL.GenerateChecksum());

            PKMNSILVER.MergePokedexData(PKMNGOLD.GetPokedexData());
            PKMNCRYSTAL.MergePokedexData(PKMNSILVER.GetPokedexData());

            PKMNCRYSTAL.SaveToFile("POKEMON CRYSTAL test.sav");

            /*
            byte[] PokemonOwned = File.ReadAllBytes(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\RBYPokedex.dat");


            ShowPokemonList(PokemonOwned);


            /*
            byte[] MixedData, Datacomp = MixedData = PokemonOwnedBlue;


            byte curChecksum = (byte)(~PKMNBLUE.Checksum);

            for (int i = 0; i != 0x26; ++i)
            {
                MixedData[i] |= PokemonOwnedRed[i];

                curChecksum -= PokemonOwnedBlue[i];
                curChecksum += MixedData[i];// (byte)((PokemonOwnedBlue[i] ^ PokemonOwnedRed[i]) & PokemonOwnedRed[i]);
            }

            curChecksum = (byte)~curChecksum;

            // Mix Records
            for (int i = 0; i != 0x13; ++i)
            {
                PKMNBLUE.Data[0x25A3 + i] |= PKMNRED.Data[0x25A3 + i];
                PKMNBLUE.Data[0x25B6 + i] |= PKMNRED.Data[0x25B6 + i];
            }

            PKMNBLUE.UpdateChecksum();

            if (curChecksum != PKMNBLUE.Checksum)
                Console.WriteLine("\r\n\r\nChecksum mismatch:\r\n\r\n    Expected: 0x{0:X}\r\n         Got: 0x{1:X}\r\n\r\n", (byte)~PKMNBLUE.Checksum, (byte)~curChecksum);


            File.WriteAllBytes("POKEMON BLUE test.sav", PKMNBLUE.Data);

            Console.WriteLine("Done.");//*/

            Console.ReadLine();

        }

        static bool[] GetPokemonOwnedList(byte[] pokemonOwnedData)
        {
            bool[] output = new bool[152];

            for (int i = 0; i != 0x13; ++i)
                for (int j = 0; j != 8; ++j)
                    output[i*8 + j] = ((pokemonOwnedData[i] >> j) & 0x01) == 1;

            return output;
        }

        static bool[] GetPokemonUnownedList(byte[] pokemonOwnedData)
        {
            bool[] output = new bool[152];

            for (int i = 0; i != 0x13; ++i)
                for (int j = 0; j != 8; ++j)
                    output[i*8 + j] = ((pokemonOwnedData[i] >> j) & 0x01) == 0;

            return output;
        }

        static void ShowPokemonList(byte[] list)
        {

            bool[] PokemonOwnedList = GetPokemonOwnedList(list);

            Console.WriteLine("Owned:\r\n");

            int count = 1;

            for (int i = 0; i != 151; ++i)
                if (PokemonOwnedList[i])
                    if (count++ % 5 != 0 )
                        Console.Write("{0}", PokemonNames[i].PadLeft(20));
                    else
                        Console.WriteLine("{0}", PokemonNames[i].PadLeft(20));

            Console.WriteLine("\r\n\r\n");

            bool[] PokemonUnownedList = GetPokemonUnownedList(list);

            Console.WriteLine("Unowned:\r\n");

            count = 1;

            for (int i = 0; i != 151; ++i)
                if (PokemonUnownedList[i])
                    if (count++ % 5 != 0)
                        Console.Write("{0}", PokemonNames[i].PadLeft(20));
                    else
                        Console.WriteLine("{0}", PokemonNames[i].PadLeft(20));


        }
    }
}