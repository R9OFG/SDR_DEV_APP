/*
 *  WaterfallView.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 01-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/  
 *  
 */

using System.Drawing.Imaging;

namespace SDR_DEV_APP
{
    // Контрол для отображения водопадной диаграммы спектра
    public class WaterfallView : UserControl
    {
        #region Объявления

        // Максимальное количество строк истории (фиксировано)
        private const int MAX_HISTORY_LINES = 512;

        // Двумерный буфер яркости: [строка по вертикали, столбец по горизонтали], значения 0..255
        private byte[,]? history;

        // Индекс текущей строки для записи новых данных
        private int currentLine = 0;

        // Флаг наличия хотя бы одной строки данных
        private bool hasData = false;

        // Цветовая палитра в формате BGR (256 уровней × 3 компонента)
        private readonly byte[] colorMap = new byte[256 * 3];

        // Направление прокрутки: true — новые строки добавляются снизу, false — сверху
        public bool ScrollDown { get; set; } = true;

        // Текущая частота дискретизации (для преобразования координат ↔ частота)
        public double CurrentSampleRate { get; set; } = 48000;

        // === Кэширование диапазонов даунсэмплирования ===
        // Храним диапазоны индексов спектра для каждого пикселя экрана при определённой ширине
        private int cachedWidth = -1;
        private (int start, int end)[]? downsampleRanges;
        // =================================================

        // === Тултип при наведении курсора ===
        // Флаг включения отображения тултипа при наведении
        public bool ShowCursorTooltip { get; set; } = false;

        // Встроенный компонент тултипа
        private readonly ToolTip tooltip;

        // Последняя позиция мыши для избежания лишних обновлений
        private Point lastMousePos = Point.Empty;

        // Таймер для плавного обновления тултипа
        private DateTime lastTooltipUpdate = DateTime.MinValue;
        private const int TOOLTIP_UPDATE_DELAY_MS = 100; // 10 раз/сек
        private const int MIN_MOUSE_MOVE = 3;             // минимальное смещение мыши (пиксели)

        // Кэширование предыдущей частоты для избежания лишних обновлений
        private float lastFreqHz = float.NaN;
        // ===================================

        #endregion

        #region Конструктор

        public WaterfallView()
        {
            DoubleBuffered = true;
            BackColor = Color.Black;

            // Инициализация тултипа
            tooltip = new ToolTip
            {
                AutoPopDelay = 32000,
                InitialDelay = 0,
                ReshowDelay = 0
            };

            // Подписка на события мыши
            MouseMove += WaterfallView_MouseMove;
            MouseLeave += WaterfallView_MouseLeave;

            // Инициализация цветовой палитры
            InitializeColorMap();
        }

        #endregion

        #region Обработка мыши (тултип)

        // Обработчик движения мыши — показываем только частоту относительно центрального нуля
        private void WaterfallView_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!ShowCursorTooltip || e.X < 0 || e.X >= ClientSize.Width || e.Y < 0 || e.Y >= ClientSize.Height)
            {
                if (lastMousePos != Point.Empty)
                {
                    tooltip.Hide(this);
                    lastMousePos = Point.Empty;
                }
                return;
            }

            // Порог движения мыши (3 пикселя)
            if (lastMousePos != Point.Empty)
            {
                int dx = Math.Abs(e.X - lastMousePos.X);
                int dy = Math.Abs(e.Y - lastMousePos.Y);
                if (dx < MIN_MOUSE_MOVE && dy < MIN_MOUSE_MOVE)
                    return;
            }

            // Дебаунсинг по времени
            if ((DateTime.Now - lastTooltipUpdate).TotalMilliseconds < TOOLTIP_UPDATE_DELAY_MS)
                return;

            // Получаем частоту относительно центрального нуля
            float freqHz = GetFrequencyAtX(e.X);

            // Кэширование значений (обновляем только при значимом изменении >50 Гц)
            if (Math.Abs(freqHz - lastFreqHz) < 50.0f && lastMousePos != Point.Empty)
                return;

            lastMousePos = e.Location;
            lastTooltipUpdate = DateTime.Now;
            lastFreqHz = freqHz;

            // Форматируем частоту относительно нуля: "+3.45 kHz", "-2.10 kHz", "0 Hz"
            string freqStr;
            if (Math.Abs(freqHz) < 100) // Очень близко к нулю
                freqStr = "0 Hz";
            else if (Math.Abs(freqHz) < 1000)
                freqStr = $"{freqHz:+0;-0} Hz";
            else if (Math.Abs(freqHz) < 1_000_000)
                freqStr = $"{freqHz / 1000:+0.00;-0.00} kHz";
            else
                freqStr = $"{freqHz / 1_000_000:+0.000;-0.000} MHz";

            // Показываем тултип (только частота, без уровня)
            tooltip.Show($"Freq: {freqStr}", this, e.X + 12, e.Y + 18);
        }

        // Скрытие тултипа при выходе мыши за пределы контрола
        private void WaterfallView_MouseLeave(object? sender, EventArgs e)
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
            lastFreqHz = float.NaN;
        }

        // Публичный метод для принудительного скрытия тултипа
        public void HideCursorTooltip()
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
        }

        #endregion

        #region Управление историей

        // Очищает историю водопада
        public void ClearHistory()
        {
            currentLine = 0;
            hasData = false;
            if (history != null)
                Array.Clear(history, 0, history.Length);
            Invalidate();
        }

        #endregion

        #region Добавление спектра и инициализация палитры

        // Инициализация цветовой палитры (голубо-жёлто-красная)
        private void InitializeColorMap()
        {
            for (int i = 0; i < 256; i++)
            {
                byte r, g, b;
                // Градиент: тёмно-синий → голубой → жёлтый → красный
                if (i < 64)
                {
                    r = 0; g = 0; b = (byte)(i * 4);
                }
                else if (i < 128)
                {
                    r = 0; g = (byte)((i - 64) * 4); b = 255;
                }
                else if (i < 192)
                {
                    r = (byte)((i - 128) * 4); g = 255; b = (byte)(255 - (i - 128) * 4);
                }
                else
                {
                    r = 255; g = (byte)(255 - (i - 192) * 4); b = 0;
                }
                // Сохраняем в формате BGR
                colorMap[i * 3 + 0] = b; // Blue
                colorMap[i * 3 + 1] = g; // Green
                colorMap[i * 3 + 2] = r; // Red
            }
        }

        // Добавляет новую строку спектра в водопад
        // minDB/maxDB — диапазон для цветовой насыщенности
        public void AddSpectrum(ReadOnlySpan<float> dbValues, float minDB, float maxDB)
        {
            if (dbValues.Length == 0) return;

            // При изменении ширины спектра — пересоздаём буфер и сбрасываем кэш
            if (history == null || history.GetLength(1) != dbValues.Length)
            {
                history = new byte[MAX_HISTORY_LINES, dbValues.Length];
                cachedWidth = -1;
                downsampleRanges = null;
            }

            // Нормализация значений в диапазон 0..255
            float range = maxDB - minDB;
            if (range <= 0) range = 1; // избегаем деления на ноль

            // === ОПТИМИЗАЦИЯ: предварительный расчёт множителя для ускорения цикла ===
            float scale = 255.0f / range;
            float offset = -minDB * scale;
            // ======================================================================

            for (int x = 0; x < dbValues.Length; x++)
            {
                // Быстрый расчёт без деления в цикле
                byte val = (byte)Math.Clamp((int)(dbValues[x] * scale + offset), 0, 255);
                history[currentLine, x] = val;
            }

            currentLine = (currentLine + 1) % MAX_HISTORY_LINES;
            hasData = true;
            Invalidate();
        }

        #endregion

        #region Отрисовка водопада

        // Отрисовка водопада
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!hasData || history == null) return;

            int width = ClientSize.Width;
            int height = Math.Min(ClientSize.Height, MAX_HISTORY_LINES);
            if (width <= 1 || height <= 1) return;

            // === ОПТИМИЗАЦИЯ: повторное использование битмапа для снижения аллокаций ===
            // Для простоты оставляем текущую реализацию (создание нового битмапа),
            // но в продакшене можно кэшировать битмап при неизменном размере окна
            // ========================================================================

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                int spectrumWidth = history.GetLength(1);

                // Кэширование диапазонов даунсэмплирования
                bool needsDownsampling = width < spectrumWidth;
                if (needsDownsampling && width != cachedWidth)
                {
                    cachedWidth = width;
                    downsampleRanges = new (int, int)[width];
                    for (int x = 0; x < width; x++)
                    {
                        int start = (x * spectrumWidth) / width;
                        int end = ((x + 1) * spectrumWidth) / width;
                        if (end == start) end = start + 1;
                        downsampleRanges[x] = (start, end);
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    // Определение строки из истории с учётом направления прокрутки
                    int srcY = ScrollDown
                        ? (currentLine - (height - y) + MAX_HISTORY_LINES) % MAX_HISTORY_LINES
                        : (currentLine - y + MAX_HISTORY_LINES) % MAX_HISTORY_LINES;

                    if (needsDownsampling)
                    {
                        // Максимальное даунсэмплирование для сохранения узких пиков
                        for (int x = 0; x < width; x++)
                        {
                            var (start, end) = downsampleRanges![x];
                            byte maxVal = 0;
                            for (int sx = start; sx < end; sx++)
                            {
                                byte sample = history[srcY, sx];
                                if (sample > maxVal) maxVal = sample;
                            }

                            // Запись цвета из палитры
                            int idx = maxVal * 3;
                            int dst = y * stride + x * 3;
                            ptr[dst + 0] = colorMap[idx + 0]; // B
                            ptr[dst + 1] = colorMap[idx + 1]; // G
                            ptr[dst + 2] = colorMap[idx + 2]; // R
                        }
                    }
                    else
                    {
                        // Прямое копирование (увеличение или 1:1)
                        for (int x = 0; x < width; x++)
                        {
                            int srcX = (x * spectrumWidth) / width;
                            srcX = Math.Min(srcX, spectrumWidth - 1);
                            byte val = history[srcY, srcX];

                            int idx = val * 3;
                            int dst = y * stride + x * 3;
                            ptr[dst + 0] = colorMap[idx + 0]; // B
                            ptr[dst + 1] = colorMap[idx + 1]; // G
                            ptr[dst + 2] = colorMap[idx + 2]; // R
                        }
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            e.Graphics.DrawImage(bmp, 0, 0, width, height);
            bmp.Dispose();
        }

        #endregion

        #region Преобразования координата/частота

        // Преобразование координаты X в частоту (Гц) относительно центрального нуля
        // Водопад отображает комплексный спектр с центрированием (0 Гц в центре)
        public float GetFrequencyAtX(float x)
        {
            int width = ClientSize.Width;
            if (width <= 1) return 0.0f;

            // Нормализуем: [0..width] → [-0.5..+0.5]
            float normX = (x / width - 0.5f);
            return (float)(normX * CurrentSampleRate);
        }

        // Преобразование частоты (Гц) в координату X
        public float GetXAtFrequency(float freqHz)
        {
            int width = ClientSize.Width;
            if (width <= 1) return width / 2.0f;

            // Нормализуем: [-Найквист..+Найквист] → [0..width]
            float normFreq = freqHz / (float)CurrentSampleRate;
            return (normFreq + 0.5f) * width;
        }

        #endregion
    }
}