/*
 *  IniSettings.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 06-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

using System.Reflection;

namespace SDR_DEV_APP
{
    // Класс для работы с настройками приложения через .ini-файл
    public static class IniSettings
    {
        // Путь к файлу настроек: рядом с исполняемым файлом
        private static readonly string IniFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            "SDR_DEV_APP.ini"
        );

        // Имена секций и ключей в .ini-файле
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

        // Ключи для параметров спектра
        private const string REF_LEVEL_KEY = "RefLevelDB";
        private const string DISPLAY_RANGE_KEY = "DisplayRangeDB";
        private const string FFT_SIZE_KEY = "FftSize";
        private const string FULL_COLOR_KEY = "FullSpectrumColor";
        private const string I_COLOR_KEY = "IChannelColor";
        private const string Q_COLOR_KEY = "QChannelColor";
        private const string SWAP_IQ_KEY = "SwapIQ";

        // Ключи для параметров водопада
        private const string WF_REF_KEY = "WaterfallColorRefDB";
        private const string WF_RANGE_KEY = "WaterfallColorRangeDB";
        private const string WF_SCROLL_DOWN_KEY = "WaterfallScrollDown";

        // Ключи коррекций
        private const string CORRECTIONS_SECTION = "Corrections";
        private const string DC_CORRECTION_KEY = "DcCorrectionEnabled";
        private const string GAIN_BALANCE_KEY = "GainBalanceEnabled";
        private const string GAIN_RATIO_KEY = "GainRatio";
        private const string PHASE_CORRECTION_KEY = "PhaseCorrectionEnabled";
        private const string PHASE_COEFF_KEY = "PhaseCoeff";

        // Ключи фильтра Fs
        private const string FILTERING_SECTION = "Filtering";
        private const string DIGITAL_LPF_KEY = "DigitalLpfEnabled";

        // Ключи демодуляций
        private const string DEMODULATION_SECTION = "Demodulation";
        private const string DEMOD_TYPE_KEY = "DemodType";
        private const string DEMOD_BANDWIDTH_KEY = "DemodBandwidthHz";

        // Ключи аудиовывода
        private const string AUDIO_OUTPUT_SECTION = "AudioOutput";
        private const string AGC_ENABLED_KEY = "AGCEnabled";
        private const string AGC_THRESHOLD_KEY = "AGCThreshold";
        private const string AGC_ATTACK_KEY = "AGCAttackTimeMs";
        private const string AGC_DECAY_KEY = "AGCDecayTimeMs";
        private const string VOLUME_KEY = "VolumePercent";

        // Выбранное аудиоустройство записи (ввод)
        public static string SelectedAudioDevice { get; set; } = string.Empty;

        // Выбранное аудиоустройство воспроизведения (вывод)
        public static string SelectedAudioRenderDevice { get; set; } = string.Empty;

        // Позиция и размеры окна
        public static int WindowLeft { get; set; } = -1;
        public static int WindowTop { get; set; } = -1;
        public static int WindowWidth { get; set; } = -1;
        public static int WindowHeight { get; set; } = -1;
        // Состояние окна (нормальное, развёрнутое и т.д.)
        public static FormWindowState WindowState { get; set; } = FormWindowState.Normal;

        // Уровень опорного сигнала для спектра (в дБ)
        public static float RefLevelDB { get; set; } = 10.0f;
        // Диапазон отображения спектра (в дБ)
        public static float DisplayRangeDB { get; set; } = 190.0f;
        // Размер FFT-буфера
        public static int FftSize { get; set; } = 8192;

        // Цвета отображения спектра
        public static Color FullSpectrumColor { get; set; } = Color.LightSeaGreen;
        public static Color IChannelColor { get; set; } = Color.Lime;
        public static Color QChannelColor { get; set; } = Color.Lime;

        // Уровень опорного цвета водопада (в дБ)
        public static float WaterfallColorRefDB { get; set; } = -50.0f;
        // Диапазон цветовой палитры водопада (в дБ)
        public static float WaterfallColorRangeDB { get; set; } = 60.0f;
        // Направление прокрутки водопада (вниз = true)
        public static bool WaterfallScrollDown { get; set; } = true;

        // Коррекции
        public static bool DcCorrectionEnabled { get; set; } = false;
        public static bool GainBalanceEnabled { get; set; } = false;
        public static float GainRatio { get; set; } = 1.0f;
        public static bool PhaseCorrectionEnabled { get; set; } = false;
        public static float PhaseCoeff { get; set; } = 0.0f;

        // Фильтрация
        public static bool DigitalLpfEnabled { get; set; } = false;

        // Демодуляция
        public static DemodulationType DemodType { get; set; } = DemodulationType.USB;
        public static float DemodBandwidthHz { get; set; } = 2700.0f;

        // Аудиовывод
        public static bool AGCEnabled { get; set; } = false;
        public static float AGCThreshold { get; set; } = 0.7f;
        public static float AGCAttackTimeMs { get; set; } = 1.0f;
        public static float AGCDecayTimeMs { get; set; } = 500.0f;
        public static float VolumePercent { get; set; } = 10.0f;

        // Преобразует строку вида "R,G,B" в объект Color; при ошибке возвращает значение по умолчанию
        private static Color ParseColor(string value, Color defaultColor)
        {
            if (string.IsNullOrEmpty(value)) return defaultColor;
            try
            {
                // Формат: R,G,B (например: 255,255,0)
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

        // Инверсия I/Q
        public static bool SwapIQ { get; set; } = false;

        // Преобразует строку в тип демодуляции
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

        // Преобразует тип демодуляции в строку
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

        // Загружает настройки из .ini-файла
        public static void Load()
        {
            // Читаем строки файла или создаём пустой список, если файла нет
            var lines = File.Exists(IniFilePath) ? [.. File.ReadAllLines(IniFilePath)] : new List<string>();
            // Убеждаемся, что в начале файла есть заголовок
            EnsureHeader(lines);
            // Если заголовок неверный — перезаписываем его
            if (lines.Count >= 2 && (lines[0] != "; SDR_DEV_APP settings" || !lines[1].StartsWith("; https://r9ofg.ru    ")))
            {
                File.WriteAllLines(IniFilePath, lines);
            }

            // Загрузка аудиоустройства записи
            ReadValue(lines, AUDIO_SECTION, CAPTURE_DEVICE_KEY, out string device);
            SelectedAudioDevice = device;

            // Загрузка аудиоустройства воспроизведения ← ДОБАВЛЕНО
            ReadValue(lines, AUDIO_SECTION, RENDER_DEVICE_KEY, out string renderDevice);
            SelectedAudioRenderDevice = renderDevice;

            // Загрузка позиции и размеров окна
            ReadValue(lines, WINDOW_SECTION, LEFT_KEY, out string leftStr);
            ReadValue(lines, WINDOW_SECTION, TOP_KEY, out string topStr);
            ReadValue(lines, WINDOW_SECTION, WIDTH_KEY, out string widthStr);
            ReadValue(lines, WINDOW_SECTION, HEIGHT_KEY, out string heightStr);

            WindowLeft = int.TryParse(leftStr, out int l) ? l : -1;
            WindowTop = int.TryParse(topStr, out int t) ? t : -1;
            WindowWidth = int.TryParse(widthStr, out int w) ? w : -1;
            WindowHeight = int.TryParse(heightStr, out int h) ? h : -1;

            // Загрузка состояния окна
            ReadValue(lines, WINDOW_SECTION, WINDOW_STATE_KEY, out string stateStr);
            WindowState = stateStr switch
            {
                "Maximized" => FormWindowState.Maximized,
                "Minimized" => FormWindowState.Minimized,
                _ => FormWindowState.Normal
            };

            // Загрузка параметров спектра
            ReadValue(lines, SPECTRUM_SECTION, REF_LEVEL_KEY, out string refStr);
            ReadValue(lines, SPECTRUM_SECTION, DISPLAY_RANGE_KEY, out string rangeStr);
            ReadValue(lines, SPECTRUM_SECTION, FFT_SIZE_KEY, out string fftStr);

            RefLevelDB = float.TryParse(refStr, out float r) ? r : 10.0f;
            DisplayRangeDB = float.TryParse(rangeStr, out float d) ? d : 190.0f;
            FftSize = int.TryParse(fftStr, out int f) ? f : 8192;

            // Загрузка цветов спектра
            ReadValue(lines, SPECTRUM_SECTION, FULL_COLOR_KEY, out string fullColorStr);
            ReadValue(lines, SPECTRUM_SECTION, I_COLOR_KEY, out string iColorStr);
            ReadValue(lines, SPECTRUM_SECTION, Q_COLOR_KEY, out string qColorStr);

            FullSpectrumColor = ParseColor(fullColorStr, Color.LightSeaGreen);
            IChannelColor = ParseColor(iColorStr, Color.Lime);
            QChannelColor = ParseColor(qColorStr, Color.Lime);

            // Загрузка параметров водопада
            ReadValue(lines, WATERFALL_SECTION, WF_REF_KEY, out string wfRefStr);
            ReadValue(lines, WATERFALL_SECTION, WF_RANGE_KEY, out string wfRangeStr);
            ReadValue(lines, WATERFALL_SECTION, WF_SCROLL_DOWN_KEY, out string wfScrollStr);

            WaterfallColorRefDB = float.TryParse(wfRefStr, out float wr) ? wr : -50.0f;
            WaterfallColorRangeDB = float.TryParse(wfRangeStr, out float wrg) ? wrg : 60.0f;
            WaterfallScrollDown = string.IsNullOrEmpty(wfScrollStr) || (wfScrollStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                       wfScrollStr.Equals("true", StringComparison.OrdinalIgnoreCase));

            // Загрузка инверсии I/Q
            ReadValue(lines, SPECTRUM_SECTION, SWAP_IQ_KEY, out string swapIqStr);
            SwapIQ = swapIqStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                     swapIqStr.Equals("true", StringComparison.OrdinalIgnoreCase);

            // Загрузка коррекций
            ReadValue(lines, CORRECTIONS_SECTION, DC_CORRECTION_KEY, out string dcCorrStr);
            ReadValue(lines, CORRECTIONS_SECTION, GAIN_BALANCE_KEY, out string gainBalStr);
            ReadValue(lines, CORRECTIONS_SECTION, GAIN_RATIO_KEY, out string gainRatioStr);
            ReadValue(lines, CORRECTIONS_SECTION, PHASE_CORRECTION_KEY, out string phaseCorrStr);
            ReadValue(lines, CORRECTIONS_SECTION, PHASE_COEFF_KEY, out string phaseCoeffStr);

            DcCorrectionEnabled = dcCorrStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                  dcCorrStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            GainBalanceEnabled = gainBalStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                 gainBalStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            GainRatio = float.TryParse(gainRatioStr, out float gr) ? gr : 1.0f;
            PhaseCorrectionEnabled = phaseCorrStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                      phaseCorrStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            PhaseCoeff = float.TryParse(phaseCoeffStr, out float pc) ? pc : 0.0f;

            // Загрузка фильтрации
            ReadValue(lines, FILTERING_SECTION, DIGITAL_LPF_KEY, out string lpfStr);
            DigitalLpfEnabled = lpfStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                lpfStr.Equals("true", StringComparison.OrdinalIgnoreCase);

            // Загрузка демодуляции
            ReadValue(lines, DEMODULATION_SECTION, DEMOD_TYPE_KEY, out string demodTypeStr);
            ReadValue(lines, DEMODULATION_SECTION, DEMOD_BANDWIDTH_KEY, out string demodBwStr);

            DemodType = ParseDemodType(demodTypeStr, DemodulationType.USB);
            DemodBandwidthHz = float.TryParse(demodBwStr, out float bw) ? bw : 2700.0f;

            // Загрузка аудиовывода
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_ENABLED_KEY, out string agcStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_THRESHOLD_KEY, out string agcThreshStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_ATTACK_KEY, out string agcAttackStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, AGC_DECAY_KEY, out string agcDecayStr);
            ReadValue(lines, AUDIO_OUTPUT_SECTION, VOLUME_KEY, out string volStr);

            AGCEnabled = agcStr.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                         agcStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            AGCThreshold = float.TryParse(agcThreshStr, out float at) ? at : 0.7f;
            AGCAttackTimeMs = float.TryParse(agcAttackStr, out float aa) ? aa : 1.0f;
            AGCDecayTimeMs = float.TryParse(agcDecayStr, out float ad) ? ad : 500.0f;
            VolumePercent = float.TryParse(volStr, out float v) ? v : 10.0f;
        }

        // Сохраняет текущие настройки в .ini-файл
        public static void Save()
        {
            var lines = File.Exists(IniFilePath) ? [.. File.ReadAllLines(IniFilePath)] : new List<string>();
            EnsureHeader(lines);

            // Сохранение аудиоустройства записи
            WriteValue(lines, AUDIO_SECTION, CAPTURE_DEVICE_KEY, SelectedAudioDevice);
            // Сохранение аудиоустройства воспроизведения ← ДОБАВЛЕНО
            WriteValue(lines, AUDIO_SECTION, RENDER_DEVICE_KEY, SelectedAudioRenderDevice);

            // Сохранение позиции и размеров окна
            WriteValue(lines, WINDOW_SECTION, LEFT_KEY, WindowLeft.ToString());
            WriteValue(lines, WINDOW_SECTION, TOP_KEY, WindowTop.ToString());
            WriteValue(lines, WINDOW_SECTION, WIDTH_KEY, WindowWidth.ToString());
            WriteValue(lines, WINDOW_SECTION, HEIGHT_KEY, WindowHeight.ToString());

            // Сохранение состояния окна
            string stateStr = WindowState switch
            {
                FormWindowState.Maximized => "Maximized",
                FormWindowState.Minimized => "Minimized",
                _ => "Normal"
            };
            WriteValue(lines, WINDOW_SECTION, WINDOW_STATE_KEY, stateStr);

            // Сохранение параметров спектра
            WriteValue(lines, SPECTRUM_SECTION, REF_LEVEL_KEY, RefLevelDB.ToString("F1"));
            WriteValue(lines, SPECTRUM_SECTION, DISPLAY_RANGE_KEY, DisplayRangeDB.ToString("F1"));
            WriteValue(lines, SPECTRUM_SECTION, FFT_SIZE_KEY, FftSize.ToString());

            // Сохранение цветов спектра в формате R,G,B
            WriteValue(lines, SPECTRUM_SECTION, FULL_COLOR_KEY, $"{FullSpectrumColor.R},{FullSpectrumColor.G},{FullSpectrumColor.B}");
            WriteValue(lines, SPECTRUM_SECTION, I_COLOR_KEY, $"{IChannelColor.R},{IChannelColor.G},{IChannelColor.B}");
            WriteValue(lines, SPECTRUM_SECTION, Q_COLOR_KEY, $"{QChannelColor.R},{QChannelColor.G},{QChannelColor.B}");

            // Сохранение параметров водопада
            WriteValue(lines, WATERFALL_SECTION, WF_REF_KEY, WaterfallColorRefDB.ToString("F1"));
            WriteValue(lines, WATERFALL_SECTION, WF_RANGE_KEY, WaterfallColorRangeDB.ToString("F1"));
            WriteValue(lines, WATERFALL_SECTION, WF_SCROLL_DOWN_KEY, WaterfallScrollDown ? "1" : "0");

            // Сохранение инверсии I/Q
            WriteValue(lines, SPECTRUM_SECTION, SWAP_IQ_KEY, SwapIQ ? "1" : "0");

            // Сохранение коррекций
            WriteValue(lines, CORRECTIONS_SECTION, DC_CORRECTION_KEY, DcCorrectionEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, GAIN_BALANCE_KEY, GainBalanceEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, GAIN_RATIO_KEY, GainRatio.ToString("F4"));
            WriteValue(lines, CORRECTIONS_SECTION, PHASE_CORRECTION_KEY, PhaseCorrectionEnabled ? "1" : "0");
            WriteValue(lines, CORRECTIONS_SECTION, PHASE_COEFF_KEY, PhaseCoeff.ToString("F4"));

            // Сохранение фильтрации
            WriteValue(lines, FILTERING_SECTION, DIGITAL_LPF_KEY, DigitalLpfEnabled ? "1" : "0");

            // Сохранение демодуляции
            WriteValue(lines, DEMODULATION_SECTION, DEMOD_TYPE_KEY, DemodTypeToString(DemodType));
            WriteValue(lines, DEMODULATION_SECTION, DEMOD_BANDWIDTH_KEY, DemodBandwidthHz.ToString("F0"));

            // Сохранение аудиовывода
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_ENABLED_KEY, AGCEnabled ? "1" : "0");
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_THRESHOLD_KEY, AGCThreshold.ToString("F2"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_ATTACK_KEY, AGCAttackTimeMs.ToString("F1"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, AGC_DECAY_KEY, AGCDecayTimeMs.ToString("F0"));
            WriteValue(lines, AUDIO_OUTPUT_SECTION, VOLUME_KEY, VolumePercent.ToString("F0"));

            // Запись всех строк в файл с обработкой ошибок
            try
            {
                File.WriteAllLines(IniFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Ищет значение по секции и ключу в списке строк .ini-файла
        private static void ReadValue(List<string> lines, string section, string key, out string value)
        {
            value = string.Empty;
            bool inSection = false;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // Определяем начало нужной секции
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    inSection = trimmed[1..^1].Equals(section, StringComparison.OrdinalIgnoreCase);
                }
                // Внутри секции ищем строку вида "ключ=значение"
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

        // Обновляет или добавляет значение в указанную секцию .ini-файла
        private static void WriteValue(List<string> lines, string section, string key, string value)
        {
            bool sectionFound = false;
            bool keyFound = false;
            int insertIndex = lines.Count;

            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                // Поиск нужной секции
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    if (trimmed[1..^1].Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionFound = true;
                        insertIndex = i + 1;
                    }
                    else if (sectionFound)
                    {
                        break; // вышли за пределы нужной секции
                    }
                }
                // Поиск существующего ключа внутри секции
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

            // Если секции нет — создаём её в конце
            if (!sectionFound)
            {
                lines.Add($"[{section}]");
                insertIndex = lines.Count;
            }

            // Если ключ не найден — добавляем новую строку
            if (!keyFound)
            {
                lines.Insert(insertIndex, $"{key}={value}");
            }
        }

        // Гарантирует наличие заголовка в начале .ini-файла
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