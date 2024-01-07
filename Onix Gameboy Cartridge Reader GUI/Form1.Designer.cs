namespace Onix_Gameboy_Cartridge_Reader_GUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.connectToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tbConsoleOutput = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.gbPrimaryActions = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.bMergePokedex = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.bDisplayHeader = new System.Windows.Forms.Button();
            this.bDisplayRomTitle = new System.Windows.Forms.Button();
            this.bFullCartInfo = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.bWriteSaveFromFile = new System.Windows.Forms.Button();
            this.bQuickSaveWrite = new System.Windows.Forms.Button();
            this.bWriteQuickSaveFull = new System.Windows.Forms.Button();
            this.bWriteFromFileFull = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.bQuickSaveDump = new System.Windows.Forms.Button();
            this.bDumpSaveToFile = new System.Windows.Forms.Button();
            this.bQuickROMDump = new System.Windows.Forms.Button();
            this.bDumpROMtoFile = new System.Windows.Forms.Button();
            this.lbDumpedROMs = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbPrimaryActions.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1246, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // connectToToolStripMenuItem
            // 
            this.connectToToolStripMenuItem.Name = "connectToToolStripMenuItem";
            this.connectToToolStripMenuItem.Size = new System.Drawing.Size(106, 24);
            this.connectToToolStripMenuItem.Text = "Connect To...";
            this.connectToToolStripMenuItem.Click += new System.EventHandler(this.connectToToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.89497F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.10503F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbDumpedROMs, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1246, 489);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tbConsoleOutput, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.progressBar1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.gbPrimaryActions, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(253, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(869, 483);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // tbConsoleOutput
            // 
            this.tbConsoleOutput.BackColor = System.Drawing.Color.Black;
            this.tbConsoleOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbConsoleOutput.ForeColor = System.Drawing.Color.White;
            this.tbConsoleOutput.Location = new System.Drawing.Point(3, 254);
            this.tbConsoleOutput.Multiline = true;
            this.tbConsoleOutput.Name = "tbConsoleOutput";
            this.tbConsoleOutput.ReadOnly = true;
            this.tbConsoleOutput.Size = new System.Drawing.Size(863, 194);
            this.tbConsoleOutput.TabIndex = 0;
            this.tbConsoleOutput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(3, 454);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(863, 26);
            this.progressBar1.TabIndex = 0;
            // 
            // gbPrimaryActions
            // 
            this.gbPrimaryActions.Controls.Add(this.groupBox3);
            this.gbPrimaryActions.Controls.Add(this.pictureBox1);
            this.gbPrimaryActions.Controls.Add(this.groupBox2);
            this.gbPrimaryActions.Controls.Add(this.groupBox4);
            this.gbPrimaryActions.Controls.Add(this.groupBox1);
            this.gbPrimaryActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPrimaryActions.Enabled = false;
            this.gbPrimaryActions.Location = new System.Drawing.Point(3, 3);
            this.gbPrimaryActions.Name = "gbPrimaryActions";
            this.gbPrimaryActions.Size = new System.Drawing.Size(863, 245);
            this.gbPrimaryActions.TabIndex = 1;
            this.gbPrimaryActions.TabStop = false;
            this.gbPrimaryActions.Text = "Actions";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel5);
            this.groupBox3.Location = new System.Drawing.Point(666, 21);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(191, 218);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pokemon Tools";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.bMergePokedex, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.button2, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(185, 197);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // bMergePokedex
            // 
            this.bMergePokedex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bMergePokedex.Location = new System.Drawing.Point(3, 3);
            this.bMergePokedex.Name = "bMergePokedex";
            this.bMergePokedex.Size = new System.Drawing.Size(179, 43);
            this.bMergePokedex.TabIndex = 0;
            this.bMergePokedex.Text = "Merge Pokedex";
            this.bMergePokedex.UseVisualStyleBackColor = true;
            this.bMergePokedex.Click += new System.EventHandler(this.bMergePokedex_Click);
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button2.Location = new System.Drawing.Point(3, 52);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(179, 43);
            this.button2.TabIndex = 1;
            this.button2.Text = "Lottery Check";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(398, 166);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(240, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Location = new System.Drawing.Point(365, 21);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(295, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Info";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.bDisplayHeader, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.bDisplayRomTitle, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.bFullCartInfo, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(289, 79);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // bDisplayHeader
            // 
            this.bDisplayHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bDisplayHeader.Location = new System.Drawing.Point(3, 3);
            this.bDisplayHeader.Name = "bDisplayHeader";
            this.bDisplayHeader.Size = new System.Drawing.Size(138, 33);
            this.bDisplayHeader.TabIndex = 0;
            this.bDisplayHeader.Text = "Display Header";
            this.bDisplayHeader.UseVisualStyleBackColor = true;
            this.bDisplayHeader.Click += new System.EventHandler(this.bDisplayHeader_Click);
            // 
            // bDisplayRomTitle
            // 
            this.bDisplayRomTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bDisplayRomTitle.Location = new System.Drawing.Point(147, 3);
            this.bDisplayRomTitle.Name = "bDisplayRomTitle";
            this.bDisplayRomTitle.Size = new System.Drawing.Size(139, 33);
            this.bDisplayRomTitle.TabIndex = 1;
            this.bDisplayRomTitle.Text = "Display ROM TItle";
            this.bDisplayRomTitle.UseVisualStyleBackColor = true;
            this.bDisplayRomTitle.Click += new System.EventHandler(this.bDisplayRomTitle_Click);
            // 
            // bFullCartInfo
            // 
            this.bFullCartInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bFullCartInfo.Location = new System.Drawing.Point(3, 42);
            this.bFullCartInfo.Name = "bFullCartInfo";
            this.bFullCartInfo.Size = new System.Drawing.Size(138, 34);
            this.bFullCartInfo.TabIndex = 2;
            this.bFullCartInfo.Text = "Full Cart Info";
            this.bFullCartInfo.UseVisualStyleBackColor = true;
            this.bFullCartInfo.Click += new System.EventHandler(this.bFullCartInfo_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tableLayoutPanel6);
            this.groupBox4.Location = new System.Drawing.Point(6, 127);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(353, 100);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Save Write Tools";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.bWriteSaveFromFile, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.bQuickSaveWrite, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.bWriteQuickSaveFull, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.bWriteFromFileFull, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(347, 79);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // bWriteSaveFromFile
            // 
            this.bWriteSaveFromFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bWriteSaveFromFile.Location = new System.Drawing.Point(176, 3);
            this.bWriteSaveFromFile.Name = "bWriteSaveFromFile";
            this.bWriteSaveFromFile.Size = new System.Drawing.Size(168, 33);
            this.bWriteSaveFromFile.TabIndex = 1;
            this.bWriteSaveFromFile.Text = "Write From File Fast";
            this.bWriteSaveFromFile.UseVisualStyleBackColor = true;
            this.bWriteSaveFromFile.Click += new System.EventHandler(this.bWriteSaveFromFile_Click);
            // 
            // bQuickSaveWrite
            // 
            this.bQuickSaveWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bQuickSaveWrite.Location = new System.Drawing.Point(3, 3);
            this.bQuickSaveWrite.Name = "bQuickSaveWrite";
            this.bQuickSaveWrite.Size = new System.Drawing.Size(167, 33);
            this.bQuickSaveWrite.TabIndex = 0;
            this.bQuickSaveWrite.Text = "Write Quick Save Fast";
            this.bQuickSaveWrite.UseVisualStyleBackColor = true;
            this.bQuickSaveWrite.Click += new System.EventHandler(this.bQuickSaveWrite_Click);
            // 
            // bWriteQuickSaveFull
            // 
            this.bWriteQuickSaveFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bWriteQuickSaveFull.Location = new System.Drawing.Point(3, 42);
            this.bWriteQuickSaveFull.Name = "bWriteQuickSaveFull";
            this.bWriteQuickSaveFull.Size = new System.Drawing.Size(167, 34);
            this.bWriteQuickSaveFull.TabIndex = 2;
            this.bWriteQuickSaveFull.Text = "Write Quick Save Full";
            this.bWriteQuickSaveFull.UseVisualStyleBackColor = true;
            this.bWriteQuickSaveFull.Click += new System.EventHandler(this.bWriteQuickSaveFull_Click);
            // 
            // bWriteFromFileFull
            // 
            this.bWriteFromFileFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bWriteFromFileFull.Location = new System.Drawing.Point(176, 42);
            this.bWriteFromFileFull.Name = "bWriteFromFileFull";
            this.bWriteFromFileFull.Size = new System.Drawing.Size(168, 34);
            this.bWriteFromFileFull.TabIndex = 3;
            this.bWriteFromFileFull.Text = "Write From File Full";
            this.bWriteFromFileFull.UseVisualStyleBackColor = true;
            this.bWriteFromFileFull.Click += new System.EventHandler(this.bWriteFromFileFull_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Location = new System.Drawing.Point(6, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dump Tools";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.bQuickSaveDump, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.bDumpSaveToFile, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.bQuickROMDump, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.bDumpROMtoFile, 1, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(344, 79);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // bQuickSaveDump
            // 
            this.bQuickSaveDump.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bQuickSaveDump.Location = new System.Drawing.Point(3, 3);
            this.bQuickSaveDump.Name = "bQuickSaveDump";
            this.bQuickSaveDump.Size = new System.Drawing.Size(166, 33);
            this.bQuickSaveDump.TabIndex = 0;
            this.bQuickSaveDump.Text = "Dump Quick Save";
            this.bQuickSaveDump.UseVisualStyleBackColor = true;
            this.bQuickSaveDump.Click += new System.EventHandler(this.bQuickSaveDump_Click);
            // 
            // bDumpSaveToFile
            // 
            this.bDumpSaveToFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bDumpSaveToFile.Location = new System.Drawing.Point(175, 3);
            this.bDumpSaveToFile.Name = "bDumpSaveToFile";
            this.bDumpSaveToFile.Size = new System.Drawing.Size(166, 33);
            this.bDumpSaveToFile.TabIndex = 1;
            this.bDumpSaveToFile.Text = "Dump Save To File";
            this.bDumpSaveToFile.UseVisualStyleBackColor = true;
            this.bDumpSaveToFile.Click += new System.EventHandler(this.bDumpSaveToFile_Click);
            // 
            // bQuickROMDump
            // 
            this.bQuickROMDump.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bQuickROMDump.Location = new System.Drawing.Point(3, 42);
            this.bQuickROMDump.Name = "bQuickROMDump";
            this.bQuickROMDump.Size = new System.Drawing.Size(166, 34);
            this.bQuickROMDump.TabIndex = 2;
            this.bQuickROMDump.Text = "Dump Quick ROM";
            this.bQuickROMDump.UseVisualStyleBackColor = true;
            this.bQuickROMDump.Click += new System.EventHandler(this.bQuickROMDump_Click);
            // 
            // bDumpROMtoFile
            // 
            this.bDumpROMtoFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bDumpROMtoFile.Location = new System.Drawing.Point(175, 42);
            this.bDumpROMtoFile.Name = "bDumpROMtoFile";
            this.bDumpROMtoFile.Size = new System.Drawing.Size(166, 34);
            this.bDumpROMtoFile.TabIndex = 3;
            this.bDumpROMtoFile.Text = "Dump ROM To File";
            this.bDumpROMtoFile.UseVisualStyleBackColor = true;
            this.bDumpROMtoFile.Click += new System.EventHandler(this.bDumpROMtoFile_Click);
            // 
            // lbDumpedROMs
            // 
            this.lbDumpedROMs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbDumpedROMs.FormattingEnabled = true;
            this.lbDumpedROMs.ItemHeight = 16;
            this.lbDumpedROMs.Location = new System.Drawing.Point(3, 3);
            this.lbDumpedROMs.Name = "lbDumpedROMs";
            this.lbDumpedROMs.Size = new System.Drawing.Size(244, 483);
            this.lbDumpedROMs.TabIndex = 2;
            this.lbDumpedROMs.DoubleClick += new System.EventHandler(this.lbDumpedROMs_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1246, 517);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Onix Gameboy Cartridge Reader - [NOT CONNECTED]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.gbPrimaryActions.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem connectToToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox tbConsoleOutput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox gbPrimaryActions;
        private System.Windows.Forms.ListBox lbDumpedROMs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button bQuickSaveDump;
        private System.Windows.Forms.Button bDumpSaveToFile;
        private System.Windows.Forms.Button bQuickROMDump;
        private System.Windows.Forms.Button bDumpROMtoFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button bDisplayHeader;
        private System.Windows.Forms.Button bDisplayRomTitle;
        private System.Windows.Forms.Button bFullCartInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button bQuickSaveWrite;
        private System.Windows.Forms.Button bWriteSaveFromFile;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button bWriteQuickSaveFull;
        private System.Windows.Forms.Button bWriteFromFileFull;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button bMergePokedex;
        private System.Windows.Forms.Button button2;
    }
}

