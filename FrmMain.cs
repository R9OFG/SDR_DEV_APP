/*
 *  FrmMain.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 07-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 *  An application for analyzing quadrature I/Q signals received from SDR devices via a sound card or USB Audio Class or WAV files.
 */

using NAudio.CoreAudioApi;
using System.Buffers;
using System.Diagnostics;
using System.Reflection;

[assembly: AssemblyCopyright("© R9OFG 2026")]

namespace SDR_DEV_APP
{
    public partial class FrmMain : Form
    {
        #region Объявления

        #region Основное

        // Глобальные флаги для предотвращения спама MessageBox
        private bool _hasShownSpectrumTimerError = false;
        private bool _hasShownCaptureError = false;

        // Минимальный размер формы приложения
        private const int formWidthMin = 920;
        private const int formHeightMin = 600;
        // Текущий активный источник сигнала(захват с аудиоустройства или воспроизведение WAV файла)
        private ISignalSource? currentSource;
        // Контейнер для всех изменяемых пользователем параметров приложения
        private readonly ChangedParams changedParams = new();
        // Список обнаруженных аудиоустройств записи (ввод) через WASAPI
        private readonly List<MMDevice> captureDevices = [];
        // Список обнаруженных аудиоустройств воспроизведения (вывод) через WASAPI
        private readonly List<MMDevice> renderDevices = [];
        // Текстовая метка для отображения в списках устройств при отсутствии реальных устройств
        private const string NO_DEVICES_TEXT = "(No devices)";
        // Экземпляр аудиовыхода для воспроизведения демодулированного сигнала
        private WasapiAudioOutput? audioOutput;
        // Текущая частота дискретизации активного источника сигнала (в Гц)
        // Обновляется при:
        //   • Старте захвата: currentSampleRate = currentSource.SampleRate (обычно 44100/48000/96000 Гц)
        //   • Открытии WAV: currentSampleRate = wavSource.SampleRate (может быть 8000..192000 Гц)
        // Критически важно:
        //   • Обновлять до запуска источника (иначе первые сэмплы обрабатываются с неверной Fs)
        //   • Синхронизировать со всеми графиками (waterfallView.CurrentSampleRate = ...)
        //   • Использовать в расчётах демодуляции и фильтрации (частота среза зависит от Fs)
        private double currentSampleRate = 48000;
        #endregion

        #region Фильтрация Fs

        // Флаг фильтраций Fs
        private bool isDigitalLpfEnabled = false;
        // Отношение частоты среза к Fs (48% от Найквиста)
        private const float LPF_CUTOFF_RATIO = 0.48f;
        // Порядок КИХ-фильтра (128-1 коэффициентов)
        private const int LPF_ORDER = 127;
        // Коэффициенты фильтра
        private float[] lpfCoeffs;
        // Буфер для канала I
        private float[] filterBufferI;
        // Буфер для канала Q
        private float[] filterBufferQ;
        // Индекс записи в буфер
        private int filterIndex = 0;
        #endregion

        #region Коррекции

        // Флаги коррекций
        private bool isDcCorrectionEnabled = false;
        private bool isGainBalanceEnabled = false;
        private bool isPhaseCorrectionEnabled = false;

        // Поля коррекции DC
        private float currentDcI = 0.0f;
        private float currentDcQ = 0.0f;
        // Поля амплитудной коррекции
        private float gainRatio = 1.0f;
        private float ampImbalanceVolts = 0.0f;
        private float residualImbalanceVolts = 0.0f;
        // Поля фазовой коррекции
        private float phaseCoeff = 0.0f;
        private float phaseErrorDeg = 0.0f;
        #endregion        

        #region Демодуляция

        // Тип демодуляции, перечисление в классе DSP
        private DemodulationType demodType = DemodulationType.USB;

        // Центр полосы относительно нуля (Гц)
        private float demodCenterFreqHz = 0.0f;
        // Ширина полосы (Гц)
        private float demodBandwidthHz = 2700.0f;

        // Стандартные ширины полосы для каждого вида модуляции (Гц)
        private const float DEFAULT_BW_LSB = 2700.0f;
        private const float DEFAULT_BW_USB = 2700.0f;
        private const float DEFAULT_BW_AM = 6000.0f;
        private const float DEFAULT_BW_FM = 12500.0f;
        // Поле для сохранения фазы между вызовами DSP.DemodulateSSB
        private double demodPhaseAccum = 0.0;
        
        // Уровень АРУ
        private float agcGain = 1.0f;
        // Целевой пиковый уровень АРУ
        private float agcTargetLevel = 0.7f;
        // Скорость уменьшения усиления при превышении целевого уровня (в миллисекундах)
        private float agcAttackTimeMs = 1.0f;
        // Скорость увеличения усиления при затухании сигнала (в миллисекундах)
        private float agcDecayTimeMs = 500.0f;
        // Уровень громкости в процентах
        private float volumePercent = 10.0f;
        #endregion

        #region Строка состояния и ее поля

        private readonly StatusStrip? statusStrip;
        // Источник сигнала
        private readonly ToolStripStatusLabel? toolStripStatusLabelSignalSource;
        // Частота дискретизации
        private readonly ToolStripStatusLabel? toolStripStatusLabelSampleRate;
        // Размер буфера
        private readonly ToolStripStatusLabel? toolStripStatusLabelBufferSize;
        // Резервное поле
        private readonly ToolStripStatusLabel? toolStripStatusLabelReserve;
        // Номер текущей сборки приложения
        private readonly ToolStripStatusLabel? toolStripStatusLabelReleaseNumber;
        #endregion

        #region Окно водопада

        private float[]? lastFullSpectrum = null;
        // Контрол окна водопада
        private WaterfallView? waterfallView;
        #endregion

        #region Окна спектров

        // Таймер для периодического обновления спектральных графиков
        private readonly System.Windows.Forms.Timer? spectrumTimer;
        // Защита от накопления тиков + ранний выход + дебаунс меток
        private bool _isSpectrumTickBusy = false;
        private DateTime _lastStatsUpdate = DateTime.MinValue;
        private DateTime _lastWaterfallFrame = DateTime.MinValue;
        // Метки статистики: 5 раз/сек
        private const int StatsUpdateIntervalMs = 200;
        // Водопад: макс 25 кадров/сек
        private const int MinWaterfallFrameIntervalMs = 40;
        // Метка панель комплексного спектра
        private Label? fSpectrumLabel;
        // Контрол окна комплексного спектра
        private ComplexSpectrumView? fullSpectrumView;
        // Метки для окон спектров Real/Q
        private Label? iSpectrumLabel;
        private Label? qSpectrumLabel;
        // Кольцевой буфер для спектров Real I/Q
        private float[] iqBufferI;
        private float[] iqBufferQ;
        private int iqBufferIndex;
        // Контролы окон спектров Real I/Q каналов
        private RealIQSpectrumView? spectrumViewI;
        private RealIQSpectrumView? spectrumViewQ;
        #endregion

        #region Окно осциллографа

        // КЭШ состояния вкладки осциллографа
        private bool isScopeTabActive = false;
        // Кольцевой буфер для осциллографа (без аллокаций в аудио-коллбэке)
        private readonly short[] oscBufferI = new short[65536];
        private readonly short[] oscBufferQ = new short[65536];
        private int oscBufferWriteIndex = 0;
        private int oscBufferReadIndex = 0;
        private readonly object oscBufferLock = new();
        private bool oscilloscopeUpdatePending = false;
        // Для статистики буфера
        private int lastBufferLength = 0;
        // Контрол для отображения временной области сигналов I и Q
        private readonly OscilloscopeView oscilloscopeView;
        // Флаг автоматической подстройки вертикальной развертки
        private bool isOscSensAutoScaling = false;
        // Целевая амплитуда для автоподстройки
        private const float TARGET_PEAK = 32768.0f;
        #endregion
        #endregion

        #region Конструктор
        public FrmMain()
        {
            InitializeComponent();

            // Заголовок приложения
            this.Text = "Quadrature I/Q Signal Analyzer, by R9OFG, beta v1.0, 2026";

            // Минимальный размер окна приложения
            this.MinimumSize = new System.Drawing.Size(formWidthMin, formHeightMin);

            // Всплывающие подсказки
            ToolTip toolTip = new() { AutoPopDelay = 5000, InitialDelay = 1000, ReshowDelay = 500, ShowAlways = true };

            #region Основное
            // Панели контролов
            // Индекс 0 = самый верхний среди приклеенных снизу
            groupBox1.Dock = DockStyle.Bottom;
            panelControls.Controls.SetChildIndex(groupBox1, 0);
            groupBox2.Dock = DockStyle.Bottom;
            panelControls.Controls.SetChildIndex(groupBox2, 1);
            
            // Изменяемые параметры со значениями по умолчанию
            changedParams = new ChangedParams();
            changedParams.SetDefaults();

            // Загружаем настройки из INI в changedParams
            IniSettings.Load();
            // Водопад и спектры
            changedParams.RefLevelDB = IniSettings.RefLevelDB;
            changedParams.DisplayRangeDB = IniSettings.DisplayRangeDB;
            changedParams.FftSize = IniSettings.FftSize;
            changedParams.FullSpectrumColor = IniSettings.FullSpectrumColor;
            changedParams.IChannelColor = IniSettings.IChannelColor;
            changedParams.QChannelColor = IniSettings.QChannelColor;
            changedParams.WaterfallColorRefDB = IniSettings.WaterfallColorRefDB;
            changedParams.WaterfallColorRangeDB = IniSettings.WaterfallColorRangeDB;
            changedParams.WaterfallScrollDown = IniSettings.WaterfallScrollDown;
            changedParams.SwapIQ = IniSettings.SwapIQ;
            // Коррекции
            changedParams.DcCorrectionEnabled = IniSettings.DcCorrectionEnabled;
            changedParams.GainBalanceEnabled = IniSettings.GainBalanceEnabled;
            changedParams.GainRatio = IniSettings.GainRatio;
            changedParams.PhaseCorrectionEnabled = IniSettings.PhaseCorrectionEnabled;
            changedParams.PhaseCoeff = IniSettings.PhaseCoeff;
            // Фильтрация
            changedParams.DigitalLpfEnabled = IniSettings.DigitalLpfEnabled;
            // Демодуляция
            changedParams.DemodType = IniSettings.DemodType;
            changedParams.DemodBandwidthHz = IniSettings.DemodBandwidthHz;
            // Аудиовывод
            changedParams.AGCEnabled = IniSettings.AGCEnabled;
            changedParams.AGCThreshold = IniSettings.AGCThreshold;
            changedParams.AGCAttackTimeMs = IniSettings.AGCAttackTimeMs;
            changedParams.AGCDecayTimeMs = IniSettings.AGCDecayTimeMs;
            changedParams.VolumePercent = IniSettings.VolumePercent;

            // Восстанавливаем сохраненную позицию окна приложения
            RestoreWindowPosition();
            this.FormClosing += FrmMain_FormClosing;
            
            // Заполняем список устройств ввода звука
            LoadAudioCaptureDevices();
            // Восстанавливаем сохраненое значение
            RestoreSelectedAudioCaptureDevice();
            // Подписываемся на события
            cbInputAudioDeviceList.DropDown += OnAudioCaptureDeviceListDropDown;
            cbInputAudioDeviceList.SelectedIndexChanged += OnAudioCaptureDeviceSelectionChanged;

            // Заполняем список устройств вывода звука
            LoadAudioRenderDevices();
            // Восстанавливаем сохраненое значение
            RestoreSelectedAudioRenderDevice();
            // Подписываемся на события
            cbOutputAudioDeviceList.DropDown += OnAudioRenderDeviceListDropDown;
            cbOutputAudioDeviceList.SelectedIndexChanged += OnAudioRenderDeviceSelectionChanged;
            chkMuteAudioOut.CheckedChanged += ChkMuteAudioOut_CheckedChanged;
            
            // Начальное состояние контролов управления
            btnStopCapture.Enabled = false;
            btnPause.Enabled = false;
            btnStopPlayWAVFile.Enabled = false;
            chkLoopWavPlayback.Checked = false;
            progressBarWavPosition.Visible = false;
            LblWAVFilePositionTime.Text = "";
            UpdateStartButtonState();
            chkMuteAudioOut.Enabled = false;
            chkMuteAudioOut.Checked = true;
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;

            // Подписка на обработчики кнопок управления
            btnStartCapture.Click += BtnStartCapture_Click;
            btnStopCapture.Click += BtnStopCapture_Click;
            btnOpenAudioManager.Click += BtnOpenAudioManager_Click;
            btnOpenWav.Click += BtnOpenWav_Click;
            btnPause.Click += BtnPause_Click;
            btnStopPlayWAVFile.Click += BtnStopCapture_Click;
            
            // Обработчик чекбокса зацикливания вопросизведения WAV файла
            chkLoopWavPlayback.CheckedChanged += (s, e) =>
            {
                toolTip.SetToolTip(chkLoopWavPlayback, chkLoopWavPlayback.Checked ? "Loop playback enabled" : "Loop playback disabled");
                if (currentSource is WavSignalSource wavSource)
                {
                    wavSource.Loop = chkLoopWavPlayback.Checked;
                    if (wavSource.IsRunning)
                    {
                        toolStripStatusLabelSignalSource!.Text = chkLoopWavPlayback.Checked
                            ? "Signal Source: WAV file (Loop)"
                            : "Signal Source: WAV file";
                    }
                }
            };

            #endregion

            // ============================================================
            #region Контролы водопада и графиков

            // Допустимые размеры FFT (степени двойки)
            var fftSizes = new[] { 512, 1024, 2048, 4096, 8192, 16384};
            foreach (int size in fftSizes) { CbFftSize.Items.Add(size.ToString()); }
            // Выбираем текущее значение из настроек
            CbFftSize.SelectedItem = changedParams.FftSize.ToString();
            // Событие изменения значения размера FFT
            CbFftSize.SelectedIndexChanged += (sender, e) =>
            {
                if (CbFftSize.SelectedItem is string selectedText &&
                    int.TryParse(selectedText, out int newSize))
                {
                    // Обновляем параметр
                    changedParams.FftSize = newSize;
                    // Пересоздаём буферы
                    iqBufferI = new float[newSize];
                    iqBufferQ = new float[newSize];
                    iqBufferIndex = 0;
                    Array.Clear(iqBufferI, 0, newSize);
                    Array.Clear(iqBufferQ, 0, newSize);
                    // Очищаем историю водопада
                    waterfallView?.ClearHistory();
                    // Адаптация интервала таймера отрисовки спектров под размер FFT
                    AdjustSpectrumTimerInterval(newSize);
                }
            };

            // Нуд верхнего предела графиков спектров в дБ
            NudPlotTop.Minimum = -50;
            NudPlotTop.Maximum = 50;
            NudPlotTop.Increment = 10;
            NudPlotTop.Value = (decimal)changedParams.RefLevelDB;
            // Событие изменения значения нуда верхнего предела графиков в дБ
            NudPlotTop.ValueChanged += (sender, e) =>
            {
                changedParams.RefLevelDB = (float)NudPlotTop.Value;
                fullSpectrumView?.SetData(lastFullSpectrum, changedParams.RefLevelDB, changedParams.DisplayRangeDB);
            };

            // Нуд диапазона графиков в дБ
            NudPlotRange.Minimum = 80;
            NudPlotRange.Maximum = 290;
            NudPlotRange.Increment = 10;
            NudPlotRange.Value = (decimal)changedParams.DisplayRangeDB;
            // Событие изменения значения нуда диапазона графиков в дБ
            NudPlotRange.ValueChanged += (sender, e) =>
            {
                changedParams.DisplayRangeDB = (float)NudPlotRange.Value;
                fullSpectrumView?.SetData(lastFullSpectrum, changedParams.RefLevelDB, changedParams.DisplayRangeDB);
            };

            // Нуд верхнего цветового диапазона водопада в дБ
            NudWfColorRef.Minimum = -50;
            NudWfColorRef.Maximum = 50;
            NudWfColorRef.Increment = 10;
            NudWfColorRef.Value = (decimal)changedParams.WaterfallColorRefDB;
            // Событие изменения нуда верха цветового диапазона водопада в дБ
            NudWfColorRef.ValueChanged += (sender, e) => { changedParams.WaterfallColorRefDB = (float)NudWfColorRef.Value; };
            
            // Нуд цветового диапазона водопада в дБ
            NudWfColorRange.Minimum = 20;
            NudWfColorRange.Maximum = 290;
            NudWfColorRange.Increment = 10;
            NudWfColorRange.Value = (decimal)changedParams.WaterfallColorRangeDB;
            // Событие изменения нуда цветового диапазона водопада в дБ
            NudWfColorRange.ValueChanged += (sender, e) => { changedParams.WaterfallColorRangeDB = (float)NudWfColorRange.Value; };

            // Обработчик изменения состояния чекбокса включения/выключения информации о точке курсора мышки на спектрах
            chkShowCursorInfo.CheckedChanged += (s, e) =>
            {
                bool enabled = chkShowCursorInfo.Checked;
                fullSpectrumView!.ShowCursorTooltip = enabled;
                spectrumViewI!.ShowCursorTooltip = enabled;
                spectrumViewQ!.ShowCursorTooltip = enabled;
                waterfallView!.ShowCursorTooltip = enabled;

                if (!enabled)
                {
                    fullSpectrumView.HideCursorTooltip();
                    spectrumViewI.HideCursorTooltip();
                    spectrumViewQ.HideCursorTooltip();
                    waterfallView.HideCursorTooltip();
                }
            };

            // Осциллограф
            rbTriggerFree.Checked = true;
            // Обработчики переключения режима триггера
            rbTriggerFree.Checked = true;
            rbTriggerFree.CheckedChanged += (s, e) => { if (rbTriggerFree.Checked) oscilloscopeView!.TriggerMode = TriggerMode.None; };
            rbTriggerI.CheckedChanged += (s, e) => { if (rbTriggerI.Checked) oscilloscopeView!.TriggerMode = TriggerMode.IChannel; };
            rbTriggerQ.CheckedChanged += (s, e) => { if (rbTriggerQ.Checked) oscilloscopeView!.TriggerMode = TriggerMode.QChannel; };

            // NumericUpDown для уровня триггера (в вольтах, ±1 В)
            nudTriggerLevel.Minimum = -0.5m;
            nudTriggerLevel.Maximum = 0.5m;
            nudTriggerLevel.Increment = 0.001m;
            nudTriggerLevel.DecimalPlaces = 3;
            nudTriggerLevel.Value = 0.0m;
            // Обработчик изменения уровня триггера
            nudTriggerLevel.ValueChanged += (s, e) =>
            {
                short level = (short)((float)nudTriggerLevel.Value * 32768.0f / 3.3f);
                oscilloscopeView!.TriggerLevel = level;
            };
            #endregion

            // ============================================================
            #region Контрол фильтрации Fs

            // Чекбокс цифрового ФНЧ для Fs
            chkDigitalLpf.Text = "Digital FIR LPF";
            chkDigitalLpf.Checked = false;
            // Обработчик включения/выключения цифрового фильтра
            chkDigitalLpf.CheckedChanged += (s, e) =>
            {
                isDigitalLpfEnabled = chkDigitalLpf.Checked;
                // При включении фильтра — пересчитываем под текущую частоту Fs
                if (isDigitalLpfEnabled && currentSampleRate > 0) RecalculateDigitalLpf(currentSampleRate);
                changedParams.DigitalLpfEnabled = chkDigitalLpf.Checked;
            };

            // Инициализация цифрового фильтра для Fs
            lpfCoeffs = [];
            filterBufferI = [];
            filterBufferQ = [];
            filterIndex = 0;
            #endregion

            // ============================================================
            #region Контролы коррекций

            // Инверсия I/Q
            chkSwapIQ.Checked = changedParams.SwapIQ;
            // Событие инверсии I/Q
            chkSwapIQ.CheckedChanged += (s, e) => { changedParams.SwapIQ = chkSwapIQ.Checked; };

            // Коррекция DC
            chkDcCorrection.Checked = false;
            // Обработчик коррекции DC
            chkDcCorrection.CheckedChanged += (s, e) => { isDcCorrectionEnabled = chkDcCorrection.Checked; changedParams.DcCorrectionEnabled = chkDcCorrection.Checked; };

            // Амплитудная коррекция
            chkGainBalance.Checked = false;
            chkGainBalance.Text = "Corr Amp Balance: 0.00 µV";
            chkGainBalance.ForeColor = System.Drawing.Color.Black;
            chkGainBalance.Checked = false;
            // Обработчик включения/выключения амплитудной коррекции
            chkGainBalance.CheckedChanged += (s, e) =>
            {
                isGainBalanceEnabled = chkGainBalance.Checked;
                nudGainRatio.Enabled = isGainBalanceEnabled;
                changedParams.GainBalanceEnabled = chkGainBalance.Checked;
            };
            // Контрол коэффициента
            nudGainRatio.Minimum = 0.1m;
            nudGainRatio.Maximum = 2.5m;
            nudGainRatio.Increment = 0.001m;
            nudGainRatio.Value = 1.0m;
            nudGainRatio.DecimalPlaces = 3;
            nudGainRatio.Enabled = false;
            // Обработчик изменения значения коэффициента амплитудной коррекции
            nudGainRatio.ValueChanged += (s, e) => { gainRatio = (float)nudGainRatio.Value; changedParams.GainRatio = gainRatio; };
            // Обработчик двойного клика для сброса к умолчанию (только с зажатым Ctrl)
            nudGainRatio.DoubleClick += (s, e) => { if ((Control.ModifierKeys & Keys.Control) != 0) nudGainRatio.Value = 1.0m; changedParams.GainRatio = gainRatio; };

            // Фазовая коррекция
            chkPhaseCorrection.Text = "Corr I/Q Phase Balance: 0.0°";
            chkPhaseCorrection.ForeColor = System.Drawing.Color.Black;
            chkPhaseCorrection.Checked = false;
            // Обработчик включения/выключения фазовой коррекции
            chkPhaseCorrection.CheckedChanged += (s, e) =>
            {
                isPhaseCorrectionEnabled = chkPhaseCorrection.Checked;
                nudPhaseCoeff.Enabled = isPhaseCorrectionEnabled;
                changedParams.PhaseCorrectionEnabled = chkPhaseCorrection.Checked;
            };
            // Контрол коэффициента фазовой коррекции
            nudPhaseCoeff.Minimum = -0.9m;
            nudPhaseCoeff.Maximum = 0.9m;
            nudPhaseCoeff.Increment = 0.0001m;
            nudPhaseCoeff.Value = 0.0m;
            nudPhaseCoeff.DecimalPlaces = 4;
            nudPhaseCoeff.Enabled = false;
            // Обработчик изменения значения коэффициента фазовой коррекции
            nudPhaseCoeff.ValueChanged += (s, e) => { phaseCoeff = (float)nudPhaseCoeff.Value; changedParams.PhaseCoeff = phaseCoeff; };
            // Обработчик двойного клика для сброса к умолчанию (только с зажатым Ctrl)
            nudPhaseCoeff.DoubleClick += (s, e) => { if ((Control.ModifierKeys & Keys.Control) != 0) nudPhaseCoeff.Value = 0.0m; changedParams.PhaseCoeff = phaseCoeff; };
            #endregion

            // ============================================================
            #region Контролы демодуляции

            // Нуд ширина полосы пропускания
            nudBw.Minimum = 100;
            nudBw.Maximum = 12500;
            nudBw.Value = 2700;
            nudBw.Increment = 100;

            // Виды модуляции
            rbLsb.Text = "LSB";
            rbUsb.Text = "USB";
            rbAm.Text = "AM";
            rbFm.Text = "FM";
            // По умолчанию - USB
            rbUsb.Checked = true;

            // Обработчики радиокнопок выбора вида демодуляции
            rbLsb.CheckedChanged += (s, e) =>
            {
                if (rbLsb.Checked)
                {
                    demodType = DemodulationType.LSB;
                    nudBw.Value = (decimal)DEFAULT_BW_LSB;
                    demodBandwidthHz = DEFAULT_BW_LSB;
                    changedParams.DemodType = DemodulationType.LSB;
                    changedParams.DemodBandwidthHz = demodBandwidthHz;

                    if (fullSpectrumView != null)
                    {
                        fullSpectrumView.DemodType = demodType;
                        fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                        fullSpectrumView.Invalidate();
                        UpdateDemodInfoLabel();
                    }
                }
            };

            rbUsb.CheckedChanged += (s, e) =>
            {
                if (rbUsb.Checked)
                {
                    demodType = DemodulationType.USB;
                    nudBw.Value = (decimal)DEFAULT_BW_USB;
                    demodBandwidthHz = DEFAULT_BW_USB;
                    changedParams.DemodType = DemodulationType.USB;
                    changedParams.DemodBandwidthHz = demodBandwidthHz;

                    if (fullSpectrumView != null)
                    {
                        fullSpectrumView.DemodType = demodType;
                        fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                        fullSpectrumView.Invalidate();
                        UpdateDemodInfoLabel();
                    }
                }
            };

            rbAm.CheckedChanged += (s, e) =>
            {
                if (rbAm.Checked)
                {
                    demodType = DemodulationType.AM;
                    nudBw.Value = (decimal)DEFAULT_BW_AM;
                    demodBandwidthHz = DEFAULT_BW_AM;
                    changedParams.DemodType = DemodulationType.AM;
                    changedParams.DemodBandwidthHz = demodBandwidthHz;

                    if (fullSpectrumView != null)
                    {
                        fullSpectrumView.DemodType = demodType;
                        fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                        fullSpectrumView.Invalidate();
                        UpdateDemodInfoLabel();
                    }
                }
            };

            rbFm.CheckedChanged += (s, e) =>
            {
                if (rbFm.Checked)
                {
                    demodType = DemodulationType.FM;
                    nudBw.Value = (decimal)DEFAULT_BW_FM;
                    demodBandwidthHz = DEFAULT_BW_FM;
                    changedParams.DemodType = DemodulationType.FM;
                    changedParams.DemodBandwidthHz = demodBandwidthHz;

                    if (fullSpectrumView != null)
                    {
                        fullSpectrumView.DemodType = demodType;
                        fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                        fullSpectrumView.Invalidate();
                        UpdateDemodInfoLabel();
                    }
                }
            };

            // Обработчик нуда размера полосы пропускания
            nudBw.ValueChanged += (s, e) =>
            {
                demodBandwidthHz = (float)nudBw.Value;
                changedParams.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView!.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView?.Invalidate();
                UpdateDemodInfoLabel();
            };
            #endregion

            // ============================================================
            #region Контролы АРУ и громкости аудио вывода

            // Обработчик включения/выключения АРУ
            chkAGC.CheckedChanged += (s, e) =>
            {
                nudAGCThreshold.Enabled = chkAGC.Checked;
                nudAGCAttackTime.Enabled = chkAGC.Checked;
                nudAGCDecayTime.Enabled = chkAGC.Checked;
                changedParams.AGCEnabled = chkAGC.Checked;
            };
            // Настройка нуда для Threshold
            nudAGCThreshold.DecimalPlaces = 2;
            nudAGCThreshold.Increment = 0.05m;
            nudAGCThreshold.Minimum = 0.01m;
            nudAGCThreshold.Maximum = 1.0m;
            nudAGCThreshold.Value = 0.7m;
            // Обработчик изменения значения нуда
            nudAGCThreshold.ValueChanged += (s, e) => { agcTargetLevel = (float)nudAGCThreshold.Value; changedParams.AGCThreshold = agcTargetLevel; };
            // Обработчик двойного клика для сброса к умолчанию (только с зажатым Ctrl)
            nudAGCThreshold.DoubleClick += (s, e) => { if ((Control.ModifierKeys & Keys.Control) != 0) nudAGCThreshold.Value = 0.7m; };

            // Настройка нуда для Attack Time
            nudAGCAttackTime.DecimalPlaces = 1;
            nudAGCAttackTime.Increment = 0.1m;
            nudAGCAttackTime.Minimum = 0.1m;
            nudAGCAttackTime.Maximum = 100.0m;
            nudAGCAttackTime.Value = 1.0m;
            // Обработчик изменения значения нуда
            nudAGCAttackTime.ValueChanged += (s, e) => { agcAttackTimeMs = (float)nudAGCAttackTime.Value; changedParams.AGCAttackTimeMs = agcAttackTimeMs; };
            // Обработчик двойного клика для сброса к умолчанию (только с зажатым Ctrl)
            nudAGCAttackTime.DoubleClick += (s, e) => { if ((Control.ModifierKeys & Keys.Control) != 0) nudAGCAttackTime.Value = 1.0m; };

            // Настройка нуда для Decay Time
            nudAGCDecayTime.DecimalPlaces = 0;
            nudAGCDecayTime.Increment = 10m;
            nudAGCDecayTime.Minimum = 10m;
            nudAGCDecayTime.Maximum = 5000m;
            nudAGCDecayTime.Value = 500m;
            // Обработчик изменения значения нуда
            nudAGCDecayTime.ValueChanged += (s, e) => { agcDecayTimeMs = (float)nudAGCDecayTime.Value; changedParams.AGCDecayTimeMs = agcDecayTimeMs; };
            // Обработчик двойного клика для сброса к умолчанию (только с зажатым Ctrl)
            nudAGCDecayTime.DoubleClick += (s, e) => { if ((Control.ModifierKeys & Keys.Control) != 0) nudAGCDecayTime.Value = 500m; };

            // Настройка нуда для Volume
            nudVolume.DecimalPlaces = 0;
            nudVolume.Increment = 5m;
            nudVolume.Minimum = 0m;
            nudVolume.Maximum = 100m;
            nudVolume.Value = 10m;
            // Обработчик изменения значения нуда
            nudVolume.ValueChanged += (s, e) => { volumePercent = (float)nudVolume.Value; changedParams.VolumePercent = volumePercent; };
            
            // Всплывающие подсказки на нудах
            toolTip.SetToolTip(nudAGCThreshold, "AGC Threshold");
            toolTip.SetToolTip(nudAGCAttackTime, "AGC Attack Time (ms)");
            toolTip.SetToolTip(nudAGCDecayTime, "AGC Decay Time (ms)");
            toolTip.SetToolTip(nudVolume, "Audio Output Volume (%)");

            // Начальное состояние
            chkAGC.Checked = false;
            nudAGCThreshold.Enabled = false;
            nudAGCAttackTime.Enabled = false;
            nudAGCDecayTime.Enabled = false;
            nudVolume.Enabled = false;
            #endregion

            // ============================================================
            #region Строка состояния

            statusStrip = new StatusStrip { Dock = DockStyle.Bottom, SizingGrip = false, Padding = new Padding(0) };

            // Основные поля (слева направо)
            toolStripStatusLabelSignalSource = new ToolStripStatusLabel
            {
                Text = "Signal Source:",
                Width = 120,
                BorderSides = ToolStripStatusLabelBorderSides.All
            };

            toolStripStatusLabelSampleRate = new ToolStripStatusLabel
            {
                Text = "Sample Rate:",
                Width = 180,
                BorderSides = ToolStripStatusLabelBorderSides.All
            };

            toolStripStatusLabelBufferSize = new ToolStripStatusLabel
            {
                Text = "I/Q:",
                Width = 120,
                BorderSides = ToolStripStatusLabelBorderSides.All
            };

            // Резервное поле — заполняет всё оставшееся пространство + кликабельная ссылка на сайт
            toolStripStatusLabelReserve = new ToolStripStatusLabel
            {
                Spring = true,
                BorderSides = ToolStripStatusLabelBorderSides.All,
                Text = "Description and other info...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = System.Drawing.Color.Blue,
                ToolTipText = "Click to open https://r9ofg.ru"
            };
            // Обработчик клика — открытие сайта в браузере
            toolStripStatusLabelReserve.Click += (s, e) =>
            {
                try
                {
                    // UseShellExecute = true требуется для .NET Core/.NET 5+
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://r9ofg.ru") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to open website:\n{ex.Message}",
                        "SpectrumView — Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
            // Изменение курсора при наведении (опционально, но улучшает UX)
            toolStripStatusLabelReserve.MouseMove += (s, e) => { this.Cursor = Cursors.Hand; };
            toolStripStatusLabelReserve.MouseLeave += (s, e) => { this.Cursor = Cursors.Default; };

            // Поле версии — фиксированной ширины, всегда справа
            toolStripStatusLabelReleaseNumber = new ToolStripStatusLabel
            {
                Text = GetReleaseVersion(),
                Width = 120,
                BorderSides = ToolStripStatusLabelBorderSides.All,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            // Порядок добавления критичен для расположения
            statusStrip.Items.AddRange( [
                toolStripStatusLabelSignalSource,
                toolStripStatusLabelSampleRate,
                toolStripStatusLabelBufferSize,
                toolStripStatusLabelReserve,
                toolStripStatusLabelReleaseNumber
            ]);

            this.Controls.Add(statusStrip);
            #endregion

            // ============================================================
            #region Вкладки спектров и осциллографа

            isScopeTabActive = tabControl1.SelectedTab == tabPageScope;
            // Обработчик изменения активной вкладки — ОБЯЗАТЕЛЬНО в UI-потоке!
            tabControl1.SelectedIndexChanged += (s, e) => { isScopeTabActive = tabControl1.SelectedTab == tabPageScope; };

            // ============================================================
            // Спектры
            // ============================================================
            // Инициализация буферов спектров Real I/Q здесь, после того как fftSize уже известно
            iqBufferI = new float[changedParams.FftSize];
            iqBufferQ = new float[changedParams.FftSize];
            iqBufferIndex = 0;
            // Заполняем нулями
            Array.Clear(iqBufferI, 0, changedParams.FftSize);
            Array.Clear(iqBufferQ, 0, changedParams.FftSize);
            // Инициализации вкладки водопада и спектров
            SetupSpectrumTab();

            // ============================================================
            // Осциллограф
            // ============================================================
            var oscilloscope = new OscilloscopeView { Dock = DockStyle.Fill };
            tabPageScope.Controls.Add(oscilloscope);
            oscilloscopeView = oscilloscope;
            
            // Вертикальная развёртка
            trackBarSensitivity.Minimum = 0;
            trackBarSensitivity.Maximum = 100;
            trackBarSensitivity.Value = 50;
            trackBarSensitivity.SmallChange = 1;
            trackBarSensitivity.LargeChange = 10;
            trackBarSensitivity.TickFrequency = 10;
            // Подписка на событие
            trackBarSensitivity.ValueChanged += TrackBarSensitivity_ValueChanged;
            // Применяем начальное значение
            float initialLogValue = trackBarSensitivity.Value / 100.0f;
            float initialSens = (float)Math.Pow(10, initialLogValue * 5);
            ApplySensitivity(initialSens);
            // Подписка на событие
            chkAutoScale.CheckedChanged += ChkAutoScale_CheckedChanged;
            // Включаем автоподстройку по умолчанию
            chkAutoScale.Checked = true;
            isOscSensAutoScaling = true;
            trackBarSensitivity.Enabled = false;

            // Горизонтальная развёртка (логарифмическая)
            trackBarTimeDiv.Minimum = 0;
            trackBarTimeDiv.Maximum = 100;
            trackBarTimeDiv.Value = 13; // ~250 us/div
            trackBarTimeDiv.SmallChange = 1;
            trackBarTimeDiv.LargeChange = 10;
            trackBarTimeDiv.TickFrequency = 10;
            // Подписка на событие
            trackBarTimeDiv.ValueChanged += TrackBarTimeDiv_ValueChanged;
            // Событие активной вкладки осциллографа
            tabPageScope.Enter += (s, e) =>
            {
                if (oscilloscopeView != null && !oscilloscopeView.IsDisposed)
                {
                    TrackBarTimeDiv_ValueChanged(trackBarTimeDiv, EventArgs.Empty);
                }
            };
            #endregion

            // ============================================================
            #region Таймер спектрального анализа и отрисовки водопада и графиков

            spectrumTimer = new System.Windows.Forms.Timer { Interval = 33 };
            spectrumTimer.Tick += SpectrumTimer_Tick;
            spectrumTimer.Start();
            // Адаптация интервала таймера к текущему размеру FFT
            AdjustSpectrumTimerInterval(changedParams.FftSize);
            #endregion

            // ============================================================
            #region Применяем значения/состояния из настроек

            // Коррекции
            chkDcCorrection.Checked = changedParams.DcCorrectionEnabled;
            chkGainBalance.Checked = changedParams.GainBalanceEnabled;
            nudGainRatio.Value = (decimal)changedParams.GainRatio;
            chkPhaseCorrection.Checked = changedParams.PhaseCorrectionEnabled;
            nudPhaseCoeff.Value = (decimal)changedParams.PhaseCoeff;

            // Фильтрация
            chkDigitalLpf.Checked = changedParams.DigitalLpfEnabled;

            // Демодуляция
            switch (changedParams.DemodType)
            {
                case DemodulationType.LSB: rbLsb.Checked = true; break;
                case DemodulationType.USB: rbUsb.Checked = true; break;
                case DemodulationType.AM: rbAm.Checked = true; break;
                case DemodulationType.FM: rbFm.Checked = true; break;
            }
            nudBw.Value = (decimal)changedParams.DemodBandwidthHz;

            // Аудиовывод
            chkAGC.Checked = changedParams.AGCEnabled;
            nudAGCThreshold.Value = (decimal)changedParams.AGCThreshold;
            nudAGCAttackTime.Value = (decimal)changedParams.AGCAttackTimeMs;
            nudAGCDecayTime.Value = (decimal)changedParams.AGCDecayTimeMs;
            nudVolume.Value = (decimal)changedParams.VolumePercent;
            #endregion

            // Обработчик изменения размеров основного окна приложения
            Resize += (s, e) => LayoutTabControl();
            LayoutTabControl();
        }
        #endregion

        #region Работа с источником Real I/Q сигналов

        // Обработчик поступающих I/Q отсчётов от любого источника (аудиоустройство, WAV и др.)
        private void OnSamplesReceived(float[] iSamples, float[] qSamples)
        {
            if (iSamples.Length == 0) return;

            #region 1. Применяем цифровой ФНЧ к СЫРЫМ сэмплам
            if (isDigitalLpfEnabled && lpfCoeffs != null && lpfCoeffs.Length > 0)
            {
                DSP.ApplyFlatTopLpf(iSamples.AsSpan(), qSamples.AsSpan(), lpfCoeffs, filterBufferI, filterBufferQ, ref filterIndex);
            }
            #endregion
            // =============================================

            #region 2. Применяем свап I/Q (если включён)
            bool swapIQ = chkSwapIQ.Checked;
            Span<float> iSpan = swapIQ ? qSamples.AsSpan() : iSamples.AsSpan();
            Span<float> qSpan = swapIQ ? iSamples.AsSpan() : qSamples.AsSpan();
            #endregion
            // =============================================

            #region 3. Цепочка коррекций

            #region DC коррекция
            var (measuredI, measuredQ) = DSP.MeasureDcOffset(iSpan, qSpan);
            currentDcI = measuredI;
            currentDcQ = measuredQ;
            if (isDcCorrectionEnabled) DSP.ApplyDcCorrection(iSpan, qSpan);
            #endregion

            #region Амплитудная коррекция (ручная)
            // Измеряем разбаланс ДО коррекции (для отображения при выключенной коррекции)
            float iPeak = 0.0f;
            float qPeak = 0.0f;
            for (int i = 0; i < iSpan.Length; i++)
            {
                float absI = Math.Abs(iSpan[i]);
                float absQ = Math.Abs(qSpan[i]);
                if (absI > iPeak) iPeak = absI;
                if (absQ > qPeak) qPeak = absQ;
            }
            ampImbalanceVolts = Math.Abs(iPeak - qPeak);
            // Применяем коррекцию если включена
            if (isGainBalanceEnabled)
            {
                DSP.ApplyGainBalance(qSpan, gainRatio);

                // Измеряем остаточный разбаланс ПОСЛЕ коррекции (для цветовой индикации)
                float iPeakCorr = 0.0f;
                float qPeakCorr = 0.0f;
                for (int i = 0; i < iSpan.Length; i++)
                {
                    float absI = Math.Abs(iSpan[i]);
                    float absQ = Math.Abs(qSpan[i]);
                    if (absI > iPeakCorr) iPeakCorr = absI;
                    if (absQ > qPeakCorr) qPeakCorr = absQ;
                }
                residualImbalanceVolts = Math.Abs(iPeakCorr - qPeakCorr);
            }
            else
            {
                residualImbalanceVolts = ampImbalanceVolts; // для единообразия
            }
            #endregion

            #region Фазовая коррекция (ручная)
            if (isPhaseCorrectionEnabled)
            {
                DSP.ApplyPhaseCorrection(iSpan, qSpan, phaseCoeff);
                // Измеряем ОСТАТОЧНЫЙ дисбаланс ПОСЛЕ коррекции
                var (phaseDeg, _) = DSP.MeasurePhaseError(iSpan, qSpan);
                phaseErrorDeg = phaseDeg;
            }
            else
            {
                // Измеряем дисбаланс без коррекции
                var (phaseDeg, _) = DSP.MeasurePhaseError(iSpan, qSpan);
                phaseErrorDeg = phaseDeg;
            }
            #endregion
            #endregion
            // =============================================

            #region 4. Демодуляция AM/SSB, AGC и вывод звука

            // Демодуляция запускается если аудиовыход не заглушён
            if (audioOutput != null && audioOutput.IsRunning && !chkMuteAudioOut.Checked)
            {
                // Аренда буферов из пула
                float[] tempDemodI = ArrayPool<float>.Shared.Rent(iSpan.Length);
                float[] tempDemodQ = ArrayPool<float>.Shared.Rent(qSpan.Length);
                float[] audioBuffer = ArrayPool<float>.Shared.Rent(iSpan.Length);

                try
                {
                    // tempDemod* полностью перезаписываются — обнуление не нужно
                    iSpan.CopyTo(tempDemodI);
                    qSpan.CopyTo(tempDemodQ);

                    // Обнуляем аудио-буфер перед использованием!
                    Array.Clear(audioBuffer, 0, iSpan.Length);

                    #region Определяем выбранный тип демодуляции

                    switch (demodType)
                    {
                        case DemodulationType.AM:
                            DSP.DemodulateAM(
                                tempDemodI.AsSpan(0, iSpan.Length),
                                tempDemodQ.AsSpan(0, qSpan.Length),
                                (float)currentSampleRate,
                                demodCenterFreqHz,
                                demodBandwidthHz,
                                audioBuffer.AsSpan(0, iSpan.Length), // ← безопасный спан ограниченной длины
                                ref demodPhaseAccum
                            );
                            break;
                        case DemodulationType.LSB:
                        case DemodulationType.USB:
                            DSP.DemodulateSSB(
                                tempDemodI.AsSpan(0, iSpan.Length),
                                tempDemodQ.AsSpan(0, qSpan.Length),
                                (float)currentSampleRate,
                                demodCenterFreqHz,
                                demodBandwidthHz,
                                demodType,
                                audioBuffer.AsSpan(0, iSpan.Length), // ← безопасный спан
                                ref demodPhaseAccum
                            );
                            break;
                        case DemodulationType.FM:
                            DSP.DemodulateFM(
                                tempDemodI.AsSpan(0, iSpan.Length),
                                tempDemodQ.AsSpan(0, qSpan.Length),
                                (float)currentSampleRate,
                                audioBuffer.AsSpan(0, iSpan.Length),
                                lpfCutoff: demodBandwidthHz // используем ширину полосы как частоту среза LPF
                            );
                            break;
                    }
                    #endregion

                    #region Применение AGC и уровня громкости

                    // Безопасная проверка: используем только актуальную длину
                    bool hasAudio = false;
                    for (int i = 0; i < iSpan.Length; i++)
                    {
                        if (audioBuffer[i] != 0.0f)
                        {
                            hasAudio = true;
                            break;
                        }
                    }

                    if (hasAudio)
                    {
                        // 1. Применение AGC (только если чекбокс включён)
                        if (chkAGC.Checked)
                        {
                            float targetLevel = agcTargetLevel;
                            float attackTimeMs = agcAttackTimeMs;
                            float decayTimeMs = agcDecayTimeMs;
                            float sampleRateAgc = (float)currentSampleRate;

                            // Вычисляем пиковый уровень ТОЛЬКО в актуальной части буфера
                            float currentPeak = 0.0f;
                            for (int i = 0; i < iSpan.Length; i++)
                            {
                                float absSample = Math.Abs(audioBuffer[i]);
                                if (absSample > currentPeak) currentPeak = absSample;
                            }

                            float deltaTime = iSpan.Length / sampleRateAgc;
                            float alphaAttack = 1.0f - MathF.Exp(-deltaTime / (attackTimeMs / 1000.0f));
                            float alphaDecay = 1.0f - MathF.Exp(-deltaTime / (decayTimeMs / 1000.0f));

                            float targetGain = 1.0f;
                            if (currentPeak > 0.0f)
                            {
                                targetGain = targetLevel / currentPeak;
                                targetGain = Math.Clamp(targetGain, 0.001f, 100.0f);
                            }

                            if (targetGain < agcGain)
                            {
                                agcGain += alphaAttack * (targetGain - agcGain);
                            }
                            else
                            {
                                agcGain += alphaDecay * (targetGain - agcGain);
                            }

                            // Применяем усиление ТОЛЬКО к актуальной части
                            for (int i = 0; i < iSpan.Length; i++)
                            {
                                audioBuffer[i] *= agcGain;
                            }
                        }

                        // 2. Применение общей громкости

                        // Защита от перегрузки ДО умножения на громкость
                        for (int i = 0; i < audioBuffer.Length; i++)
                        {
                            audioBuffer[i] = Math.Clamp(audioBuffer[i], -3.0f, 3.0f); // tanh эффективен в этом диапазоне
                        }

                        float volumeGain = volumePercent / 100.0f;
                        for (int i = 0; i < iSpan.Length; i++)
                        {
                            audioBuffer[i] *= volumeGain;
                        }

                        // Финальное мягкое ограничение (limiter) + жёсткая защита от клиппинга
                        for (int i = 0; i < audioBuffer.Length; i++)
                        {
                            // 1. Мягкое ограничение через tanh (сохраняет форму сигнала)
                            float limited = MathF.Tanh(audioBuffer[i]);

                            // 2. ЖЁСТКАЯ защита от клиппинга (гарантирует [-1.0, +1.0])
                            audioBuffer[i] = Math.Clamp(limited, -1.0f, 1.0f);
                        }
                    }
                    #endregion

                    // Передаём ТОЛЬКО актуальную часть буфера в аудиовыход
                    if (hasAudio)
                    {
                        audioOutput.WriteSamples(
                            audioBuffer.AsSpan(0, iSpan.Length),
                            audioBuffer.AsSpan(0, iSpan.Length),
                            swapIQ: false
                        );
                    }
                }
                finally
                {
                    // Возврат массивов в пул
                    ArrayPool<float>.Shared.Return(tempDemodI);
                    ArrayPool<float>.Shared.Return(tempDemodQ);
                    ArrayPool<float>.Shared.Return(audioBuffer);
                }
            }
            #endregion
            // =============================================

            #region 5. Для осциллографа: копируем в кольцевой буфер БЕЗ аллокаций
            
            if (isScopeTabActive && oscilloscopeView != null && !oscilloscopeView.IsDisposed)
            {
                lock (oscBufferLock)
                {
                    // Рассчитываем свободное место в кольцевом буфере
                    int spaceAvailable = oscBufferReadIndex > oscBufferWriteIndex
                        ? oscBufferReadIndex - oscBufferWriteIndex - 1
                        : oscBufferI.Length - oscBufferWriteIndex + oscBufferReadIndex - 1;

                    if (spaceAvailable >= iSpan.Length)
                    {
                        // Копируем без переполнения
                        for (int i = 0; i < iSpan.Length; i++)
                        {
                            oscBufferI[oscBufferWriteIndex] = (short)(iSpan[i] * 32767.0f);
                            oscBufferQ[oscBufferWriteIndex] = (short)(qSpan[i] * 32767.0f);
                            oscBufferWriteIndex = (oscBufferWriteIndex + 1) % oscBufferI.Length;
                        }
                        oscilloscopeUpdatePending = true;
                    }
                    // else: буфер переполнен — пропускаем пакет (лучше потерять данные, чем ломать звук)
                }
            }
            #endregion
            // =============================================

            #region 6. Для спектров: заполняем кольцевой буфер float (безопасно при любых размерах пакетов)
            int fftSize = changedParams.FftSize;
            int writePos = iqBufferIndex;

            // === Оптимизация: если пакет больше буфера — оставляем ТОЛЬКО последние данные ===
            if (iSpan.Length > fftSize)
            {
                int skip = iSpan.Length - fftSize;
                iSpan = iSpan[skip..];
                qSpan = qSpan[skip..];
                writePos = 0; // начинаем запись с начала буфера (старые данные полностью перезаписываются)
            }

            // === Копирование с учётом кольцевой структуры ===
            int remaining = fftSize - writePos;
            int toWrite = Math.Min(iSpan.Length, remaining);

            // Первая часть: до конца буфера
            if (toWrite > 0)
            {
                iSpan[..toWrite].CopyTo(iqBufferI.AsSpan(writePos));
                qSpan[..toWrite].CopyTo(iqBufferQ.AsSpan(writePos));
            }

            int remainder = iSpan.Length - toWrite;
            if (remainder > 0)
            {
                // Вторая часть: с начала буфера (гарантированно помещается, т.к. remainder <= fftSize)
                iSpan[toWrite..].CopyTo(iqBufferI.AsSpan(0));
                qSpan[toWrite..].CopyTo(iqBufferQ.AsSpan(0));
                iqBufferIndex = remainder;
            }
            else
            {
                iqBufferIndex = (writePos + toWrite) % fftSize;
            }
            #endregion
            // =============================================

            #region 8. Сохраняем длину буфера для обновления статус-бара (без BeginInvoke!)
            // ← ИСПРАВЛЕНО: убран опасный BeginInvoke из аудио-коллбэка
            lastBufferLength = iSamples.Length;
            #endregion
            // =============================================
        }

        // Сброс состояния обработки
        private void ResetSignalProcessingState()
        {
            // Сброс фазы демодулятора
            demodPhaseAccum = 0.0;

            // Сброс AGC
            agcGain = 1.0f;

            // Сброс индекса кольцевого буфера спектра
            iqBufferIndex = 0;
            Array.Clear(iqBufferI, 0, changedParams.FftSize);
            Array.Clear(iqBufferQ, 0, changedParams.FftSize);

            // Сброс буферов цифрового фильтра (если используется)
            if (isDigitalLpfEnabled && filterBufferI != null && filterBufferQ != null)
            {
                filterIndex = 0;
                Array.Clear(filterBufferI, 0, filterBufferI.Length);
                Array.Clear(filterBufferQ, 0, filterBufferQ.Length);
            }

            // Очистка буфера аудиовывода (если активен)
            audioOutput?.ClearBuffer();
        }

        // Пересчёт коэффициентов цифрового ФНЧ при изменении частоты дискретизации
        private void RecalculateDigitalLpf(double sampleRate)
        {
            // Частота среза = LPF_CUTOFF_RATIO × Найквиста (для защиты от алиасинга)
            float cutoffHz = (float)(sampleRate * LPF_CUTOFF_RATIO);

            // Пересчитываем коэффициенты через DSP, LPF_ORDER - кол-во коэффициентов
            lpfCoeffs = DSP.DesignFlatTopLpf(cutoffHz, (float)sampleRate, LPF_ORDER);

            // Пересоздаём буферы под новый порядок фильтра
            filterBufferI = new float[lpfCoeffs.Length];
            filterBufferQ = new float[lpfCoeffs.Length];
            filterIndex = 0;
            Array.Clear(filterBufferI, 0, filterBufferI.Length);
            Array.Clear(filterBufferQ, 0, filterBufferQ.Length);
        }
        #endregion

        #region Вкладка спектров

        // Настройка вкладки спектров
        // ============================================================
        //   1. Водопад
        //   2. Комплексный спектр (I+jQ) с нулём в центре
        //   3. Два отдельных спектра для Real I/Q каналов в логарифмической шкале частот
        private void SetupSpectrumTab()
        {
            // Основной контейнер — TableLayoutPanel с одной колонкой и тремя строками
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(0), Margin = new Padding(0) };

            // Очищаем и задаём пропорции строк в процентах от высоты вкладки:
            tlp.RowStyles.Clear();
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Waterfall
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Full Spectrum
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Real I/Q Spectra

            // Одна колонка на всю ширину
            tlp.ColumnStyles.Clear();
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // === 1. Панель Waterfall ===
            waterfallView = new WaterfallView
            {
                Dock = DockStyle.Fill,
                CurrentSampleRate = currentSampleRate, // необходимо обновлять при старте захвата
                ScrollDown = changedParams.WaterfallScrollDown
            };
            // Заголовок поверх водопада (прозрачный фон для наложения)
            waterfallView.Controls.Add(new Label
            {
                Text = "Waterfall",
                BackColor = System.Drawing.Color.Transparent,
                AutoSize = true,
                ForeColor = System.Drawing.Color.LightGray,
                Font = new Font("Arial", 8),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3)
            });
            // Событие двойного клика, смена направления водопада
            waterfallView.DoubleClick += (s, e) =>
            {
                // Переключаем направление
                changedParams.WaterfallScrollDown = !changedParams.WaterfallScrollDown;
                // Применяем к контролу
                waterfallView.ScrollDown = changedParams.WaterfallScrollDown;
                // Перерисовываем
                waterfallView.Invalidate();
            };
            // Обычный клик: округление до 100 Гц | Ctrl+клик: округление до 1 кГц
            waterfallView.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                float freq = waterfallView.GetFrequencyAtX(e.X);
                // Округление в зависимости от нажатой клавиши Ctrl
                if ((Control.ModifierKeys & Keys.Control) != 0)
                    freq = MathF.Round(freq / 1000.0f) * 1000.0f; // 1 кГц шаг
                else
                    freq = MathF.Round(freq / 100.0f) * 100.0f;   // 100 Гц шаг
                // Установка f_shift на freq_click (якорь)
                demodCenterFreqHz = freq;
                demodPhaseAccum = 0.0; // сброс фазы ТОЛЬКО при смене частоты
                // Ограничиваем диапазон ±Найквиста
                float nyquist = (float)(currentSampleRate / 2.0);
                demodCenterFreqHz = Math.Clamp(demodCenterFreqHz, -nyquist, nyquist);
                fullSpectrumView!.DemodCenterFreqHz = demodCenterFreqHz;
                fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView.DemodType = demodType;
                fullSpectrumView.Invalidate();
                UpdateDemodInfoLabel();
            };

            // === 2. Панель полного спектра (комплексный FFT с нулём в центре) ===
            var spectrumPanel = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Fill
            };
            // Заголовок над графиком
            fSpectrumLabel = new Label
            {
                Text = "Spectrum",
                AutoSize = true,
                ForeColor = changedParams.FullSpectrumColor,
                Font = new Font("Arial", 8),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(3)
            };
            // Контрол отображения полного спектра
            fullSpectrumView = new ComplexSpectrumView
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Transparent,
                SpectrumColor = changedParams.FullSpectrumColor
            };
            // Событие двойного клика выбора цвета графика
            fullSpectrumView.DoubleClick += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = changedParams.FullSpectrumColor };
                if (dlg.ShowDialog(this) == DialogResult.OK) // ← this как Owner
                {
                    changedParams.FullSpectrumColor = dlg.Color;
                    fullSpectrumView.SpectrumColor = dlg.Color;
                    fSpectrumLabel.ForeColor = dlg.Color;
                    fullSpectrumView.Invalidate();
                }
            };
            // Событие одного клика - перемещение в точку клика якорем индикатора полосы
            // Обычный клик: округление до 100 Гц | Ctrl+клик: округление до 1 кГц
            fullSpectrumView.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                float freq = fullSpectrumView.GetFrequencyAtX(e.X);

                // Округление в зависимости от нажатой клавиши Ctrl
                if ((Control.ModifierKeys & Keys.Control) != 0)
                    freq = MathF.Round(freq / 1000.0f) * 1000.0f; // 1 кГц шаг
                else
                    freq = MathF.Round(freq / 100.0f) * 100.0f;   // 100 Гц шаг

                // Установка f_shift на freq_click (якорь)
                demodCenterFreqHz = freq;
                demodPhaseAccum = 0.0; // сброс фазы ТОЛЬКО при смене частоты
                                       // Ограничиваем диапазон ±Найквиста
                float nyquist = (float)(currentSampleRate / 2.0);
                demodCenterFreqHz = Math.Clamp(demodCenterFreqHz, -nyquist, nyquist);
                fullSpectrumView.DemodCenterFreqHz = demodCenterFreqHz;
                fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView.DemodType = demodType;
                fullSpectrumView.Invalidate();
                UpdateDemodInfoLabel();
            };
            // Обычное вращение колеса мышки: ±100 Гц | Ctrl+вращение: ±500 Гц
            fullSpectrumView.MouseWheel += (s, e) =>
            {
                // Определяем шаг в зависимости от нажатой клавиши Ctrl
                float step = ((Control.ModifierKeys & Keys.Control) != 0) ? 500.0f : 100.0f;
                float delta = e.Delta > 0 ? step : -step;
                demodCenterFreqHz += delta;

                // Ограничиваем диапазон ±Найквиста
                float nyquist = (float)(currentSampleRate / 2.0);
                demodCenterFreqHz = Math.Clamp(demodCenterFreqHz, -nyquist, nyquist);

                // Обновляем отображение
                fullSpectrumView.DemodCenterFreqHz = demodCenterFreqHz;
                fullSpectrumView.Invalidate();

                // Обновляем метку текущего состояния моды и полосы пропускания
                UpdateDemodInfoLabel();
            };

            // Важно: сначала добавляем график, потом заголовок — иначе заголовок закроется графиком
            spectrumPanel.Controls.Add(fullSpectrumView);
            spectrumPanel.Controls.Add(fSpectrumLabel);

            // === 3. Панели для спектров реальных I и Q каналов ===
            var iPanel = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            var qPanel = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Заголовки с цветовой дифференциацией
            iSpectrumLabel = new Label
            {
                Text = "I Channel",
                ForeColor = changedParams.IChannelColor,
                Font = new Font("Arial", 8),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3)
            };

            qSpectrumLabel = new Label
            {
                Text = "Q Channel",
                ForeColor = changedParams.QChannelColor,
                Font = new Font("Arial", 8),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3)
            };

            iPanel.Controls.Add(iSpectrumLabel);
            qPanel.Controls.Add(qSpectrumLabel);

            // Экземпляры пользовательских контроллов для отображения спектров
            spectrumViewI = new RealIQSpectrumView(isI: true)
            {
                Dock = DockStyle.Fill,
                SpectrumColor = changedParams.IChannelColor
            };
            spectrumViewQ = new RealIQSpectrumView(isI: false)
            {
                Dock = DockStyle.Fill,
                SpectrumColor = changedParams.QChannelColor
            };

            // Событие двойного клика выбора цвета графика
            spectrumViewI.DoubleClick += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = changedParams.IChannelColor };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    changedParams.IChannelColor = dlg.Color;
                    spectrumViewI.SpectrumColor = dlg.Color;
                    iSpectrumLabel.ForeColor = dlg.Color;
                    spectrumViewI.Invalidate();
                }
            };

            // Событие двойного клика выбора цвета графика
            spectrumViewQ.DoubleClick += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = changedParams.QChannelColor };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    changedParams.QChannelColor = dlg.Color;
                    spectrumViewQ.SpectrumColor = dlg.Color;
                    qSpectrumLabel.ForeColor = dlg.Color;
                    spectrumViewQ.Invalidate();
                }
            };

            iPanel.Controls.Add(spectrumViewI);
            qPanel.Controls.Add(spectrumViewQ);

            // Горизонтальная компоновка I и Q с зазором 5px между ними
            var iqTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3, // 3 колонки: I | зазор | Q
                Padding = new Padding(3),
                Margin = new Padding(0),
            };

            // Колонки: 50% | 5px (фиксированный зазор) | 50%
            iqTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            iqTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6)); // ← критично: абсолютная ширина 5px
            iqTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            iqTlp.Controls.Add(iPanel, 0, 0);  // I — левая колонка (индекс 0)
            // Средняя колонка (индекс 1) остаётся пустой — создаёт визуальный зазор
            iqTlp.Controls.Add(qPanel, 2, 0);  // Q — правая колонка (индекс 2)

            // === Финальная сборка: добавляем все три секции в основной layout ===
            tlp.Controls.Add(waterfallView, 0, 0); // строка 0
            tlp.Controls.Add(spectrumPanel, 0, 1); // строка 1
            tlp.Controls.Add(iqTlp, 0, 2); // строка 2

            // Привязываем к вкладке
            tabPageSpectrum.Controls.Add(tlp);
        }

        // Действия с панелью вкладок при изменении размеров окна приложения
        private void LayoutTabControl()
        {
            if (tabControl1 != null && statusStrip != null && panelControls != null)
            {
                int statusBarHeight = statusStrip.Height;
                int controlsHeight = panelControls.Height;

                tabControl1.Bounds = new Rectangle(
                    0,
                    controlsHeight,
                    this.ClientSize.Width,
                    Math.Max(0, this.ClientSize.Height - controlsHeight - statusBarHeight)
                );
            }
        }

        // Таймер анализа и отрисовки графиков
        private void SpectrumTimer_Tick(object? sender, EventArgs e)
        {
            // Защита от накопления тиков в очереди сообщений
            if (_isSpectrumTickBusy) return;
            _isSpectrumTickBusy = true;

            try
            {
                #region Ранний выход: проверяем нужные вкладки ДО любых вычислений

                // Выходим сразу если ни спектр, ни осциллограф не активны — экономим ЦП
                if (tabControl1.SelectedTab != tabPageSpectrum && tabControl1.SelectedTab != tabPageScope)
                {
                    _isSpectrumTickBusy = false;
                    return;
                }

                #endregion

                #region Обновление осциллографа из кольцевого буфера

                if (tabControl1.SelectedTab == tabPageScope && oscilloscopeUpdatePending)
                {
                    lock (oscBufferLock)
                    {
                        if (oscBufferWriteIndex == oscBufferReadIndex) // буфер пуст
                        {
                            oscilloscopeUpdatePending = false;
                            _isSpectrumTickBusy = false;
                            return;
                        }

                        int count = oscBufferWriteIndex >= oscBufferReadIndex
                            ? oscBufferWriteIndex - oscBufferReadIndex
                            : oscBufferI.Length - oscBufferReadIndex;

                        // Ограничиваем объём данных для плавной отрисовки
                        count = Math.Min(count, 32768);

                        var iList = new List<short>(count);
                        var qList = new List<short>(count);

                        for (int i = 0; i < count; i++)
                        {
                            int idx = (oscBufferReadIndex + i) % oscBufferI.Length;
                            iList.Add(oscBufferI[idx]);
                            qList.Add(oscBufferQ[idx]);
                        }

                        oscBufferReadIndex = (oscBufferReadIndex + count) % oscBufferI.Length;
                        oscilloscopeUpdatePending = oscBufferReadIndex != oscBufferWriteIndex;

                        // Обновляем осциллограф ВНЕ аудио-коллбэка
                        UpdateOscilloscopeAndDerivedLabels(iList, qList);
                    }
                }

                #endregion

                #region Обновление водопада и спектров (только для вкладки спектра)

                // Прерываемся только при отсутствии данных или неактивном источнике
                if (iqBufferI == null || iqBufferQ == null || currentSource == null || !currentSource.IsRunning)
                {
                    _isSpectrumTickBusy = false;
                    return;
                }

                // === 1. Копируем данные из кольцевого буфера ===
                float[] iSignal = new float[changedParams.FftSize];
                float[] qSignal = new float[changedParams.FftSize];
                int copyIndex = iqBufferIndex;
                for (int i = 0; i < changedParams.FftSize; i++)
                {
                    iSignal[i] = iqBufferI[copyIndex];
                    qSignal[i] = iqBufferQ[copyIndex];
                    copyIndex = (copyIndex + 1) % changedParams.FftSize;
                }

                // === 2. Вычисляем спектры и статистику ===
                // Комплексный спектр
                float[] shifted = DSP.ComputeComplexSpectrum(iSignal, qSignal, changedParams.FftSize);
                lastFullSpectrum = shifted;
                var (fullPeak, fullFreq, fullSNR) = DSP.AnalyzeFullSpectrum(shifted, currentSampleRate);

                // Real I/Q спектры
                float[] iSpectrumDB = DSP.ComputeRealSpectrum(iSignal, changedParams.FftSize);
                float[] qSpectrumDB = DSP.ComputeRealSpectrum(qSignal, changedParams.FftSize);
                var (iPeak, iFreq, iSNR) = DSP.AnalyzeRealSpectrum(iSpectrumDB, currentSampleRate);
                var (qPeak, qFreq, qSNR) = DSP.AnalyzeRealSpectrum(qSpectrumDB, currentSampleRate);

                // === 3. Обновляем статистику меток с дебаунсом (раз в 200 мс) ===
                if ((DateTime.Now - _lastStatsUpdate).TotalMilliseconds >= StatsUpdateIntervalMs)
                {
                    _lastStatsUpdate = DateTime.Now;

                    if (fSpectrumLabel != null && !fSpectrumLabel.IsDisposed && fSpectrumLabel.IsHandleCreated)
                    {
                        string freqText = fullFreq >= 0 ? $"+{fullFreq:F0}" : $"{fullFreq:F0}";
                        fSpectrumLabel.Text = $"Spectrum, Peak: {fullPeak:F1} dB, Freq: {freqText} Hz, SNR: {fullSNR:F1} dB";
                    }
                    if (iSpectrumLabel != null && !iSpectrumLabel.IsDisposed && iSpectrumLabel.IsHandleCreated)
                        iSpectrumLabel.Text = $"I Channel, Peak: {iPeak:F1} dB, Freq: {iFreq:F0} Hz, SNR: {iSNR:F1} dB";
                    if (qSpectrumLabel != null && !qSpectrumLabel.IsDisposed && qSpectrumLabel.IsHandleCreated)
                        qSpectrumLabel.Text = $"Q Channel, Peak: {qPeak:F1} dB, Freq: {qFreq:F0} Hz, SNR: {qSNR:F1} dB";

                    // Обновление текста чекбокса DC коррекции
                    if (chkDcCorrection != null && !chkDcCorrection.IsDisposed && chkDcCorrection.IsHandleCreated)
                    {
                        string iText = FormatVolts(currentDcI);
                        string qText = FormatVolts(currentDcQ);
                        string prefix = isDcCorrectionEnabled ? "Corr DC ✓" : "Corr DC";
                        chkDcCorrection.Text = $"{prefix}: I={iText}, Q={qText}";

                        // Цвет: ЗЕЛЁНЫЙ при включённой коррекции (всегда), иначе красный при смещении > 10 мВ
                        if (isDcCorrectionEnabled)
                            chkDcCorrection.ForeColor = System.Drawing.Color.LimeGreen; // ← ярко-зелёный для лучшей видимости
                        else
                            chkDcCorrection.ForeColor = (Math.Abs(currentDcI) > 0.01f || Math.Abs(currentDcQ) > 0.01f)
                                ? System.Drawing.Color.Red
                                : System.Drawing.Color.Black;
                    }

                    // Обновление текста чекбокса амплитудной коррекции
                    if (chkGainBalance != null && !chkGainBalance.IsDisposed && chkGainBalance.IsHandleCreated)
                    {
                        if (isGainBalanceEnabled)
                        {
                            // При включённой коррекции — показываем ОСТАТОЧНЫЙ разбаланс после коррекции
                            string residualText = FormatVolts(residualImbalanceVolts);
                            chkGainBalance.Text = $"Corr Amp Balance ✓: {residualText}";
                            chkGainBalance.ForeColor = (residualImbalanceVolts <= 0.00005f) // ≤ 50 µV = хорошо
                                ? System.Drawing.Color.LimeGreen
                                : System.Drawing.Color.Red;
                        }
                        else
                        {
                            // При выключенной коррекции — показываем ИСХОДНЫЙ разбаланс до коррекции
                            string imbalanceText = FormatVolts(ampImbalanceVolts);
                            chkGainBalance.Text = $"Corr Amp Balance: {imbalanceText}";
                            chkGainBalance.ForeColor = (ampImbalanceVolts > 0.00005f) // > 50 µV = плохо
                                ? System.Drawing.Color.Red
                                : System.Drawing.Color.Black;
                        }
                    }

                    // Обновление текста чекбокса фазовой коррекции
                    if (chkPhaseCorrection != null && !chkPhaseCorrection.IsDisposed && chkPhaseCorrection.IsHandleCreated)
                    {
                        string sign = phaseErrorDeg >= 0 ? "+" : "";
                        if (isPhaseCorrectionEnabled)
                        {
                            // При включённой коррекции — показываем остаточный дисбаланс после коррекции
                            chkPhaseCorrection.Text = $"Corr I/Q Phase Balance ✓: {sign}{phaseErrorDeg:F2}°";
                            chkPhaseCorrection.ForeColor = (Math.Abs(phaseErrorDeg) <= 0.2f)
                                ? System.Drawing.Color.LimeGreen  // ≤ 0,2° = отлично
                                : System.Drawing.Color.Red;       // > 0,2° = нужно подстроить
                        }
                        else
                        {
                            // При выключенной коррекции — показываем исходный дисбаланс
                            chkPhaseCorrection.Text = $"Corr I/Q Phase Balance: {sign}{phaseErrorDeg:F2}°";
                            chkPhaseCorrection.ForeColor = (Math.Abs(phaseErrorDeg) > 0.2f)
                                ? System.Drawing.Color.Red        // > 0,2° = плохо
                                : System.Drawing.Color.Black;     // ≤ 0,2° = норма
                        }
                    }
                }

                // === 4. Отрисовка графиков только на вкладке спектра ===
                if (tabControl1.SelectedTab != tabPageSpectrum)
                {
                    _isSpectrumTickBusy = false;
                    return;
                }

                // Водопад: добавляем только если источник активен и не в паузе + ограничение 25 FPS
                bool shouldUpdateWaterfall;
                if (currentSource is WavSignalSource wavSource)
                    shouldUpdateWaterfall = wavSource.IsRunning && !wavSource.IsPaused;
                else
                    shouldUpdateWaterfall = currentSource!.IsRunning;

                if (shouldUpdateWaterfall &&
                    (DateTime.Now - _lastWaterfallFrame).TotalMilliseconds >= MinWaterfallFrameIntervalMs)
                {
                    if (waterfallView != null && !waterfallView.IsDisposed && waterfallView.IsHandleCreated)
                    {
                        float wfMinDB = changedParams.WaterfallColorRefDB - changedParams.WaterfallColorRangeDB;
                        float wfMaxDB = changedParams.WaterfallColorRefDB;
                        waterfallView.AddSpectrum(lastFullSpectrum, wfMinDB, wfMaxDB);
                        _lastWaterfallFrame = DateTime.Now;
                    }
                }

                // Обновление самих графиков спектра — только если контрол готов к отрисовке
                if (fullSpectrumView != null && !fullSpectrumView.IsDisposed && fullSpectrumView.IsHandleCreated)
                    fullSpectrumView.SetData(shifted, changedParams.RefLevelDB, changedParams.DisplayRangeDB);
                if (spectrumViewI != null && !spectrumViewI.IsDisposed && spectrumViewI.IsHandleCreated)
                    spectrumViewI.SetData(iSpectrumDB, changedParams.RefLevelDB, changedParams.DisplayRangeDB);
                if (spectrumViewQ != null && !spectrumViewQ.IsDisposed && spectrumViewQ.IsHandleCreated)
                    spectrumViewQ.SetData(qSpectrumDB, changedParams.RefLevelDB, changedParams.DisplayRangeDB);

                // Обновляем статус-бар с информацией о буфере
                if (toolStripStatusLabelBufferSize != null && !this.IsDisposed)
                    toolStripStatusLabelBufferSize.Text = $"I/Q: {lastBufferLength} Samples ({lastBufferLength * 2} Floats)";

                #endregion
            }
            catch (Exception ex)
            {
                // Показываем пользователю ОДИН раз за сессию (защита от спама MessageBox)
                if (!_hasShownSpectrumTimerError)
                {
                    _hasShownSpectrumTimerError = true;
                    MessageBox.Show(
                        $"Spectrum update error:\n{ex.Message}\n\n" +
                        "Graph rendering may be suspended. " +
                        "Try switching tabs or restarting the capture.",
                        "SpectrumView — Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            finally
            {
                _isSpectrumTickBusy = false;
            }
        }

        // Адаптируем частоту обновления спектра под размер FFT
        private void AdjustSpectrumTimerInterval(int fftSize)
        {
            int intervalMs = 133;

            if (fftSize <= 4096) intervalMs = 33;       // ~30 Гц
            else if (fftSize <= 8192) intervalMs = 45;  // ~22 Гц
            else if (fftSize <= 16384) intervalMs = 66; // ~15 Гц

            // Применяем новый интервал БЕЗ пересоздания таймера
            spectrumTimer?.Stop();
            spectrumTimer!.Interval = intervalMs;
            spectrumTimer?.Start();
        }

        // Обновление информационной метки демодуляции в спектре
        private void UpdateDemodInfoLabel()
        {
            if (fullSpectrumView == null) return;

            string mode = demodType switch
            {
                DemodulationType.LSB => "LSB",
                DemodulationType.USB => "USB",
                DemodulationType.AM => "AM",
                DemodulationType.FM => "FM",
                _ => "USB"
            };

            // Форматируем частоту с кратными приставками
            string freqText = FormatFrequency(demodCenterFreqHz);

            fullSpectrumView.DemodInfoText = $"Mode: {mode} | BW: {(int)demodBandwidthHz} Hz | Freq: {freqText}";
            fullSpectrumView.Invalidate();
        }

        #endregion

        #region Вкладка осциллографа
        
        // Обновление меток статистики сигнала на панели осциллографа
        private void UpdateOscilloscopeAndDerivedLabels(List<short> iList, List<short> qList)
        {
            if (oscilloscopeView == null || iList.Count == 0) return;

            // === Амплитуды пиков ===
            short iPeak = (short)iList.Max(Math.Abs);
            short qPeak = (short)qList.Max(Math.Abs);

            // Пик → дБ относительно полной шкалы (32768 = 0 dBFS)
            static float ToDb(short amp) => amp == 0 ? -100.0f : 20.0f * (float)Math.Log10(Math.Abs(amp) / 32768.0f);
            float iPeakDb = ToDb(iPeak);
            float qPeakDb = ToDb(qPeak);

            // Пик → вольты
            float iPeakVolts = Math.Abs(iPeak) / 32768.0f;
            float qPeakVolts = Math.Abs(qPeak) / 32768.0f;

            // RMS → вольты
            double iRms = Math.Sqrt(iList.Average(s => (long)s * s)) / 32768.0;
            double qRms = Math.Sqrt(qList.Average(s => (long)s * s)) / 32768.0;

            // Передаём ВСЮ статистику в осциллограф
            oscilloscopeView.SetStatistics(
                iPeakDb, qPeakDb,
                iPeakVolts, qPeakVolts,  // ← амплитуда в вольтах
                (float)iRms, (float)qRms
            );
            // === Осциллограф ===
            try
            {
                oscilloscopeView.AppendSamples([.. iList], [.. qList]);

                if (oscilloscopeView.InvokeRequired)
                    oscilloscopeView.BeginInvoke(new Action(oscilloscopeView.Refresh));
                else
                    oscilloscopeView.Refresh();

                if (isOscSensAutoScaling)
                {
                    // Вызываем автоподстройку сразу — без задержки
                    UpdateAutoScale();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OSC] Error: {ex.Message}");
            }
        }

        // Вертикальная развертка
        private void ApplySensitivity(float sens)
        {
            oscilloscopeView.Sensitivity = sens;

            float voltsPerDiv = 1.0f / sens;
            lblSensitivity_V.Text = FormatVolts(voltsPerDiv) + "/div";
        }

        // Ползунок вертикальной развертки
        private void TrackBarSensitivity_ValueChanged(object? sender, EventArgs e)
        {
            if (isOscSensAutoScaling) return;

            float logValue = trackBarSensitivity.Value / 100.0f;
            float sens = (float)Math.Pow(10, logValue * 5);
            ApplySensitivity(sens);
        }

        // Чекбокс автоподстройки вертикальной развертки по уровню сигнала
        private void ChkAutoScale_CheckedChanged(object? sender, EventArgs e)
        {
            isOscSensAutoScaling = chkAutoScale.Checked;
            trackBarSensitivity.Enabled = !isOscSensAutoScaling;

            if (isOscSensAutoScaling)
            {
                UpdateAutoScale();
            }
        }

        // Автоподстройка ретикальной развертки по уровню сигнала
        private void UpdateAutoScale()
        {
            if (!isOscSensAutoScaling || oscilloscopeView == null) return;

            var (iPeak, qPeak) = oscilloscopeView.GetPeakAmplitude();
            short peak = Math.Max(iPeak, qPeak);

            if (peak <= 0)
            {
                ApplySensitivity(1.0f);
                return;
            }

            // Сигнал занимает ~70% высоты окна
            const float TARGET_FILL_RATIO = 0.7f; // 70%
            float autoSensitivity = (TARGET_PEAK * TARGET_FILL_RATIO) / peak;
            autoSensitivity = Math.Max(1, Math.Min(100000, autoSensitivity));

            trackBarSensitivity.ValueChanged -= TrackBarSensitivity_ValueChanged;
            float logValue = (float)(Math.Log10(autoSensitivity) / 5.0);
            int trackBarValue = (int)Math.Round(logValue * 100);
            trackBarValue = Math.Max(0, Math.Min(100, trackBarValue));
            trackBarSensitivity.Value = trackBarValue;
            trackBarSensitivity.ValueChanged += TrackBarSensitivity_ValueChanged;

            ApplySensitivity(autoSensitivity);
        }

        // Горизонтальная развертка (логарифмическая, диапазон 100 мкс/дел — 10 мс/дел)
        private void TrackBarTimeDiv_ValueChanged(object? sender, EventArgs e)
        {
            if (oscilloscopeView == null) return;

            // Логарифмический диапазон: 100 мкс (мин) → 10 мс (макс)
            double logMin = Math.Log10(100);     // 100 мкс — минимум
            double logMax = Math.Log10(10_000);  // 10 мс — максимум (10_000 мкс)

            double logValue = logMin + (logMax - logMin) * (trackBarTimeDiv.Value / 100.0);
            double timePerDivUsec = Math.Pow(10, logValue);

            // Общая длительность развёртки = 10 делений × время на деление
            double totalTimeSec = (timePerDivUsec * 10) / 1_000_000.0;
            int samples = (int)Math.Round(totalTimeSec * currentSampleRate);

            // Ограничиваем количество отсчётов буфером осциллографа (65536)
            samples = Math.Max(64, Math.Min(65536, samples));
            oscilloscopeView.Timebase = samples;

            // Форматируем подпись
            string timeText = timePerDivUsec >= 1000
                ? $"{(timePerDivUsec / 1000):F2} ms/div"
                : $"{timePerDivUsec:F0} µs/div";

            lblTimePerDiv.Text = timeText;
        }
        #endregion

        #region Контролы управления
        
        // Заполнение списка аудиоустройств записи
        private void LoadAudioCaptureDevices()
        {
            cbInputAudioDeviceList.Items.Clear();
            captureDevices.Clear();

            var enumerator = new MMDeviceEnumerator();
            int index = 0;
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                captureDevices.Add(device);
                cbInputAudioDeviceList.Items.Add($"[{index}] {device.FriendlyName}");
                index++;
            }

            if (cbInputAudioDeviceList.Items.Count == 0)
                cbInputAudioDeviceList.Items.Add(NO_DEVICES_TEXT);
        }

        // Выбор аудиоустройства записи в списке
        private void OnAudioCaptureDeviceListDropDown(object? sender, EventArgs e)
        {
            string? currentSelection = cbInputAudioDeviceList.SelectedItem?.ToString();
            LoadAudioCaptureDevices();

            if (captureDevices.Count > 0 && !string.IsNullOrEmpty(currentSelection) && currentSelection != NO_DEVICES_TEXT)
            {
                for (int i = 0; i < cbInputAudioDeviceList.Items.Count; i++)
                {
                    if (cbInputAudioDeviceList.Items[i]?.ToString() == currentSelection)
                    {
                        cbInputAudioDeviceList.SelectedIndex = i;
                        UpdateStartButtonState();
                        return;
                    }
                }
            }

            if (captureDevices.Count > 0)
                cbInputAudioDeviceList.SelectedIndex = 0;

            UpdateStartButtonState();
        }

        // Действия при выборе аудиоустройства записи
        private void OnAudioCaptureDeviceSelectionChanged(object? sender, EventArgs e)
        {
            UpdateStartButtonState();
        }

        // Восстановление выбора аудиоустройства записи из ini
        private void RestoreSelectedAudioCaptureDevice()
        {
            if (captureDevices.Count == 0)
            {
                cbInputAudioDeviceList.SelectedIndex = 0;
                return;
            }

            cbInputAudioDeviceList.SelectedIndex = 0;

            string saved = IniSettings.SelectedAudioDevice;
            if (!string.IsNullOrEmpty(saved))
            {
                for (int i = 0; i < cbInputAudioDeviceList.Items.Count; i++)
                {
                    if (cbInputAudioDeviceList.Items[i]?.ToString() == saved)
                    {
                        cbInputAudioDeviceList.SelectedIndex = i;
                        break;
                    }
                }
            }

            UpdateStartButtonState();
        }

        // Обновление статуса кнопки старта захвата
        private void UpdateStartButtonState()
        {
            bool isValid = false;

            if (cbInputAudioDeviceList.SelectedItem is string selectedText)
            {
                if (selectedText != NO_DEVICES_TEXT && captureDevices.Count > 0)
                {
                    int selectedIndex = cbInputAudioDeviceList.SelectedIndex;
                    if (selectedIndex >= 0 && selectedIndex < captureDevices.Count)
                    {
                        isValid = true;
                    }
                }
            }

            btnStartCapture.Enabled = isValid;
        }

        // Кнопка старта захвата аудио данных
        private void BtnStartCapture_Click(object? sender, EventArgs e)
        {
            if (cbInputAudioDeviceList.SelectedIndex < 0 || cbInputAudioDeviceList.SelectedIndex >= captureDevices.Count)
            {
                MessageBox.Show("Select a valid device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var device = captureDevices[cbInputAudioDeviceList.SelectedIndex];
            try
            {
                currentSource = new WasapiSignalSource(device);
                currentSource.SamplesAvailable += OnSamplesReceived;
                // Подписка на ошибки захвата
                currentSource.CaptureError += (msg) =>
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => ShowCaptureError(msg)));
                    else
                        ShowCaptureError(msg);
                };
                currentSource.Start();
                ResetSignalProcessingState();

                // Обновляем частоту дискретизации ВЕЗДЕ
                currentSampleRate = currentSource.SampleRate;
                
                // Пересчет входного КИХ фильтра под Fs истоника
                if (isDigitalLpfEnabled) RecalculateDigitalLpf(currentSampleRate);

                if (tabControl1.SelectedTab == tabPageScope && oscilloscopeView != null)
                {
                    TrackBarTimeDiv_ValueChanged(trackBarTimeDiv, EventArgs.Empty);
                }

                fullSpectrumView!.DemodCenterFreqHz = demodCenterFreqHz;
                fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView.DemodType = demodType;
                // Обновляем метку текущего состояния моды и полосы пропускания
                UpdateDemodInfoLabel();
                // Обновляем водопад, спектры и статус бар
                ApplySampleRateToViews(currentSampleRate);

                // Обновляем строку состояния
                toolStripStatusLabelSignalSource!.Text = $"Signal Source: {cbInputAudioDeviceList.Text}";
                toolStripStatusLabelSampleRate!.Text = $"Sample Rate: {currentSampleRate} Hz";

                btnStartCapture.Enabled = false;
                btnStopCapture.Enabled = true;
                btnOpenAudioManager.Enabled = false;
                cbInputAudioDeviceList.Enabled = false;
                btnOpenWav.Enabled = false;

                chkMuteAudioOut.Enabled = true;

                // Очистка осциллографа и водопада
                oscilloscopeView?.Clear();
                waterfallView?.ClearHistory();
                oscilloscopeView?.Refresh();

                groupBox1.Enabled = true;
                groupBox2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start capture:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentSource = null;
                chkMuteAudioOut.Enabled = false;

                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }

            // Запускаем вывод аудио, если не заглушено и выбрано устройство
            if (!chkMuteAudioOut.Checked && renderDevices.Count > 0 && cbOutputAudioDeviceList.SelectedIndex >= 0)
            {
                try
                {
                    var renderDevice = renderDevices[cbOutputAudioDeviceList.SelectedIndex];
                    audioOutput = new WasapiAudioOutput(renderDevice);
                    audioOutput.Start((int)currentSampleRate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start audio output:\n{ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chkMuteAudioOut.Checked = true;
                    chkMuteAudioOut.Enabled = false;

                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                }
            }
        }

        // Кнопка завершения захвата аудио данных
        private async void BtnStopCapture_Click(object? sender, EventArgs e)
        {
            btnStopCapture.Enabled = false;

            if (currentSource != null)
            {
                currentSource.SamplesAvailable -= OnSamplesReceived;

                if (currentSource is WavSignalSource wavSource)
                {
                    wavSource.PositionChanged -= OnWavPositionChanged;
                    wavSource.PlaybackCompleted -= OnWavPlaybackCompleted;
                }

                var sourceToStop = currentSource;
                currentSource = null;
                
                // Сбрасываем состояние ПЕРЕД остановкой, чтобы избежать гонок
                ResetSignalProcessingState();

                // Останавливаем в фоне, чтобы не блокировать UI
                await Task.Run(() => sourceToStop.Stop());
            }

            // Полностью останавливаем и освобождаем вывод
            audioOutput?.Dispose();
            audioOutput = null;

            // Обновляем UI только после завершения остановки
            Invoke((Action)(() =>
            {
                cbInputAudioDeviceList.Enabled = true;
                btnStopCapture.Enabled = false;
                btnStartCapture.Enabled = true;
                btnOpenAudioManager.Enabled = true;
                btnOpenWav.Enabled = true;
                btnPause.Enabled = false;
                btnStopPlayWAVFile.Enabled = false;

                progressBarWavPosition.Visible = false;
                LblWAVFilePositionTime.Text = "";

                chkMuteAudioOut.Checked = true;
                chkMuteAudioOut.Enabled = false;

                toolStripStatusLabelSignalSource!.Text = "Signal Source:";
                toolStripStatusLabelSampleRate!.Text = "Sample Rate:";
                toolStripStatusLabelBufferSize!.Text = "I/Q:";

                oscilloscopeView?.Clear();
                oscilloscopeView?.Refresh();

                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }));
        }

        // Открывает диспетчер аудиоустройств
        private void BtnOpenAudioManager_Click(object? sender, EventArgs e)
        {
            try
            {
                // Открываем панель управления звуковыми устройствами
                System.Diagnostics.Process.Start("control.exe", "mmsys.cpl");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open audio device manager:\n{ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Обработчик кнопки открытия WAV
        private void BtnOpenWav_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*",
                Title = "Select WAV File with I/Q Data",
                CheckFileExists = true
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Останавливаем и отписываемся от текущего источника (если есть)
            if (currentSource != null)
            {
                // Отписываемся от событий предыдущего WAV-источника
                if (currentSource is WavSignalSource oldWav)
                {
                    oldWav.PositionChanged -= OnWavPositionChanged;
                    oldWav.PlaybackCompleted -= OnWavPlaybackCompleted;
                }
                currentSource.SamplesAvailable -= OnSamplesReceived;
                currentSource.Stop();
                currentSource = null;
            }

            try
            {
                // Создаём новый источник
                var wavSource = new WavSignalSource(dlg.FileName)
                {
                    Loop = chkLoopWavPlayback.Checked
                };

                // Подписка на события
                wavSource.SamplesAvailable += OnSamplesReceived;
                wavSource.PositionChanged += OnWavPositionChanged;
                wavSource.PlaybackCompleted += OnWavPlaybackCompleted;

                progressBarWavPosition.Minimum = 0;
                progressBarWavPosition.Maximum = 100;
                progressBarWavPosition.Value = 0;
                progressBarWavPosition.Visible = true;
                progressBarWavPosition.Style = ProgressBarStyle.Continuous;

                chkMuteAudioOut.Enabled = true;

                groupBox1.Enabled = true;
                groupBox2.Enabled = true;

                // Запускаем источник
                currentSampleRate = wavSource.SampleRate;
                // Подписка на ошибки захвата
                wavSource.CaptureError += (msg) =>
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => ShowCaptureError(msg)));
                    else
                        ShowCaptureError(msg);
                };
                wavSource.Start();
                ResetSignalProcessingState();

                // Запускаем вывод аудио для WAV-файла
                if (!chkMuteAudioOut.Checked && renderDevices.Count > 0 && cbOutputAudioDeviceList.SelectedIndex >= 0)
                {
                    try
                    {
                        var renderDevice = renderDevices[cbOutputAudioDeviceList.SelectedIndex];
                        audioOutput = new WasapiAudioOutput(renderDevice);
                        audioOutput.Start((int)currentSampleRate);
                        chkMuteAudioOut.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to start audio output:\n{ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        chkMuteAudioOut.Checked = true;
                        chkMuteAudioOut.Enabled = false;

                        groupBox1.Enabled = false;
                        groupBox2.Enabled = false;
                    }
                }

                currentSource = wavSource;

                // Обновляем частоту дискретизации
                currentSampleRate = currentSource.SampleRate;

                // Пересчет входного КИХ фильтра под Fs истоника
                if (isDigitalLpfEnabled) RecalculateDigitalLpf(currentSampleRate);

                fullSpectrumView!.DemodCenterFreqHz = demodCenterFreqHz;
                fullSpectrumView.DemodBandwidthHz = demodBandwidthHz;
                fullSpectrumView.DemodType = demodType;
                // Обновляем метку текущего состояния моды и полосы пропускания
                UpdateDemodInfoLabel();
                // Обновляем водопад, спектры и статусбар
                ApplySampleRateToViews(currentSampleRate);

                // Обновляем строку состояния
                if (!chkLoopWavPlayback.Checked)
                    toolStripStatusLabelSignalSource!.Text = $"Signal Source: WAV file";
                else
                    toolStripStatusLabelSignalSource!.Text = $"Signal Source: WAV file (Loop)";
                toolStripStatusLabelSampleRate!.Text = $"Sample Rate: {currentSampleRate} Hz";
                UpdateWavTimeLabel(wavSource, 0.0); // инициализируем метку времени

                // Блокируем управление аудиоустройством
                btnStartCapture.Enabled = false;
                btnStopCapture.Enabled = false;
                btnOpenAudioManager.Enabled = false;
                cbInputAudioDeviceList.Enabled = false;
                btnOpenWav.Enabled = false;
                btnPause.Enabled = true;
                btnPause.Text = "Pause";
                btnStopPlayWAVFile.Enabled = true;

                // Очистка графиков
                oscilloscopeView?.Clear();
                waterfallView?.ClearHistory();
                oscilloscopeView?.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open WAV file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Восстанавливаем управление аудиоустройством
                UpdateStartButtonState();
                cbInputAudioDeviceList.Enabled = true;
                btnOpenAudioManager.Enabled = true;
                btnOpenWav.Enabled = true;
                btnPause.Enabled = false;
                btnStopPlayWAVFile.Enabled = false;

                progressBarWavPosition.Visible = false;
                LblWAVFilePositionTime.Text = "";

                toolStripStatusLabelSignalSource!.Text = "Signal Source:";
                toolStripStatusLabelSampleRate!.Text = "Sample Rate:";
                toolStripStatusLabelBufferSize!.Text = "I/Q:";

                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }
        }

        // Обработчик кнопки паузы/возобновления
        private void BtnPause_Click(object? sender, EventArgs e)
        {
            if (currentSource == null) return;

            // WAV-файл поддерживает настоящую паузу
            if (currentSource is WavSignalSource wavSource)
            {
                if (wavSource.IsPaused)
                {
                    wavSource.Resume();
                    btnPause.Text = "Pause";
                    toolStripStatusLabelSignalSource!.Text = $"Signal Source: WAV file";

                    chkMuteAudioOut.Enabled = true;

                    if (wavSource.IsPaused && audioOutput != null)
                    {
                        // Очищаем буфер вывода, чтобы избежать "хвоста" звука после паузы
                        audioOutput.ClearBuffer();
                    }
                }
                else
                {
                    wavSource.Pause();
                    btnPause.Text = "▶ Resume";
                    toolStripStatusLabelSignalSource!.Text = $"Signal Source: WAV file (Paused)";

                    chkMuteAudioOut.Enabled = false;
                }
            }
        }

        // Обновление прогресса воспроизведения
        private void OnWavPositionChanged(double percent)
        {
            if (progressBarWavPosition == null || progressBarWavPosition.IsDisposed) return;

            int value = (int)Math.Round(percent * 100);
            value = Math.Max(progressBarWavPosition.Minimum, Math.Min(progressBarWavPosition.Maximum, value));

            if (progressBarWavPosition.InvokeRequired)
            {
                progressBarWavPosition.Invoke(new Action(() =>
                {
                    if (!progressBarWavPosition.IsDisposed)
                        progressBarWavPosition.Value = value;
                }));
            }
            else if (!progressBarWavPosition.IsDisposed)
            {
                progressBarWavPosition.Value = value;
            }

            // Обновляем время в метке
            if (currentSource is WavSignalSource wavSource)
                UpdateWavTimeLabel(wavSource, percent);
        }

        // Обновление метки времени
        private void UpdateWavTimeLabel(WavSignalSource source, double percent)
        {
            if (LblWAVFilePositionTime == null || LblWAVFilePositionTime.IsDisposed)
                return;

            double currentSec = source.TotalDurationSeconds * percent;
            double totalSec = source.TotalDurationSeconds;

            string currentTime = TimeSpan.FromSeconds(currentSec).ToString(@"mm\:ss");
            string totalTime = TimeSpan.FromSeconds(totalSec).ToString(@"mm\:ss");
            string newText = $"Current: {currentTime} / Total: {totalTime}";

            // Потокобезопасное обновление текста метки
            if (LblWAVFilePositionTime.InvokeRequired)
            {
                try
                {
                    LblWAVFilePositionTime.Invoke(new Action(() =>
                    {
                        if (!LblWAVFilePositionTime.IsDisposed)
                            LblWAVFilePositionTime.Text = newText;
                    }));
                }
                catch (ObjectDisposedException) { /* Игнорируем — форма закрывается */ }
                catch (InvalidOperationException) { /* Игнорируем — контрол уничтожен */ }
            }
            else
            {
                LblWAVFilePositionTime.Text = newText;
            }
        }

        // Обработчик завершения воспроизведения
        private void OnWavPlaybackCompleted()
        {
            // При зацикливании это событие НЕ вызывается — задача продолжает работать
            // Поэтому здесь только обычная остановка

            audioOutput?.Stop();
            audioOutput?.Dispose();
            audioOutput = null;

            chkMuteAudioOut.Enabled = false;
            progressBarWavPosition.Visible = false;
            progressBarWavPosition.Value = 0;
            LblWAVFilePositionTime.Text = "";

            // Восстанавливаем управление аудиоустройством
            UpdateStartButtonState();
            cbInputAudioDeviceList.Enabled = true;
            btnOpenAudioManager.Enabled = true;
            btnOpenWav.Enabled = true;
            btnPause.Enabled = false;
            btnStopPlayWAVFile.Enabled = false;

            toolStripStatusLabelSignalSource!.Text = "Signal Source:";
            toolStripStatusLabelSampleRate!.Text = "Sample Rate:";
            toolStripStatusLabelBufferSize!.Text = "I/Q:";

            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
        }

        // Заполнение списка аудиоустройств воспроизведения
        // Отвечает ТОЛЬКО за обновление списка и валидный SelectedIndex
        private void LoadAudioRenderDevices()
        {
            cbOutputAudioDeviceList.Items.Clear();
            renderDevices.Clear();

            var enumerator = new MMDeviceEnumerator();
            int index = 0;

            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                renderDevices.Add(device);
                cbOutputAudioDeviceList.Items.Add($"[{index}] {device.FriendlyName}");
                index++;
            }

            if (cbOutputAudioDeviceList.Items.Count == 0)
            {
                cbOutputAudioDeviceList.Items.Add(NO_DEVICES_TEXT);
                cbOutputAudioDeviceList.SelectedIndex = 0;
                return;
            }

            // Гарантируем валидный индекс
            cbOutputAudioDeviceList.SelectedIndex = 0;
        }

        // Восстановление выбора аудиоустройства воспроизведения из ini и запуск аудио
        private void RestoreSelectedAudioRenderDevice()
        {
            if (renderDevices.Count == 0 || cbOutputAudioDeviceList.Items.Count == 0) return;

            string saved = IniSettings.SelectedAudioRenderDevice;

            if (!string.IsNullOrEmpty(saved))
            {
                for (int i = 0; i < cbOutputAudioDeviceList.Items.Count; i++)
                {
                    if (cbOutputAudioDeviceList.Items[i]?.ToString() == saved)
                    {
                        cbOutputAudioDeviceList.SelectedIndex = i;
                        StartAudioOutput();
                        return;
                    }
                }
            }

            // Если сохранённое не найдено — fallback на первое устройство
            if (cbOutputAudioDeviceList.SelectedIndex < 0) cbOutputAudioDeviceList.SelectedIndex = 0;
            StartAudioOutput();
        }

        // Обновление списка устройств при раскрытии ComboBox
        private void OnAudioRenderDeviceListDropDown(object? sender, EventArgs e)
        {
            string? currentSelection = cbOutputAudioDeviceList.SelectedItem?.ToString();

            LoadAudioRenderDevices();

            if (!string.IsNullOrEmpty(currentSelection) && currentSelection != NO_DEVICES_TEXT)
            {
                for (int i = 0; i < cbOutputAudioDeviceList.Items.Count; i++)
                {
                    if (cbOutputAudioDeviceList.Items[i]?.ToString() == currentSelection)
                    {
                        cbOutputAudioDeviceList.SelectedIndex = i;
                        return;
                    }
                }
            }

            cbOutputAudioDeviceList.SelectedIndex = 0;
        }

        // Действия при выборе аудиоустройства воспроизведения пользователем
        private void OnAudioRenderDeviceSelectionChanged(object? sender, EventArgs e)
        {
            if (chkMuteAudioOut.Checked)
                return;

            if (currentSource == null || !currentSource.IsRunning)
                return;

            StartAudioOutput();
        }

        // Обработчик чекбокса "Заглушить звук"
        private void ChkMuteAudioOut_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkMuteAudioOut.Checked)
            {
                // Полностью останавливаем и освобождаем вывод
                audioOutput?.Stop();
                audioOutput?.Dispose();
                audioOutput = null;

                chkAGC.Enabled = false;
                nudVolume.Enabled = false;
            }
            else
            {
                if (currentSource != null && currentSource.IsRunning) StartAudioOutput();
            }
        }

        // Старт аудиовывода на выбранном устройстве
        private void StartAudioOutput()
        {
            if (chkMuteAudioOut.Checked) return;

            if (renderDevices.Count == 0 || cbOutputAudioDeviceList.SelectedIndex < 0) return;

            audioOutput?.Stop();
            audioOutput?.Dispose();
            audioOutput = null;

            chkAGC.Enabled = false;
            nudVolume.Enabled = false;

            try
            {
                var renderDevice = renderDevices[cbOutputAudioDeviceList.SelectedIndex];
                audioOutput = new WasapiAudioOutput(renderDevice);
                audioOutput.Start((int)currentSampleRate);
                chkAGC.Enabled = true;
                nudVolume.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start audio output:\n{ex.Message}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                chkMuteAudioOut.Checked = true;
                audioOutput = null;
            }
        }

        // Обновление водопада, спектров и статус бара
        private void ApplySampleRateToViews(double sampleRate)
        {
            if (waterfallView != null) waterfallView.CurrentSampleRate = sampleRate;
            if (fullSpectrumView != null) fullSpectrumView.CurrentSampleRate = sampleRate;
            if (spectrumViewI != null) spectrumViewI.CurrentSampleRate = sampleRate;
            if (spectrumViewQ != null) spectrumViewQ.CurrentSampleRate = sampleRate;
            if (tabControl1.SelectedTab == tabPageScope && oscilloscopeView != null)
                TrackBarTimeDiv_ValueChanged(trackBarTimeDiv, EventArgs.Empty);
        }

        // Отдельный метод для показа ошибки (вызывается из безопасного потока формы)
        private void ShowCaptureError(string message)
        {
            if (_hasShownCaptureError) return;
            _hasShownCaptureError = true;

            MessageBox.Show(
                $"{message}\n\nPlayback has been stopped.",
                "SpectrumView — Playback Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        #endregion

        #region Форма приложения
        
        // Восстановление размеров и положения окна приложения из ini
        private void RestoreWindowPosition()
        {
            this.MinimumSize = new System.Drawing.Size(formWidthMin, formHeightMin);

            if (IniSettings.WindowWidth > 0 && IniSettings.WindowHeight > 0)
            {
                int width = Math.Max(IniSettings.WindowWidth, formWidthMin);
                int height = Math.Max(IniSettings.WindowHeight, formHeightMin);

                var screen = Screen.FromPoint(new Point(IniSettings.WindowLeft, IniSettings.WindowTop));
                var workingArea = screen.WorkingArea;

                bool isVisible =
                    IniSettings.WindowLeft < workingArea.Right &&
                    IniSettings.WindowTop < workingArea.Bottom &&
                    IniSettings.WindowLeft + width > workingArea.Left &&
                    IniSettings.WindowTop + height > workingArea.Top;

                if (isVisible)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Bounds = new Rectangle(
                        IniSettings.WindowLeft,
                        IniSettings.WindowTop,
                        width,
                        height
                    );
                }
                else
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                    this.Size = new System.Drawing.Size(width, height);
                }
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Size = new System.Drawing.Size(formWidthMin, formHeightMin);
            }

            if (IniSettings.WindowState == FormWindowState.Maximized ||
                IniSettings.WindowState == FormWindowState.Minimized)
            {
                this.Load += (s, e) =>
                {
                    this.WindowState = IniSettings.WindowState;
                };
            }
        }

        // Действия при закрытии формы приложения
        private void FrmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Останавливаем таймер спектров
            spectrumTimer?.Stop();
            spectrumTimer?.Dispose();

            // Запоминаем сохраняемые значения контролов
            if (cbInputAudioDeviceList.SelectedItem is string deviceName)
            {
                if (!string.IsNullOrEmpty(deviceName) && deviceName != NO_DEVICES_TEXT)
                {
                    IniSettings.SelectedAudioDevice = deviceName;
                }
            }

            if (cbOutputAudioDeviceList.SelectedItem is string renderDeviceName)
            {
                if (!string.IsNullOrEmpty(renderDeviceName) && renderDeviceName != NO_DEVICES_TEXT)
                {
                    IniSettings.SelectedAudioRenderDevice = renderDeviceName;
                }
            }

            if (this.WindowState == FormWindowState.Normal)
            {
                IniSettings.WindowLeft = this.Left;
                IniSettings.WindowTop = this.Top;
                IniSettings.WindowWidth = this.Width;
                IniSettings.WindowHeight = this.Height;
            }
            else
            {
                IniSettings.WindowLeft = this.RestoreBounds.Left;
                IniSettings.WindowTop = this.RestoreBounds.Top;
                IniSettings.WindowWidth = this.RestoreBounds.Width;
                IniSettings.WindowHeight = this.RestoreBounds.Height;
            }

            IniSettings.WindowState = this.WindowState;

            // Сохраняем текущие настройки в INI
            // Водопад и спектры
            IniSettings.RefLevelDB = changedParams.RefLevelDB;
            IniSettings.DisplayRangeDB = changedParams.DisplayRangeDB;
            IniSettings.FftSize = changedParams.FftSize;
            IniSettings.FullSpectrumColor = changedParams.FullSpectrumColor;
            IniSettings.IChannelColor = changedParams.IChannelColor;
            IniSettings.QChannelColor = changedParams.QChannelColor;
            IniSettings.WaterfallColorRefDB = changedParams.WaterfallColorRefDB;
            IniSettings.WaterfallColorRangeDB = changedParams.WaterfallColorRangeDB;
            IniSettings.WaterfallScrollDown = changedParams.WaterfallScrollDown;
            IniSettings.SwapIQ = changedParams.SwapIQ;
            // Коррекции
            IniSettings.DcCorrectionEnabled = changedParams.DcCorrectionEnabled;
            IniSettings.GainBalanceEnabled = changedParams.GainBalanceEnabled;
            IniSettings.GainRatio = changedParams.GainRatio;
            IniSettings.PhaseCorrectionEnabled = changedParams.PhaseCorrectionEnabled;
            IniSettings.PhaseCoeff = changedParams.PhaseCoeff;
            // Фильтрация
            IniSettings.DigitalLpfEnabled = changedParams.DigitalLpfEnabled;
            // Демодуляция
            IniSettings.DemodType = changedParams.DemodType;
            IniSettings.DemodBandwidthHz = changedParams.DemodBandwidthHz;
            // Аудиовывод
            IniSettings.AGCEnabled = changedParams.AGCEnabled;
            IniSettings.AGCThreshold = changedParams.AGCThreshold;
            IniSettings.AGCAttackTimeMs = changedParams.AGCAttackTimeMs;
            IniSettings.AGCDecayTimeMs = changedParams.AGCDecayTimeMs;
            IniSettings.VolumePercent = changedParams.VolumePercent;
            IniSettings.Save();
        }
        #endregion

        #region Вспомогательные методы
        
        // Форматирования частоты
        private static string FormatFrequency(float freqHz)
        {
            if (Math.Abs(freqHz) >= 1000)
                return $"{freqHz / 1000:+0.0;-0.0} kHz";
            else
                return $"{freqHz:+0;-0} Hz";
        }

        // Фформатирование V/mV/uV
        private static string FormatVolts(float volts)
        {
            float absVolts = Math.Abs(volts);

            if (absVolts >= 1.0f)
                return $"{volts:+0.00;-0.00} V";
            else if (absVolts >= 0.001f)
                return $"{(volts * 1000):+0.00;-0.00} mV";
            else
                return $"{(volts * 1_000_000):+0.00;-0.00} µV";
        }

        // Получение номера релиза из атрибутов сборки
        private static string GetReleaseVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Сначала пытаемся получить из AssemblyFileVersion (более точный для релизов)
            var fileVersionAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (fileVersionAttr != null && !string.IsNullOrEmpty(fileVersionAttr.Version))
            {
                return $"Version: {fileVersionAttr.Version}";
            }

            // Если нет — берём из AssemblyVersion
            var version = assembly.GetName().Version;
            return $"Version: {version!.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        #endregion
    }
}