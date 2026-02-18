/*
 *  RealIQSpectrumView.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 01-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

namespace SDR_DEV_APP
{
    // Контрол для отображения спектра одного из каналов IQ (I или Q) в логарифмическом масштабе частот
    public class RealIQSpectrumView : UserControl
    {
        #region Объявления

        // Цвет спектра (по умолчанию: Lime для I, Cyan для Q)
        public Color SpectrumColor { get; set; }

        // Буфер данных спектра в дБ
        private float[] spectrumData = [];

        // Объект для потокобезопасного доступа к данным
        private readonly object dataLock = new();

        // Флаг: true — отображается I-канал, false — Q-канал
        private readonly bool isIChannel;

        // Есть ли данные для отрисовки
        private bool hasData = false;

        // Верхняя граница диапазона (в дБ)
        private float refLevelDB;

        // Отображаемый диапазон (в дБ)
        private float displayRangeDB;

        // Частота дискретизации текущего сигнала (ограничена снизу 1000 Гц)
        private double currentSampleRate = 48000;
        public double CurrentSampleRate
        {
            get => currentSampleRate;
            set { currentSampleRate = Math.Max(1000, value); Invalidate(); }
        }

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

        // Кэширование предыдущих значений
        private float lastFreqHz = float.NaN;
        private float lastLevelDB = float.NaN;
        // ===================================

        #endregion

        #region Конструктор

        public RealIQSpectrumView(bool isI = true)
        {
            this.isIChannel = isI;
            DoubleBuffered = true;
            BackColor = Color.Black;
            SpectrumColor = isI ? Color.Lime : Color.Cyan;

            // Инициализация тултипа
            tooltip = new ToolTip
            {
                AutoPopDelay = 32000,
                InitialDelay = 0,
                ReshowDelay = 0
            };

            // Подписка на события мыши
            MouseMove += RealIQSpectrumView_MouseMove;
            MouseLeave += RealIQSpectrumView_MouseLeave;
        }

        #endregion

        #region Обработка мыши (тултип)

        // Обработчик движения мыши — показываем уровень и частоту
        private void RealIQSpectrumView_MouseMove(object? sender, MouseEventArgs e)
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

            // Вычисляем уровень по вертикальной позиции курсора (как на сетке графика)
            float yNorm = e.Y / (float)ClientSize.Height;
            float levelDB = refLevelDB - yNorm * displayRangeDB;

            // Получаем частоту с учётом логарифмической шкалы
            float freqHz = GetFrequencyAtX(e.X);

            // Кэширование значений (обновляем только при значимом изменении)
            bool freqChanged = Math.Abs(freqHz - lastFreqHz) > 50.0f;
            bool levelChanged = Math.Abs(levelDB - lastLevelDB) > 0.3f;

            if (!freqChanged && !levelChanged && lastMousePos != Point.Empty)
                return;

            lastMousePos = e.Location;
            lastTooltipUpdate = DateTime.Now;
            lastFreqHz = freqHz;
            lastLevelDB = levelDB;

            // Форматируем частоту
            string freqStr = Math.Abs(freqHz) < 1000
                ? $"{freqHz:F0} Hz"
                : Math.Abs(freqHz) < 1_000_000
                    ? $"{freqHz / 1000:F2} kHz"
                    : $"{freqHz / 1_000_000:F3} MHz";

            // Форматируем уровень
            string levelStr = Math.Abs(levelDB) < 10 ? $"{levelDB:F0} dB" : $"{levelDB:F1} dB";

            // Показываем тултип
            tooltip.Show($"Freq: {freqStr}\nLevel: {levelStr}", this, e.X + 12, e.Y + 18);
        }

        // Скрытие тултипа при выходе мыши за пределы контрола
        private void RealIQSpectrumView_MouseLeave(object? sender, EventArgs e)
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
            lastFreqHz = float.NaN;
            lastLevelDB = float.NaN;
        }

        // Публичный метод для принудительного скрытия тултипа
        public void HideCursorTooltip()
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
        }

        #endregion

        #region Установка данных

        // Обновление данных спектра
        public void SetData(ReadOnlySpan<float> dbValues, float refLevel, float range)
        {
            lock (dataLock)
            {
                hasData = true;
                spectrumData = dbValues.ToArray();
                refLevelDB = refLevel;
                displayRangeDB = range;
            }
            Invalidate();
        }

        #endregion

        #region Отрисовка спектра

        // Основной метод отрисовки спектра
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 1 || height <= 1) return;

            // Очищаем фон чёрным
            e.Graphics.Clear(Color.Black);

            if (!hasData) return;

            // Рисуем сетку
            DrawGrid(e.Graphics, width, height);

            if (spectrumData.Length == 0) return;

            // Безопасно копируем данные для отрисовки
            float[] localData;
            lock (dataLock)
            {
                localData = (float[])spectrumData.Clone();
            }

            // Масштаб по вертикали: пикселей на дБ
            float yScale = (float)height / displayRangeDB;

            // Создаём перо заданного цвета для линии спектра
            using var pen = new Pen(SpectrumColor, 1);

            // Преобразуем точки спектра в экранные координаты с логарифмической шкалой X
            var points = new Point[localData.Length];
            // Шаг по частоте: предполагается, что входной спектр — это FFT размером N,
            // и localData.Length = N/2 (только положительные частоты)
            double df = currentSampleRate / (localData.Length * 2);

            double logMin = Math.Log10(30.0);
            double logMax = Math.Log10(Math.Max(30.0, currentSampleRate / 2.0));

            for (int k = 0; k < localData.Length; k++)
            {
                double freq = k * df;

                // Логарифмическая шкала X: от 30 Гц до Fs/2
                float x = freq >= 30
                    ? (float)((Math.Log10(freq) - logMin) / (logMax - logMin) * width)
                    : 0;

                // Вычисляем Y-координату: чем выше уровень (дБ), тем выше точка
                float y = height - (localData[k] - (refLevelDB - displayRangeDB)) * yScale;
                points[k] = new Point((int)x, (int)Math.Clamp(y, 0, height));
            }

            // Рисуем линию спектра, если есть хотя бы две точки
            if (points.Length > 1)
                e.Graphics.DrawLines(pen, points);

            // Добавляем подписи к осям
            DrawYLabels(e.Graphics, height);
            DrawXLabels(e.Graphics, width, height, logMin, logMax);
        }

        // Рисование сетки (горизонтальные и вертикальные линии)
        private void DrawGrid(Graphics g, int width, int height)
        {
            using var gridPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1);

            // Горизонтальные линии: каждые 20 дБ
            for (int dB = (int)(refLevelDB - displayRangeDB); dB <= (int)refLevelDB; dB += 20)
            {
                float y = height - (dB - (refLevelDB - displayRangeDB)) * ((float)height / displayRangeDB);
                if (y >= 0 && y <= height)
                    g.DrawLine(gridPen, 0, (int)y, width, (int)y);
            }

            // ЛОГАРИФМИЧЕСКАЯ ЧАСТОТНАЯ СЕТКА (1–2–5 по декадам)
            double maxFreq = Math.Max(30.0, currentSampleRate / 2.0);
            double logMin = Math.Log10(30.0);
            double logMax = Math.Log10(maxFreq);
            double[] baseSteps = [1, 2, 5];
            double decade = 10.0;

            while (decade <= maxFreq * 10)
            {
                foreach (double step in baseSteps)
                {
                    double freq = step * decade;
                    if (freq < 30.0 || freq > maxFreq)
                        continue;

                    double logF = Math.Log10(freq);
                    float x = (float)((logF - logMin) / (logMax - logMin) * width);
                    g.DrawLine(gridPen, (int)x, 0, (int)x, height);
                }
                decade *= 10.0;
            }
        }

        // Подписи Y-оси (уровни в дБ)
        private void DrawYLabels(Graphics g, int height)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);

            for (int dB = (int)(refLevelDB - displayRangeDB); dB <= (int)refLevelDB; dB += 20)
            {
                float y = height - (dB - (refLevelDB - displayRangeDB)) * ((float)height / displayRangeDB);
                if (y >= 0 && y <= height)
                {
                    string label = $"{dB} dB";
                    SizeF size = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, 2, (int)(y - size.Height / 2));
                }
            }
        }

        // Подписи X-оси (частоты в логарифмическом масштабе)
        private void DrawXLabels(Graphics g, int width, int height, double logMin, double logMax)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);

            double maxFreq = Math.Max(30.0, currentSampleRate / 2.0);
            double[] baseSteps = [1, 2, 5];
            double decade = 10.0;

            while (decade <= maxFreq * 10)
            {
                foreach (double step in baseSteps)
                {
                    double freq = step * decade;
                    if (freq < 30.0 || freq > maxFreq)
                        continue;

                    string text = freq >= 1000
                        ? $"{freq / 1000:F0}k"
                        : $"{(int)freq}";

                    double logF = Math.Log10(freq);
                    float x = (float)((logF - logMin) / (logMax - logMin) * width);

                    SizeF size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (int)(x - size.Width / 2), height - 16);
                }
                decade *= 10.0;
            }
        }

        #endregion

        #region Преобразования координата/частота (логарифмическая шкала)

        // Преобразование координаты X в частоту (Гц) с учётом логарифмической шкалы
        public float GetFrequencyAtX(float x)
        {
            int width = ClientSize.Width;
            if (width <= 1) return 30.0f;

            double logMin = Math.Log10(30.0);
            double logMax = Math.Log10(Math.Max(30.0, currentSampleRate / 2.0));

            // Обратное преобразование логарифмической шкалы
            double logFreq = (x / width) * (logMax - logMin) + logMin;
            double freq = Math.Pow(10.0, logFreq);

            // Ограничиваем диапазон
            freq = Math.Max(30.0, Math.Min(currentSampleRate / 2.0, freq));
            return (float)freq;
        }

        // Преобразование частоты (Гц) в координату X с учётом логарифмической шкалы
        public float GetXAtFrequency(float freqHz)
        {
            int width = ClientSize.Width;
            if (width <= 1) return 0.0f;

            double logMin = Math.Log10(30.0);
            double logMax = Math.Log10(Math.Max(30.0, currentSampleRate / 2.0));

            double freq = Math.Max(30.0, Math.Min(currentSampleRate / 2.0, freqHz));
            double logFreq = Math.Log10(freq);
            return (float)((logFreq - logMin) / (logMax - logMin) * width);
        }

        #endregion
    }
}