// Use this code inside a project created with the Visual C# > Windows Desktop > Console Application template.
// Replace the code in Program.cs with this code.

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Onix_Gameboy_Cartridge_Reader
{
    public class Onix_Gameboy_Cartridge_Reader
    {
        static bool _continue, _suspendConsole, _cancelOperation;
        static SerialPort _serialPort;
        static object dataLock = new object();
        static List<byte> ReceivedBytes = new List<byte>();
        static byte[] RBYPokedex = new byte[0x26];
        static string RBYPokedexFile = "RBYPokedex.dat";

        static byte[] GSCPokedex = new byte[0x40];
        static string GSCPokedexFile = "GSCPokedex.dat";


        public static void Main()
        {
            bool exitFlag = false;

            string name;
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = default;

            while (!exitFlag)
                try
                {
                    readThread = new Thread(Read);
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelOperationHandler);

                    if (File.Exists(RBYPokedexFile))
                        RBYPokedex = File.ReadAllBytes(RBYPokedexFile);

                    if (File.Exists(GSCPokedexFile))
                        GSCPokedex = File.ReadAllBytes(GSCPokedexFile);

                    // Create a new SerialPort object with default settings.
                    _serialPort = new SerialPort();

                    string selectedPort = "";

                    while (selectedPort.Equals(""))
                    {
                        Console.Clear();
                        Console.WriteLine("Choose a Serial Port:\r\n");

                        string[] ports = SerialPort.GetPortNames();
                        Array.Sort(ports);

                        foreach (string s in ports)
                            Console.WriteLine("   {0}", s);

                        string res = Console.ReadLine();

                        foreach (string s in ports)
                            if (res.ToLower().Contains(s.ToLower()))
                            {
                                selectedPort = s;
                                break;
                            }
                    }


                    // Allow the user to set the appropriate properties.
                    _serialPort.PortName = selectedPort;// SetPortName(_serialPort.PortName);
                    _serialPort.BaudRate = 115200; // SetPortBaudRate(_serialPort.BaudRate);
                    _serialPort.Parity = Parity.None; //SetPortParity(_serialPort.Parity);
                    _serialPort.DataBits = 8;// SetPortDataBits(_serialPort.DataBits);
                    _serialPort.StopBits = StopBits.One;// SetPortStopBits(_serialPort.StopBits);
                    _serialPort.Handshake = Handshake.None;// SetPortHandshake(_serialPort.Handshake);

                    // Set the read/write timeouts
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;

                    //_serialPort.ReadBufferSize

                    _serialPort.Open();
                    _continue = true;
                    readThread.Start();

                    name = "Self";

                    Console.WriteLine("Enter QUIT or Q to exit");

                    while (_continue)
                    {
                        if (_suspendConsole)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            message = Console.ReadLine();


                            if (message.ToLower().StartsWith("x"))
                            {
                                byte[] byteOut = new byte[] { byte.Parse(message.Substring(1), System.Globalization.NumberStyles.HexNumber) };

                                _serialPort.Write(byteOut, 0, 1);
                            }
                            else if (stringComparer.Equals("head", message))
                            {
                                _serialPort.Write(new byte[] { 0x03, 0xFF }, 0, 1);
                            }
                            else if (stringComparer.Equals("title", message) || stringComparer.Equals("t", message))
                            {
                                lock (dataLock)
                                    _suspendConsole = true;

                                byte[] header = GetBytes(0x0000, 0x0200, false);

                                string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
                                if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

                                Console.WriteLine("{0}\r\n", ROMName);

                                lock (dataLock)
                                    _suspendConsole = false;
                            }
                            else if (stringComparer.Equals("readbank", message))
                            {
                                ReadCommand(0x4000, 0x50, false);
                            }
                            else if (stringComparer.Equals("read", message))
                            {
                                ReadCommand(0x0100, 0x50, false);
                            }
                            else if (stringComparer.Equals("write", message))
                            {
                                WriteCommand(0x2100, 0x06, false);
                            }
                            else if (stringComparer.Equals("reset", message))
                            {
                                WriteCommand(0x2100, 0x00, false);
                            }
                            else if (message.ToLower().StartsWith("dumprom"))
                            {
                                bool fail = false;
                                string[] messageList = message.Split(' ');
                                int banks = 0;
                                if (messageList.Length > 3)
                                    if (!int.TryParse(messageList[3], out banks))
                                    {
                                        Console.WriteLine("\r\ndumprom\r\n\r\n\tUsage: dumprom [Text to Append to filename] [filename] [Number of banks to read]\r\n\r\n\tWhen run without parameters, dumprom will attempt to get the cartridge's name\r\n\t\tand number of banks from the ROM header\r\n\tTo omit certain parameter, simply add extra spaces as if they were there. Examples:\r\n\r\n\t\t> dumprom  MyRom - Outputs MyRom.gb\r\n\t\t> dumprom _32_Banks   32 - Outputs the first 32 banks of the ROM to [ROM Title]_32_Banks.gb");
                                        fail = true;
                                    }

                                if (!fail)
                                    DumpROMToFile(messageList.Length > 1 ? messageList[1] : "", messageList.Length > 2 ? messageList[2] : "", banks);
                            }
                            else if (stringComparer.Equals("dumpram", message))
                            {
                                DumpRAMToFile();
                            }
                            else if (message.ToLower().StartsWith("dumpram "))
                            {
                                DumpRAMToFile(message.Substring("dumpram ".Length));
                            }
                            else if (stringComparer.Equals("fulldump", message))
                            {
                                DumpROMToFile();
                                DumpRAMToFile();
                            }
                            else if (stringComparer.Equals("writeram", message))
                            {

                                lock (dataLock)
                                    _suspendConsole = true;

                                byte[] header = GetBytes(0x0000, 0x0200, false);

                                string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
                                if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

                                WriteRAMFromFile(ROMName + ".sav");

                                lock (dataLock)
                                    _suspendConsole = false;
                            }
                            else if (stringComparer.Equals("writeramgsc", message))
                            {

                                lock (dataLock)
                                    _suspendConsole = true;

                                byte[] header = GetBytes(0x0000, 0x0200, false);

                                string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
                                if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

                                WriteGSCRAMFromFile(ROMName + ".sav");

                                lock (dataLock)
                                    _suspendConsole = false;
                            }
                            else if (message.StartsWith("writeram "))
                            {
                                string filename = message.Substring("writeram ".Length);

                                if(File.Exists(filename))
                                    WriteRAMFromFile(filename);
                            }
                            else if (stringComparer.Equals("mergerbydex", message) || stringComparer.Equals("mrby", message))
                            {
                                MergePokedexRBY();
                            }
                            else if (stringComparer.Equals("mergegscdex", message) || stringComparer.Equals("mgsc", message))
                            {
                                MergePokedexGSC();
                            }
                            else if (stringComparer.Equals("quit", message) || stringComparer.Equals("q", message))
                            {
                                exitFlag = true;
                                _continue = false;
                            }
                            else if (stringComparer.Equals("tp", message))
                                TestProgressBar();
                            else if(message.ToLower().StartsWith("run"))
                            {
                                string[] args = message.ToLower().Split(' ');

                                switch (args[1])
                                {
                                    case "red":
                                        if (File.Exists("POKEMON RED.gb"))
                                            OpenWithDefaultProgram("POKEMON RED.gb");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;

                                    case "blue":
                                        if (File.Exists("POKEMON BLUE.gb"))
                                            OpenWithDefaultProgram("POKEMON BLUE.gb");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;

                                    case "yellow":
                                        if (File.Exists("POKEMON YELLOW.gb"))
                                            OpenWithDefaultProgram("POKEMON YELLOW.gb");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;

                                    //POKEMON_SLVAAXE  POKEMON_GLDAAUE
                                    case "gold":
                                        if (File.Exists("POKEMON_GLDAAUE.gb"))
                                            OpenWithDefaultProgram("POKEMON_GLDAAUE.gb");
                                        else if (File.Exists("POKEMON_GLDAAUE.gbc"))
                                            OpenWithDefaultProgram("POKEMON_GLDAAUE.gbc");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;

                                    case "silver":
                                        if (File.Exists("POKEMON_SLVAAXE.gb"))
                                            OpenWithDefaultProgram("POKEMON_SLVAAXE.gb");
                                        else if (File.Exists("POKEMON_SLVAAXE.gbc"))
                                            OpenWithDefaultProgram("POKEMON_SLVAAXE.gbc");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;

                                    case "crystal":
                                        if (File.Exists("PM_CRYSTAL.gb"))
                                            OpenWithDefaultProgram("PM_CRYSTAL.gb");
                                        else if (File.Exists("PM_CRYSTAL.gbc"))
                                            OpenWithDefaultProgram("PM_CRYSTAL.gbc");
                                        else
                                            Console.WriteLine("ROM not found. It might need to be dumped first");
                                        break;
                                }
                            }
                            else
                            {
                                _serialPort.WriteLine(
                                    String.Format("<{0}>: {1}", name, message)
                                    );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (readThread != null && readThread.ThreadState==System.Threading.ThreadState.Running) readThread.Join();
                    _serialPort.Close();
                }

            readThread.Join();
            _serialPort.Close();
        }

        public static void CancelOperationHandler(object sender, ConsoleCancelEventArgs e)
        {
            lock (dataLock)
                _cancelOperation = true;
        }

        public static byte[] GetBytes(ushort Address, ushort numBytes, bool SRAM)
        {
            lock (dataLock)
                ReceivedBytes.Clear();

            ReadCommand(Address, numBytes, SRAM);

            bool ready = false;
            while (!ready)
            {
                lock (dataLock)
                    if (ReceivedBytes.Count >= numBytes + 14)
                        ready = true;

                if (!ready)
                    Thread.Sleep(1000);
            }

            byte[] buffer;// = new byte[0x4000 + 16];

            lock (dataLock)
            {
                buffer = ReceivedBytes.ToArray();
                ReceivedBytes.Clear();
            }

            byte[] output = new byte[numBytes];

            int len = buffer[6] | buffer[7] << 8;
            Array.Copy(buffer, 8, output, 0, len);

            return output;
        }

        public static void DumpROMToFile(string appendText = "", string filename = "", int banks = 0)
        {
            DateTime start = DateTime.Now;
            lock (dataLock)
                _suspendConsole = true;
            List<byte> ROM = new List<byte>();

            ROM.AddRange(GetBytes(0x0000, 0x4000, false));

            string ROMName = filename == "" ? System.Text.Encoding.ASCII.GetString(ROM.ToArray(), 0x134, 0x0F) : filename;
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            int BankCount = 2;

            if (banks > 0)
                BankCount = banks;
            else
                switch (ROM[0x148])
                {
                    case 0x52:
                        BankCount = 72;
                        break;

                    case 0x53:
                        BankCount = 80;
                        break;

                    case 0x54:
                        BankCount = 96;
                        break;

                    default:
                        BankCount = 2 << ROM[0x148];
                        break;
                }

            Console.WriteLine("Reading ROM Bank 1 of " + BankCount.ToString());
            for (int i = 1; i != BankCount; ++i)
            {
                if (BankCount > 2)
                {
                    WriteCommand(0x2100, (byte)i, false);
                    if (i > 255)
                        WriteCommand(0x3100, 0x01, false);
                }
                ROM.AddRange(GetBytes(0x4000, 0x4000, false));

                lock (dataLock)
                    if (_cancelOperation)
                        break;

                Console.WriteLine("Reading ROM Bank {0} of {1}", i + 1, BankCount);
            }

            WriteCommand(0x2100, 0x01, false);

            if (System.IO.File.Exists(ROMName + appendText + ".gb"))
                System.IO.File.Delete(ROMName + appendText + ".gb");

            System.IO.File.WriteAllBytes(ROMName + appendText + ".gb", ROM.ToArray());

            Console.WriteLine("\r\nWrote {1} bytes to {2}\r\nCompleted in {0}", (DateTime.Now - start).ToString(), ROM.Count, ROMName + appendText + ".gb");

            lock (dataLock)
                _suspendConsole = false;
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public static void DumpRAMToFile(string filename = "")
        {

            lock (dataLock)
                _suspendConsole = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            int RAMBankSize = 8, RAMBanks = 1;

            bool hasRTC = Bank0[0x0147] == 0x0F || Bank0[0x0147] == 0x10;

            switch (Bank0[0x149])
            {
                case 0x00:
                    RAMBankSize = RAMBanks = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBanks = 4;
                    break;

                case 0x04:
                    RAMBanks = 16;
                    break;

                case 0x05:
                    RAMBanks = 8;
                    break;
            }

            List<byte> RAM = new List<byte>();

            string outFilename = filename.Equals("") ? ROMName + ".sav" : filename;
            if (!outFilename.ToLower().EndsWith(".sav"))
                outFilename += ".sav";

            Console.WriteLine("Dumping RAM to {0}", outFilename);

            WriteCommand(0x0000, 0x0A, true);


            for (byte i = 0; i != RAMBanks; ++i)
            {
                //ModeCommand(0x00);
                WriteCommand(0x4100, i, false);
                RAM.AddRange(GetBytes(0xA000, 0x2000, true));

                Console.WriteLine("Read Bank {0} of {1}", i + 1, RAMBanks);
            }

            if(hasRTC)
            {
                Console.WriteLine("Dumping RTC Registers...");

                //latch RTC Registers
                WriteCommand(0x6100, 0x00, false);
                WriteCommand(0x6100, 0x01, false);

                byte[] RTCData = new byte[20];
                for(byte i = 0x00; i != 0x05; ++i)
                {
                    WriteCommand(0x4100, (byte)(0x08+i), false);
                    RTCData[i*4] = GetBytes(0xA000, 0x01, true)[0];
                }
                RAM.AddRange(RTCData);
                RAM.AddRange(RTCData);
                byte[] unixtime = BitConverter.GetBytes(UnixTimeNow());
                RAM.AddRange(unixtime);

            }

            WriteCommand(0x0000, 0x00, false);

            //ModeCommand(0x00);

            if (System.IO.File.Exists(outFilename))
                System.IO.File.Delete(outFilename);

            System.IO.File.WriteAllBytes(outFilename, RAM.ToArray());

            Console.WriteLine("Done!");

            lock (dataLock)
                _suspendConsole = false;
        }

        public static void MergePokedexRBY()
        {

            lock (dataLock)
                _suspendConsole = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F).Trim();
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON RED") && !ROMName.Equals("POKEMON BLUE") && !ROMName.Equals("POKEMON YELLOW"))
            {
                Console.WriteLine("This function must be used with a Gen 1 North American Pokemon game");

                lock (dataLock)
                    _suspendConsole = false;
                return;
            }

            int RAMBankSize = 8, RAMBanks = 1;

            switch (Bank0[0x149])
            {
                case 0x00:
                    RAMBankSize = RAMBanks = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBanks = 4;
                    break;

                case 0x04:
                    RAMBanks = 16;
                    break;

                case 0x05:
                    RAMBanks = 8;
                    break;
            }

            List<byte> RAM = new List<byte>();

            Console.WriteLine("Pokedex merge starting...");

            WriteCommand(0x0000, 0x0A, true);

            WriteCommand(0x4100, 1, false);

            byte[] Bank1 = GetBytes(0xA000, 0x2000, true);
            Console.WriteLine("Read Bank 1");

            for (int i = 0; i != 0x26; ++i)
                RBYPokedex[i] |= Bank1[0x05A3 + i];

            RBYPokedex.CopyTo(Bank1, 0x05A3);
            Console.WriteLine("Merged with local Pokedex");

            byte checksum = GenerateGen1Bank1Checksum(Bank1);
            Console.WriteLine("Recalculated checksum");


            Thread.Sleep(500);

            WriteCommand((ushort)(0xA5A3), RBYPokedex, true);
            Console.WriteLine("Written merged Pokedex to cartridge");

            WriteCommand((ushort)(0xB523), checksum, true);
            Console.WriteLine("Updated cartridge Checksum");

            ModeCommand(0x00);

            WriteCommand(0x0000, 0x00, false);

            File.WriteAllBytes(RBYPokedexFile, RBYPokedex);

            Console.WriteLine("Done!\r\n");

            lock (dataLock)
                _suspendConsole = false;
        }

        public static byte GetRAMBankNumberFromAddress(ushort addr)
        {
            return (byte)(addr / 0x2000);
        }

        public static ushort RemapAddressToRAMArea(ushort addr)
        {
            return (ushort)(0xA000 + (addr - 0x2000*GetRAMBankNumberFromAddress(addr)));
        }

        public static void MergePokedexGSC()
        {

            lock (dataLock)
                _suspendConsole = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F).Trim();
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON_SLVAAXE") && !ROMName.Equals("POKEMON_GLDAAUE") && !ROMName.Equals("PM_CRYSTAL"))
            {
                Console.WriteLine("This function must be used with a Gen 2 North American Pokemon game");


                lock (dataLock)
                    _suspendConsole = false;
                return;
            }

            int RAMBankSize = 8, RAMBanks = 1;

            switch (Bank0[0x149])
            {
                case 0x00:
                    RAMBankSize = RAMBanks = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBanks = 4;
                    break;

                case 0x04:
                    RAMBanks = 16;
                    break;

                case 0x05:
                    RAMBanks = 8;
                    break;
            }

            Console.WriteLine("Pokedex merge starting...");

            WriteCommand(0x0000, 0x0A, true);

            WriteCommand(0x4100, 1, false);

            byte[] primary = GetBytes(0xA000, 0x1000, true);
            Console.WriteLine("Read Primary Save");

            Gen2SaveFile saveFile = Gen2SaveFile.FromPrimaryData(primary);

            Console.WriteLine("Save file is {0}", saveFile.IsCrystal ? "Crystal" : "Gold/Silver");

            byte[] savePokedex = saveFile.GetPokedexData();

            //for (int i = 0; i != 0x40; ++i)
            //        GSCPokedex[i] |= savePokedex[0x05A3 + i];
            saveFile.MergePokedexData(GSCPokedex);

            GSCPokedex = saveFile.GetPokedexData();

            Console.WriteLine("Merged with local Pokedex");

            saveFile.UpdateChecksum();
            Console.WriteLine("Recalculated checksum");

            byte[] checksum = new byte[2];

            Array.Copy(saveFile.Data, saveFile.ChecksumAddress, checksum, 0, 2);

            Thread.Sleep(500);

            WriteCommand((ushort)(0xA000 + saveFile.PokedexAddress-0x2000), GSCPokedex, true);
            Console.WriteLine("Written merged Pokedex to cartridge Primary Save");

            WriteCommand((ushort)(0xA000 + saveFile.ChecksumAddress-0x2000), checksum, true);
            Console.WriteLine("Updated cartridge Primary Checksum");



            WriteCommand(0x4100, GetRAMBankNumberFromAddress(saveFile.PokedexSecondaryAddress), false);
            Thread.Sleep(100);

            WriteCommand(RemapAddressToRAMArea(saveFile.PokedexSecondaryAddress), GSCPokedex, true);
            Console.WriteLine("Written merged Pokedex to cartridge Secondary Save");


            WriteCommand(0x4100, GetRAMBankNumberFromAddress(saveFile.ChecksumSecondaryAddress), false);
            Thread.Sleep(100);

            WriteCommand(RemapAddressToRAMArea((ushort)(saveFile.ChecksumSecondaryAddress)), checksum, true);
            Console.WriteLine("Updated cartridge Secondary Checksum");

            ModeCommand(0x00);

            WriteCommand(0x0000, 0x00, false);

            File.WriteAllBytes(GSCPokedexFile, GSCPokedex);

            Console.WriteLine("Done!\r\n");

            lock (dataLock)
                _suspendConsole = false;
        }

        public static byte GenerateGen1Bank1Checksum(byte[] data) // Expects to be given the data of Bank1 only.
        {
            byte checksum = 0;

            for (int i = 0x0598; i != 0x1523; ++i)
                checksum += data[i];

            return (byte)~checksum;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void DisplayPercentage(double percent, string message = "")
        {
            int barLength = 80;
            int percInt = (int)(percent * (double)(barLength));
            string progressBar = "";

            for (int i = 0; i != (barLength + 1); ++i)
                progressBar += (percInt >= i) ? '█' : ' ';

            Console.WriteLine("|{0}|  {1:P}  {2}", progressBar, percent, message);
        }

        public static void TestProgressBar(int seconds = 10, double resolution = 0.2d)
        {

            lock (dataLock)
                _suspendConsole = true;

            double currentPercent = 0.0d;

            int totalTicks = (int)(seconds / resolution);

            for (int i = 0; i != totalTicks; ++i)
            {
                currentPercent = i * (1.0d / totalTicks);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
                DisplayPercentage(currentPercent);

                Thread.Sleep((int)(resolution * 1000));
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
            DisplayPercentage(1.0d);

            lock (dataLock)
                _suspendConsole = false;
        }

        public static void WriteRAMFromFile(string filename)
        {

            byte[] file = System.IO.File.ReadAllBytes(filename);

            lock (dataLock)
                _suspendConsole = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            int RAMBankSize = 8, RAMBanks = 1, bufferSize = 0x40, RBS = RAMBankSize * (0x0400 / bufferSize);

            bool hasRTC = Bank0[0x0147] == 0x0F || Bank0[0x0147] == 0x10;

            switch (Bank0[0x149])
            {
                case 0x00:
                    RAMBankSize = RAMBanks = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBanks = 4;
                    break;

                case 0x04:
                    RAMBanks = 16;
                    break;

                case 0x05:
                    RAMBanks = 8;
                    break;
            }

            Console.WriteLine("\r\nWriting save file {0} to cartridge...", filename);

            List<byte> RAM = new List<byte>();

            WriteCommand(0x0000, 0x0A, true);

            byte[] bytesOut = new byte[bufferSize];


            DateTime start = DateTime.Now;
            double currentPercent = 0.0f, totalUnits = RBS * RAMBanks;

            Console.WriteLine("\r\nProgress:\r\n\r\n");
            for (int i = 0; i != RAMBanks; ++i)
            {
                //ModeCommand(0x00); // This issue was fixed by pulsing write on the arduino program
                //Console.WriteLine();
                WriteCommand(0x4100, (byte)i, true);

                for (ushort j = 0; j != RBS; ++j)
                {
                    //Console.WriteLine();
                    Array.Copy(file, i * 0x2000 + j * bufferSize, bytesOut, 0, bufferSize);
                    WriteCommand((ushort)(0xA000 + j * bufferSize), bytesOut, true);
                    //WriteCommand((ushort)(0xA000 + j), file[i*bufferSize00 + j], true);

                    currentPercent = (double)(i * RBS + j) / totalUnits;

                    TimeSpan t = DateTime.Now - start;
                    double secondsPerPercent = t.TotalSeconds / currentPercent;
                    int secondsRemaining = (int)(secondsPerPercent * (1.0 - currentPercent));
                    TimeSpan remaining = TimeSpan.FromSeconds(secondsRemaining);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                    DisplayPercentage(currentPercent, remaining.ToString(@"mm\:ss"));
                }

                ModeCommand(0x00);
                //Console.WriteLine("{0} of {1}", i + 1, RAMBanks);
            }


            TimeSpan elapsed = DateTime.Now - start;

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
            DisplayPercentage(1.0d);

            WriteCommand(0x0000, 0x00, false);


            //System.IO.File.WriteAllBytes(ROMName + ".sav", RAM.ToArray());

            Console.WriteLine("Done! Elapsed time: {0}", elapsed.ToString(@"mm\:ss"));

            lock (dataLock)
                _suspendConsole = false;
        }
        

        public static void WriteGSCRAMFromFile(string filename)
        {
            int[,] GSSaveDataBlocks = new int[,]
            {
                { 0x0000, 1176},
                { 0x0600, 1584},
                { 0x0C60, 1},
                { 0x0C68, 2951},
                { 0x1D04, 4202},
                { 0x31BA, 3462},
                { 0x4000, 2},
                { 0x444E, 6626},
                { 0x6000, 7792}
            };

            byte[] file = System.IO.File.ReadAllBytes(filename);

            lock (dataLock)
                _suspendConsole = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON_SLVAAXE") && !ROMName.Equals("POKEMON_GLDAAUE") && !ROMName.Equals("PM_CRYSTAL"))
            {
                Console.WriteLine("This function must be used with a Gen 2 North American Pokemon game");


                lock (dataLock)
                    _suspendConsole = false;
                return;
            }

            int RAMBankSize = 8, RAMBanks = 1, bufferSize = 0x40, RBS = RAMBankSize * (0x0400 / bufferSize);

            switch (Bank0[0x149])
            {
                case 0x00:
                    RAMBankSize = RAMBanks = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBanks = 4;
                    break;

                case 0x04:
                    RAMBanks = 16;
                    break;

                case 0x05:
                    RAMBanks = 8;
                    break;
            }

            Gen2SaveFile gen2Save = new Gen2SaveFile(file);

            DateTime start = DateTime.Now;

            if(gen2Save.IsCrystal)
            {
                Console.WriteLine("\r\nWriting Primary Save data to cartridge...");
                WriteBlockToBank(gen2Save.Data, 0x2009, (byte)1, (ushort)0xA009, 0x2B83 - 0x2000);
                WriteBlockToBank(gen2Save.Data, 0x2D0D, (byte)1, (ushort)0xAD0D, 2);

                Console.WriteLine("\r\nWriting Secondary Save data to cartridge...");
                WriteBlockToBank(gen2Save.Data, 0x2009, (byte)0, (ushort)0xB209, 0x2B83 - 0x2000);
                WriteBlockToBank(gen2Save.Data, 0x2D0D, (byte)0, (ushort)0xBF0D, 2);
            }
            else
            {
                Console.WriteLine("\r\nWriting Primary Save data to cartridge...");
                WriteBlockToBank(gen2Save.Data, 0x2009, (byte)1, (ushort)0xA009, 0x2D6B - 0x2000);

                Console.WriteLine("\r\nWriting Secondary Save data to cartridge...");
                for (int i = 0; i != Gen2SaveFile.PrimaryToSecondaryInfoGS.GetLength(0); ++i)
                {
                    Console.WriteLine("\r\n   Writing Secondary block {0}...", i+1);
                    ushort startAddr = Gen2SaveFile.PrimaryToSecondaryInfoGS[i, 0],
                        bankNumber = GetRAMBankNumberFromAddress(Gen2SaveFile.PrimaryToSecondaryInfoGS[i, 1]),
                        ROMAddr = RemapAddressToRAMArea(Gen2SaveFile.PrimaryToSecondaryInfoGS[i, 1]),
                        dataLength = Gen2SaveFile.PrimaryToSecondaryInfoGS[i, 2];

                    WriteBlockToBank(gen2Save.Data,
                        startAddr,
                        (byte)bankNumber,
                        ROMAddr,
                        dataLength);
                }
            }


            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine("Done! Elapsed time: {0}\r\n", elapsed.ToString(@"mm\:ss"));

            lock (dataLock)
                _suspendConsole = false;
        }

        public static void WriteBlockToBank(byte[] data, byte bankNumber, ushort destStartAddress, int length)
        {

            List<byte> dataSub = new List<byte>(data);

            WriteCommand(0x0000, 0x0A, true);

            byte[] bytesOut;


            DateTime start = DateTime.Now;
            double currentPercent = 0.0f, totalUnits = (double)(Math.Ceiling(data.Length / 64d));

            Console.WriteLine("\r\nProgress:\r\n\r\n");

            //ModeCommand(0x00); // This issue was fixed by pulsing write on the arduino program
            //Console.WriteLine();
            WriteCommand(0x4100, bankNumber, true);

            for (ushort j = 0; j != (int)totalUnits; ++j)
            {
                 bytesOut = new byte[dataSub.Count<0x40?dataSub.Count:0x40];

                //Console.WriteLine();
                Array.Copy(data, j*0x40, bytesOut, 0, bytesOut.Length);

                if (dataSub.Count > 0x40) 
                    dataSub.RemoveRange(0, 0x40);

                WriteCommand((ushort)(destStartAddress + j*0x40), bytesOut, true);
                //WriteCommand((ushort)(0xA000 + j), file[i*bufferSize00 + j], true);

                currentPercent = (double)j / totalUnits;

                TimeSpan t = DateTime.Now - start;
                double secondsPerPercent = t.TotalSeconds / currentPercent;
                int secondsRemaining = (int)(secondsPerPercent * (1.0 - currentPercent));
                TimeSpan remaining = TimeSpan.FromSeconds(secondsRemaining);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
                DisplayPercentage(currentPercent, remaining.ToString(@"mm\:ss"));
            }

            ModeCommand(0x00);

            TimeSpan elapsed = DateTime.Now - start;

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
            DisplayPercentage(1.0d);

            WriteCommand(0x0000, 0x00, false);
        }

        public static void WriteBlockToBank(byte[] data, int sourceStartAddress, byte bankNumber, ushort destStartAddress, int length)
        {
            byte[] tmp = new byte[length];
            Array.Copy(data, sourceStartAddress, tmp, 0, length);
            WriteBlockToBank(tmp, bankNumber, destStartAddress, length);

        }

        public static void ReadCommand(ushort Address, ushort numBytes, bool SRAM)
        {
            byte[] command = {
            0x01,
            (byte)(Address&0xFF),
            (byte)(Address>>8),
            (byte)(numBytes&0xFF),
            (byte)(numBytes>>8),
            (byte)(SRAM?0x01:0x00),
            0xFF };

            _serialPort.Write(command, 0, command.Length);
        }

        public static void ModeCommand(byte mode)
        {
            bool modeChanged = false;

            byte[] command = {
            0x04,
            mode,
            0xFF };

            _serialPort.Write(command, 0, command.Length);

            while (!modeChanged)
            {
                Thread.Sleep(250);
                lock (dataLock)
                    if (ReceivedBytes.Count > 0)
                        modeChanged = ReceivedBytes[ReceivedBytes.Count - 1] == 0xFF;
            }

        }

        public static void WriteCommand(ushort Address, byte data, bool SRAM)
        {
            bool writeDone = false;

            byte[] command = {
            0x02,
            (byte)(Address&0xFF),
            (byte)(Address>>8),
            0x01,
            data,
            (byte)(SRAM?0x01:0x00),
            0xFF };

            ReceivedBytes.Clear();
            _serialPort.Write(command, 0, command.Length);

            while (!writeDone)
            {
                Thread.Sleep(250);
                lock (dataLock)
                    if (ReceivedBytes.Count > 0)
                        writeDone = ReceivedBytes[ReceivedBytes.Count - 1] == 0xFF;
            }
        }


        const int WriteMaxLen = 64;
        /// <summary>
        /// / Sends an array of bytes to the Gameboy Cartridge.
        /// </summary>
        /// <param name="Address">The address on the Gameboy Cartridge to read.</param>
        /// <param name="data">An array containing the data to send to the cartridge. A maximum of 48 bytes can be sent at one time. More than this will throw an ArgumentException.</param>
        /// <param name="SRAM">A bool indicating whether or not data is to be written to the SRAM.</param>
        public static void WriteCommand(ushort Address, byte[] data, bool SRAM)
        {
            bool writeDone = false;

            /*
            byte[] command = {
                0x02,
                (byte)(Address&0xFF),
                (byte)(Address>>8),
                0x01,
                data,
                (byte)(SRAM?0x01:0x00),
                0xFF };//*/

            if (data.Length > WriteMaxLen)
                throw new ArgumentException("data length exceeds limit. Maximum length is " + WriteMaxLen.ToString() + " bytes.");
            List<byte> commandLong = new List<byte>();
            commandLong.AddRange(new byte[] { 0x02, (byte)(Address & 0xFF), (byte)(Address >> 8), (byte)data.Length });
            commandLong.AddRange(data);
            commandLong.Add((byte)(SRAM ? 0x01 : 0x00));
            commandLong.Add(0xFF);
            byte[] command = commandLong.ToArray();

            /*
            Console.Write("<< ");
            foreach (byte b in command)
                Console.Write(b.ToString("X").PadLeft(2, '0') + " ");
            Console.WriteLine();//*/

            ReceivedBytes.Clear();
            _serialPort.Write(command, 0, command.Length);

            while (!writeDone)
            {
                //Thread.Sleep(250);
                lock (dataLock)
                    if (ReceivedBytes.Count > 0)
                        writeDone = ReceivedBytes[ReceivedBytes.Count - 1] == 0xFF;
            }
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                        lock (dataLock)
                        {

                            while (_serialPort.BytesToRead > 0)
                            {
                                byte[] data = new byte[_serialPort.BytesToRead];
                                _serialPort.Read(data, 0, data.Length);

                                ReceivedBytes.AddRange(data);

                                if (!_suspendConsole)
                                    foreach (byte b in data)
                                        Console.Write((char)b);
                            }
                        }

                }
                catch (TimeoutException) { }
                catch (System.UnauthorizedAccessException) { _continue = false; }
            }
        }
    }
}