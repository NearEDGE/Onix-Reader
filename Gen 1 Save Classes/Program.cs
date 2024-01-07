using System.IO;
using System.Text;
using Microsoft.Win32;
using Onix_Gameboy_Cartridge_Reader;

namespace Pokemon_Save_Classes
{
    internal class Program
    {
        static string[]? PokemonNames;

        static string[] RegionNames = { "Kanto", "Johto", "Hoenn", "Sinnoh", "Unova", "Kalos", "Alola", "Galar", "Hisui", "Paldea" };
        static int[] GenerationCount = { 151, 251, 386, 493, 649, 721, 809, 898, 905, 1025 };

        static byte[] GSCPokedex = new byte[0x40];
        static string GSCPokedexFile = @"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\GSCPokedex.dat";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            PokemonNames = File.ReadAllLines("Pokemon Names Gen 1 - 9.txt");
            Gen2SaveFile PKMNGOLD = new Gen2SaveFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\POKEMON_GLDAAUE.sav");
            Gen2SaveFile PKMNSILVER = new Gen2SaveFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\POKEMON_SLVAAXE.sav");
            Gen2SaveFile PKMNCRYSTAL = new Gen2SaveFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\PM_CRYSTAL.sav");

            if (File.Exists(GSCPokedexFile))
                GSCPokedex = File.ReadAllBytes(GSCPokedexFile);

            PKMNGOLD.MergePokedexData(GSCPokedex);
            GSCPokedex = PKMNGOLD.GetPokedexData();

            PKMNSILVER.MergePokedexData(GSCPokedex);
            GSCPokedex = PKMNSILVER.GetPokedexData();
            
            PKMNCRYSTAL.MergePokedexData(GSCPokedex);
            GSCPokedex = PKMNCRYSTAL.GetPokedexData();

            PKMNSILVER.MergePokedexData(GSCPokedex);
            PKMNGOLD.MergePokedexData(GSCPokedex);

            PKMNSILVER.SaveToFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\POKEMON_SLVAAXE.sav");
            PKMNGOLD.SaveToFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\POKEMON_GLDAAUE.sav");
            PKMNCRYSTAL.SaveToFile(@"..\..\..\..\Onix Gameboy Cartridge Reader\bin\Debug\PM_CRYSTAL.sav");

            byte[] PokemonOwned = GSCPokedex;

            ShowPokemonList(PokemonOwned, 2);
            /*
            Console.WriteLine("\r\n\r\nSave Data Test results:\r\n\r\n");

            byte[] SaveFileTest = File.ReadAllBytes("DataTest.sav");

            List<int[]> DataBocks = new List<int[]>();

            int blockStart = -1, blockLen = 0, lastData = 0;
            bool inBlock = false;

            for (int i = 0; i != SaveFileTest.Length; ++i)
                if (inBlock)
                    if (SaveFileTest[i] == 0x76 && PKMNSILVER.Data[i] == 0)
                    {
                        if ((i - lastData) > 7)
                        {
                            blockLen = 1 + lastData - blockStart;
                            DataBocks.Add(new int[] { blockStart, blockLen });
                            inBlock = false;
                        }
                    }
                    else
                        lastData = i;
                else if (SaveFileTest[i] != 0x76 || PKMNSILVER.Data[i] != 0)
                {
                    inBlock = true;
                    blockStart = lastData = i;
                }

            string[] DataBlockList = new string[DataBocks.Count];

            int count = 0;
            for (int i = 0; i != DataBocks.Count; ++i)
            {
                DataBlockList[i] = String.Format("[0x{0}, {1}],", DataBocks[i][0].ToString("X").PadLeft(4,'0'), DataBocks[i][1]);
                Console.WriteLine("0x{0} | 0x{1}", DataBocks[i][0].ToString("X").PadLeft(4,'0'), DataBocks[i][1].ToString("X").PadLeft(4,'0'));
                count += DataBocks[i][1];
            }

            Console.WriteLine("Total Length of real data: 0x{0:X}", count);

            File.WriteAllLines("GoldSilverDataBlockListV2.txt", DataBlockList);

            //*
            int least = 0;
            int[] indChecks = new int[256];
            int possibleValues = 256;

            byte[] data = new byte[0x8000];

            foreach (byte b in PKMNGOLD.Data)
                ++indChecks[b];

            foreach(byte b in PKMNSILVER.Data)
                ++indChecks[b];

            foreach(byte b in PKMNCRYSTAL.Data)
                ++indChecks[b];

            if (possibleValues > 0)
                for (int i = 0; i != 256; ++i)
                    if (indChecks[i] < indChecks[least])
                        least = i;

            Console.WriteLine("0x{0:X}  appears {1} times", least, indChecks[least]);

            
            //*
            for(int i = 0; i < 0x8000; ++i)
                data[i] = (byte)least;

            File.WriteAllBytes("GarbageFill.sav", data);//*/
            //Array.Copy(File.ReadAllBytes("PM_CRYSTAL.sav"), 0x2000, data, 0, 0x1000);




            //Gen2SaveFile PKMNSILVER = new Gen2SaveFile("SILVERCORRUPTED.sav");
            //Gen2SaveFile PKMNGOLD = new Gen2SaveFile("POKEMON_GLDAAUE.sav");
            //Gen2SaveFile PKMNSILVER = new Gen2SaveFile("POKEMON SILVER test.sav");
            //Gen1SaveFile PKMNBLUE = new Gen1SaveFile("POKEMON BLUE.sav");
            //Gen1SaveFile PKMNRED = new Gen1SaveFile("POKEMON RED.sav");

            //Console.WriteLine(" Original: {0:X}\r\nGenerated: {1:X}", PKMNCRYSTAL.Checksum, PKMNCRYSTAL.GenerateChecksum());

            //PKMNSILVER.UpdateChecksum();
            //PKMNSILVER.SaveToFile("SILVERTEST.sav");
            //PKMNCRYSTAL.MergePokedexData(PKMNSILVER.GetPokedexData());

            //PKMNCRYSTAL.SaveToFile("POKEMON CRYSTAL test.sav");

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

        static bool[] GetPokemonOwnedList(byte[] pokemonOwnedData, int generation = 1)
        {
            bool[] output = new bool[GenerationCount[generation-1]];

            int len = (int)Math.Ceiling(GenerationCount[generation - 1] / 8.0d);
            for (int i = 0; i != len; ++i)
                for (int j = 0; j != 8 && (i * 8 + j)<GenerationCount[generation - 1]; ++j)
                    output[i*8 + j] = ((pokemonOwnedData[i] >> j) & 0x01) == 1;

            return output;
        }

        static string RegionNameFromCount(int count)
        {
            for (int i = 0; i != GenerationCount.Length; ++i)
                if (GenerationCount[i] == count)
                    return RegionNames[i+1];

            return RegionNames[0];
        }

        static void ShowPokemonList(byte[] list, int generation = 1)
        {
            string extraNotification = "          ";

            bool[] PokemonOwnedList = GetPokemonOwnedList(list, generation);

            Console.WriteLine("Owned:\r\n");

            int count = 1, countTotal = 0;

            for (int i = 0; i != PokemonOwnedList.Length; ++i)
            {

                if (GenerationCount.Contains(i ) || i == 0)
                    if (i == 0)
                        Console.WriteLine("Kanto:\r\n");
                    else if (i != (GenerationCount[generation - 1]))
                    {
                        Console.WriteLine("\r\n\r\n{0}:\r\n", RegionNameFromCount(i));
                        count = 1;
                    }

                if (PokemonOwnedList[i])
                {

                    if (count++ % 5 != 0)
                        Console.Write("{0}", PokemonNames[i].PadLeft(20));
                    else
                        Console.WriteLine("{0}", PokemonNames[i].PadLeft(20));
                    ++countTotal;
                }
            }

            if (generation == 2)
                extraNotification += "Pokemon until all Pokemon Stadium 2 features: " + (150 - countTotal).ToString();

            Console.WriteLine("\r\n\r\nTotal Caught: {0}{1}\r\n\r\n", countTotal, extraNotification);

            

            Console.WriteLine("\r\n\r\nUnowned List\r\n");

            count = 1;

            for (int i = 0; i != PokemonOwnedList.Length; ++i)
            {

                if (GenerationCount.Contains(i) || i == 0)
                    if (i == 0)
                        Console.WriteLine("Kanto:\r\n");
                    else if (i != (GenerationCount[generation - 1] - 1))
                    {
                        Console.WriteLine("\r\n\r\n{0}:\r\n", RegionNameFromCount(i));
                        count = 1;
                    }

                if (!PokemonOwnedList[i])
                {

                    if (count++ % 5 != 0)
                        Console.Write("{0}", PokemonNames[i].PadLeft(20));
                    else
                        Console.WriteLine("{0}", PokemonNames[i].PadLeft(20));
                    //++countTotal;
                }
            }

            Console.WriteLine("\r\n\r\nTotal Remaining: {0}\r\n\r\n", GenerationCount[generation - 1] - countTotal);


        }
    }
}