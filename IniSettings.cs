/*
 *  IniSettings.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 18-03-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

using System.Reflection;

namespace SDR_DEV_APP
{
    public static class IniSettings
    {
        private static readonly string IniFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            "SDR_DEV_APP.ini"
        );

        private const string AUDIO_SECTION = "Audio";
        private const string CAPTURE_DEVICE_KEY = "SelectedCaptureDevice";
        private const string RENDER_DEVICE_KEY = "SelectedRenderDevice";

        private const string WINDOW_SECTION = "Window";
        private const string LEFT_KEY = "Left";
        private const string TOP_KEY = "Top";
        private const string WIDTH_KEY = "Width";
        private const string HEIGHT_KEY = "Height";
        private const string WINDOW_STATE_KEY = "WindowState";

        private const string SPECTRUM_SECTION = "Spectrum";
        private const string WATERFALL_SECTION = "Waterfall";

        private const string REF_LEVEL_KEY = "RefLevelDB";
        private const string DISPLAY_RANGE_KEY = "DisplayRangeDB";
        private const string FFT_SIZE_KEY = "FftSize";
        private const string FULL_COLOR_KEY = "FullSpectrumColor";
        private const string I_COLOR_KEY = "IChannelColor";
        private const string Q_COLOR_KEY = "QChannelColor";
        private const string SWAP_IQ_KEY = "SwapIQ";

        private const string WF_REF_KEY = "WaterfallColorRefDB";
        private const string WF_RANGE_KEY = "WaterfallColorRangeDB";
        private const string WF_SCROLL_DOWN_KEY = "WaterfallScrollDown";

        private const string CORRECTIONS_SECTION = "Corrections";
        private const string DC_CORRECTION_KEY = "DcCorrectionEnabled";
        private const string GAIN_BALANCE_KEY = "GainBalanceEnabled";
        private const string GAIN_RATIO_KEY = "GainRatio";
        private const string PHASE_CORRECTION_KEY = "PhaseCorrectionEnabled";
        private const string PHASE_COEFF_KEY = "PhaseCoeff";

        private const string FILTERING_SECTION = "Filtering";
        private const string DIGITAL_LPF_KEY = "DigitalLpfEnabled";

        private const string DEMODULATION_SECTION = "Demodulation";
        private const string DEMOD_TYPE_KEY = "DemodType";
        private const string DEMOD_BANDWIDTH_KEY = "DemodBandwidthHz";

        private const string AUDIO_OUTPUT_SECTION = "AudioOutput";
        private const string AGC_ENABLED_KEY = "AGCEnabled";
        private const string AGC_THRESHOLD_KEY = "AGCThreshold";
        private const string AGC_ATTACK_KEY = "AGCAttackTimeMs";
        private const string AGC_DECAY_KEY = "AGCDecayTimeMs";
        private const string VOLUME_KEY = "VolumePercent";

        public static string SelectedAudioDevice { get; set; } = string.Empty;
        public static string SelectedAudioRenderDevice { get; set; } = string.Empty;

        public static int WindowLeft { get; set; } = -1;
        public static int WindowTop { get; set; } = -1;
        public static int WindowWidth { get; set; } = -1;
        public static int WindowHeight { get; set; } = -1;
        public static FormWindowState WindowState { get; set; } = FormWindowState.Normal;

        public static float RefLevelDB { get; set; } = 10.0f;
        public static float DisplayRangeDB { get; set; } = 190.0f;
        public static int FftSize { get; set; } = 8192;

        public static Color FullSpectrumColor { get; set; } = Color.LightSeaGreen;
        public static Color IChannelColor { get; set; } = Color.Lime;
        public static Color QChannelColor { get; set; } = Color.Lime;

        public static float WaterfallColorRefDB { get; set; } = -50.0f;
        public static float WaterfallColorRangeDB { get; set; } = 60.0f;
        public static bool WaterfallScrollDown { get; set; } = true;

        public static bool DcCorrectionEnabled { get; set; } = false;
        public static bool GainBalanceEnabled { get; set; } = false;
        public static float GainRatio { get; set; } = 1.0f;
        public static bool PhaseCorrectionEnabled { get; set; } = false;
        public static float PhaseCoeff { get; set; } = 0.0f;

        public static bool DigitalLpfEnabled { get; set; } = false;

        public static DemodulationType DemodType { get; set; } = DemodulationType.USB;
        public static float DemodBandwidthHz { get; set; } = 2700.0f;

        // === FIX: теперь dB ===
        public static bool AGCEnabled { get; set; } = false;
        public static float AGCTargetLevelDb { get; set; } = -12.0f;
        public static float AGCAttackTimeMs { get; set; } = 10.0f;
        public static float AGCDecayTimeMs { get; set; } = 500.0f;
        public static float VolumePercent { get; set; } = 10.0f;

        public static bool SwapIQ { get; set; } = false;

        private static Color ParseColor(string value, Color defaultColor)
        {
            if (string.IsNullOrEmpty(value)) return defaultColor;
            try
            {
                var parts = value.Split(',');
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0], out byte r) &&
                    byte.TryParse(parts[1], out byte g) &&
                    byte.TryParse(parts[2], out byte b))
                {
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }
            return defaultColor;
        }

        private static DemodulationType ParseDemodType(string value, DemodulationType defaultType)
        {
            return value switch
            {
                "LSB" => DemodulationType.LSB,
                "USB" => DemodulationType.USB,
                "AM" => DemodulationType.AM,
                "FM" => DemodulationType.FM,
                _ => defaultType
            };
        }

        private static string DemodTypeToString(DemodulationType type)
        {
            return type switch
            {
                DemodulationType.LSB => "LSB",
                DemodulationType.USB => "USB",
                DemodulationType.AM => "AM",
                DemodulationType.FM => "FM",
                _ => "USB"
            };
        }

        public static void Load()
        {
            var lines = File.Exists(IniFilePath) ? [.. File.ReadAllLines(IniFilePath)] : new List<string>();
            EnsureHeader(lines);

            // === Window ===
            ReadValue(lines, WINDOW_SECTION, LEFT_KEY, out string leftStr);
            ReadValue(lines, WINDOW_SECTION, TOP_KEY, out string topStr);
            ReadValue(lines, WINDOW_SECTION, WIDTH_KEY, out string widthStr);
            ReadValue(lines, WINDOW_SECTION, HEIGHT_KEY, out string heightStr);
            ReadValue(lines, WINDOW_SECTION, WINDOW_STATE_KEY, out string stateStr);

            WindowLeft = int.TryParse(leftStr, out int l) ? l : -1;
            WindowTop = int.TryParse(topStr, out int t) ? t : -1;
            WindowWidth = int.TryParse(widthStr, out int w) ? w : -1;
            WindowHeight = int.TryParse(heightStr, out int h) ? h : -1;

            WindowState = stateStr switch
            {
                "Maximized" => FormWindowState.Maximized,
                "Minimized" => FormWindowState.Minimized,
                _ => FormWindowState.Normal
            };

            // === Spectrum ===
            ReadValue(lines, SPECTRUM_SECTION, REF_LEVEL_KEY, out string refStr);
            ReadValue(lines, SPECTRUM_SECTION, DISPLAY_RANGE_KEY, out string rangeStr);
            ReadValue(lines, SPECTRUM_SECTION, FFT_SIZE_KEY, out string fftStr);
            ReadValue(lines, SPECTRUM_SECTION, FULL_COLOR_KEY, out string fullColorStr);
            ReadValue(lines, SPECTRUM_SECTION, I_COLOR_KEY, out string iColorStr);
            ReadValue(lines, SPECTRUM_SECTION, Q_COLOR_KEY, out string qColorStr);
            ReadValue(lines, SPECTRUM_SECTION, SWAP_IQ_KEY, out string swapStr);

            RefLevelDB = float.TryParse(refStr, out float rf) ? rf : 10.0f;
            DisplayRangeDB = float.TryParse(rangeStr, out float dr) ? dr : 190.0f;
            FftSize = int.TryParse(fftStr, out int fs) ? fs : 8192;
            FullSpectrumColor = ParseColor(fullColorStr, Color.LightSeaGreen);
            IChannelColor = ParseColor(iColorStr, Color.Lime);
            QChannelColor = ParseColor(qColorStr, Color.Lime);
            SwapIQ = swapStr == "1";

            // === Waterfall ===
            ReadValue(lines, WATERFALL_SECTION, WF_REF_KEY, out string wfRefStr);
            ReadValue(lines, WATERFALL_SECTION, WF_RANGE_KEY, out string wfRangeStr);
            ReadValue(lines, WATERFALL_SECTION, WF_SCROLL_DOWN_KEY, out string wfScrollStr);

            WaterfallColorRefDB = float.TryParse(wfRefStr, out float wfr) ? wfr : -50.0f;
            WaterfallColorRangeDB = float.TryParse(wfRangeStr, out float wfrg) ? wfrg : 60.0f;
            WaterfallScrollDown = wfScrollStr != "0";

            // === Corrections ===
            ReadValue(lines, CORRECTIONS_SECTION, DC_CORRECTION_KEY, out string dcStr);
            ReadValue(lines, CORRECTIONS_SECTION, GAIN_BALANCE_KEY, out string gbStr);
            ReadValue(lines, CORRECTIONS_SECTION, GAIN_RATIO_KEY, out string grStr);
            ReadValue(lines, CORRECTIONS_SECTION, PHASE_CORRECTION_KEY, out string pcStr);
            ReadValue(lines, CORRECTIONS_SECTION, PHASE_COEFF_KEY, out string phStr);

            DcCorrectionEnabled = dcStr == "1";
            GainBalanceEnabled = gbStr == "1";
            GainRatio = float.TryParse(grStr, out float gr) ? gr : 1.0f;
            PhaseCorrectionEnabled = pcStr == "1";
            PhaseCoeff = float.TryParse(phStr, out float ph) ? ph : 0.0f;

            // === Filtering ===
            ReadValue(lines, FILTERING_SECTION, DIGITAL_LPF_KEY, out string lpfStr);
            DigitalLpfEnabled = lpfStr == "1";

            // === Demodulation ===
            ReadValue(lines, DEMODULATION_SECTION, DEMOD_TYPE_KEY, out string demodStr);
            ReadValue(lines, DEMODULATION_SECTION, DEMOD_BANDWIDTH_KEY, out string demodBwStr);

            DemodType = ParseDemodType(demodStr, DemodulationType.USB);
            DemodBandwidthHz = float.TryParse(demodBwStr, out float dbw) ? dbw : 2700.0f;

            // === AudioOutput ===
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_ENABLED_KEY, out string agcStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_THRESHOLD_KEY, out string agcThreshStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_ATTACK_KEY, out string agcAttackStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_DECAY_KEY, out string agcDecayStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, VOLUME_KEY, out string volStr);

            AGCEnabled = agcStr == "1";

            if (float.TryParse(agcThreshStr, out float at))
            {
                if (at > 0.0f && at <= 1.0f)
                {
                    AGCTargetLevelDb = 20.0f * MathF.Log10(at); // старый формат
                }
                else
                {
                    AGCTargetLevelDb = at;
                }
            }
            else
            {
                AGCTargetLevelDb = -12.0f;
            }
            AGCTargetLevelDb = Math.Clamp(AGCTargetLevelDb, -40.0f, 0.0f);

            AGCAttackTimeMs = float.TryParse(agcAttackStr, out float aa) ? aa : 10.0f;
            AGCDecayTimeMs = float.TryParse(agcDecayStr, out float ad) ? ad : 500.0f;
            VolumePercent = float.TryParse(volStr, out float v) ? v : 10.0f;

            // === Devices ===
            ReadValue(lines, AUDIO_SECTION, CAPTURE_DEVICE_KEY, out string device);
            ReadValue(lines, AUDIO_SECTION, RENDER_DEVICE_KEY, out string renderDevice);

            SelectedAudioDevice = device;
            SelectedAudioRenderDevice = renderDevice;
        }

        public static void Save()
        {
            var lines = File.Exists(IniFilePath) ? [.. File.ReadAllLines(IniFilePath)] : new List<string>();
            EnsureHeader(lines);

            // === Window ===
            WriteValue(lines, WINDOW_SECTION, LEFT_KEY, WindowLeft.ToString());
            WriteValue(lines, WINDOW_SECTION, TOP_KEY, WindowTop.ToString());
            WriteValue(lines, WINDOW_SECTION, WIDTH_KEY, WindowWidth.ToString());
            WriteValue(lines, WINDOW_SECTION, HEIGHT_KEY, WindowHeight.ToString());
            WriteValue(lines, WINDOW_SECTION, WINDOW_STATE_KEY, WindowState.ToString());

            // === Spectrum ===
            WriteValue(lines, SPECTRUM_SECTION, REF_LEVEL_KEY, RefLevelDB.ToString("F2"));
            WriteValue(lines, SPECTRUM_SECTION, DISPLAY_RANGE_KEY, DisplayRangeDB.ToString("F2"));
            WriteValue(lines, SPECTRUM_SECTION, FFT_SIZE_KEY, FftSize.ToString());
            WriteValue(lines, SPECTRUM_SECTION, FULL_COLOR_KEY, $"{FullSpectrumColor.R},{FullSpectrumColor.G},{FullSpectrumColor.B}");
            WriteValue(lines, SPECTRUM_SECTION, I_COLOR_KEY, $"{IChannelColor.R},{IChannelColor.G},{IChannelColor.B}");
            WriteValue(lines, SPECTRUM_SECTION, Q_COLOR_KEY, $"{QChannelColor.R},{QChannelColor.G},{QChannelColor.B}");
            WriteValue(lines, SPECTRUM_SECTION, SWAP_IQ_KEY, SwapIQ ? "1" : "0");

            // === Waterfall ===
            WriteValue(lines, WATERFALL_SECTION, WF_REF_KEY, WaterfallColorRefDB.ToString("F2"));
            WriteValue(lines, WATERFALL_SECTION, WF_RANGE_KEY, WaterfallColorRangeDB.ToString("F2"));
            WriteValue(lines, WATERFALL_SECTION, WF_SCROLL_DOWN_KEY, WaterfallScrollDown ? "1" : "0");

            // === Corrections ===
            WriteValue(lines, CORRECTIONS_SECTION, DC_CORRECTION_KEY, DcCorrectionEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, GAIN_BALANCE_KEY, GainBalanceEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, GAIN_RATIO_KEY, GainRatio.ToString("F2"));
            WriteValue(lines, CORRECTIONS_SECTION, PHASE_CORRECTION_KEY, PhaseCorrectionEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, PHASE_COEFF_KEY, PhaseCoeff.ToString("F2"));

            // === Filtering ===
            WriteValue(lines, FILTERING_SECTION, DIGITAL_LPF_KEY, DigitalLpfEnabled ? "1" : "0");

            // === Demodulation ===
            WriteValue(lines, DEMODULATION_SECTION, DEMOD_TYPE_KEY, DemodTypeToString(DemodType));
            WriteValue(lines, DEMODULATION_SECTION, DEMOD_BANDWIDTH_KEY, DemodBandwidthHz.ToString("F2"));

            // === AudioOutput ===
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_ENABLED_KEY, AGCEnabled ? "1" : "0");
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_THRESHOLD_KEY, AGCTargetLevelDb.ToString("F2"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_ATTACK_KEY, AGCAttackTimeMs.ToString("F1"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_DECAY_KEY, AGCDecayTimeMs.ToString("F0"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, VOLUME_KEY, VolumePercent.ToString("F0"));

            // === Devices ===
            WriteValue(lines, AUDIO_SECTION, CAPTURE_DEVICE_KEY, SelectedAudioDevice);
            WriteValue(lines, AUDIO_SECTION, RENDER_DEVICE_KEY, SelectedAudioRenderDevice);

            try
            {
                File.WriteAllLines(IniFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void ReadValue(List<string> lines, string section, string key, out string value)
        {
            value = string.Empty;
            bool inSection = false;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    inSection = trimmed[1..^1].Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                else if (inSection && trimmed.Contains('=') && !trimmed.StartsWith(';'))
                {
                    var parts = trimmed.Split(['='], 2);
                    if (parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = parts[1].Trim(' ', '"');
                        return;
                    }
                }
            }
        }

        private static void WriteValue(List<string> lines, string section, string key, string value)
        {
            bool sectionFound = false;
            bool keyFound = false;
            int insertIndex = lines.Count;

            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    if (trimmed[1..^1].Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionFound = true;
                        insertIndex = i + 1;
                    }
                    else if (sectionFound)
                    {
                        break;
                    }
                }
                else if (sectionFound && trimmed.Contains('=') && !trimmed.StartsWith(';'))
                {
                    var parts = trimmed.Split(['='], 2);
                    if (parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{key}={value}";
                        keyFound = true;
                        break;
                    }
                }
            }

            if (!sectionFound)
            {
                lines.Add($"[{section}]");
                insertIndex = lines.Count;
            }

            if (!keyFound)
            {
                lines.Insert(insertIndex, $"{key}={value}");
            }
        }

        private static void EnsureHeader(List<string> lines)
        {
            if (lines.Count == 0)
            {
                lines.Add("; SDR_DEV_APP settings");
                lines.Add("; https://r9ofg.ru    ");
                lines.Add("");
            }
            else
            {
                if (lines[0] != "; SDR_DEV_APP settings")
                    lines.Insert(0, "; SDR_DEV_APP settings");
                if (lines.Count < 2 || !lines[1].StartsWith("; https://r9ofg.ru    "))
                    lines.Insert(1, "; https://r9ofg.ru    ");
            }
        }
    }
}