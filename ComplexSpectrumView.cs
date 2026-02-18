/*
 *  ComplexSpectrumView.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 03-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

namespace SDR_DEV_APP
{
    // Контрол для отображения комплексного спектра с нулевой частотой в центре
    internal class ComplexSpectrumView : UserControl
    {
        #region Объявления

        // Цвет линии и заливки спектра
        public Color SpectrumColor { get; set; } = Color.Yellow;

        // Буфер данных спектра в дБ
        private float[] spectrumData = [];

        // Объект для потокобезопасного доступа к данным
        private readonly object dataLock = new();

        // Уровень опорного сигнала (верхняя граница диапазона, в дБ)
        private float refLevelDB;

        // Отображаемый диапазон (в дБ)
        private float displayRangeDB;

        // Частота дискретизации (ограничена снизу 1000 Гц)
        private double currentSampleRate = 48000;
        public double CurrentSampleRate
        {
            get => currentSampleRate;
            set { currentSampleRate = Math.Max(1000, value); Invalidate(); }
        }

        // Полоса демодуляции на комплексном спектре
        public float DemodCenterFreqHz { get; set; } = 0.0f;
        public float DemodBandwidthHz { get; set; } = 2700.0f;
        
        // Тип демодуляции для правильного отображения границ полосы
        public DemodulationType DemodType { get; set; } = DemodulationType.USB;

        // Информация о демодуляции для отображения в углу спектра
        public string DemodInfoText { get; set; } = "";
        
        // === Индикатор пикового уровня в полосе демодуляции ===
        // Пиковый уровень сигнала внутри полосы демодуляции (в дБ)
        private float peakLevelInBandDB = float.MinValue;

        // Отображаемое значение по шкале S-метра (S1-S9, S9+10 и т.д.)
        private string sMeterDisplay = "--";
        // =========================================================

        // === Тултип при наведении курсора ===
        // Флаг включения отображения тултипа при наведении
        public bool ShowCursorTooltip { get; set; } = false;

        // Встроенный компонент тултипа
        private readonly ToolTip tooltip;

        // Последняя позиция мыши для избежания лишних обновлений
        private Point lastMousePos = Point.Empty;

        // Таймер для плавного обновления тултипа
        private DateTime lastTooltipUpdate = DateTime.MinValue;
        private const int TOOLTIP_UPDATE_DELAY_MS = 100; // 10 раз/сек — оптимально для плавности без нагрузки
        private const int MIN_MOUSE_MOVE = 3;             // минимальное смещение мыши для обновления (пиксели)

        // Кэширование предыдущих значений для избежания лишних обновлений
        private float lastFreqHz = float.NaN;
        private float lastLevelDB = float.NaN;
        // ===================================

        #endregion

        #region Конструктор

        public ComplexSpectrumView()
        {
            // Включаем двойную буферизацию для плавной отрисовки
            DoubleBuffered = true;

            // Фон прозрачный
            BackColor = Color.Transparent;

            // Инициализация тултипа (без проблем совместимости с .NET 6+)
            tooltip = new ToolTip
            {
                AutoPopDelay = 32000,  // долго показываем
                InitialDelay = 0,      // мгновенно
                ReshowDelay = 0        // без задержки при движении
            };

            // Подписка на события мыши
            MouseMove += ComplexSpectrumView_MouseMove;
            MouseLeave += ComplexSpectrumView_MouseLeave;
        }

        #endregion

        #region Обработка мыши (тултип)

        // Обработчик движения мыши — показываем уровень по вертикальной позиции курсора
        private void ComplexSpectrumView_MouseMove(object? sender, MouseEventArgs e)
        {
            // Скрываем тултип если функция отключена или курсор вне области
            if (!ShowCursorTooltip || e.X < 0 || e.X >= ClientSize.Width || e.Y < 0 || e.Y >= ClientSize.Height)
            {
                if (lastMousePos != Point.Empty)
                {
                    tooltip.Hide(this);
                    lastMousePos = Point.Empty;
                }
                return;
            }

            // === ОПТИМИЗАЦИЯ 1: Порог движения мыши ===
            if (lastMousePos != Point.Empty)
            {
                int dx = Math.Abs(e.X - lastMousePos.X);
                int dy = Math.Abs(e.Y - lastMousePos.Y);
                if (dx < MIN_MOUSE_MOVE && dy < MIN_MOUSE_MOVE)
                    return;
            }

            // === ОПТИМИЗАЦИЯ 2: Дебаунсинг по времени ===
            if ((DateTime.Now - lastTooltipUpdate).TotalMilliseconds < TOOLTIP_UPDATE_DELAY_MS)
                return;

            // Вычисляем уровень по вертикальной позиции курсора (как на сетке графика)
            float yNorm = e.Y / (float)ClientSize.Height; // 0.0 = верх, 1.0 = низ
            float levelDB = refLevelDB - yNorm * displayRangeDB;

            // Получаем частоту
            float freqHz = GetFrequencyAtX(e.X);

            // === ОПТИМИЗАЦИЯ 3: Кэширование значений ===
            // Обновляем тултип только при значимом изменении (>50 Гц или >0.3 дБ)
            bool freqChanged = Math.Abs(freqHz - lastFreqHz) > 50.0f;
            bool levelChanged = Math.Abs(levelDB - lastLevelDB) > 0.3f;

            if (!freqChanged && !levelChanged && lastMousePos != Point.Empty)
                return;

            lastMousePos = e.Location;
            lastTooltipUpdate = DateTime.Now;
            lastFreqHz = freqHz;
            lastLevelDB = levelDB;

            // === ОПТИМИЗАЦИЯ 4: Быстрое форматирование без лишних аллокаций ===
            // Форматируем частоту
            string freqStr;
            if (Math.Abs(freqHz) < 1000)
                freqStr = $"{freqHz:F0} Hz";
            else if (Math.Abs(freqHz) < 1_000_000)
                freqStr = $"{freqHz / 1000:F2} kHz";
            else
                freqStr = $"{freqHz / 1_000_000:F3} MHz";

            // Форматируем уровень
            string levelStr = Math.Abs(levelDB) < 10 ? $"{levelDB:F0} dB" : $"{levelDB:F1} dB";

            // Показываем тултип БЕЗ BeginInvoke (синхронно, но с оптимизациями выше)
            // Это устраняет конфликты с основной отрисовкой и подтормаживания
            tooltip.Show($"Freq: {freqStr}\nLevel: {levelStr}", this, e.X + 12, e.Y + 18);
        }

        // Скрытие тултипа при выходе мыши за пределы контрола
        private void ComplexSpectrumView_MouseLeave(object? sender, EventArgs e)
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
            lastFreqHz = float.NaN;
            lastLevelDB = float.NaN;
        }

        // Публичный метод для принудительного скрытия тултипа (вызывается с формы при выключении чекбокса)
        public void HideCursorTooltip()
        {
            tooltip.Hide(this);
            lastMousePos = Point.Empty;
        }

        #endregion

        #region Установка данных и расчёт пика в полосе

        // Устанавливает новые данные спектра и параметры масштабирования
        public void SetData(ReadOnlySpan<float> dbValues, float refLevel, float range)
        {
            lock (dataLock)
            {
                // Копируем входные данные в внутренний буфер
                spectrumData = dbValues.ToArray();
                refLevelDB = refLevel;
                displayRangeDB = range;

                // Расчёт пика в полосе демодуляции
                if (DemodBandwidthHz > 0 && spectrumData.Length > 0)
                {
                    peakLevelInBandDB = CalculatePeakInDemodBand(spectrumData.AsSpan());
                    if (peakLevelInBandDB > float.MinValue + 100)
                        sMeterDisplay = ConvertToSMeter(peakLevelInBandDB);
                    else
                        sMeterDisplay = "--";
                }
                else
                {
                    peakLevelInBandDB = float.MinValue;
                    sMeterDisplay = "--";
                }
            }

            // Запрашиваем перерисовку
            Invalidate();
        }

        // Расчёт пикового уровня ВНУТРИ полосы демодуляции
        private float CalculatePeakInDemodBand(ReadOnlySpan<float> spectrumDB)
        {
            int n = spectrumDB.Length;
            if (n < 2 || DemodBandwidthHz <= 0)
                return float.MinValue;

            double fs = currentSampleRate;
            double nyquist = fs / 2.0;

            // Определяем границы полосы в Гц согласно типу демодуляции
            float leftFreq, rightFreq;
            switch (DemodType)
            {
                case DemodulationType.AM:
                case DemodulationType.FM:
                    leftFreq = DemodCenterFreqHz - DemodBandwidthHz / 2;
                    rightFreq = DemodCenterFreqHz + DemodBandwidthHz / 2;
                    break;
                case DemodulationType.USB:
                    leftFreq = DemodCenterFreqHz;
                    rightFreq = DemodCenterFreqHz + DemodBandwidthHz;
                    break;
                case DemodulationType.LSB:
                    leftFreq = DemodCenterFreqHz - DemodBandwidthHz;
                    rightFreq = DemodCenterFreqHz;
                    break;
                default:
                    leftFreq = DemodCenterFreqHz - DemodBandwidthHz / 2;
                    rightFreq = DemodCenterFreqHz + DemodBandwidthHz / 2;
                    break;
            }

            // Ограничиваем границы полосы частотой Найквиста
            leftFreq = (float)Math.Max(-nyquist, Math.Min(nyquist, leftFreq));
            rightFreq = (float)Math.Max(-nyquist, Math.Min(nyquist, rightFreq));
            if (leftFreq >= rightFreq)
                return float.MinValue;

            // Преобразуем частоты в индексы спектра (с учётом FFTShift: 0 Гц в центре)
            int leftIdx = (int)Math.Floor(((leftFreq + nyquist) / fs) * n);
            int rightIdx = (int)Math.Ceiling(((rightFreq + nyquist) / fs) * n);
            leftIdx = Math.Max(0, Math.Min(n - 1, leftIdx));
            rightIdx = Math.Max(0, Math.Min(n - 1, rightIdx));
            if (leftIdx >= rightIdx)
                return float.MinValue;

            // Находим максимальный уровень в диапазоне индексов
            float peak = float.MinValue;
            for (int i = leftIdx; i <= rightIdx; i++)
            {
                if (spectrumDB[i] > peak)
                    peak = spectrumDB[i];
            }
            return peak;
        }

        // Конвертация уровня сигнала в S-метр по стандарту (частоты < 30 МГц)
        // Стандарт: S9 = -73 dBm. Условно считаем, что значение в дБ на спектре = dBm.
        private static string ConvertToSMeter(float peakDb)
        {
            float dbAboveS9 = peakDb + 73.0f;

            // Уровни ВЫШЕ S9 (проверяем от самых высоких к низким!)
            if (dbAboveS9 >= 55) return "S9+60";
            if (dbAboveS9 >= 45) return "S9+50";
            if (dbAboveS9 >= 35) return "S9+40";
            if (dbAboveS9 >= 25) return "S9+30";
            if (dbAboveS9 >= 15) return "S9+20";
            if (dbAboveS9 >= 5) return "S9+10";
            if (dbAboveS9 >= -3) return "S9";  // гистерезис ±3 дБ для стабильности

            // Уровни НИЖЕ S9: каждый балл = 6 дБ (стандарт IARU)
            int stepsBelow = (int)Math.Ceiling((-dbAboveS9) / 6.0f);
            int sNumber = 9 - stepsBelow;
            sNumber = Math.Clamp(sNumber, 1, 9);
            return $"S{sNumber}";
        }

        #endregion

        #region Отрисовка спектра

        // Отрисовка спектра
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 1 || height <= 1) return;

            // Очищаем фон
            e.Graphics.Clear(BackColor);

            // Безопасно копируем данные для отрисовки (быстрое копирование вместо Clone)
            float[] localData;
            lock (dataLock)
            {
                if (spectrumData.Length == 0) return;
                localData = new float[spectrumData.Length];
                Array.Copy(spectrumData, localData, spectrumData.Length);
            }

            if (localData.Length < 2) return;

            // Масштаб по вертикали: пикселей на дБ
            float yScale = height / displayRangeDB;
            int n = localData.Length;
            PointF[] points = new PointF[n];

            // Преобразуем индексы в координаты с нулевой частотой по центру
            for (int i = 0; i < n; i++)
            {
                float _x = ((float)i / (n - 1) - 0.5f) * width + width / 2;
                float _y = height - (localData[i] - (refLevelDB - displayRangeDB)) * yScale;
                points[i] = new PointF(_x, Math.Clamp(_y, 0, height - 1));
            }

            // Заливка под спектр с вертикальным градиентом
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(points);
                path.AddLine(points[^1].X, height, points[0].X, height);
                path.CloseFigure();

                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new RectangleF(0, 0, width, height),
                    Color.FromArgb(200, SpectrumColor),
                    Color.FromArgb(80, SpectrumColor),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                e.Graphics.FillPath(brush, path);
            }

            // Линия спектра поверх заливки
            using var pen = new Pen(SpectrumColor, 1);
            e.Graphics.DrawLines(pen, points);

            // Рисуем сетку и подписи
            DrawGridAndLabels(e.Graphics, width, height);

            // Отрисовка полосы демодуляции (если включена)
            if (DemodBandwidthHz > 0)
            {
                // Расчёт границ полосы демодуляции с учётом типа
                float leftFreq, rightFreq;
                switch (DemodType)
                {
                    case DemodulationType.AM:
                    case DemodulationType.FM:
                        leftFreq = DemodCenterFreqHz - DemodBandwidthHz / 2;
                        rightFreq = DemodCenterFreqHz + DemodBandwidthHz / 2;
                        break;
                    case DemodulationType.USB:
                        leftFreq = DemodCenterFreqHz;
                        rightFreq = DemodCenterFreqHz + DemodBandwidthHz;
                        break;
                    case DemodulationType.LSB:
                        leftFreq = DemodCenterFreqHz - DemodBandwidthHz;
                        rightFreq = DemodCenterFreqHz;
                        break;
                    default:
                        leftFreq = DemodCenterFreqHz - DemodBandwidthHz / 2;
                        rightFreq = DemodCenterFreqHz + DemodBandwidthHz / 2;
                        break;
                }

                float leftX = GetXAtFrequency(leftFreq);
                float rightX = GetXAtFrequency(rightFreq);
                leftX = Math.Max(0, leftX);
                rightX = Math.Min(Width, rightX);

                // Полупрозрачный прямоугольник полосы
                using var brush = new SolidBrush(Color.FromArgb(40, Color.LimeGreen));
                e.Graphics.FillRectangle(brush, leftX, 0, rightX - leftX, Height);

                // Границы полосы
                using var penBW = new Pen(Color.LimeGreen, 1.5f);
                e.Graphics.DrawLine(penBW, leftX, 0, leftX, Height);
                e.Graphics.DrawLine(penBW, rightX, 0, rightX, Height);

                // Якорь полосы — жёлтая линия на всю высоту
                float anchorX = GetXAtFrequency(DemodCenterFreqHz);
                using var centerPen = new Pen(Color.Yellow, 2f);
                e.Graphics.DrawLine(centerPen, anchorX, 0, anchorX, Height);
            }

            // Отрисовка информации о демодуляции и индикатора пика
            using var font = new Font("Consolas", 9, FontStyle.Bold);
            using var brushInfo = new SolidBrush(Color.FromArgb(220, Color.White));

            float x = Width - 8;
            float y = 8;

            // Если есть текст демодуляции — отрисовываем его
            if (!string.IsNullOrEmpty(DemodInfoText))
            {
                SizeF size = e.Graphics.MeasureString(DemodInfoText, font);
                x = Width - size.Width - 8;

                using var shadowBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
                e.Graphics.FillRectangle(shadowBrush, x - 4, y - 2, size.Width + 8, size.Height + 4);
                e.Graphics.DrawString(DemodInfoText, font, brushInfo, x, y);
                y += size.Height + 4;
            }

            // Индикатор пика в полосе
            if (peakLevelInBandDB > float.MinValue + 100)
            {
                string peakText = $"Peak in BW: {peakLevelInBandDB:F1} dB | {sMeterDisplay}";
                SizeF peakSize = e.Graphics.MeasureString(peakText, font);
                x = Width - 203; // фиксированная позиция для стабильного отображения

                using var shadowBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
                e.Graphics.FillRectangle(shadowBrush, x - 4, y - 2, peakSize.Width + 8, peakSize.Height + 4);
                e.Graphics.DrawString(peakText, font, brushInfo, x, y);
            }
            else if (string.IsNullOrEmpty(DemodInfoText))
            {
                string noSignalText = "No signal in BW";
                SizeF size = e.Graphics.MeasureString(noSignalText, font);
                x = Width - size.Width - 8;

                using var shadowBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
                e.Graphics.FillRectangle(shadowBrush, x - 4, y - 2, size.Width + 8, size.Height + 4);
                e.Graphics.DrawString(noSignalText, font, brushInfo, x, y);
            }
        }

        // Отрисовка сетки и текстовых меток (частоты и уровни в дБ)
        private void DrawGridAndLabels(Graphics g, int width, int height)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);
            using var gridPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1);

            // Горизонтальные линии (уровни в дБ)
            for (int dB = (int)(refLevelDB - displayRangeDB); dB <= (int)refLevelDB; dB += 20)
            {
                float y = height - (dB - (refLevelDB - displayRangeDB)) * (height / displayRangeDB);
                g.DrawLine(gridPen, 0, y, width, y);
                string label = $"{dB} dB";
                SizeF size = g.MeasureString(label, font);
                g.DrawString(label, font, brush, 2, y - size.Height / 2);
            }

            double fs = currentSampleRate;
            double halfFs = fs / 2.0;

            // Шаг подписей частоты: до 96 кГц — 5 кГц, выше — 10 кГц
            double labelStep = (currentSampleRate <= 96_000.0) ? 5_000.0 : 10_000.0;

            // Вертикальные линии сетки
            double startFreq = Math.Floor(-halfFs / labelStep) * labelStep;
            for (double f = startFreq; f <= halfFs; f += labelStep)
            {
                float x = (float)((f / fs) * width + width / 2);
                if (x >= 0 && x <= width)
                    g.DrawLine(gridPen, x, 0, x, height);
            }

            // Подписи частот
            for (double f = startFreq; f <= halfFs; f += labelStep)
            {
                if (f < -halfFs || f > halfFs) continue;
                
                float x = (float)((f / fs) * width + width / 2);
                if (x < 0 || x > width) continue;
                
                string label = Math.Abs(f) < 1e-6 ? "0" :
                              f > 0 ? $"+{f / 1000:F0}k" : $"{f / 1000:F0}k";

                SizeF size = g.MeasureString(label, font);
                g.DrawString(label, font, brush, x - size.Width / 2, height - size.Height - 2);
            }
        }

        #endregion

        #region Преобразования координата/частота

        // Преобразование координаты X в частоту (Гц)
        public float GetFrequencyAtX(float x)
        {
            int width = ClientSize.Width;
            if (width <= 1) return 0.0f;
            float normX = (x / width - 0.5f);
            return (float)(normX * currentSampleRate);
        }

        // Преобразование частоты (Гц) в координату X
        public float GetXAtFrequency(float freqHz)
        {
            int width = ClientSize.Width;
            if (width <= 1) return width / 2.0f;
            float normFreq = freqHz / (float)currentSampleRate;
            return (normFreq + 0.5f) * width;
        }

        #endregion
    }
}