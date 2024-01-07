using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Onix_Gameboy_Cartridge_Reader;

namespace Onix_Gameboy_Cartridge_Reader_GUI
{
    public partial class Form1 : Form
    {
        public delegate void ConsoleWriteCharDelegate(char c);
        public delegate void ConsoleWriteStringDelegate(string s);
        public delegate void UpdateCheckImage(Bitmap bitmap);
        public delegate void UpdateProgressBar(double percent);

        public delegate void NotifyActionInProgress();
        public delegate void NotifyActionCompleted();

        public delegate void ConnectionFailure();
        public delegate void ConnectionReady();


        public NotifyActionInProgress OnNotifyActionInProgress;
        public NotifyActionInProgress NotifyActionInProgressHandler;

        public NotifyActionCompleted OnNotifyActionCompleted;
        public NotifyActionCompleted NotifyActionCompletedHandler;

        public ConsoleWriteCharDelegate OnConsoleWriteChar;
        public ConsoleWriteStringDelegate OnConsoleWriteString;

        public ConnectionFailure OnConnectionFailure;
        public ConnectionFailure ConnectionFailureHandler;

        public ConnectionReady OnConnectionReady;
        public ConnectionReady ConnectionReadyHandler;

        public UpdateCheckImage OnCheckImageUpdate;
        public UpdateCheckImage UpdateCheckImageHandler;

        public UpdateProgressBar OnProgressBarUpdate;
        public UpdateProgressBar UpdateProgressBarHandler;

        public UpdateProgressBar OnProgressBarIndeterminate;
        public UpdateProgressBar IndeterminateProgressBarHandler;

        Task WorkInProgress;

        bool _continue, _suspendInput;
        SerialPort _serialPort;
        object dataLock = new object();
        List<byte> ReceivedBytes = new List<byte>();
        byte[] RBYPokedex = new byte[0x26];
        string RBYPokedexFile = "RBYPokedex.dat";

        byte[] GSCPokedex = new byte[0x40];
        string GSCPokedexFile = "GSCPokedex.dat";
        string[] Gen1Names = File.ReadAllLines("PokemonIndexListGenI.txt");
        string[] PokemonNames = File.ReadAllLines("Pokemon Names Gen 1 - 9.txt");

        List<string[]> LottoData = new List<string[]>();

        string name;
        string message;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        Thread readThread = default;

        TextBoxWriter ConsoleTextBoxWriter;

        string selectedPort = "";

        public Form1()
        {
            InitializeComponent();

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            readThread = new Thread(ReadSerialTalk);

            OnConsoleWriteChar += ConsoleTextBoxWriter_OnConsoleWriteChar;
            OnConsoleWriteString += ConsoleTextBoxWriter_OnConsoleWriteString;

            ConsoleTextBoxWriter = new TextBoxWriter();
            ConsoleTextBoxWriter.OnConsoleWriteChar += (char c) => { BeginInvoke(OnConsoleWriteChar, c); };
            ConsoleTextBoxWriter.OnConsoleWriteString += (string s) => { BeginInvoke(OnConsoleWriteString, s); };

            ConnectionFailureHandler += HandleConnectionFailure;
            OnConnectionFailure += () => { BeginInvoke(ConnectionFailureHandler); };

            ConnectionReadyHandler += HandleConnectionReady;
            OnConnectionReady += () => { BeginInvoke(ConnectionReadyHandler); };

            UpdateCheckImageHandler += HandleUpdateCheckImage;
            OnCheckImageUpdate += (Bitmap bitmap) => { BeginInvoke(UpdateCheckImageHandler, bitmap); };

            UpdateProgressBarHandler += HandleProgressBarUpdate;
            OnProgressBarUpdate += (double percent) => { BeginInvoke(UpdateProgressBarHandler, percent); };

            IndeterminateProgressBarHandler += HandleProgressBarIndeterminate;
            OnProgressBarIndeterminate += (double percent) => { BeginInvoke(IndeterminateProgressBarHandler, percent); };

            Console.SetOut(ConsoleTextBoxWriter);
        }

        private void HandleConnectionReady()
        {
            gbPrimaryActions.Enabled = true;
            this.Text = "Onix Gameboy Cartridge Reader - [CONNECTED]";
            WorkInProgress = Task.Run(() => DisplayFullCartInfo());
        }

        private void HandleUpdateCheckImage(Bitmap bitmap)
        {
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }

        private void HandleProgressBarUpdate(double percent)
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = (int)(percent * 100.0);
        }

        private void HandleProgressBarIndeterminate(double percent)
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void ConsoleTextBoxWriter_OnConsoleWriteChar(char c)
        {
            tbConsoleOutput.AppendText("" + c);
        }

        private void ConsoleTextBoxWriter_OnConsoleWriteString(string s)
        {
            tbConsoleOutput.AppendText(s);
        }

        private void connectToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            connectToToolStripMenuItem.DropDownItems.Clear();

            foreach (string port in ports)
            {
                connectToToolStripMenuItem.DropDownItems.Add(new System.Windows.Forms.ToolStripMenuItem(port));
                connectToToolStripMenuItem.DropDownItems[connectToToolStripMenuItem.DropDownItems.Count - 1].Click += connectToPortFromContextMenu;
            }
        }

        private void HandleConnectionFailure()
        {
            readThread = new Thread(ReadSerialTalk);
            gbPrimaryActions.Enabled = false;
            this.Text = "Onix Gameboy Cartridge Reader - [NOT CONNECTED]";
            connectToToolStripMenuItem.Enabled = true;
        }

        private void connectToPortFromContextMenu(object sender, EventArgs e)
        {
            connectToToolStripMenuItem.Enabled = false;
            string port = ((ToolStripMenuItem)sender).Text;
            ConnectToPort(port);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectToToolStripMenuItem_Click(sender, e);



            if (File.Exists(RBYPokedexFile))
                RBYPokedex = File.ReadAllBytes(RBYPokedexFile);

            if (File.Exists(GSCPokedexFile))
                GSCPokedex = File.ReadAllBytes(GSCPokedexFile);

            if (File.Exists("PokemonLottoData.txt"))
            {
                string[] lines = File.ReadAllLines("PokemonLottoData.txt");
                foreach (string line in lines)
                    //Game|TID No.|Alt Pokemon ID's...
                    LottoData.Add(line.Split('|'));
            }

            RescanROMsFolder();
        }

        private void RescanROMsFolder()
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (string file in files)
                if (file.EndsWith(".gb") || file.EndsWith(".gbc"))
                    lbDumpedROMs.Items.Add(file.Substring(file.LastIndexOf("\\") + 1));

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //tbConsoleOutput.
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (dataLock)
                _continue = false;

            if (readThread.ThreadState == System.Threading.ThreadState.Running)
                readThread.Join();
        }

        private void lbDumpedROMs_DoubleClick(object sender, EventArgs e)
        {
            string filename = lbDumpedROMs.Items[lbDumpedROMs.SelectedIndex].ToString();

            Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + filename + "\"";
            fileopener.Start();
        }

        private void ConnectToPort(string portName)
        {
            try
            {
                // Allow the user to set the appropriate properties.
                _serialPort.PortName = portName;// SetPortName(_serialPort.PortName);
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

                Thread.Sleep(500);

                
                WorkInProgress = Task.Run(() => WaitForConnectionToBeReady());

                name = "Self";
            }
            catch (Exception ex) { Console.WriteLine("Connection Failed:\r\n   {0}", ex.Message); }
        }

        public void ReadSerialTalk()
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

                                if (!_suspendInput)
                                    foreach (byte b in data)
                                        Console.Write((char)b);
                            }
                        }

                }
                catch (TimeoutException) { }
                catch (Exception) { _continue = false; OnConnectionFailure?.Invoke(); }
            }
        }



        private byte[] GetBytes(ushort Address, ushort numBytes, bool SRAM)
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

        private void WaitForConnectionToBeReady()
        {
            lock (dataLock)
            {
                _suspendInput = true;
                ReceivedBytes.Clear();
            }

            

            bool ready = false;
            while (!ready)
            {
                lock (dataLock)
                    if (ReceivedBytes.Count >= 2)
                        ready = true;
                    else
                        _serialPort.Write(new byte[] { 0x00, 0xFF }, 0, 1);


                if (!ready)
                    Thread.Sleep(1000);

            }

            lock (dataLock)
                _suspendInput = false;
            
            OnConnectionReady();
        }

        private void DumpROMToFile(string appendText = "", string filename = "", int banks = 0)
        {
            DateTime start = DateTime.Now;
            lock (dataLock)
                _suspendInput = true;
            List<byte> ROM = new List<byte>();

            ROM.AddRange(GetBytes(0x0000, 0x4000, false));

            string fileExt = ".gb";

            if (ROM[0x143] == 0x80 || ROM[0x143] == 0xC0)
                fileExt += 'c';

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

                Console.WriteLine("Reading ROM Bank {0} of {1}", i + 1, BankCount);
            }

            WriteCommand(0x2100, 0x01, false);

            if (System.IO.File.Exists(ROMName + appendText + fileExt))
                System.IO.File.Delete(ROMName + appendText + fileExt);

            System.IO.File.WriteAllBytes(ROMName + appendText + fileExt, ROM.ToArray());

            Console.WriteLine("\r\nWrote {1} bytes to {2}\r\nCompleted in {0}", (DateTime.Now - start).ToString(), ROM.Count, ROMName + appendText + fileExt);

            lock (dataLock)
                _suspendInput = false;
        }

        private void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        private long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        private void DumpRAMToFile(string filename = "")
        {

            lock (dataLock)
                _suspendInput = true;

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

                OnProgressBarUpdate((i + 1.0) / (hasRTC ? RAMBanks * 1.1 : RAMBanks));
            }

            if (hasRTC)
            {
                Console.WriteLine("Dumping RTC Registers...");

                //latch RTC Registers
                WriteCommand(0x6100, 0x00, false);
                WriteCommand(0x6100, 0x01, false);

                byte[] RTCData = new byte[20];
                for (byte i = 0x00; i != 0x05; ++i)
                {
                    WriteCommand(0x4100, (byte)(0x08 + i), false);
                    RTCData[i * 4] = GetBytes(0xA000, 0x01, true)[0];
                }
                RAM.AddRange(RTCData);
                RAM.AddRange(RTCData);
                byte[] unixtime = BitConverter.GetBytes(UnixTimeNow());
                RAM.AddRange(unixtime);

            }

            OnProgressBarUpdate(1.0);

            WriteCommand(0x0000, 0x00, false);

            //ModeCommand(0x00);

            if (System.IO.File.Exists(outFilename))
                System.IO.File.Delete(outFilename);

            System.IO.File.WriteAllBytes(outFilename, RAM.ToArray());

            Console.WriteLine("Done!");

            lock (dataLock)
                _suspendInput = false;
        }

        private byte[] DumpRAMToArray(bool ignoreRTC = true)
        {

            byte[] ROMBank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(ROMBank0, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            int RAMBankSize = 8, RAMBanks = 1;

            bool hasRTC = ROMBank0[0x0147] == 0x0F || ROMBank0[0x0147] == 0x10;

            switch (ROMBank0[0x149])
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


            Console.WriteLine("Dumping RAM...");

            WriteCommand(0x0000, 0x0A, true);


            for (byte i = 0; i != RAMBanks; ++i)
            {
                //ModeCommand(0x00);
                WriteCommand(0x4100, i, false);
                RAM.AddRange(GetBytes(0xA000, 0x2000, true));

                Console.WriteLine("Read Bank {0} of {1}", i + 1, RAMBanks);
            }

            if (hasRTC && !ignoreRTC)
            {
                Console.WriteLine("Dumping RTC Registers...");
                byte[] RTCData = new byte[20];
                for (byte i = 0x00; i != 0x05; ++i)
                {
                    WriteCommand(0x4100, (byte)(0x08 + i), false);
                    RTCData[i * 4] = GetBytes(0xA000, 0x01, true)[0];
                }
                RAM.AddRange(RTCData);
                RAM.AddRange(RTCData);
                byte[] unixtime = BitConverter.GetBytes(UnixTimeNow());
                RAM.AddRange(unixtime);

            }

            WriteCommand(0x0000, 0x00, false);

            Console.WriteLine("Done!");

            return RAM.ToArray();
        }

        private void MergePokedexRBY()
        {

            lock (dataLock)
                _suspendInput = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F).Trim();
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON RED") && !ROMName.Equals("POKEMON BLUE") && !ROMName.Equals("POKEMON YELLOW"))
            {
                Console.WriteLine("This function must be used with a Gen 1 North American Pokemon game");

                lock (dataLock)
                    _suspendInput = false;
                return;
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
                _suspendInput = false;
        }

        private byte GetRAMBankNumberFromAddress(ushort addr)
        {
            return (byte)(addr / 0x2000);
        }

        private ushort RemapAddressToRAMArea(ushort addr)
        {
            return (ushort)(0xA000 + (addr - 0x2000 * GetRAMBankNumberFromAddress(addr)));
        }

        private byte[] ArraySub(byte[] data, int start, int len)
        {
            byte[] ret = new byte[len];

            Array.Copy(data, start, ret, 0, len);

            return ret;
        }

        private byte[] GetCurrentSaveFile(string romName)
        {
            byte[] ret;

            if (RAMIsDumped(romName))
                ret = File.ReadAllBytes(romName + ".sav");
            else
                ret = DumpRAMToArray();

            return ret;
        }

        private void PokeLottoCheck()
        {

            lock (dataLock)
                _suspendInput = true;

            //byte[] save = File.ReadAllBytes("POKEMON_SLVAAXE.sav");
            //byte[] header = ArraySub(File.ReadAllBytes("POKEMON_SLVAAXE.gb"), 0, 0x200);
            byte[] header = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F).Trim();
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON RED") && !ROMName.Equals("POKEMON BLUE") && !ROMName.Equals("POKEMON YELLOW") &&
                !ROMName.Equals("POKEMON_SLVAAXE") && !ROMName.Equals("POKEMON_GLDAAUE") && !ROMName.Equals("PM_CRYSTAL"))
            {
                Console.WriteLine("This function must be used with a North American Gameboy/Gameboy Color Pokemon game");


                lock (dataLock)
                    _suspendInput = false;
                return;
            }

            Console.WriteLine("Lottery Check in progress...");

            //WriteCommand(0x4100, 1, false);

            byte[] save = GetCurrentSaveFile(ROMName);

            bool doLottoCheck = false;

            string currentLottoNumber = "00000";

            if (ROMName.Contains("RED") || ROMName.Contains("BLUE") || ROMName.Contains("YELLOW"))
            {
                List<ushort> FoundIDs = new List<ushort>();

                //Check the trainer ID first
                ushort tid = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(save, 0x2605));
                FoundIDs.Add(tid);
                Console.WriteLine("Trainer ID: {0}", tid.ToString().PadLeft(5, '0'));

                Console.WriteLine("Checking party pokemon...");
                //Next, check the party Pokemon
                {
                    ushort partyDataAddr = 0x2F2C;

                    for (int i = 0; i != save[partyDataAddr]; ++i)
                    {
                        PokemonDataGen1 partyPokemon = PokemonDataGen1.PokemonDataFromData(save, partyDataAddr + 8 + 0x2C * i, false);
                        //Console.Write("{0} ", Gen1Names[partyPokemon.SpeciesID]);
                        if (partyPokemon.OTID != tid)
                            if (!FoundIDs.Contains(partyPokemon.OTID))
                            {
                                FoundIDs.Add(partyPokemon.OTID);
                                Console.WriteLine("\r\n   Found ID: {0}", partyPokemon.OTID.ToString().PadLeft(5, '0'));
                            }
                    }
                }

                //Finally, check the boxed Pokemon
                {
                    ushort currentBox = 0x30C0,
                        boxGroup1 = 0x4000,
                        boxGroup2 = 0x6000; //6 boxes each, 0x0462 in size each

                    int currentBoxIndex = save[0x284C] & 0x7F;

                    Console.WriteLine("Checking boxed pokemon...");

                    for (int b = 0; b != 6; ++b) //for each box...
                    {
                        if (b == currentBoxIndex)
                        {
                            int boxCount = save[currentBox];
                            for (int i = 0; i != boxCount; ++i) //for each pokemon in the current box
                            {   //                                                                        current box pokemon data   + index of current Pokemon
                                PokemonDataGen1 boxPokemon = PokemonDataGen1.PokemonDataFromData(save, (currentBox + 0x16) + i * 0x21, true);
                                //Console.Write("{0} ", Gen1Names[boxPokemon.SpeciesID]);
                                if (boxPokemon.OTID != tid)
                                    if (!FoundIDs.Contains(boxPokemon.OTID))
                                    {
                                        FoundIDs.Add(boxPokemon.OTID);
                                        Console.WriteLine("\r\n   Found ID: {0}", boxPokemon.OTID.ToString().PadLeft(5, '0'));
                                    }
                            }

                        }
                        else
                        {
                            int boxCount = save[boxGroup1 + b * 0x0462];
                            for (int i = 0; i != boxCount; ++i) //for each pokemon in the current box
                            {   //                                                                        current box pokemon data   + index of current Pokemon
                                PokemonDataGen1 boxPokemon = PokemonDataGen1.PokemonDataFromData(save, (boxGroup1 + (b * 0x0462) + 0x16) + i * 0x21, true);
                                //Console.Write("{0} ", Gen1Names[boxPokemon.SpeciesID]);
                                if (boxPokemon.OTID != tid)
                                    if (!FoundIDs.Contains(boxPokemon.OTID))
                                    {
                                        FoundIDs.Add(boxPokemon.OTID);
                                        Console.WriteLine("\r\n   Found ID: {0}", boxPokemon.OTID.ToString().PadLeft(5, '0'));
                                    }
                            }
                        }
                        Console.WriteLine("Checked Box {0}", 7 + b);
                    }

                    for (int b = 0; b != 6; ++b) //for each box...
                    {
                        if ((b + 6) == currentBoxIndex)
                        {
                            int boxCount = save[currentBox];
                            for (int i = 0; i != boxCount; ++i) //for each pokemon in the current box
                            {   //                                                                        current box pokemon data   + index of current Pokemon
                                PokemonDataGen1 boxPokemon = PokemonDataGen1.PokemonDataFromData(save, (currentBox + 0x16) + i * 0x21, true);
                                //Console.Write("{0} ", Gen1Names[boxPokemon.SpeciesID]);
                                if (boxPokemon.OTID != tid)
                                    if (!FoundIDs.Contains(boxPokemon.OTID))
                                    {
                                        FoundIDs.Add(boxPokemon.OTID);
                                        Console.WriteLine("\r\n   Found ID: {0}", boxPokemon.OTID.ToString().PadLeft(5, '0'));
                                    }
                            }

                        }
                        else
                        {
                            int boxCount = save[boxGroup2 + b * 0x0462];
                            for (int i = 0; i != boxCount; ++i) //for each pokemon in the current box
                            {   //                                                                        current box pokemon data   + index of current Pokemon
                                PokemonDataGen1 boxPokemon = PokemonDataGen1.PokemonDataFromData(save, (boxGroup2 + (b * 0x0462) + 0x16) + i * 0x21, true);
                                //Console.Write("{0} ", Gen1Names[boxPokemon.SpeciesID]);
                                if (boxPokemon.OTID != tid)
                                    if (!FoundIDs.Contains(boxPokemon.OTID))
                                    {
                                        FoundIDs.Add(boxPokemon.OTID);
                                        Console.WriteLine("\r\n   Found ID: {0}", boxPokemon.OTID.ToString().PadLeft(5, '0'));
                                    }
                            }
                        }
                        Console.WriteLine("Checked Box {0}", 1 + b);
                    }
                }

                foreach (string[] game in LottoData)
                    if (ushort.Parse(game[1]) == tid)
                    {
                        LottoData.Remove(game);
                        break;
                    }

                List<string> output = new List<string>();

                output.Add(ROMName.Replace("POKEMON ", ""));
                foreach (ushort id in FoundIDs)
                    output.Add(id.ToString().PadLeft(5, '0'));

                LottoData.Add(output.ToArray());
            }
            else
            {
                List<int> FoundIDs = new List<int>();

                doLottoCheck = true;
                Gen2SaveFile gen2Save = new Gen2SaveFile(save);

                string gameFriendlyName = "";

                if (ROMName.Contains("AAXE"))
                    gameFriendlyName = "SILVER";
                else if (ROMName.Contains("AAUE"))
                    gameFriendlyName = "GOLD";
                else
                    gameFriendlyName = ROMName.Substring(3);

                currentLottoNumber = gen2Save.LottoNumber.ToString().PadLeft(5, '0');

                FoundIDs.Add(gen2Save.TrainerID);
                Console.WriteLine("Trainer ID: {0}", gen2Save.TrainerID.ToString().PadLeft(5, '0'));

                //Party Pokemon
                foreach (PokemonDataGen2 pokemon in gen2Save.GetPartyPokemon())
                    if (!FoundIDs.Contains(pokemon.OTID))
                    {
                        FoundIDs.Add(pokemon.OTID);
                        Console.WriteLine("\r\n   Found ID: {0} - {1}", pokemon.OTID.ToString().PadLeft(5, '0'), PokemonNames[pokemon.SpeciesID - 1]);
                    }

                Console.Write("Checking box ");
                for (int i = 0; i != 14; ++i)
                {
                    Console.Write("{0} ", 1 + i);

                    foreach (PokemonDataGen2 pokemon in gen2Save.GetBoxedPokemon(i))
                    {
                        //Console.Write("{0} ", PokemonNames[pokemon.SpeciesID-1]);

                        if (!FoundIDs.Contains(pokemon.OTID))
                        {
                            FoundIDs.Add(pokemon.OTID);
                            Console.WriteLine("\r\n   Found ID: {0} - {1}", pokemon.OTID.ToString().PadLeft(5, '0'), PokemonNames[pokemon.SpeciesID - 1]);
                        }

                    }
                }


                foreach (string[] game in LottoData)
                    if (ushort.Parse(game[1]) == gen2Save.TrainerID)
                    {
                        LottoData.Remove(game);
                        break;
                    }

                List<string> output = new List<string>();

                output.Add(gameFriendlyName);
                foreach (ushort id in FoundIDs)
                    output.Add(id.ToString().PadLeft(5, '0'));

                LottoData.Add(output.ToArray());

            }

            //Lotto check!
            if (doLottoCheck)
            {
                Console.WriteLine("This week's Lucky Number is: {0}", currentLottoNumber);

                int maxMatchStrength = 0;
                string matchID = "";
                string[] matchingGame = new string[2] { "None", "" };

                foreach (string[] game in LottoData)
                {
                    if (game[1].Equals(matchID))
                        matchingGame = game;

                    if (maxMatchStrength != 5)
                        for (int i = 2; i != game.Length; ++i)
                        {
                            int matchStrength = 0;

                            if (game[i][4] == currentLottoNumber[4])
                            {
                                for (int j = 4; j != -1; --j)
                                    if (game[i][j] == currentLottoNumber[j])
                                        ++matchStrength;
                                    else
                                        break;

                                if (matchStrength > maxMatchStrength)
                                {
                                    maxMatchStrength = matchStrength;
                                    matchID = game[i];
                                    matchingGame = game;


                                    if (matchStrength == 5)
                                        break;
                                }
                            }
                        }
                }

                if (matchingGame[1].Equals(matchID))
                    Console.WriteLine(maxMatchStrength == 5 ? "\r\n\r\n{0} Trainer ID {1} is a perfect match!!!\r\n" : "\r\n\r\n{0} Trainer ID {1} matches lotto by {2} digits!", matchingGame[0], matchingGame[1], maxMatchStrength);
                else if (maxMatchStrength > 0)
                    Console.WriteLine(maxMatchStrength == 5 ? "\r\n\r\n{0} Trainer ID {1} has a Pokemon that is a perfect match!!!\r\n   ID No. {3}" : "\r\n\r\n{0} Trainer ID {1} has a Pokemon that matches lotto by {2} digit" + ((maxMatchStrength > 1) ? "s!" : "!") + "\r\n   ID No. {3}", matchingGame[0], matchingGame[1], maxMatchStrength, matchID);
                else
                    Console.WriteLine("No matches found!");


            }

            string[] lottoDataFileOut = new string[LottoData.Count];

            for (int i = 0; i != LottoData.Count; ++i)
            {
                string line = "";
                foreach (string s in LottoData[i])
                    line += s + "|";
                line = line.Remove(line.Length - 1);
                lottoDataFileOut[i] = line;
            }

            File.WriteAllLines("PokemonLottoData.txt", lottoDataFileOut);

            Console.WriteLine("Done!\r\n");

            lock (dataLock)
                _suspendInput = false;
        }

        private void MergePokedexGSC()
        {

            lock (dataLock)
                _suspendInput = true;

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F).Trim();
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            if (!ROMName.Equals("POKEMON_SLVAAXE") && !ROMName.Equals("POKEMON_GLDAAUE") && !ROMName.Equals("PM_CRYSTAL"))
            {
                Console.WriteLine("This function must be used with a Gen 2 North American Pokemon game");


                lock (dataLock)
                    _suspendInput = false;
                return;
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

            WriteCommand((ushort)(0xA000 + saveFile.PokedexAddress - 0x2000), GSCPokedex, true);
            Console.WriteLine("Written merged Pokedex to cartridge Primary Save");

            WriteCommand((ushort)(0xA000 + saveFile.ChecksumAddress - 0x2000), checksum, true);
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
                _suspendInput = false;
        }

        private byte GenerateGen1Bank1Checksum(byte[] data) // Expects to be given the data of Bank1 only.
        {
            byte checksum = 0;

            for (int i = 0x0598; i != 0x1523; ++i)
                checksum += data[i];

            return (byte)~checksum;
        }

        private void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private void DisplayPercentage(double percent, string message = "")
        {
            int barLength = 80;
            int percInt = (int)(percent * (double)(barLength));
            string progressBar = "";

            for (int i = 0; i != (barLength + 1); ++i)
                progressBar += (percInt >= i) ? '█' : ' ';

            Console.WriteLine("|{0}|  {1:P}  {2}", progressBar, percent, message);
        }

        private void TestProgressBar(int seconds = 10, double resolution = 0.2d)
        {

            lock (dataLock)
                _suspendInput = true;

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
                _suspendInput = false;
        }

        private void WriteRAMFromFile(string filename)
        {

            byte[] file = System.IO.File.ReadAllBytes(filename);

            lock (dataLock)
                _suspendInput = true;

            OnProgressBarIndeterminate(0);

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

            OnProgressBarUpdate(0.0);

            //Console.WriteLine("\r\nProgress:\r\n\r\n");
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

                    OnProgressBarUpdate(currentPercent);
                    /*
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                    DisplayPercentage(currentPercent, remaining.ToString(@"mm\:ss"));//*/
                }

                ModeCommand(0x00);
                //Console.WriteLine("{0} of {1}", i + 1, RAMBanks);
            }


            TimeSpan elapsed = DateTime.Now - start;

            OnProgressBarUpdate(1.0);
            /*
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
            DisplayPercentage(1.0d);//*/

            WriteCommand(0x0000, 0x00, false);


            //System.IO.File.WriteAllBytes(ROMName + ".sav", RAM.ToArray());

            Console.WriteLine("Done! Elapsed time: {0}", elapsed.ToString(@"mm\:ss"));

            lock (dataLock)
                _suspendInput = false;
        }

        private byte[] GetBytesFromSRAMApplyingAddressRemap(ushort address, ushort length)
        {

            WriteCommand(0x0000, 0x0A, true);

            WriteCommand(0x4100, GetRAMBankNumberFromAddress(address), false);
            //Thread.Sleep(100);
            ushort addr = RemapAddressToRAMArea(address);
            byte[] ret = GetBytes(addr, length, true);

            WriteCommand(0x0000, 0x00, true);
            return ret;
        }

        private byte GetByteFromSRAMApplyingAddressRemap(ushort address)
        {

            WriteCommand(0x0000, 0x0A, true);

            WriteCommand(0x4100, GetRAMBankNumberFromAddress(address), false);
            //Thread.Sleep(100);
            ushort addr = RemapAddressToRAMArea(address);
            byte[] ret = GetBytes(addr, 1, true);

            WriteCommand(0x0000, 0x00, true);
            return ret[0];
        }

        private bool RAMIsDumped(string romName)
        {
            bool ret = false;

            if (File.Exists(romName + ".sav"))
            {
                byte[] save = File.ReadAllBytes(romName + ".sav");

                switch (romName)
                {
                    case "PM_CRYSTAL":
                        {
                            ushort checksum1 = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(CrystalAddressList.Checksum1, 2), 0),
                                checksum2 = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(CrystalAddressList.Checksum2, 2), 0),
                                tid = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(CrystalAddressList.TrainerID, 2), 0);

                            uint timePlayed = BitConverter.ToUInt32(GetBytesFromSRAMApplyingAddressRemap(CrystalAddressList.TimePlayed, 4), 0);

                            byte firstPokemonDV = GetByteFromSRAMApplyingAddressRemap(CrystalAddressList.FirstPokemonDV);


                            ushort sChecksum1 = BitConverter.ToUInt16(save, CrystalAddressList.Checksum1),
                            a = BitConverter.ToUInt16(save, CrystalAddressList.Checksum2),
                            t = BitConverter.ToUInt16(save, CrystalAddressList.TrainerID);

                            uint p = BitConverter.ToUInt32(save, CrystalAddressList.TimePlayed);

                            byte f = save[CrystalAddressList.FirstPokemonDV];
                            if (checksum1 == sChecksum1 &&
                                checksum2 == a &&
                                tid == t &&
                                timePlayed == p &&
                                firstPokemonDV == f)
                                ret = true;
                        }

                        break;

                    case "POKEMON_SLVAAXE":
                    case "POKEMON_GLDAAUE":
                        {
                            ushort checksum1 = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(GoldSilverAddressList.Checksum1, 2), 0),
                                checksum2 = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(GoldSilverAddressList.Checksum2, 2), 0),
                                tid = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(GoldSilverAddressList.TrainerID, 2), 0);

                            uint timePlayed = BitConverter.ToUInt32(GetBytesFromSRAMApplyingAddressRemap(GoldSilverAddressList.TimePlayed, 4), 0);

                            byte firstPokemonDV = GetByteFromSRAMApplyingAddressRemap(GoldSilverAddressList.FirstPokemonDV);

                            if (checksum1 == BitConverter.ToUInt16(save, GoldSilverAddressList.Checksum1) &&
                                checksum2 == BitConverter.ToUInt16(save, GoldSilverAddressList.Checksum2) &&
                                tid == BitConverter.ToUInt16(save, GoldSilverAddressList.TrainerID) &&
                                timePlayed == BitConverter.ToUInt32(save, GoldSilverAddressList.TimePlayed) &&
                                firstPokemonDV == save[GoldSilverAddressList.FirstPokemonDV])
                                ret = true;
                        }

                        break;

                    case "POKEMON RED":
                    case "POKEMON BLUE":
                    case "POKEMON YELLOW":
                        {
                            byte checksum = GetByteFromSRAMApplyingAddressRemap(0x3523),
                                firstPokemonDV = GetByteFromSRAMApplyingAddressRemap(0x2F2C + 8 + 0x19);

                            uint timePlayed = (uint)(GetByteFromSRAMApplyingAddressRemap(0x2CED) << 24 |
                                GetByteFromSRAMApplyingAddressRemap(0x2CEF) << 16 |
                                GetByteFromSRAMApplyingAddressRemap(0x2CF0) << 8 |
                                GetByteFromSRAMApplyingAddressRemap(0x2CF1));

                            ushort tid = BitConverter.ToUInt16(GetBytesFromSRAMApplyingAddressRemap(0x2605, 2), 0);

                            if (checksum == save[0x3523] &&
                                tid == BitConverter.ToUInt16(save, 0x2605) &&
                                timePlayed == (uint)(save[0x2CED] << 24 | save[0x2CEF] << 16 | save[0x2CF0] << 8 | save[0x2CF1]) &&
                                firstPokemonDV == save[0x2F2C + 8 + 0x19])
                                ret = true;
                        }
                        break;
                }
            }

            return ret;
        }

        private void WriteRAMDiffFromFile(string filename)
        {

            Console.WriteLine("Reading current save");

            byte[] file = System.IO.File.ReadAllBytes(filename);

            lock (dataLock)
                _suspendInput = true;

            OnProgressBarIndeterminate(0);

            byte[] Bank0 = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(Bank0, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            int bufferSize = 0x40;

            //bool hasRTC = Bank0[0x0147] == 0x0F || Bank0[0x0147] == 0x10;



            DateTime start = DateTime.Now;

            byte[] curSave = DumpRAMToArray();

            if (curSave.Length != file.Length && file.Length != (curSave.Length + 44) && file.Length != (curSave.Length + 48))
            {
                Console.WriteLine("Save size does not match. \r\nAborting.\r\n");
                return;
            }

            List<ushort[]> diffList = new List<ushort[]>();

            double dataSizeOut = 0;

            DateTime writeStart = DateTime.Now;
            {

                int blockStart = -1, blockLen = 0, lastMismatch = 0;
                bool inBlock = false;

                for (int i = 0; i != curSave.Length; ++i)
                    if (inBlock)
                    {
                        if (curSave[i] == file[i])
                        {
                            if ((i - lastMismatch) > 7)
                            {
                                blockLen = i - blockStart;
                                diffList.Add(new ushort[] { (ushort)blockStart, (ushort)blockLen });
                                dataSizeOut += blockLen;
                                inBlock = false;
                            }
                        }
                        else
                            lastMismatch = i;

                    }
                    else if (curSave[i] != file[i])
                    {
                        inBlock = true;
                        blockStart = lastMismatch = i;
                    }

                if (inBlock)
                    diffList.Add(new ushort[] { (ushort)blockStart, (ushort)(curSave.Length - blockStart) });
            }

            if (diffList.Count == 0)
            {
                Console.WriteLine("Nothing to write.\r\nAborting.\r\n");
            }
            else
            {

                Console.WriteLine("\r\nWriting save file differential from {0} to cartridge...", filename);

                List<byte> RAM = new List<byte>();

                WriteCommand(0x0000, 0x0A, true);

                byte[] bytesOut = new byte[bufferSize];


                double currentPercent = 0.0f;

                //Console.WriteLine("\r\nProgress:\r\n\r\n");

                DateTime beginWrite = DateTime.Now;
                OnProgressBarUpdate(0.0);
                //DisplayPercentage(currentPercent);

                double dataSizeWritten = 0;

                foreach (ushort[] diff in diffList)
                {
                    WriteBlockToBank(file, diff[0], GetRAMBankNumberFromAddress(diff[0]), RemapAddressToRAMArea(diff[0]), diff[1], true);
                    dataSizeWritten += diff[1];

                    currentPercent = dataSizeWritten / dataSizeOut;

                    TimeSpan t = DateTime.Now - start;
                    double secondsPerByte = t.TotalSeconds / dataSizeWritten;
                    int secondsRemaining = (int)(secondsPerByte * (dataSizeOut - dataSizeWritten));
                    TimeSpan remaining = TimeSpan.FromSeconds(secondsRemaining);


                    OnProgressBarUpdate(currentPercent);
                }

                OnProgressBarUpdate(1.0);
            }

            ModeCommand(0x00);

            WriteCommand(0x0000, 0x00, false);


            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine("Done! Elapsed time: {0}", elapsed.ToString(@"mm\:ss"));

            lock (dataLock)
                _suspendInput = false;
        }


        private void WriteBlockToBank(byte[] data, byte bankNumber, ushort destStartAddress, int length, bool quiet = false)
        {

            List<byte> dataSub = new List<byte>(data);

            WriteCommand(0x0000, 0x0A, true);

            byte[] bytesOut;


            DateTime start = DateTime.Now;
            double currentPercent = 0.0f, totalUnits = (double)(Math.Ceiling(data.Length / 64d));

            if (!quiet) Console.WriteLine("\r\nProgress:\r\n\r\n");


            //ModeCommand(0x00); // This issue was fixed by pulsing write on the arduino program
            //Console.WriteLine();
            WriteCommand(0x4100, bankNumber, true);

            for (ushort j = 0; j != (int)totalUnits; ++j)
            {
                bytesOut = new byte[dataSub.Count < 0x40 ? dataSub.Count : 0x40];

                //Console.WriteLine();
                Array.Copy(data, j * 0x40, bytesOut, 0, bytesOut.Length);

                if (dataSub.Count > 0x40)
                    dataSub.RemoveRange(0, 0x40);

                WriteCommand((ushort)(destStartAddress + j * 0x40), bytesOut, true);
                //WriteCommand((ushort)(0xA000 + j), file[i*bufferSize00 + j], true);

                currentPercent = (double)j / totalUnits;

                TimeSpan t = DateTime.Now - start;
                double secondsPerPercent = t.TotalSeconds / currentPercent;
                int secondsRemaining = (int)(secondsPerPercent * (1.0 - currentPercent));
                TimeSpan remaining = TimeSpan.FromSeconds(secondsRemaining);
                if (!quiet)
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                    DisplayPercentage(currentPercent, remaining.ToString(@"mm\:ss"));
                }
            }

            ModeCommand(0x00);

            TimeSpan elapsed = DateTime.Now - start;
            if (!quiet)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
                DisplayPercentage(1.0d);
            }

            WriteCommand(0x0000, 0x00, false);
        }

        private void WriteBlockToBank(byte[] data, int sourceStartAddress, byte bankNumber, ushort destStartAddress, int length, bool quiet = false)
        {
            byte[] tmp = new byte[length];
            Array.Copy(data, sourceStartAddress, tmp, 0, length);
            WriteBlockToBank(tmp, bankNumber, destStartAddress, length, quiet);

        }

        private void ReadCommand(ushort Address, ushort numBytes, bool SRAM)
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

        private void ModeCommand(byte mode)
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

        private void WriteCommand(ushort Address, byte data, bool SRAM)
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

        private void bQuickSaveDump_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => DumpRAMToFile());
        }

        private void bQuickROMDump_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => DumpROMToFile());
        }

        private void bDumpSaveToFile_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
            {
                SaveFileDialog SFD = new SaveFileDialog();

                if (SFD.ShowDialog() == DialogResult.OK)
                    WorkInProgress = Task.Run(() => DumpRAMToFile(SFD.FileName));
            }
        }

        private void bDumpROMtoFile_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
            {
                SaveFileDialog SFD = new SaveFileDialog();

                if (SFD.ShowDialog() == DialogResult.OK)
                    WorkInProgress = Task.Run(() => DumpROMToFile(SFD.FileName));
            }
        }

        private void bDisplayHeader_Click(object sender, EventArgs e)
        {
            lock (dataLock)
                _suspendInput = true;

            //ReadCommand(0x0000, 1, false);    
            _serialPort.Write(new byte[] { 0x03, 0xFF }, 0, 1);


            lock (dataLock)
                _suspendInput = false;
        }

        private string GetROMTitle()
        {


            byte[] header = GetBytes(0x0000, 0x0200, false);

            string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            return ROMName;
        }

        private void DisplayROMTitle()
        {
            lock (dataLock)
                _suspendInput = true;

            Console.WriteLine("{0}\r\n", GetROMTitle());

            lock (dataLock)
                _suspendInput = false;

        }

        private void bDisplayRomTitle_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => DisplayROMTitle());

        }

        private void bFullCartInfo_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => DisplayFullCartInfo());

        }

        private void DisplayFullCartInfo()
        {
            lock (dataLock)
                _suspendInput = true;

            byte[] header = GetBytes(0x0000, 0x0200, false);

            string cartType = "ROM?";
            bool hasRAM = false;

            switch (header[0x147])
            {

                case 0x00:
                    cartType = "ROM ONLY";
                    break;


                case 0x01:
                    cartType = "MBC1";
                    break;


                case 0x02:
                    cartType = "MBC1 + RAM";
                    hasRAM = true;
                    break;


                case 0x03:
                    cartType = "MBC1 + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x05:
                    cartType = "MBC2";
                    break;


                case 0x06:
                    cartType = "MBC2 + BATTERY";
                    break;


                case 0x08:
                    cartType = "ROM + RAM";
                    hasRAM = true;
                    break;


                case 0x09:
                    cartType = "ROM + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x0B:
                    cartType = "MMM01";
                    break;


                case 0x0C:
                    cartType = "MMM01 + RAM";
                    hasRAM = true;
                    break;


                case 0x0D:
                    cartType = "MMM01 + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x0F:
                    cartType = "MBC3 + TIMER + BATTERY";
                    break;


                case 0x10:
                    cartType = "MBC3 + TIMER + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x11:
                    cartType = "MBC3";
                    break;


                case 0x12:
                    cartType = "MBC3 + RAM";
                    hasRAM = true;
                    break;


                case 0x13:
                    cartType = "MBC3 + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x19:
                    cartType = "MBC5";
                    break;


                case 0x1A:
                    cartType = "MBC5 + RAM";
                    hasRAM = true;
                    break;


                case 0x1B:
                    cartType = "MBC5 + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x1C:
                    cartType = "MBC5 + RUMBLE";
                    break;


                case 0x1D:
                    cartType = "MBC5 + RUMBLE + RAM";
                    hasRAM = true;
                    break;


                case 0x1E:
                    cartType = "MBC5 + RUMBLE + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0x20:
                    cartType = "MBC6";
                    hasRAM = true;
                    break;


                case 0x22:
                    cartType = "MBC7 + SENSOR + RUMBLE + RAM + BATTERY";
                    hasRAM = true;
                    break;


                case 0xFC:
                    cartType = "POCKET CAMERA";
                    hasRAM = true;
                    break;


                case 0xFD:
                    cartType = "BANDAI TAMA5";
                    break;


                case 0xFE:
                    cartType = "HuC3";
                    hasRAM = true;
                    break;


                case 0xFF:
                    cartType = "HuC1 + RAM + BATTERY";
                    hasRAM = true;
                    break;


            }

            string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
            if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

            string targetPlatform = "Gameboy";

            if (header[0x143] == 0x80)
                targetPlatform = "CGB Enhanced";
            else if (header[0x143] == 0xC0)
                targetPlatform = "Gameboy Color";

            int BankCount = 0, RAMBankSize = 8;

            switch (header[0x148])
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
                    BankCount = 2 << header[0x148];
                    break;
            }


            switch (header[0x149])
            {
                case 0x00:
                    RAMBankSize = 0;
                    break;

                case 0x01:
                    RAMBankSize = 2;
                    break;

                case 0x03:
                    RAMBankSize = 4;
                    break;

                case 0x04:
                    RAMBankSize = 16;
                    break;

                case 0x05:
                    RAMBankSize = 8;
                    break;
            }

            string romSizeFriendly = "";

            if (BankCount > 39)
                romSizeFriendly = ((BankCount * 0x4000) / (1024 * 1024.0)).ToString("0.00") + " MB";
            else
                romSizeFriendly = ((BankCount * 0x4000) / 1024.0).ToString("0.00") + " KB";

            Console.WriteLine("ROM Name: {0}\r\n" +
                "Cartridge Type: {1}\r\n" +
                "Target Platform: {2}\r\n" +
                "ROM Size: {3}\r\n" +
                ((hasRAM) ? "RAM Size: {4}\r\n" : ""),

                ROMName,
                cartType,
                targetPlatform,
                romSizeFriendly,
                ((RAMBankSize * 0x2000) / 1024.0).ToString("0.00") + " KB");

            byte[] logoBitmap = new byte[48];
            Array.Copy(header, 0x104, logoBitmap, 0, 48);

            OnCheckImageUpdate?.Invoke(GenerateLogoImage(logoBitmap));

            lock (dataLock)
                _suspendInput = false;
        }

        private void WriteRamDiff(string filename = "")
        {
            lock (dataLock)
                _suspendInput = true;

            if (filename == "")
            {

                byte[] header = GetBytes(0x0000, 0x0200, false);

                string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
                if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

                WriteRAMDiffFromFile(ROMName + ".sav");
            }
            else
                WriteRAMDiffFromFile(filename);

            lock (dataLock)
                _suspendInput = false;
        }

        private void WriteRamFull(string filename = "")
        {
            lock (dataLock)
                _suspendInput = true;

            if (filename == "")
            {
                byte[] header = GetBytes(0x0000, 0x0200, false);

                string ROMName = System.Text.Encoding.ASCII.GetString(header, 0x134, 0x0F);
                if (ROMName.Contains("\0")) ROMName = ROMName.Substring(0, ROMName.IndexOf("\0"));

                WriteRAMFromFile(ROMName + ".sav");
            }
            else
                WriteRAMFromFile(filename);

            lock (dataLock)
                _suspendInput = false;
        }

        private Bitmap GenerateLogoImage(byte[] data)
        {

            byte[] comp = new byte[] { 0x80, 0x40, 0x20, 0x10, 8, 4, 2, 1 };

            Color[] colors = new Color[] { Color.Red, Color.Blue };

            Color foregroundColor = Color.FromArgb(unchecked((int)0xFF18521d)),
                backgroundColor = Color.FromArgb(unchecked((int)0xFFffefce));


            Bitmap bitmap = new Bitmap(48, 8);
            Graphics g = Graphics.FromImage(bitmap);

            g.Clear(backgroundColor);

            for (int y = 0; y != 8; y += 4)
                for (int x = 0; x != 48; x += 4)
                    for (int y2 = 0; y2 != 4; ++y2)
                    {
                        for (int x2 = 0; x2 != 4; ++x2)
                        {

                            if ((data[((x / 4) * 2) + (y2 / 2) + y * 6] & comp[x2 + ((y2 % 2) * 4)]) != 0)
                                bitmap.SetPixel(x + x2, y + y2, foregroundColor);
                            //Thread.Sleep(10);
                        }
                    }

            return bitmap;
        }

        private void bQuickSaveWrite_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => WriteRamDiff());
        }

        private void bWriteSaveFromFile_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
            {
                SaveFileDialog SFD = new SaveFileDialog();

                if (SFD.ShowDialog() == DialogResult.OK)
                    WorkInProgress = Task.Run(() => WriteRamDiff(SFD.FileName));
            }
        }

        private void bWriteQuickSaveFull_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
                WorkInProgress = Task.Run(() => WriteRamFull());

        }

        private void bWriteFromFileFull_Click(object sender, EventArgs e)
        {
            if (!_suspendInput)
            {
                SaveFileDialog SFD = new SaveFileDialog();

                if (SFD.ShowDialog() == DialogResult.OK)
                    WorkInProgress = Task.Run(() => WriteRamFull(SFD.FileName));
            }

        }

        private void bMergePokedex_Click(object sender, EventArgs e)
        {

            if (!_suspendInput)
            {
                lock (dataLock)
                    _suspendInput = true;

                Console.WriteLine("Checking Cartridge...");

                string title = GetROMTitle();

                switch (title)
                {
                    case "POKEMON RED":
                    case "POKEMON BLUE":
                    case "POKEMON YELLOW":
                        WorkInProgress = Task.Run(() => MergePokedexRBY());
                        break;

                    case "POKEMON_SLVAAXE":
                    case "POKEMON_GLDAAUE":
                    case "PM_CRYSTAL":
                        WorkInProgress = Task.Run(() => MergePokedexGSC());
                        break;

                }
            }
        }

        /// <summary>
        /// / Sends an array of bytes to the Gameboy Cartridge.
        /// </summary>
        /// <param name="Address">The address on the Gameboy Cartridge to read.</param>
        /// <param name="data">An array containing the data to send to the cartridge. A maximum of 48 bytes can be sent at one time. More than this will throw an ArgumentException.</param>
        /// <param name="SRAM">A bool indicating whether or not data is to be written to the SRAM.</param>
        private void WriteCommand(ushort Address, byte[] data, bool SRAM)
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
    }

    public class TextBoxWriter : TextWriter
    {

        public delegate void ConsoleWriteChar(char c);
        public delegate void ConsoleWriteString(string s);

        public event ConsoleWriteChar OnConsoleWriteChar;
        public event ConsoleWriteString OnConsoleWriteString;

        // The control where we will write text.
        //private TextBox MyControl;
        public TextBoxWriter()
        {
            // MyControl = (TextBox)control;
        }

        public override void Write(char value)
        {
            if (OnConsoleWriteChar != null)
                OnConsoleWriteChar(value);


        }

        public override void Write(string value)
        {
            if (OnConsoleWriteString != null)
                OnConsoleWriteString(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }
    }
}
