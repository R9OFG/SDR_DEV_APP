namespace SDR_DEV_APP
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            tabControl1 = new TabControl();
            tabPageSpectrum = new TabPage();
            tabPageScope = new TabPage();
            panelScopeControls = new Panel();
            groupTrigger = new GroupBox();
            LblTriggerLevel = new Label();
            nudTriggerLevel = new NumericUpDown();
            rbTriggerQ = new RadioButton();
            rbTriggerI = new RadioButton();
            rbTriggerFree = new RadioButton();
            label1 = new Label();
            trackBarTimeDiv = new TrackBar();
            lblTimePerDiv = new Label();
            chkAutoScale = new CheckBox();
            lblSensitivity_V = new Label();
            trackBarSensitivity = new TrackBar();
            LblInputAudioDevice = new Label();
            cbInputAudioDeviceList = new ComboBox();
            btnStartCapture = new Button();
            btnStopCapture = new Button();
            btnOpenAudioManager = new Button();
            btnOpenWav = new Button();
            btnPause = new Button();
            btnStopPlayWAVFile = new Button();
            LblWAVFilePositionTime = new Label();
            progressBarWavPosition = new ProgressBar();
            LblOutputAudioDevice = new Label();
            cbOutputAudioDeviceList = new ComboBox();
            chkMuteAudioOut = new CheckBox();
            groupBox1 = new GroupBox();
            chkShowCursorInfo = new CheckBox();
            NudWfColorRef = new NumericUpDown();
            LblWfColorRef = new Label();
            NudWfColorRange = new NumericUpDown();
            LblWfColorRange = new Label();
            NudPlotRange = new NumericUpDown();
            NudPlotTop = new NumericUpDown();
            LblPlotRange = new Label();
            LblPlotTop = new Label();
            CbFftSize = new ComboBox();
            LblFftSize = new Label();
            groupBox2 = new GroupBox();
            chkAGC = new CheckBox();
            nudVolume = new NumericUpDown();
            LblVolume = new Label();
            nudAGCDecayTime = new NumericUpDown();
            nudAGCAttackTime = new NumericUpDown();
            nudAGCThreshold = new NumericUpDown();
            nudBw = new NumericUpDown();
            LblBW = new Label();
            rbFm = new RadioButton();
            rbAm = new RadioButton();
            rbUsb = new RadioButton();
            rbLsb = new RadioButton();
            chkDigitalLpf = new CheckBox();
            nudPhaseCoeff = new NumericUpDown();
            nudGainRatio = new NumericUpDown();
            chkPhaseCorrection = new CheckBox();
            chkSwapIQ = new CheckBox();
            chkGainBalance = new CheckBox();
            chkDcCorrection = new CheckBox();
            panelControls = new Panel();
            chkLoopWavPlayback = new CheckBox();
            tabControl1.SuspendLayout();
            tabPageScope.SuspendLayout();
            panelScopeControls.SuspendLayout();
            groupTrigger.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTriggerLevel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarTimeDiv).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSensitivity).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRef).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotTop).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVolume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCDecayTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCAttackTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCThreshold).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBw).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPhaseCoeff).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGainRatio).BeginInit();
            panelControls.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageSpectrum);
            tabControl1.Controls.Add(tabPageScope);
            tabControl1.Location = new Point(0, 236);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(908, 450);
            tabControl1.TabIndex = 1;
            // 
            // tabPageSpectrum
            // 
            tabPageSpectrum.BackColor = SystemColors.Control;
            tabPageSpectrum.Location = new Point(4, 24);
            tabPageSpectrum.Name = "tabPageSpectrum";
            tabPageSpectrum.Padding = new Padding(3);
            tabPageSpectrum.Size = new Size(900, 422);
            tabPageSpectrum.TabIndex = 0;
            tabPageSpectrum.Text = "Spectrum";
            // 
            // tabPageScope
            // 
            tabPageScope.BackColor = SystemColors.Control;
            tabPageScope.Controls.Add(panelScopeControls);
            tabPageScope.Location = new Point(4, 24);
            tabPageScope.Name = "tabPageScope";
            tabPageScope.Padding = new Padding(3);
            tabPageScope.Size = new Size(900, 422);
            tabPageScope.TabIndex = 1;
            tabPageScope.Text = "Oscilloscope";
            // 
            // panelScopeControls
            // 
            panelScopeControls.BackColor = SystemColors.Control;
            panelScopeControls.Controls.Add(groupTrigger);
            panelScopeControls.Controls.Add(label1);
            panelScopeControls.Controls.Add(trackBarTimeDiv);
            panelScopeControls.Controls.Add(lblTimePerDiv);
            panelScopeControls.Controls.Add(chkAutoScale);
            panelScopeControls.Controls.Add(lblSensitivity_V);
            panelScopeControls.Controls.Add(trackBarSensitivity);
            panelScopeControls.Dock = DockStyle.Right;
            panelScopeControls.Location = new Point(625, 3);
            panelScopeControls.Name = "panelScopeControls";
            panelScopeControls.Size = new Size(272, 416);
            panelScopeControls.TabIndex = 0;
            // 
            // groupTrigger
            // 
            groupTrigger.Controls.Add(LblTriggerLevel);
            groupTrigger.Controls.Add(nudTriggerLevel);
            groupTrigger.Controls.Add(rbTriggerQ);
            groupTrigger.Controls.Add(rbTriggerI);
            groupTrigger.Controls.Add(rbTriggerFree);
            groupTrigger.Location = new Point(3, 262);
            groupTrigger.Name = "groupTrigger";
            groupTrigger.Size = new Size(266, 81);
            groupTrigger.TabIndex = 6;
            groupTrigger.TabStop = false;
            groupTrigger.Text = " Trigger ";
            // 
            // LblTriggerLevel
            // 
            LblTriggerLevel.AutoSize = true;
            LblTriggerLevel.Location = new Point(12, 50);
            LblTriggerLevel.Name = "LblTriggerLevel";
            LblTriggerLevel.Size = new Size(76, 15);
            LblTriggerLevel.TabIndex = 3;
            LblTriggerLevel.Text = "Trigger Level:";
            // 
            // nudTriggerLevel
            // 
            nudTriggerLevel.Location = new Point(90, 48);
            nudTriggerLevel.Name = "nudTriggerLevel";
            nudTriggerLevel.ReadOnly = true;
            nudTriggerLevel.Size = new Size(60, 23);
            nudTriggerLevel.TabIndex = 4;
            // 
            // rbTriggerQ
            // 
            rbTriggerQ.AutoSize = true;
            rbTriggerQ.Location = new Point(182, 23);
            rbTriggerQ.Name = "rbTriggerQ";
            rbTriggerQ.Size = new Size(81, 19);
            rbTriggerQ.TabIndex = 2;
            rbTriggerQ.TabStop = true;
            rbTriggerQ.Text = "Channel Q";
            rbTriggerQ.UseVisualStyleBackColor = true;
            // 
            // rbTriggerI
            // 
            rbTriggerI.AutoSize = true;
            rbTriggerI.Location = new Point(92, 23);
            rbTriggerI.Name = "rbTriggerI";
            rbTriggerI.Size = new Size(75, 19);
            rbTriggerI.TabIndex = 1;
            rbTriggerI.TabStop = true;
            rbTriggerI.Text = "Channel I";
            rbTriggerI.UseVisualStyleBackColor = true;
            // 
            // rbTriggerFree
            // 
            rbTriggerFree.AutoSize = true;
            rbTriggerFree.Location = new Point(12, 23);
            rbTriggerFree.Name = "rbTriggerFree";
            rbTriggerFree.Size = new Size(54, 19);
            rbTriggerFree.TabIndex = 0;
            rbTriggerFree.TabStop = true;
            rbTriggerFree.Text = "None";
            rbTriggerFree.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.BackColor = SystemColors.ActiveBorder;
            label1.Location = new Point(136, 9);
            label1.Name = "label1";
            label1.Size = new Size(2, 262);
            label1.TabIndex = 3;
            // 
            // trackBarTimeDiv
            // 
            trackBarTimeDiv.LargeChange = 100;
            trackBarTimeDiv.Location = new Point(190, 49);
            trackBarTimeDiv.Maximum = 100000;
            trackBarTimeDiv.Minimum = 1;
            trackBarTimeDiv.Name = "trackBarTimeDiv";
            trackBarTimeDiv.Orientation = Orientation.Vertical;
            trackBarTimeDiv.Size = new Size(45, 215);
            trackBarTimeDiv.SmallChange = 100;
            trackBarTimeDiv.TabIndex = 5;
            trackBarTimeDiv.TickFrequency = 10000;
            trackBarTimeDiv.Value = 50;
            // 
            // lblTimePerDiv
            // 
            lblTimePerDiv.Location = new Point(162, 14);
            lblTimePerDiv.Name = "lblTimePerDiv";
            lblTimePerDiv.Size = new Size(86, 15);
            lblTimePerDiv.TabIndex = 4;
            lblTimePerDiv.Text = "1 ms/div";
            lblTimePerDiv.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkAutoScale
            // 
            chkAutoScale.Checked = true;
            chkAutoScale.CheckState = CheckState.Checked;
            chkAutoScale.Location = new Point(41, 30);
            chkAutoScale.Name = "chkAutoScale";
            chkAutoScale.Size = new Size(54, 22);
            chkAutoScale.TabIndex = 1;
            chkAutoScale.Text = "Auto";
            chkAutoScale.UseVisualStyleBackColor = true;
            // 
            // lblSensitivity_V
            // 
            lblSensitivity_V.Location = new Point(18, 13);
            lblSensitivity_V.Name = "lblSensitivity_V";
            lblSensitivity_V.Size = new Size(98, 16);
            lblSensitivity_V.TabIndex = 0;
            lblSensitivity_V.Text = "0.01 V/div";
            lblSensitivity_V.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackBarSensitivity
            // 
            trackBarSensitivity.Location = new Point(51, 49);
            trackBarSensitivity.Maximum = 100000;
            trackBarSensitivity.Minimum = 1;
            trackBarSensitivity.Name = "trackBarSensitivity";
            trackBarSensitivity.Orientation = Orientation.Vertical;
            trackBarSensitivity.Size = new Size(45, 215);
            trackBarSensitivity.TabIndex = 2;
            trackBarSensitivity.TickFrequency = 10000;
            trackBarSensitivity.Value = 50;
            // 
            // LblInputAudioDevice
            // 
            LblInputAudioDevice.AutoSize = true;
            LblInputAudioDevice.Location = new Point(15, 12);
            LblInputAudioDevice.Name = "LblInputAudioDevice";
            LblInputAudioDevice.Size = new Size(111, 15);
            LblInputAudioDevice.TabIndex = 0;
            LblInputAudioDevice.Text = "Input Audio Device:";
            // 
            // cbInputAudioDeviceList
            // 
            cbInputAudioDeviceList.DropDownStyle = ComboBoxStyle.DropDownList;
            cbInputAudioDeviceList.FormattingEnabled = true;
            cbInputAudioDeviceList.Location = new Point(129, 9);
            cbInputAudioDeviceList.Margin = new Padding(3, 2, 3, 2);
            cbInputAudioDeviceList.Name = "cbInputAudioDeviceList";
            cbInputAudioDeviceList.Size = new Size(318, 23);
            cbInputAudioDeviceList.TabIndex = 1;
            // 
            // btnStartCapture
            // 
            btnStartCapture.Location = new Point(453, 9);
            btnStartCapture.Margin = new Padding(3, 2, 3, 2);
            btnStartCapture.Name = "btnStartCapture";
            btnStartCapture.Size = new Size(113, 22);
            btnStartCapture.TabIndex = 2;
            btnStartCapture.Text = "Start capture IQ";
            btnStartCapture.UseVisualStyleBackColor = true;
            // 
            // btnStopCapture
            // 
            btnStopCapture.Location = new Point(572, 9);
            btnStopCapture.Margin = new Padding(3, 2, 3, 2);
            btnStopCapture.Name = "btnStopCapture";
            btnStopCapture.Size = new Size(113, 22);
            btnStopCapture.TabIndex = 3;
            btnStopCapture.Text = "Stop capture IQ";
            btnStopCapture.UseVisualStyleBackColor = true;
            // 
            // btnOpenAudioManager
            // 
            btnOpenAudioManager.Location = new Point(691, 9);
            btnOpenAudioManager.Margin = new Padding(3, 2, 3, 2);
            btnOpenAudioManager.Name = "btnOpenAudioManager";
            btnOpenAudioManager.Size = new Size(113, 22);
            btnOpenAudioManager.TabIndex = 4;
            btnOpenAudioManager.Text = "Audio Devices";
            btnOpenAudioManager.UseVisualStyleBackColor = true;
            // 
            // btnOpenWav
            // 
            btnOpenWav.Location = new Point(10, 36);
            btnOpenWav.Margin = new Padding(3, 2, 3, 2);
            btnOpenWav.Name = "btnOpenWav";
            btnOpenWav.Size = new Size(113, 22);
            btnOpenWav.TabIndex = 5;
            btnOpenWav.Text = "Open WAV File";
            btnOpenWav.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(129, 36);
            btnPause.Margin = new Padding(3, 2, 3, 2);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(113, 22);
            btnPause.TabIndex = 6;
            btnPause.Text = "Pause";
            btnPause.UseVisualStyleBackColor = true;
            // 
            // btnStopPlayWAVFile
            // 
            btnStopPlayWAVFile.Location = new Point(248, 36);
            btnStopPlayWAVFile.Margin = new Padding(3, 2, 3, 2);
            btnStopPlayWAVFile.Name = "btnStopPlayWAVFile";
            btnStopPlayWAVFile.Size = new Size(113, 22);
            btnStopPlayWAVFile.TabIndex = 7;
            btnStopPlayWAVFile.Text = "Stop Play WAV File";
            btnStopPlayWAVFile.UseVisualStyleBackColor = true;
            // 
            // LblWAVFilePositionTime
            // 
            LblWAVFilePositionTime.AutoSize = true;
            LblWAVFilePositionTime.Location = new Point(691, 43);
            LblWAVFilePositionTime.Name = "LblWAVFilePositionTime";
            LblWAVFilePositionTime.Size = new Size(135, 15);
            LblWAVFilePositionTime.TabIndex = 10;
            LblWAVFilePositionTime.Text = "LblWAVFilePositionTime";
            // 
            // progressBarWavPosition
            // 
            progressBarWavPosition.Location = new Point(420, 43);
            progressBarWavPosition.Name = "progressBarWavPosition";
            progressBarWavPosition.Size = new Size(265, 10);
            progressBarWavPosition.TabIndex = 9;
            // 
            // LblOutputAudioDevice
            // 
            LblOutputAudioDevice.AutoSize = true;
            LblOutputAudioDevice.Location = new Point(5, 65);
            LblOutputAudioDevice.Name = "LblOutputAudioDevice";
            LblOutputAudioDevice.Size = new Size(121, 15);
            LblOutputAudioDevice.TabIndex = 11;
            LblOutputAudioDevice.Text = "Output Audio Device:";
            // 
            // cbOutputAudioDeviceList
            // 
            cbOutputAudioDeviceList.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOutputAudioDeviceList.FormattingEnabled = true;
            cbOutputAudioDeviceList.Location = new Point(129, 62);
            cbOutputAudioDeviceList.Margin = new Padding(3, 2, 3, 2);
            cbOutputAudioDeviceList.Name = "cbOutputAudioDeviceList";
            cbOutputAudioDeviceList.Size = new Size(318, 23);
            cbOutputAudioDeviceList.TabIndex = 12;
            // 
            // chkMuteAudioOut
            // 
            chkMuteAudioOut.AutoSize = true;
            chkMuteAudioOut.Location = new Point(454, 64);
            chkMuteAudioOut.Name = "chkMuteAudioOut";
            chkMuteAudioOut.Size = new Size(112, 19);
            chkMuteAudioOut.TabIndex = 13;
            chkMuteAudioOut.Text = "Mute Audio Out";
            chkMuteAudioOut.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkShowCursorInfo);
            groupBox1.Controls.Add(NudWfColorRef);
            groupBox1.Controls.Add(LblWfColorRef);
            groupBox1.Controls.Add(NudWfColorRange);
            groupBox1.Controls.Add(LblWfColorRange);
            groupBox1.Controls.Add(NudPlotRange);
            groupBox1.Controls.Add(NudPlotTop);
            groupBox1.Controls.Add(LblPlotRange);
            groupBox1.Controls.Add(LblPlotTop);
            groupBox1.Controls.Add(CbFftSize);
            groupBox1.Controls.Add(LblFftSize);
            groupBox1.Location = new Point(5, 87);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(903, 53);
            groupBox1.TabIndex = 14;
            groupBox1.TabStop = false;
            groupBox1.Text = " FFT / WaterFall ";
            // 
            // chkShowCursorInfo
            // 
            chkShowCursorInfo.AutoSize = true;
            chkShowCursorInfo.Location = new Point(653, 23);
            chkShowCursorInfo.Name = "chkShowCursorInfo";
            chkShowCursorInfo.Size = new Size(117, 19);
            chkShowCursorInfo.TabIndex = 10;
            chkShowCursorInfo.Text = "Mouse Point Info";
            chkShowCursorInfo.UseVisualStyleBackColor = true;
            // 
            // NudWfColorRef
            // 
            NudWfColorRef.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            NudWfColorRef.Location = new Point(447, 22);
            NudWfColorRef.Maximum = new decimal(new int[] { 190, 0, 0, 0 });
            NudWfColorRef.Minimum = new decimal(new int[] { 80, 0, 0, 0 });
            NudWfColorRef.Name = "NudWfColorRef";
            NudWfColorRef.ReadOnly = true;
            NudWfColorRef.Size = new Size(60, 23);
            NudWfColorRef.TabIndex = 7;
            NudWfColorRef.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // LblWfColorRef
            // 
            LblWfColorRef.AutoSize = true;
            LblWfColorRef.Location = new Point(399, 24);
            LblWfColorRef.Name = "LblWfColorRef";
            LblWfColorRef.Size = new Size(47, 15);
            LblWfColorRef.TabIndex = 6;
            LblWfColorRef.Text = "WF Ref:";
            // 
            // NudWfColorRange
            // 
            NudWfColorRange.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            NudWfColorRange.Location = new Point(577, 22);
            NudWfColorRange.Maximum = new decimal(new int[] { 190, 0, 0, 0 });
            NudWfColorRange.Minimum = new decimal(new int[] { 80, 0, 0, 0 });
            NudWfColorRange.Name = "NudWfColorRange";
            NudWfColorRange.ReadOnly = true;
            NudWfColorRange.Size = new Size(60, 23);
            NudWfColorRange.TabIndex = 9;
            NudWfColorRange.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // LblWfColorRange
            // 
            LblWfColorRange.AutoSize = true;
            LblWfColorRange.Location = new Point(513, 24);
            LblWfColorRange.Name = "LblWfColorRange";
            LblWfColorRange.Size = new Size(63, 15);
            LblWfColorRange.TabIndex = 8;
            LblWfColorRange.Text = "WF Range:";
            // 
            // NudPlotRange
            // 
            NudPlotRange.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            NudPlotRange.Location = new Point(335, 22);
            NudPlotRange.Maximum = new decimal(new int[] { 190, 0, 0, 0 });
            NudPlotRange.Minimum = new decimal(new int[] { 80, 0, 0, 0 });
            NudPlotRange.Name = "NudPlotRange";
            NudPlotRange.ReadOnly = true;
            NudPlotRange.Size = new Size(60, 23);
            NudPlotRange.TabIndex = 5;
            NudPlotRange.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // NudPlotTop
            // 
            NudPlotTop.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            NudPlotTop.Location = new Point(201, 22);
            NudPlotTop.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            NudPlotTop.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            NudPlotTop.Name = "NudPlotTop";
            NudPlotTop.ReadOnly = true;
            NudPlotTop.Size = new Size(60, 23);
            NudPlotTop.TabIndex = 3;
            NudPlotTop.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // LblPlotRange
            // 
            LblPlotRange.AutoSize = true;
            LblPlotRange.Location = new Point(267, 24);
            LblPlotRange.Name = "LblPlotRange";
            LblPlotRange.Size = new Size(67, 15);
            LblPlotRange.TabIndex = 4;
            LblPlotRange.Text = "Plot Range:";
            // 
            // LblPlotTop
            // 
            LblPlotTop.AutoSize = true;
            LblPlotTop.Location = new Point(147, 24);
            LblPlotTop.Name = "LblPlotTop";
            LblPlotTop.Size = new Size(53, 15);
            LblPlotTop.TabIndex = 2;
            LblPlotTop.Text = "Plot Top:";
            // 
            // CbFftSize
            // 
            CbFftSize.DropDownStyle = ComboBoxStyle.DropDownList;
            CbFftSize.FormattingEnabled = true;
            CbFftSize.Location = new Point(62, 22);
            CbFftSize.Margin = new Padding(3, 2, 3, 2);
            CbFftSize.Name = "CbFftSize";
            CbFftSize.Size = new Size(78, 23);
            CbFftSize.TabIndex = 1;
            // 
            // LblFftSize
            // 
            LblFftSize.AutoSize = true;
            LblFftSize.Location = new Point(10, 24);
            LblFftSize.Name = "LblFftSize";
            LblFftSize.Size = new Size(51, 15);
            LblFftSize.TabIndex = 0;
            LblFftSize.Text = "FFT Size:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkAGC);
            groupBox2.Controls.Add(nudVolume);
            groupBox2.Controls.Add(LblVolume);
            groupBox2.Controls.Add(nudAGCDecayTime);
            groupBox2.Controls.Add(nudAGCAttackTime);
            groupBox2.Controls.Add(nudAGCThreshold);
            groupBox2.Controls.Add(nudBw);
            groupBox2.Controls.Add(LblBW);
            groupBox2.Controls.Add(rbFm);
            groupBox2.Controls.Add(rbAm);
            groupBox2.Controls.Add(rbUsb);
            groupBox2.Controls.Add(rbLsb);
            groupBox2.Controls.Add(chkDigitalLpf);
            groupBox2.Controls.Add(nudPhaseCoeff);
            groupBox2.Controls.Add(nudGainRatio);
            groupBox2.Controls.Add(chkPhaseCorrection);
            groupBox2.Controls.Add(chkSwapIQ);
            groupBox2.Controls.Add(chkGainBalance);
            groupBox2.Controls.Add(chkDcCorrection);
            groupBox2.Location = new Point(5, 141);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(903, 74);
            groupBox2.TabIndex = 15;
            groupBox2.TabStop = false;
            groupBox2.Text = " Correction / Filtering / Demodulation ";
            // 
            // chkAGC
            // 
            chkAGC.AutoSize = true;
            chkAGC.Location = new Point(487, 47);
            chkAGC.Name = "chkAGC";
            chkAGC.Size = new Size(50, 19);
            chkAGC.TabIndex = 13;
            chkAGC.Text = "AGC";
            chkAGC.UseVisualStyleBackColor = true;
            // 
            // nudVolume
            // 
            nudVolume.Location = new Point(790, 46);
            nudVolume.Name = "nudVolume";
            nudVolume.ReadOnly = true;
            nudVolume.Size = new Size(60, 23);
            nudVolume.TabIndex = 18;
            // 
            // LblVolume
            // 
            LblVolume.AutoSize = true;
            LblVolume.Location = new Point(739, 49);
            LblVolume.Name = "LblVolume";
            LblVolume.Size = new Size(50, 15);
            LblVolume.TabIndex = 17;
            LblVolume.Text = "Volume:";
            // 
            // nudAGCDecayTime
            // 
            nudAGCDecayTime.Location = new Point(673, 46);
            nudAGCDecayTime.Name = "nudAGCDecayTime";
            nudAGCDecayTime.ReadOnly = true;
            nudAGCDecayTime.Size = new Size(60, 23);
            nudAGCDecayTime.TabIndex = 16;
            // 
            // nudAGCAttackTime
            // 
            nudAGCAttackTime.Location = new Point(607, 46);
            nudAGCAttackTime.Name = "nudAGCAttackTime";
            nudAGCAttackTime.ReadOnly = true;
            nudAGCAttackTime.Size = new Size(60, 23);
            nudAGCAttackTime.TabIndex = 15;
            // 
            // nudAGCThreshold
            // 
            nudAGCThreshold.Location = new Point(541, 46);
            nudAGCThreshold.Name = "nudAGCThreshold";
            nudAGCThreshold.ReadOnly = true;
            nudAGCThreshold.Size = new Size(60, 23);
            nudAGCThreshold.TabIndex = 14;
            // 
            // nudBw
            // 
            nudBw.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudBw.Location = new Point(201, 46);
            nudBw.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudBw.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            nudBw.Name = "nudBw";
            nudBw.ReadOnly = true;
            nudBw.Size = new Size(54, 23);
            nudBw.TabIndex = 8;
            nudBw.Value = new decimal(new int[] { 2700, 0, 0, 0 });
            // 
            // LblBW
            // 
            LblBW.AutoSize = true;
            LblBW.Location = new Point(128, 48);
            LblBW.Name = "LblBW";
            LblBW.Size = new Size(72, 15);
            LblBW.TabIndex = 7;
            LblBW.Text = "Band Width:";
            // 
            // rbFm
            // 
            rbFm.AutoSize = true;
            rbFm.Location = new Point(418, 47);
            rbFm.Name = "rbFm";
            rbFm.Size = new Size(42, 19);
            rbFm.TabIndex = 12;
            rbFm.TabStop = true;
            rbFm.Text = "FM";
            rbFm.UseVisualStyleBackColor = true;
            // 
            // rbAm
            // 
            rbAm.AutoSize = true;
            rbAm.Location = new Point(368, 47);
            rbAm.Name = "rbAm";
            rbAm.Size = new Size(44, 19);
            rbAm.TabIndex = 11;
            rbAm.TabStop = true;
            rbAm.Text = "AM";
            rbAm.UseVisualStyleBackColor = true;
            // 
            // rbUsb
            // 
            rbUsb.AutoSize = true;
            rbUsb.Location = new Point(317, 47);
            rbUsb.Name = "rbUsb";
            rbUsb.Size = new Size(46, 19);
            rbUsb.TabIndex = 10;
            rbUsb.TabStop = true;
            rbUsb.Text = "USB";
            rbUsb.UseVisualStyleBackColor = true;
            // 
            // rbLsb
            // 
            rbLsb.AutoSize = true;
            rbLsb.Location = new Point(267, 47);
            rbLsb.Name = "rbLsb";
            rbLsb.Size = new Size(44, 19);
            rbLsb.TabIndex = 9;
            rbLsb.TabStop = true;
            rbLsb.Text = "LSB";
            rbLsb.UseVisualStyleBackColor = true;
            // 
            // chkDigitalLpf
            // 
            chkDigitalLpf.AutoSize = true;
            chkDigitalLpf.Location = new Point(10, 47);
            chkDigitalLpf.Name = "chkDigitalLpf";
            chkDigitalLpf.Size = new Size(101, 19);
            chkDigitalLpf.TabIndex = 6;
            chkDigitalLpf.Text = "Digital FIR LPF";
            chkDigitalLpf.UseVisualStyleBackColor = true;
            // 
            // nudPhaseCoeff
            // 
            nudPhaseCoeff.Location = new Point(832, 18);
            nudPhaseCoeff.Name = "nudPhaseCoeff";
            nudPhaseCoeff.ReadOnly = true;
            nudPhaseCoeff.Size = new Size(60, 23);
            nudPhaseCoeff.TabIndex = 5;
            // 
            // nudGainRatio
            // 
            nudGainRatio.Location = new Point(541, 18);
            nudGainRatio.Name = "nudGainRatio";
            nudGainRatio.ReadOnly = true;
            nudGainRatio.Size = new Size(60, 23);
            nudGainRatio.TabIndex = 3;
            // 
            // chkPhaseCorrection
            // 
            chkPhaseCorrection.AutoSize = true;
            chkPhaseCorrection.Location = new Point(636, 22);
            chkPhaseCorrection.Name = "chkPhaseCorrection";
            chkPhaseCorrection.Size = new Size(173, 19);
            chkPhaseCorrection.TabIndex = 4;
            chkPhaseCorrection.Text = "Corr I/Q Phase Balance: 0.0°";
            chkPhaseCorrection.UseVisualStyleBackColor = true;
            // 
            // chkSwapIQ
            // 
            chkSwapIQ.AutoSize = true;
            chkSwapIQ.Location = new Point(10, 22);
            chkSwapIQ.Name = "chkSwapIQ";
            chkSwapIQ.Size = new Size(74, 19);
            chkSwapIQ.TabIndex = 0;
            chkSwapIQ.Text = "Swap I/Q";
            chkSwapIQ.UseVisualStyleBackColor = true;
            // 
            // chkGainBalance
            // 
            chkGainBalance.AutoSize = true;
            chkGainBalance.Location = new Point(346, 22);
            chkGainBalance.Name = "chkGainBalance";
            chkGainBalance.Size = new Size(166, 19);
            chkGainBalance.TabIndex = 2;
            chkGainBalance.Text = "Corr Amp Balance: 0.00 µV";
            chkGainBalance.UseVisualStyleBackColor = true;
            // 
            // chkDcCorrection
            // 
            chkDcCorrection.AutoSize = true;
            chkDcCorrection.Location = new Point(103, 22);
            chkDcCorrection.Name = "chkDcCorrection";
            chkDcCorrection.Size = new Size(166, 19);
            chkDcCorrection.TabIndex = 1;
            chkDcCorrection.Text = "Corr DC: I=0.0uV, Q=0.0uV";
            chkDcCorrection.UseVisualStyleBackColor = true;
            // 
            // panelControls
            // 
            panelControls.Controls.Add(chkLoopWavPlayback);
            panelControls.Controls.Add(groupBox2);
            panelControls.Controls.Add(groupBox1);
            panelControls.Controls.Add(chkMuteAudioOut);
            panelControls.Controls.Add(cbOutputAudioDeviceList);
            panelControls.Controls.Add(LblOutputAudioDevice);
            panelControls.Controls.Add(progressBarWavPosition);
            panelControls.Controls.Add(LblWAVFilePositionTime);
            panelControls.Controls.Add(btnStopPlayWAVFile);
            panelControls.Controls.Add(btnPause);
            panelControls.Controls.Add(btnOpenWav);
            panelControls.Controls.Add(btnOpenAudioManager);
            panelControls.Controls.Add(btnStopCapture);
            panelControls.Controls.Add(btnStartCapture);
            panelControls.Controls.Add(cbInputAudioDeviceList);
            panelControls.Controls.Add(LblInputAudioDevice);
            panelControls.Dock = DockStyle.Top;
            panelControls.Location = new Point(0, 0);
            panelControls.Margin = new Padding(3, 2, 3, 2);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(909, 217);
            panelControls.TabIndex = 0;
            // 
            // chkLoopWavPlayback
            // 
            chkLoopWavPlayback.AutoSize = true;
            chkLoopWavPlayback.Location = new Point(367, 38);
            chkLoopWavPlayback.Name = "chkLoopWavPlayback";
            chkLoopWavPlayback.Size = new Size(53, 19);
            chkLoopWavPlayback.TabIndex = 8;
            chkLoopWavPlayback.Text = "Loop";
            chkLoopWavPlayback.UseVisualStyleBackColor = true;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(909, 726);
            Controls.Add(panelControls);
            Controls.Add(tabControl1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(920, 600);
            Name = "FrmMain";
            Text = "SDR_DEV_APP v1.0 beta by R9OFG";
            tabControl1.ResumeLayout(false);
            tabPageScope.ResumeLayout(false);
            panelScopeControls.ResumeLayout(false);
            panelScopeControls.PerformLayout();
            groupTrigger.ResumeLayout(false);
            groupTrigger.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudTriggerLevel).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarTimeDiv).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSensitivity).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRef).EndInit();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRange).EndInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotRange).EndInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotTop).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudVolume).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCDecayTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCAttackTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCThreshold).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBw).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPhaseCoeff).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGainRatio).EndInit();
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TabControl tabControl1;
        private TabPage tabPageSpectrum;
        private TabPage tabPageScope;
        private Panel panelScopeControls;
        private TrackBar trackBarSensitivity;
        private Label lblSensitivity_V;
        private CheckBox chkAutoScale;
        private TrackBar trackBarTimeDiv;
        private Label lblTimePerDiv;
        private Label label1;
        private GroupBox groupTrigger;
        private RadioButton rbTriggerQ;
        private RadioButton rbTriggerI;
        private RadioButton rbTriggerFree;
        private NumericUpDown nudTriggerLevel;
        private Label LblTriggerLevel;
        private Label LblInputAudioDevice;
        private ComboBox cbInputAudioDeviceList;
        private Button btnStartCapture;
        private Button btnStopCapture;
        private Button btnOpenAudioManager;
        private Button btnOpenWav;
        private Button btnPause;
        private Button btnStopPlayWAVFile;
        private Label LblWAVFilePositionTime;
        private ProgressBar progressBarWavPosition;
        private Label LblOutputAudioDevice;
        private ComboBox cbOutputAudioDeviceList;
        private CheckBox chkMuteAudioOut;
        private GroupBox groupBox1;
        private CheckBox chkShowCursorInfo;
        private NumericUpDown NudWfColorRef;
        private Label LblWfColorRef;
        private NumericUpDown NudWfColorRange;
        private Label LblWfColorRange;
        private NumericUpDown NudPlotRange;
        private NumericUpDown NudPlotTop;
        private Label LblPlotRange;
        private Label LblPlotTop;
        private ComboBox CbFftSize;
        private Label LblFftSize;
        private GroupBox groupBox2;
        private NumericUpDown nudVolume;
        private Label LblVolume;
        private NumericUpDown nudAGCDecayTime;
        private NumericUpDown nudAGCAttackTime;
        private NumericUpDown nudAGCThreshold;
        private NumericUpDown nudBw;
        private Label LblBW;
        private RadioButton rbFm;
        private RadioButton rbAm;
        private RadioButton rbUsb;
        private RadioButton rbLsb;
        private CheckBox chkDigitalLpf;
        private NumericUpDown nudPhaseCoeff;
        private NumericUpDown nudGainRatio;
        private CheckBox chkPhaseCorrection;
        private CheckBox chkSwapIQ;
        private CheckBox chkGainBalance;
        private CheckBox chkDcCorrection;
        private Panel panelControls;
        private CheckBox chkAGC;
        private CheckBox chkLoopWavPlayback;
    }
}
