//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
//
//	Permission to use, copy, modify, and/or distribute this software for any
//	purpose with or without fee is hereby granted, provided that the above
//	copyright notice and this permission notice appear in all copies.
//
//	THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//	WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
//	MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
//	ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
//	WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
//	ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
//	OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

// Decompiled source. Needs refactoring

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Text;

namespace OBJ2MAP
{
    public class MainForm : Form
    {
        private IContainer components = (IContainer)null;
        private GroupBox groupBox1;
        private Button button1;
        private TextBox OBJFilename;
        private Label label1;
        private GroupBox groupBox2;
        private Button button2;
        private TextBox MAPFilename;
        private Label label2;
        private TextBox DepthTextBox;
        private GroupBox groupBox3;
        private RadioButton RB_Spikes;
        private RadioButton RB_Extrusion;
        private RadioButton RB_Standard;
        private Label DepthLabel;
        private Button GOButton;
        private CheckBox CopyToClipboardCheck;
        private Label label4;
        private TextBox ScaleTextBox;
        private Label label3;
        private Label label5;
        private Label label7;
        private TextBox ClassTextBox;
        private Label label6;
        private TextBox DecimalsTextBox;
        private Label label8;
        private TextBox VisibleTextureTextBox;
        private Label label9;
        private TextBox HiddenTextureTextBox;
        private Label label10;
        private Label ProgressLabel;
        private ProgressBar ProgressBar;
        private RadioButton mapVerValve;
        private RadioButton mapVerClassic;
        private Label mapVersionLabel;
        private GroupBox groupBox4;
        private TextBox WADPath;
        private GroupBox wadGroupBox;
        private RadioButton wadSearchPath;
        private RadioButton wadSearchAuto;
        private TextBox wadTextureSizeY;
        private Label label11;
        private TextBox wadTextureSizeX;
        private RadioButton wadSearchSize;
        private Button button3;
        private CheckBox AxisAlignedCheckBox;

        public static bool bAxisAligned = false;

        public MainForm()
        {
            this.InitializeComponent();
            InitializeToolTips();
            MAPCreation.SetForm(this);

        }

        private void MenuFileOpen_Click(object sender, EventArgs e)
        {
        }

        static OpenFileDialog openFileDialog;
        FolderBrowserDialog folderBrowserDialog;

        private void OBJBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select OBJ File To Convert";
            openFileDialog.Filter = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FileOk += openFileDialog_FileOk;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.OBJFilename.Text = openFileDialog.FileName;
            }
        }

        private void WADBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select WAD Directory";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.WADPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        public void UpdateProgress(string _ProgressText = "", int value = 0)
        {
            // Only update the progress label text if there's something there.  [performance]
            if (_ProgressText.Length > 0)
            {
                ProgressLabel.Text = _ProgressText;
            }

            ProgressBar.Value = value;

            // Allow the app to process events so the controls will get updated.  Otherwise, they won't paint until everything is done.
            Application.DoEvents();
        }

        private void openFileDialog_FileOk(Object sender, CancelEventArgs e)
        {
            if (File.Exists(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), Path.GetFileNameWithoutExtension(openFileDialog.FileName) + ".xml")))
            {
                SceneSettings.SetPathForLoading(openFileDialog.FileName);
                this.MAPFilename.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.MapOutput);

                switch (SceneSettings.SettingsLoad(SceneSettings.EFieldNames.MapVersion))
                {
                    case "Classic":
                        this.mapVerClassic.Checked = true;
                        break;
                    case "Valve":
                        this.mapVerValve.Checked = true;
                        break;
                    default:
                        this.mapVerClassic.Checked = true;
                        break;
                }

                switch (SceneSettings.SettingsLoad(SceneSettings.EFieldNames.BrushMethod))
                {
                    case "Spikes":
                        this.RB_Spikes.Checked = true;
                        break;
                    case "Standard":
                        this.RB_Standard.Checked = true;
                        break;
                    case "Extrusion":
                        this.RB_Extrusion.Checked = true;
                        break;
                }

                switch (SceneSettings.SettingsLoad(SceneSettings.EFieldNames.WADOption))
                {
                    case "Auto":
                        this.wadSearchAuto.Checked = true;
                        break;
                    case "Path":
                        this.wadSearchPath.Checked = true;
                        break;
                    case "Size":
                        this.wadSearchSize.Checked = true;
                        break;
                    default:
                        this.wadSearchAuto.Checked = true;
                        break;
                }

                if (SceneSettings.SettingsLoad(SceneSettings.EFieldNames.CopyToClipboard) == "True")
                    this.CopyToClipboardCheck.Checked = true;
                else
                    this.CopyToClipboardCheck.Checked = false;

                this.DepthTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.Depth);
                this.ScaleTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.Scale);
                this.DecimalsTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.DecimalPlaces);
                this.ClassTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.Class);
                this.VisibleTextureTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.VisibleTexture);
                this.HiddenTextureTextBox.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.HiddenTexture);

                if (SceneSettings.SettingsLoad(SceneSettings.EFieldNames.AxisAligned) == "True")
                    this.AxisAlignedCheckBox.Checked = true;
                else
                    this.AxisAlignedCheckBox.Checked = false;

                this.WADPath.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.WADPath);
                this.wadTextureSizeX.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.wadTextureSizeX);
                this.wadTextureSizeY.Text = SceneSettings.SettingsLoad(SceneSettings.EFieldNames.wadTextureSizeY);
            }
        }

        private void MAPBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select MAP File To Write Into";
            openFileDialog.Filter = "MAP files (*.map)|*.map|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = false;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            this.MAPFilename.Text = openFileDialog.FileName;
        }

        //  Map format & method selection events
        private bool valveFormatRB = false;
        private void RB_Standard_CheckedChanged(object sender, EventArgs e)
        {
            this.DepthLabel.Enabled = false;
            this.DepthTextBox.Enabled = false;
            this.AxisAlignedCheckBox.Enabled = false;
        }

        private void RB_Extrusion_CheckedChanged(object sender, EventArgs e)
        {
            this.DepthLabel.Enabled = true;
            this.DepthTextBox.Enabled = true;
            if (mapVerValve.Checked == false)
            {
                this.AxisAlignedCheckBox.Enabled = true;
            }
        }

        private void RB_Spikes_CheckedChanged(object sender, EventArgs e)
        {
            this.DepthLabel.Enabled = true;
            this.DepthTextBox.Enabled = true;
            if (mapVerValve.Checked == false)
            {
                this.AxisAlignedCheckBox.Enabled = true;
            }
        }

        private void mapVerClassic_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.RB_Standard.Checked)
            {
                this.AxisAlignedCheckBox.Enabled = true;
            }

            this.wadGroupBox.Enabled = false;
        }

        private void mapVerValve_CheckedChanged(object sender, EventArgs e)
        {
            this.AxisAlignedCheckBox.Enabled = false;
            this.AxisAlignedCheckBox.Checked = false;
            this.wadGroupBox.Enabled = true;
        }

        private Color CheckIfDirectoryExists()
        {
            if (Directory.Exists(WADPath.Text))
            {
                return Color.White;
            }
            else
            {
                return Color.Red;
            }
        }

        private void wadSearchPath_CheckedChanged(object sender, EventArgs e)
        {
            if (wadSearchPath.Checked)
            {
                WADPath.Enabled = true;
                button3.Enabled = true;
                WADPath.BackColor = CheckIfDirectoryExists();

            }
            else
            {
                WADPath.BackColor = Color.White;
                WADPath.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void WADPath_TextChanged(object sender, EventArgs e)
        {
            WADPath.BackColor = CheckIfDirectoryExists();
        }

        private void wadSearchSize_CheckedChanged(object sender, EventArgs e)
        {
            if (wadSearchSize.Checked)
            {
                wadTextureSizeX.Enabled = true;
                wadTextureSizeY.Enabled = true;
            }
            else
            {
                wadTextureSizeX.Enabled = false;
                wadTextureSizeY.Enabled = false;
            }
        }

        private void ValidateTextureSizeTextBox(TextBox textBox)
        {
            int value;
            if (int.TryParse(textBox.Text, out value))
            {
                if (value < 1)
                    value = 64;
            }
            else
            {
                value = 64;
            }

            textBox.Text = value.ToString();
        }

        private void wadTextureSizeX_LostFocus(object sender, EventArgs e)
        {
            ValidateTextureSizeTextBox(wadTextureSizeX);
        }

        private void wadTextureSizeY_LostFocus(object sender, EventArgs e)
        {
            ValidateTextureSizeTextBox(wadTextureSizeY);
        }

        private void GoButton_Click(object sender, EventArgs e)
		{
            List<bool> SavedCtrlStates = new List<bool>();

            Stopwatch processingTime = new Stopwatch();
            processingTime.Start();

            foreach (Control C in Controls)
            {
                SavedCtrlStates.Add(C.Enabled);
                C.Enabled = false;
            }

            ProgressBar.Enabled = true;
            ProgressLabel.Enabled = true;

            ProgressLabel.Show();
            ProgressBar.Show();
            GOButton.Hide();

            UpdateProgress("Initializing...");

            string OBJFilename = this.OBJFilename.Text;
            string MAPFilename = this.MAPFilename.Text;

            MainForm.MapVersion mapVersion = MainForm.MapVersion.Classic;

            if (this.mapVerClassic.Checked)
            {
                mapVersion = MainForm.MapVersion.Classic;
            }
            else if (this.mapVerValve.Checked)
            {
                mapVersion = MainForm.MapVersion.Valve;
            }

            MainForm.EConvOption econvOption = MainForm.EConvOption.Standard;
            if (this.RB_Extrusion.Checked)
            {
                econvOption = MainForm.EConvOption.Extrusion;
            }
            else if (this.RB_Spikes.Checked)
            {
                econvOption = MainForm.EConvOption.Spikes;
            }

            MainForm.WADOption wadOption = MainForm.WADOption.Auto;
            if (this.wadSearchPath.Checked)
            {
                wadOption = MainForm.WADOption.Path;
            }
            else if (this.wadSearchSize.Checked)
            {
                wadOption = MainForm.WADOption.Size;
            }

            double Depth = double.Parse(this.DepthTextBox.Text);
            MainForm.bAxisAligned = this.AxisAlignedCheckBox.Checked;
            int NumDecimals = Math.Max(int.Parse(this.DecimalsTextBox.Text), 0);
            float ScaleFactor = float.Parse(this.ScaleTextBox.Text);
            string VisibleTextureName = VisibleTextureTextBox.Text.Length > 0 ? VisibleTextureTextBox.Text : "DEFAULT";
            string HiddenTextureName = HiddenTextureTextBox.Text.Length > 0 ? HiddenTextureTextBox.Text : "SKIP";
            bool bCopyToClipboard = this.CopyToClipboardCheck.Checked;
            string Classname = this.ClassTextBox.Text;

            StreamWriter LogFile = new StreamWriter("OBJ2MAP.log");

            // Error checking
            bool error = false;

            if (String.IsNullOrEmpty(this.OBJFilename.Text.Trim()))
            {
                int objFileTxtEmpty = (int)MessageBox.Show("Select OBJ file first!");
                error = true;
            }
            else if (!File.Exists(OBJFilename))
            {
                int num3 = (int)MessageBox.Show("OBJ file doesn't exist.");
                error = true;
            }
            else if ((double)ScaleFactor <= 0.0)
            {
                int num4 = (int)MessageBox.Show("Scale needs to be above 0%.");
                error = true;
            }
            else if (econvOption != MainForm.EConvOption.Standard && Depth < 1.0)
            {
                int num5 = (int)MessageBox.Show("Depth must be greater than 0.");
                error = true;
            }
            else if (String.IsNullOrEmpty(this.MAPFilename.Text.Trim()) && !this.CopyToClipboardCheck.Checked)
            {

                int mapFileTxtEmpty = (int)MessageBox.Show("Select MAP output file first, when \"Copy To Clipboard\" is not checked.");
                error = true;
            }
            else
            {
                // Input looks good, let's go...

                LogFile.AutoFlush = true;
                LogFile.WriteLine(">>> OBJ-2-MAP v1.3.0 starting up. <<<");
                LogFile.WriteLine(string.Format("{0}", (object)DateTime.Now));
                LogFile.WriteLine(string.Format("OBJ Filename : {0}", (object)OBJFilename));
                StreamWriter streamWriter2 = (StreamWriter)null;
                if (MAPFilename.Length > 0)
                {
                    streamWriter2 = File.CreateText(MAPFilename);
                    LogFile.WriteLine(string.Format("MAP Filename : {0}", (object)MAPFilename));
                }
                MainForm.EGRP egrp = MainForm.EGRP.Undefined;
                List<XVector> _Vertices = new List<XVector>();
                List<XFace> _Faces = new List<XFace>();
                List<XUV> _UVs = new List<XUV>();
                List<XBrush> _Brushes = new List<XBrush>();
                char[] separator1 = new char[1] { ' ' };
                char[] separator2 = new char[1] { '/' };

                string format = string.Format("F{0}", (object)NumDecimals);
                LogFile.WriteLine("");
                LogFile.WriteLine(string.Format("Method : {0}", (object)econvOption.ToString()));
                LogFile.WriteLine(string.Format("Copy To Clipboard : {0}", (object)bCopyToClipboard.ToString()));
                LogFile.WriteLine(string.Format("Depth: {0}", (object)Depth));
                LogFile.WriteLine(string.Format("Scale: {0}", (object)ScaleFactor));
                LogFile.WriteLine(string.Format("Decimal Places: {0}", (object)NumDecimals));
                LogFile.WriteLine(string.Format("Class: {0}", Classname.Length > 0 ? (object)Classname : (object)"worldspawn"));
                LogFile.WriteLine(string.Format("Visible Texture: {0}", VisibleTextureName.Length > 0 ? (object)VisibleTextureName : (object)"DEFAULT"));
                LogFile.WriteLine(string.Format("Hidden Texture: {0}", HiddenTextureName.Length > 0 ? (object)HiddenTextureName : (object)"SKIP"));
                LogFile.WriteLine("");
                LogFile.WriteLine("! Reading OBJ file into memory");

                string[] fileLines = File.ReadAllLines(OBJFilename);

                if (mapVersion == MainForm.MapVersion.Valve)
                {
                    MAPCreation.LoadOBJ(this, fileLines, egrp, LogFile, ref _Vertices, ref _Faces, ref _UVs, ref _Brushes, ScaleFactor, separator1, separator2);
                }
                else if (mapVersion == MainForm.MapVersion.Classic)
                {
                    MAPCreation_old.LoadOBJ(this, fileLines, egrp, LogFile, ref _Vertices, ref _Faces, ref _Brushes, ScaleFactor, separator1, separator2);
                }

                if (_Faces.Count > 0)
                    _Brushes.Add(new XBrush()
                    {
                        Faces = _Faces
                    });
                LogFile.WriteLine("");
                LogFile.WriteLine("Summary:");
                LogFile.WriteLine(string.Format("Vertices: {0}", (object)_Vertices.Count));
                LogFile.WriteLine(string.Format("Faces: {0}", (object)_Faces.Count));
                LogFile.WriteLine(string.Format("Brushes: {0}", (object)_Brushes.Count));
                LogFile.WriteLine("");

                //  Choose map version

                LogFile.WriteLine("! Beginning MAP construction.");
                StringBuilder mapString = new StringBuilder("" + "{\n" + string.Format("\"classname\" \"{0}\"\n", Classname.Length > 0 ? (object)Classname : (object)"worldspawn"));

                if (mapVersion == MainForm.MapVersion.Valve)
                {
                    mapString.AppendLine("\"mapversion\" \"220\"");

                    MAPCreation.AddBrushesToMAP(
                                    MAPFilename, 
                                    LogFile, 
                                    econvOption, 
                                    _Vertices, 
                                    _Faces, 
                                    _Brushes, 
                                    format, 
                                    ref mapString, 
                                    VisibleTextureName, 
                                    HiddenTextureName, 
                                    Depth,
                                    ref streamWriter2
                                    );
                }
                else if (mapVersion == MainForm.MapVersion.Classic)
                {
                    MAPCreation_old.AddBrushesToMAP(
                                    this, 
                                    econvOption, 
                                    _Vertices, 
                                    _Faces, 
                                    _Brushes, 
                                    format, 
                                    ref mapString, 
                                    VisibleTextureName, 
                                    HiddenTextureName, 
                                    Depth,
                                    ref streamWriter2
                                    );
                }

                //string text4 = mapString + "}\n";
                mapString.AppendLine("}");
                if (streamWriter2 != null)
                {
                    streamWriter2.Write(mapString.ToString());
                    streamWriter2.Close();
                }
                if (bCopyToClipboard)
                {
                    Clipboard.Clear();
                    Clipboard.SetText(mapString.ToString());
                }

                SceneSettings.SettingsSave(
                            this.MAPFilename.Text, 
                            econvOption, 
                            bCopyToClipboard, 
                            Depth, 
                            ScaleFactor, 
                            NumDecimals, 
                            Classname, 
                            VisibleTextureName, 
                            HiddenTextureName, 
                            OBJFilename, 
                            this.AxisAlignedCheckBox.Checked,
                            mapVersion,
                            wadOption,
                            WADPath.Text,
                            wadTextureSizeX.Text,
                            wadTextureSizeY.Text
                            );
            }

            int SCSIdx = 0;

            foreach (Control C in Controls)
            {
                C.Enabled = SavedCtrlStates[SCSIdx++];
            }

            string FinishMsg = "";

            processingTime.Stop();
            //Console.WriteLine("Elapsed={0}", processingTime.Elapsed);

            UpdateProgress(" ");
            ProgressLabel.Hide();
            ProgressBar.Hide();
            GOButton.Show();

            if (!error)
            {
                string str6 = "Done!";
                str6 += "\n\n" + string.Format("\"{0}\" converted successfully.\n", (object)OBJFilename) + "\n";
                str6 += "Processing time: " + processingTime.Elapsed.ToString("mm\\:ss\\.ff") + "\n\n";
                if (MAPFilename.Length > 0)
                    str6 += string.Format("Written to \"{0}\"", (object)MAPFilename);
                FinishMsg = !bCopyToClipboard ? str6 + "." : (MAPFilename.Length <= 0 ? str6 + "MAP text copied to the clipboard." : str6 + "and MAP text copied to the clipboard.");
            }
            else 
            {
                FinishMsg = "Conversion failed...";
            }
            LogFile.WriteLine(FinishMsg);
            MessageBox.Show(FinishMsg);
            LogFile.Close();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.OBJFilename.Focus();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
				this.components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.OBJFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.wadGroupBox = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.wadTextureSizeY = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.wadTextureSizeX = new System.Windows.Forms.TextBox();
            this.wadSearchSize = new System.Windows.Forms.RadioButton();
            this.wadSearchPath = new System.Windows.Forms.RadioButton();
            this.WADPath = new System.Windows.Forms.TextBox();
            this.wadSearchAuto = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.mapVersionLabel = new System.Windows.Forms.Label();
            this.mapVerValve = new System.Windows.Forms.RadioButton();
            this.mapVerClassic = new System.Windows.Forms.RadioButton();
            this.HiddenTextureTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.VisibleTextureTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.DecimalsTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ClassTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ScaleTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CopyToClipboardCheck = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.AxisAlignedCheckBox = new System.Windows.Forms.CheckBox();
            this.RB_Spikes = new System.Windows.Forms.RadioButton();
            this.RB_Extrusion = new System.Windows.Forms.RadioButton();
            this.RB_Standard = new System.Windows.Forms.RadioButton();
            this.DepthLabel = new System.Windows.Forms.Label();
            this.DepthTextBox = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.MAPFilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.GOButton = new System.Windows.Forms.Button();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.wadGroupBox.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.OBJFilename);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(436, 58);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "INPUT";
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(399, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OBJBrowse_Click);
            // 
            // OBJFilename
            // 
            this.OBJFilename.Location = new System.Drawing.Point(59, 22);
            this.OBJFilename.Name = "OBJFilename";
            this.OBJFilename.Size = new System.Drawing.Size(334, 20);
            this.OBJFilename.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "OBJ File:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.wadGroupBox);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.HiddenTextureTextBox);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.VisibleTextureTextBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.DecimalsTextBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.ClassTextBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.ScaleTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.CopyToClipboardCheck);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.MAPFilename);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox2.Location = new System.Drawing.Point(12, 75);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(436, 382);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "OUTPUT";
            // 
            // wadGroupBox
            // 
            this.wadGroupBox.Controls.Add(this.button3);
            this.wadGroupBox.Controls.Add(this.wadTextureSizeY);
            this.wadGroupBox.Controls.Add(this.label11);
            this.wadGroupBox.Controls.Add(this.wadTextureSizeX);
            this.wadGroupBox.Controls.Add(this.wadSearchSize);
            this.wadGroupBox.Controls.Add(this.wadSearchPath);
            this.wadGroupBox.Controls.Add(this.WADPath);
            this.wadGroupBox.Controls.Add(this.wadSearchAuto);
            this.wadGroupBox.Enabled = false;
            this.wadGroupBox.Location = new System.Drawing.Point(6, 276);
            this.wadGroupBox.Name = "wadGroupBox";
            this.wadGroupBox.Size = new System.Drawing.Size(424, 100);
            this.wadGroupBox.TabIndex = 0;
            this.wadGroupBox.TabStop = false;
            this.wadGroupBox.Text = "Texture Size for UV calc.";
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(390, 43);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 23);
            this.button3.TabIndex = 32;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.WADBrowse_Click);
            // 
            // wadTextureSizeY
            // 
            this.wadTextureSizeY.Enabled = false;
            this.wadTextureSizeY.Location = new System.Drawing.Point(149, 71);
            this.wadTextureSizeY.Name = "wadTextureSizeY";
            this.wadTextureSizeY.Size = new System.Drawing.Size(32, 20);
            this.wadTextureSizeY.TabIndex = 21;
            this.wadTextureSizeY.Text = "64";
            this.wadTextureSizeY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.wadTextureSizeY.LostFocus += new System.EventHandler(this.wadTextureSizeY_LostFocus);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(136, 74);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(12, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "x";
            // 
            // wadTextureSizeX
            // 
            this.wadTextureSizeX.Enabled = false;
            this.wadTextureSizeX.Location = new System.Drawing.Point(102, 71);
            this.wadTextureSizeX.Name = "wadTextureSizeX";
            this.wadTextureSizeX.Size = new System.Drawing.Size(32, 20);
            this.wadTextureSizeX.TabIndex = 20;
            this.wadTextureSizeX.Text = "64";
            this.wadTextureSizeX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.wadTextureSizeX.LostFocus += new System.EventHandler(this.wadTextureSizeX_LostFocus);
            // 
            // wadSearchSize
            // 
            this.wadSearchSize.AutoSize = true;
            this.wadSearchSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.wadSearchSize.Location = new System.Drawing.Point(9, 72);
            this.wadSearchSize.Name = "wadSearchSize";
            this.wadSearchSize.Size = new System.Drawing.Size(86, 17);
            this.wadSearchSize.TabIndex = 19;
            this.wadSearchSize.TabStop = true;
            this.wadSearchSize.Text = "Texture Size:";
            this.wadSearchSize.UseVisualStyleBackColor = true;
            this.wadSearchSize.CheckedChanged += new System.EventHandler(this.wadSearchSize_CheckedChanged);
            // 
            // wadSearchPath
            // 
            this.wadSearchPath.AutoSize = true;
            this.wadSearchPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.wadSearchPath.Location = new System.Drawing.Point(9, 46);
            this.wadSearchPath.Name = "wadSearchPath";
            this.wadSearchPath.Size = new System.Drawing.Size(156, 17);
            this.wadSearchPath.TabIndex = 17;
            this.wadSearchPath.TabStop = true;
            this.wadSearchPath.Text = "Select directory with WADs:";
            this.wadSearchPath.UseVisualStyleBackColor = true;
            this.wadSearchPath.CheckedChanged += new System.EventHandler(this.wadSearchPath_CheckedChanged);
            // 
            // WADPath
            // 
            this.WADPath.Enabled = false;
            this.WADPath.Location = new System.Drawing.Point(171, 45);
            this.WADPath.Name = "WADPath";
            this.WADPath.Size = new System.Drawing.Size(213, 20);
            this.WADPath.TabIndex = 18;
            this.WADPath.TextChanged += new System.EventHandler(this.WADPath_TextChanged);
            // 
            // wadSearchAuto
            // 
            this.wadSearchAuto.AutoSize = true;
            this.wadSearchAuto.Checked = true;
            this.wadSearchAuto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.wadSearchAuto.Location = new System.Drawing.Point(9, 20);
            this.wadSearchAuto.Name = "wadSearchAuto";
            this.wadSearchAuto.Size = new System.Drawing.Size(71, 17);
            this.wadSearchAuto.TabIndex = 16;
            this.wadSearchAuto.TabStop = true;
            this.wadSearchAuto.Text = "Automatic";
            this.wadSearchAuto.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox4.Controls.Add(this.mapVersionLabel);
            this.groupBox4.Controls.Add(this.mapVerValve);
            this.groupBox4.Controls.Add(this.mapVerClassic);
            this.groupBox4.Location = new System.Drawing.Point(6, 58);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(424, 38);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            // 
            // mapVersionLabel
            // 
            this.mapVersionLabel.AutoSize = true;
            this.mapVersionLabel.Location = new System.Drawing.Point(6, 16);
            this.mapVersionLabel.Name = "mapVersionLabel";
            this.mapVersionLabel.Size = new System.Drawing.Size(71, 13);
            this.mapVersionLabel.TabIndex = 23;
            this.mapVersionLabel.Text = "MAP Version:";
            // 
            // mapVerValve
            // 
            this.mapVerValve.AutoSize = true;
            this.mapVerValve.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mapVerValve.Location = new System.Drawing.Point(152, 16);
            this.mapVerValve.Name = "mapVerValve";
            this.mapVerValve.Size = new System.Drawing.Size(72, 17);
            this.mapVerValve.TabIndex = 4;
            this.mapVerValve.TabStop = true;
            this.mapVerValve.Text = "Valve 220";
            this.mapVerValve.UseVisualStyleBackColor = true;
            this.mapVerValve.CheckedChanged += new System.EventHandler(this.mapVerValve_CheckedChanged);
            // 
            // mapVerClassic
            // 
            this.mapVerClassic.AutoSize = true;
            this.mapVerClassic.Checked = true;
            this.mapVerClassic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mapVerClassic.Location = new System.Drawing.Point(83, 16);
            this.mapVerClassic.Name = "mapVerClassic";
            this.mapVerClassic.Size = new System.Drawing.Size(57, 17);
            this.mapVerClassic.TabIndex = 3;
            this.mapVerClassic.TabStop = true;
            this.mapVerClassic.Text = "Classic";
            this.mapVerClassic.UseVisualStyleBackColor = true;
            this.mapVerClassic.CheckedChanged += new System.EventHandler(this.mapVerClassic_CheckedChanged);
            // 
            // HiddenTextureTextBox
            // 
            this.HiddenTextureTextBox.Location = new System.Drawing.Point(309, 250);
            this.HiddenTextureTextBox.Name = "HiddenTextureTextBox";
            this.HiddenTextureTextBox.Size = new System.Drawing.Size(115, 20);
            this.HiddenTextureTextBox.TabIndex = 15;
            this.HiddenTextureTextBox.Text = "SKIP";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(193, 253);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Hidden Face Texture:";
            // 
            // VisibleTextureTextBox
            // 
            this.VisibleTextureTextBox.Location = new System.Drawing.Point(309, 224);
            this.VisibleTextureTextBox.Name = "VisibleTextureTextBox";
            this.VisibleTextureTextBox.Size = new System.Drawing.Size(115, 20);
            this.VisibleTextureTextBox.TabIndex = 14;
            this.VisibleTextureTextBox.Text = "DEFAULT";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(193, 227);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(106, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Visible Face Texture:";
            // 
            // DecimalsTextBox
            // 
            this.DecimalsTextBox.Location = new System.Drawing.Point(279, 198);
            this.DecimalsTextBox.Name = "DecimalsTextBox";
            this.DecimalsTextBox.Size = new System.Drawing.Size(40, 20);
            this.DecimalsTextBox.TabIndex = 13;
            this.DecimalsTextBox.Text = "3";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(193, 201);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Decimal Places:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Enabled = false;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(233, 179);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(191, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "(optional  - leave blank for worldspawn)";
            // 
            // ClassTextBox
            // 
            this.ClassTextBox.Location = new System.Drawing.Point(236, 156);
            this.ClassTextBox.Name = "ClassTextBox";
            this.ClassTextBox.Size = new System.Drawing.Size(188, 20);
            this.ClassTextBox.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(193, 159);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Class:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Enabled = false;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(56, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(200, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "(optional  - leave blank for clipboard only)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(276, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "%";
            // 
            // ScaleTextBox
            // 
            this.ScaleTextBox.Location = new System.Drawing.Point(236, 130);
            this.ScaleTextBox.Name = "ScaleTextBox";
            this.ScaleTextBox.Size = new System.Drawing.Size(40, 20);
            this.ScaleTextBox.TabIndex = 11;
            this.ScaleTextBox.Text = "100";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(193, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Scale:";
            // 
            // CopyToClipboardCheck
            // 
            this.CopyToClipboardCheck.Checked = true;
            this.CopyToClipboardCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CopyToClipboardCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CopyToClipboardCheck.Location = new System.Drawing.Point(196, 107);
            this.CopyToClipboardCheck.Name = "CopyToClipboardCheck";
            this.CopyToClipboardCheck.Size = new System.Drawing.Size(126, 24);
            this.CopyToClipboardCheck.TabIndex = 10;
            this.CopyToClipboardCheck.Text = "Copy To Clipboard?";
            this.CopyToClipboardCheck.CheckedChanged += new System.EventHandler(this.CopyToClipboardCheck_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.AxisAlignedCheckBox);
            this.groupBox3.Controls.Add(this.RB_Spikes);
            this.groupBox3.Controls.Add(this.RB_Extrusion);
            this.groupBox3.Controls.Add(this.RB_Standard);
            this.groupBox3.Controls.Add(this.DepthLabel);
            this.groupBox3.Controls.Add(this.DepthTextBox);
            this.groupBox3.Location = new System.Drawing.Point(6, 107);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(181, 157);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Method";
            // 
            // AxisAlignedCheckBox
            // 
            this.AxisAlignedCheckBox.Checked = true;
            this.AxisAlignedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AxisAlignedCheckBox.Enabled = false;
            this.AxisAlignedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AxisAlignedCheckBox.Location = new System.Drawing.Point(6, 124);
            this.AxisAlignedCheckBox.Name = "AxisAlignedCheckBox";
            this.AxisAlignedCheckBox.Size = new System.Drawing.Size(96, 24);
            this.AxisAlignedCheckBox.TabIndex = 9;
            this.AxisAlignedCheckBox.Text = "Axis Aligned?";
            // 
            // RB_Spikes
            // 
            this.RB_Spikes.AutoSize = true;
            this.RB_Spikes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RB_Spikes.Location = new System.Drawing.Point(6, 65);
            this.RB_Spikes.Name = "RB_Spikes";
            this.RB_Spikes.Size = new System.Drawing.Size(56, 17);
            this.RB_Spikes.TabIndex = 7;
            this.RB_Spikes.TabStop = true;
            this.RB_Spikes.Text = "Spikes";
            this.RB_Spikes.UseVisualStyleBackColor = true;
            this.RB_Spikes.CheckedChanged += new System.EventHandler(this.RB_Spikes_CheckedChanged);
            // 
            // RB_Extrusion
            // 
            this.RB_Extrusion.AutoSize = true;
            this.RB_Extrusion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RB_Extrusion.Location = new System.Drawing.Point(6, 42);
            this.RB_Extrusion.Name = "RB_Extrusion";
            this.RB_Extrusion.Size = new System.Drawing.Size(67, 17);
            this.RB_Extrusion.TabIndex = 6;
            this.RB_Extrusion.TabStop = true;
            this.RB_Extrusion.Text = "Extrusion";
            this.RB_Extrusion.UseVisualStyleBackColor = true;
            this.RB_Extrusion.CheckedChanged += new System.EventHandler(this.RB_Extrusion_CheckedChanged);
            // 
            // RB_Standard
            // 
            this.RB_Standard.AutoSize = true;
            this.RB_Standard.Checked = true;
            this.RB_Standard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RB_Standard.Location = new System.Drawing.Point(6, 19);
            this.RB_Standard.Name = "RB_Standard";
            this.RB_Standard.Size = new System.Drawing.Size(67, 17);
            this.RB_Standard.TabIndex = 5;
            this.RB_Standard.TabStop = true;
            this.RB_Standard.Text = "Standard";
            this.RB_Standard.UseVisualStyleBackColor = true;
            this.RB_Standard.CheckedChanged += new System.EventHandler(this.RB_Standard_CheckedChanged);
            // 
            // DepthLabel
            // 
            this.DepthLabel.AutoSize = true;
            this.DepthLabel.Enabled = false;
            this.DepthLabel.Location = new System.Drawing.Point(3, 101);
            this.DepthLabel.Name = "DepthLabel";
            this.DepthLabel.Size = new System.Drawing.Size(39, 13);
            this.DepthLabel.TabIndex = 7;
            this.DepthLabel.Text = "Depth:";
            // 
            // DepthTextBox
            // 
            this.DepthTextBox.Enabled = false;
            this.DepthTextBox.Location = new System.Drawing.Point(48, 98);
            this.DepthTextBox.Name = "DepthTextBox";
            this.DepthTextBox.Size = new System.Drawing.Size(42, 20);
            this.DepthTextBox.TabIndex = 8;
            this.DepthTextBox.Text = "8";
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(399, 17);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.MAPBrowse_Click);
            // 
            // MAPFilename
            // 
            this.MAPFilename.Location = new System.Drawing.Point(59, 19);
            this.MAPFilename.Name = "MAPFilename";
            this.MAPFilename.Size = new System.Drawing.Size(334, 20);
            this.MAPFilename.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "MAP File:";
            // 
            // GOButton
            // 
            this.GOButton.BackColor = System.Drawing.SystemColors.Highlight;
            this.GOButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GOButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GOButton.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.GOButton.Location = new System.Drawing.Point(12, 463);
            this.GOButton.Name = "GOButton";
            this.GOButton.Size = new System.Drawing.Size(436, 48);
            this.GOButton.TabIndex = 2;
            this.GOButton.Text = "GO!";
            this.GOButton.UseVisualStyleBackColor = false;
            this.GOButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressLabel.Location = new System.Drawing.Point(12, 460);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(435, 29);
            this.ProgressLabel.TabIndex = 3;
            this.ProgressLabel.Text = "Working...";
            this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ProgressLabel.Visible = false;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(12, 488);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(436, 23);
            this.ProgressBar.Step = 1;
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.TabIndex = 4;
            this.ProgressBar.Visible = false;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(459, 525);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.GOButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OBJ-2-MAP v1.3.0 RC1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.wadGroupBox.ResumeLayout(false);
            this.wadGroupBox.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

		}

        ToolTip toolTips = new ToolTip();
        private void InitializeToolTips()
        {
            toolTips.AutoPopDelay = 8000;
            toolTips.InitialDelay = 4000;
            toolTips.ReshowDelay = 1000;
            toolTips.ShowAlways = false;
            //toolTips.ToolTipTitle = "Info:";
            //toolTips.ToolTipIcon = ToolTipIcon.Info;
            toolTips.IsBalloon = true;
            toolTips.SetToolTip(this.OBJFilename,"OBJ file to read and convert");
            toolTips.SetToolTip(this.MAPFilename, "MAP file to save to");
            toolTips.SetToolTip(this.button1, "Select OBJ");
            toolTips.SetToolTip(this.button2, "Select MAP");
            toolTips.SetToolTip(this.button3, "Select directory with WADs");
            toolTips.SetToolTip(this.mapVerClassic, "Classic Quake MAP version. This mode doesn't support UVs.");
            toolTips.SetToolTip(this.mapVerValve, "Valve 220 MAP version. This mode supports UVs.");
            toolTips.SetToolTip(this.CopyToClipboardCheck, "Copy brushes to clipboard, so you can paste them directly in your editor");
            toolTips.SetToolTip(this.RB_Standard, "Export each mesh as a brush (meshes must be convex)");
            toolTips.SetToolTip(this.RB_Extrusion, "Extrude every face into a brush");
            toolTips.SetToolTip(this.RB_Spikes, "Make every face a spike/pyramid brush");
            toolTips.SetToolTip(this.DepthLabel, "Extrusion/Spike height amount");
            toolTips.SetToolTip(this.DepthTextBox, "Extrusion/Spike height amount");
            toolTips.SetToolTip(this.AxisAlignedCheckBox, "Align Extrusion/Spike to closest axis");
            //CopyToClipboardCheck
            //tt_OBJField
            //tt_OBJField
        }

        public enum EGRP
		{
			Undefined,
			Grouped,
			Ungrouped,
		}

		public enum EConvOption
		{
			Standard,
			Extrusion,
			Spikes,
			OptimizedSpikes, // New optimized option that uses quad detection
		}

        public enum MapVersion
        {
            Classic,
            Valve,
        }

        public enum WADOption
        {
            Auto,
            Path,
            Size,
        }

        private void loadSettingFileCheckBox_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void CopyToClipboardCheck_CheckedChanged(object sender, EventArgs e)
		{

		}

        public bool IsWadSearchSizeSelected()
        {
            return wadSearchSize.Checked;
        }

        public bool IsWadSearchPathSelected()
        {
            return wadSearchPath.Checked;
        }
        
        public Tuple<int, int> GetWadSearchSize()
        {
            int x = 64;
            int y = 64;

            int.TryParse(wadTextureSizeX.Text, out x);
            int.TryParse(wadTextureSizeY.Text, out y);

            return Tuple.Create(x, y);
        }

        public string GetWadSearchPath()
        {
            return WADPath.Text;
        }

        public string GetVisibleTextureName()
        {
            return VisibleTextureTextBox.Text;
        }
    }
}
