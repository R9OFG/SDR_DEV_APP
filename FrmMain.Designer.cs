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
            lblSeparatorOscill = new Label();
            trackBarTimeDiv = new TrackBar();
            lblTimePerDiv = new Label();
            chkAutoScale = new CheckBox();
            lblSensitivity_V = new Label();
            trackBarSensitivity = new TrackBar();
            tabPageCAT = new TabPage();
            txtCATTerminalLog = new RichTextBox();
            panelCATControls = new Panel();
            chkCAT_SwapENC = new CheckBox();
            chkCAT_Swap_IQ_Uac = new CheckBox();
            LblCATPHCurrentNote = new Label();
            nudCAT_PH_Corr_Current = new NumericUpDown();
            LblCATAMPCurrentNote = new Label();
            nudCAT_AMP_Corr_Current = new NumericUpDown();
            LblCATSiDriverNote = new Label();
            cmbCAT_SI_Driver_Value = new ComboBox();
            LblCATXTallFreqNote = new Label();
            LblCAT_Xtall_Freq = new Label();
            LblCAT_Mode = new Label();
            cmbCAT_Mode = new ComboBox();
            chkCAT_AGC = new CheckBox();
            chkCAT_PHASE_Corr = new CheckBox();
            chkCAT_AMP_Corr = new CheckBox();
            chkCAT_DC_Corr = new CheckBox();
            chkCAT_TIM8_PWM = new CheckBox();
            btnCAT_SendCommand = new Button();
            tbCAT_Message = new TextBox();
            LblCAT_Command = new Label();
            LblCatComPort = new Label();
            btnCatToggle = new Button();
            cbCatComPorts = new ComboBox();
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
            lblCAT_LO_Freq = new Label();
            groupBox2 = new GroupBox();
            chkAGC = new CheckBox();
            nudVolume = new NumericUpDown();
            LblVolume = new Label();
            nudAGCDecayTime = new NumericUpDown();
            nudAGCAttackTime = new NumericUpDown();
            nudAGCTargetLevelDb = new NumericUpDown();
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
            LblCAT_LO_Freq_Label = new Label();
            chkLoopWavPlayback = new CheckBox();
            chkCAT_TIM15_PWM = new CheckBox();
            tabControl1.SuspendLayout();
            tabPageScope.SuspendLayout();
            panelScopeControls.SuspendLayout();
            groupTrigger.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTriggerLevel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarTimeDiv).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSensitivity).BeginInit();
            tabPageCAT.SuspendLayout();
            panelCATControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudCAT_PH_Corr_Current).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudCAT_AMP_Corr_Current).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRef).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudWfColorRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NudPlotTop).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVolume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCDecayTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCAttackTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAGCTargetLevelDb).BeginInit();
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
            tabControl1.Controls.Add(tabPageCAT);
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
            panelScopeControls.Controls.Add(lblSeparatorOscill);
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
            // lblSeparatorOscill
            // 
            lblSeparatorOscill.BackColor = SystemColors.ActiveBorder;
            lblSeparatorOscill.Location = new Point(136, 9);
            lblSeparatorOscill.Name = "lblSeparatorOscill";
            lblSeparatorOscill.Size = new Size(2, 262);
            lblSeparatorOscill.TabIndex = 3;
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
            // tabPageCAT
            // 
            tabPageCAT.Controls.Add(txtCATTerminalLog);
            tabPageCAT.Controls.Add(panelCATControls);
            tabPageCAT.Location = new Point(4, 24);
            tabPageCAT.Name = "tabPageCAT";
            tabPageCAT.Size = new Size(900, 422);
            tabPageCAT.TabIndex = 2;
            tabPageCAT.Text = "CAT SDR_DEV Board";
            tabPageCAT.UseVisualStyleBackColor = true;
            // 
            // txtCATTerminalLog
            // 
            txtCATTerminalLog.Dock = DockStyle.Fill;
            txtCATTerminalLog.Location = new Point(0, 0);
            txtCATTerminalLog.Name = "txtCATTerminalLog";
            txtCATTerminalLog.Size = new Size(900, 291);
            txtCATTerminalLog.TabIndex = 14;
            txtCATTerminalLog.Text = "";
            // 
            // panelCATControls
            // 
            panelCATControls.Controls.Add(chkCAT_TIM15_PWM);
            panelCATControls.Controls.Add(chkCAT_SwapENC);
            panelCATControls.Controls.Add(chkCAT_Swap_IQ_Uac);
            panelCATControls.Controls.Add(LblCATPHCurrentNote);
            panelCATControls.Controls.Add(nudCAT_PH_Corr_Current);
            panelCATControls.Controls.Add(LblCATAMPCurrentNote);
            panelCATControls.Controls.Add(nudCAT_AMP_Corr_Current);
            panelCATControls.Controls.Add(LblCATSiDriverNote);
            panelCATControls.Controls.Add(cmbCAT_SI_Driver_Value);
            panelCATControls.Controls.Add(LblCATXTallFreqNote);
            panelCATControls.Controls.Add(LblCAT_Xtall_Freq);
            panelCATControls.Controls.Add(LblCAT_Mode);
            panelCATControls.Controls.Add(cmbCAT_Mode);
            panelCATControls.Controls.Add(chkCAT_AGC);
            panelCATControls.Controls.Add(chkCAT_PHASE_Corr);
            panelCATControls.Controls.Add(chkCAT_AMP_Corr);
            panelCATControls.Controls.Add(chkCAT_DC_Corr);
            panelCATControls.Controls.Add(chkCAT_TIM8_PWM);
            panelCATControls.Controls.Add(btnCAT_SendCommand);
            panelCATControls.Controls.Add(tbCAT_Message);
            panelCATControls.Controls.Add(LblCAT_Command);
            panelCATControls.Controls.Add(LblCatComPort);
            panelCATControls.Controls.Add(btnCatToggle);
            panelCATControls.Controls.Add(cbCatComPorts);
            panelCATControls.Dock = DockStyle.Bottom;
            panelCATControls.Location = new Point(0, 291);
            panelCATControls.Name = "panelCATControls";
            panelCATControls.Size = new Size(900, 131);
            panelCATControls.TabIndex = 13;
            // 
            // chkCAT_SwapENC
            // 
            chkCAT_SwapENC.AutoSize = true;
            chkCAT_SwapENC.Location = new Point(210, 50);
            chkCAT_SwapENC.Name = "chkCAT_SwapENC";
            chkCAT_SwapENC.Size = new Size(137, 19);
            chkCAT_SwapENC.TabIndex = 8;
            chkCAT_SwapENC.Text = "Swap Rotate Encoder";
            chkCAT_SwapENC.UseVisualStyleBackColor = true;
            // 
            // chkCAT_Swap_IQ_Uac
            // 
            chkCAT_Swap_IQ_Uac.AutoSize = true;
            chkCAT_Swap_IQ_Uac.Location = new Point(98, 50);
            chkCAT_Swap_IQ_Uac.Name = "chkCAT_Swap_IQ_Uac";
            chkCAT_Swap_IQ_Uac.Size = new Size(102, 19);
            chkCAT_Swap_IQ_Uac.TabIndex = 7;
            chkCAT_Swap_IQ_Uac.Text = "Swap IQ UAC1";
            chkCAT_Swap_IQ_Uac.UseVisualStyleBackColor = true;
            // 
            // LblCATPHCurrentNote
            // 
            LblCATPHCurrentNote.AutoSize = true;
            LblCATPHCurrentNote.Location = new Point(713, 90);
            LblCATPHCurrentNote.Name = "LblCATPHCurrentNote";
            LblCATPHCurrentNote.Size = new Size(95, 15);
            LblCATPHCurrentNote.TabIndex = 22;
            LblCATPHCurrentNote.Text = "Current PH Corr:";
            // 
            // nudCAT_PH_Corr_Current
            // 
            nudCAT_PH_Corr_Current.Location = new Point(814, 86);
            nudCAT_PH_Corr_Current.Name = "nudCAT_PH_Corr_Current";
            nudCAT_PH_Corr_Current.Size = new Size(60, 23);
            nudCAT_PH_Corr_Current.TabIndex = 23;
            // 
            // LblCATAMPCurrentNote
            // 
            LblCATAMPCurrentNote.AutoSize = true;
            LblCATAMPCurrentNote.Location = new Point(530, 90);
            LblCATAMPCurrentNote.Name = "LblCATAMPCurrentNote";
            LblCATAMPCurrentNote.Size = new Size(105, 15);
            LblCATAMPCurrentNote.TabIndex = 20;
            LblCATAMPCurrentNote.Text = "Current AMP Corr:";
            // 
            // nudCAT_AMP_Corr_Current
            // 
            nudCAT_AMP_Corr_Current.Location = new Point(640, 86);
            nudCAT_AMP_Corr_Current.Name = "nudCAT_AMP_Corr_Current";
            nudCAT_AMP_Corr_Current.Size = new Size(60, 23);
            nudCAT_AMP_Corr_Current.TabIndex = 21;
            // 
            // LblCATSiDriverNote
            // 
            LblCATSiDriverNote.AutoSize = true;
            LblCATSiDriverNote.Location = new Point(318, 90);
            LblCATSiDriverNote.Name = "LblCATSiDriverNote";
            LblCATSiDriverNote.Size = new Size(73, 15);
            LblCATSiDriverNote.TabIndex = 18;
            LblCATSiDriverNote.Text = "SI5351 Drive:";
            // 
            // cmbCAT_SI_Driver_Value
            // 
            cmbCAT_SI_Driver_Value.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCAT_SI_Driver_Value.FormattingEnabled = true;
            cmbCAT_SI_Driver_Value.Location = new Point(395, 86);
            cmbCAT_SI_Driver_Value.Name = "cmbCAT_SI_Driver_Value";
            cmbCAT_SI_Driver_Value.Size = new Size(121, 23);
            cmbCAT_SI_Driver_Value.TabIndex = 19;
            // 
            // LblCATXTallFreqNote
            // 
            LblCATXTallFreqNote.AutoSize = true;
            LblCATXTallFreqNote.Location = new Point(100, 89);
            LblCATXTallFreqNote.Name = "LblCATXTallFreqNote";
            LblCATXTallFreqNote.Size = new Size(60, 15);
            LblCATXTallFreqNote.TabIndex = 16;
            LblCATXTallFreqNote.Text = "XTall Freq:";
            // 
            // LblCAT_Xtall_Freq
            // 
            LblCAT_Xtall_Freq.AutoSize = true;
            LblCAT_Xtall_Freq.BackColor = Color.Black;
            LblCAT_Xtall_Freq.BorderStyle = BorderStyle.Fixed3D;
            LblCAT_Xtall_Freq.Font = new Font("Consolas", 18F, FontStyle.Bold, GraphicsUnit.Point, 204);
            LblCAT_Xtall_Freq.ForeColor = Color.Lime;
            LblCAT_Xtall_Freq.Location = new Point(162, 82);
            LblCAT_Xtall_Freq.Name = "LblCAT_Xtall_Freq";
            LblCAT_Xtall_Freq.Size = new Size(144, 30);
            LblCAT_Xtall_Freq.TabIndex = 17;
            LblCAT_Xtall_Freq.Text = "25.000.000";
            // 
            // LblCAT_Mode
            // 
            LblCAT_Mode.AutoSize = true;
            LblCAT_Mode.Location = new Point(722, 50);
            LblCAT_Mode.Name = "LblCAT_Mode";
            LblCAT_Mode.Size = new Size(41, 15);
            LblCAT_Mode.TabIndex = 13;
            LblCAT_Mode.Text = "Mode:";
            // 
            // cmbCAT_Mode
            // 
            cmbCAT_Mode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCAT_Mode.FormattingEnabled = true;
            cmbCAT_Mode.Location = new Point(765, 46);
            cmbCAT_Mode.Name = "cmbCAT_Mode";
            cmbCAT_Mode.Size = new Size(121, 23);
            cmbCAT_Mode.TabIndex = 14;
            // 
            // chkCAT_AGC
            // 
            chkCAT_AGC.AutoSize = true;
            chkCAT_AGC.Location = new Point(670, 50);
            chkCAT_AGC.Name = "chkCAT_AGC";
            chkCAT_AGC.Size = new Size(50, 19);
            chkCAT_AGC.TabIndex = 12;
            chkCAT_AGC.Text = "AGC";
            chkCAT_AGC.UseVisualStyleBackColor = true;
            // 
            // chkCAT_PHASE_Corr
            // 
            chkCAT_PHASE_Corr.AutoSize = true;
            chkCAT_PHASE_Corr.Location = new Point(567, 50);
            chkCAT_PHASE_Corr.Name = "chkCAT_PHASE_Corr";
            chkCAT_PHASE_Corr.Size = new Size(101, 19);
            chkCAT_PHASE_Corr.TabIndex = 11;
            chkCAT_PHASE_Corr.Text = "PH Correction";
            chkCAT_PHASE_Corr.UseVisualStyleBackColor = true;
            // 
            // chkCAT_AMP_Corr
            // 
            chkCAT_AMP_Corr.AutoSize = true;
            chkCAT_AMP_Corr.Location = new Point(456, 50);
            chkCAT_AMP_Corr.Name = "chkCAT_AMP_Corr";
            chkCAT_AMP_Corr.Size = new Size(111, 19);
            chkCAT_AMP_Corr.TabIndex = 10;
            chkCAT_AMP_Corr.Text = "AMP Correction";
            chkCAT_AMP_Corr.UseVisualStyleBackColor = true;
            // 
            // chkCAT_DC_Corr
            // 
            chkCAT_DC_Corr.AutoSize = true;
            chkCAT_DC_Corr.Location = new Point(353, 50);
            chkCAT_DC_Corr.Name = "chkCAT_DC_Corr";
            chkCAT_DC_Corr.Size = new Size(101, 19);
            chkCAT_DC_Corr.TabIndex = 9;
            chkCAT_DC_Corr.Text = "DC Correction";
            chkCAT_DC_Corr.UseVisualStyleBackColor = true;
            // 
            // chkCAT_TIM8_PWM
            // 
            chkCAT_TIM8_PWM.AutoSize = true;
            chkCAT_TIM8_PWM.Location = new Point(9, 50);
            chkCAT_TIM8_PWM.Name = "chkCAT_TIM8_PWM";
            chkCAT_TIM8_PWM.Size = new Size(84, 19);
            chkCAT_TIM8_PWM.TabIndex = 6;
            chkCAT_TIM8_PWM.Text = "TIM8 PWM";
            chkCAT_TIM8_PWM.UseVisualStyleBackColor = true;
            // 
            // btnCAT_SendCommand
            // 
            btnCAT_SendCommand.Location = new Point(774, 13);
            btnCAT_SendCommand.Name = "btnCAT_SendCommand";
            btnCAT_SendCommand.Size = new Size(112, 23);
            btnCAT_SendCommand.TabIndex = 5;
            btnCAT_SendCommand.Text = "Send Command";
            btnCAT_SendCommand.UseVisualStyleBackColor = true;
            // 
            // tbCAT_Message
            // 
            tbCAT_Message.Location = new Point(397, 13);
            tbCAT_Message.Name = "tbCAT_Message";
            tbCAT_Message.Size = new Size(371, 23);
            tbCAT_Message.TabIndex = 4;
            // 
            // LblCAT_Command
            // 
            LblCAT_Command.AutoSize = true;
            LblCAT_Command.Location = new Point(330, 16);
            LblCAT_Command.Name = "LblCAT_Command";
            LblCAT_Command.Size = new Size(67, 15);
            LblCAT_Command.TabIndex = 3;
            LblCAT_Command.Text = "Command:";
            // 
            // LblCatComPort
            // 
            LblCatComPort.AutoSize = true;
            LblCatComPort.Location = new Point(8, 17);
            LblCatComPort.Name = "LblCatComPort";
            LblCatComPort.Size = new Size(63, 15);
            LblCatComPort.TabIndex = 0;
            LblCatComPort.Text = "COM Port:";
            // 
            // btnCatToggle
            // 
            btnCatToggle.Location = new Point(204, 13);
            btnCatToggle.Name = "btnCatToggle";
            btnCatToggle.Size = new Size(112, 23);
            btnCatToggle.TabIndex = 2;
            btnCatToggle.Text = "Connect";
            btnCatToggle.UseVisualStyleBackColor = true;
            // 
            // cbCatComPorts
            // 
            cbCatComPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCatComPorts.FormattingEnabled = true;
            cbCatComPorts.Location = new Point(77, 13);
            cbCatComPorts.Name = "cbCatComPorts";
            cbCatComPorts.Size = new Size(121, 23);
            cbCatComPorts.TabIndex = 1;
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
            groupBox1.Text = " FFT / WaterFall";
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
            // lblCAT_LO_Freq
            // 
            lblCAT_LO_Freq.AutoSize = true;
            lblCAT_LO_Freq.BackColor = Color.Black;
            lblCAT_LO_Freq.BorderStyle = BorderStyle.Fixed3D;
            lblCAT_LO_Freq.Font = new Font("Consolas", 18F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblCAT_LO_Freq.ForeColor = Color.Lime;
            lblCAT_LO_Freq.Location = new Point(753, 62);
            lblCAT_LO_Freq.Name = "lblCAT_LO_Freq";
            lblCAT_LO_Freq.Size = new Size(144, 30);
            lblCAT_LO_Freq.TabIndex = 15;
            lblCAT_LO_Freq.Text = "14.100.000";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkAGC);
            groupBox2.Controls.Add(nudVolume);
            groupBox2.Controls.Add(LblVolume);
            groupBox2.Controls.Add(nudAGCDecayTime);
            groupBox2.Controls.Add(nudAGCAttackTime);
            groupBox2.Controls.Add(nudAGCTargetLevelDb);
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
            nudVolume.Location = new Point(832, 46);
            nudVolume.Name = "nudVolume";
            nudVolume.ReadOnly = true;
            nudVolume.Size = new Size(60, 23);
            nudVolume.TabIndex = 18;
            // 
            // LblVolume
            // 
            LblVolume.AutoSize = true;
            LblVolume.Location = new Point(781, 49);
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
            // nudAGCTargetLevelDb
            // 
            nudAGCTargetLevelDb.Location = new Point(541, 46);
            nudAGCTargetLevelDb.Name = "nudAGCTargetLevelDb";
            nudAGCTargetLevelDb.ReadOnly = true;
            nudAGCTargetLevelDb.Size = new Size(60, 23);
            nudAGCTargetLevelDb.TabIndex = 14;
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
            nudPhaseCoeff.Size = new Size(60, 23);
            nudPhaseCoeff.TabIndex = 5;
            // 
            // nudGainRatio
            // 
            nudGainRatio.Location = new Point(541, 18);
            nudGainRatio.Name = "nudGainRatio";
            nudGainRatio.Size = new Size(60, 23);
            nudGainRatio.TabIndex = 3;
            // 
            // chkPhaseCorrection
            // 
            chkPhaseCorrection.AutoSize = true;
            chkPhaseCorrection.Location = new Point(629, 22);
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
            panelControls.Controls.Add(LblCAT_LO_Freq_Label);
            panelControls.Controls.Add(lblCAT_LO_Freq);
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
            panelControls.Size = new Size(911, 217);
            panelControls.TabIndex = 0;
            // 
            // LblCAT_LO_Freq_Label
            // 
            LblCAT_LO_Freq_Label.AutoSize = true;
            LblCAT_LO_Freq_Label.Location = new Point(696, 69);
            LblCAT_LO_Freq_Label.Name = "LblCAT_LO_Freq_Label";
            LblCAT_LO_Freq_Label.Size = new Size(51, 15);
            LblCAT_LO_Freq_Label.TabIndex = 14;
            LblCAT_LO_Freq_Label.Text = "LO Freq:";
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
            // chkCAT_TIM15_PWM
            // 
            chkCAT_TIM15_PWM.AutoSize = true;
            chkCAT_TIM15_PWM.Location = new Point(8, 89);
            chkCAT_TIM15_PWM.Name = "chkCAT_TIM15_PWM";
            chkCAT_TIM15_PWM.Size = new Size(90, 19);
            chkCAT_TIM15_PWM.TabIndex = 15;
            chkCAT_TIM15_PWM.Text = "TIM15 PWM";
            chkCAT_TIM15_PWM.UseVisualStyleBackColor = true;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(911, 726);
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
            tabPageCAT.ResumeLayout(false);
            panelCATControls.ResumeLayout(false);
            panelCATControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudCAT_PH_Corr_Current).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudCAT_AMP_Corr_Current).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)nudAGCTargetLevelDb).EndInit();
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
        private Label lblSeparatorOscill;
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
        private NumericUpDown nudAGCTargetLevelDb;
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
        private TabPage tabPageCAT;
        private Panel panelCATControls;
        private CheckBox chkCAT_AGC;
        private CheckBox chkCAT_PHASE_Corr;
        private CheckBox chkCAT_AMP_Corr;
        private CheckBox chkCAT_DC_Corr;
        private CheckBox chkCAT_TIM8_PWM;
        private Button btnCAT_SendCommand;
        private TextBox tbCAT_Message;
        private Label LblCAT_Command;
        private Label LblCatComPort;
        private Button btnCatToggle;
        private ComboBox cbCatComPorts;
        private RichTextBox txtCATTerminalLog;
        private Label LblCAT_Mode;
        private ComboBox cmbCAT_Mode;
        private Label lblCAT_LO_Freq;
        private Label LblCAT_LO_Freq_Label;
        private Label LblCATXTallFreqNote;
        private Label LblCAT_Xtall_Freq;
        private Label LblCATSiDriverNote;
        private ComboBox cmbCAT_SI_Driver_Value;
        private NumericUpDown nudCAT_AMP_Corr_Current;
        private Label LblCATAMPCurrentNote;
        private Label LblCATPHCurrentNote;
        private NumericUpDown nudCAT_PH_Corr_Current;
        private CheckBox chkCAT_SwapENC;
        private CheckBox chkCAT_Swap_IQ_Uac;
        private CheckBox chkCAT_TIM15_PWM;
    }
}
