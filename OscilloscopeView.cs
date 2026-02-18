/*
 *  OscilloscopeView.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 05-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/  
 *  
 */

namespace SDR_DEV_APP
{
    // Режимы синхронизации осциллографа
    public enum TriggerMode { None, IChannel, QChannel }

    // Контрол для отображения временных сигналов I и Q каналов (осциллограф)
    public class OscilloscopeView : UserControl
    {
        #region Объявления

        // Размер кольцевого буфера
        private const int BUFFER_SIZE = 65536;
        private readonly short[] iBuffer = new short[BUFFER_SIZE];
        private readonly short[] qBuffer = new short[BUFFER_SIZE];
        private int writeIndex = 0;
        private readonly object bufferLock = new();

        // Статистика для отображения
        private float? iPeakDb = null;
        private float? qPeakDb = null;
        private float? iPeakVolts = null;
        private float? qPeakVolts = null;
        private float? iRmsVolts = null;
        private float? qRmsVolts = null;

        // Параметры отображения
        private float sensitivity = 1.0f;
        private int timebase = 1024;
        private int iOffset = 0;
        private int qOffset = 0;

        // Триггерная синхронизация
        private TriggerMode triggerMode = TriggerMode.None;
        private short triggerLevel = 0;
        private int triggerPosition = 20;

        // Свойства для внешнего управления
        public float Sensitivity
        {
            get => sensitivity;
            set { sensitivity = Math.Max(1.0f, value); Invalidate(); }
        }

        public int Timebase
        {
            get => timebase;
            set { timebase = Math.Max(64, Math.Min(BUFFER_SIZE, value)); Invalidate(); }
        }

        public int IOffset
        {
            get => iOffset;
            set { iOffset = value; Invalidate(); }
        }

        public int QOffset
        {
            get => qOffset;
            set { qOffset = value; Invalidate(); }
        }

        public TriggerMode TriggerMode
        {
            get => triggerMode;
            set { triggerMode = value; Invalidate(); }
        }

        public short TriggerLevel
        {
            get => triggerLevel;
            set { triggerLevel = value; Invalidate(); }
        }

        public int TriggerPosition
        {
            get => triggerPosition;
            set { triggerPosition = Math.Clamp(value, 0, 100); Invalidate(); }
        }

        #endregion

        #region Конструктор и вспомогательные методы

        public OscilloscopeView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.Black;
        }

        private static string FormatVolts(float volts)
        {
            volts = Math.Abs(volts);
            if (volts >= 1.0f)
                return $"{volts:F2} V";
            else if (volts >= 0.001f)
                return $"{(volts * 1000):F2} mV";
            else
                return $"{(volts * 1_000_000):F1} µV";
        }

        public void AppendSamples(ReadOnlySpan<short> iSamples, ReadOnlySpan<short> qSamples)
        {
            lock (bufferLock)
            {
                int count = Math.Min(iSamples.Length, qSamples.Length);
                if (count <= 0) return;

                for (int i = 0; i < count; i++)
                {
                    int index = (writeIndex + i) % BUFFER_SIZE;
                    iBuffer[index] = iSamples[i];
                    qBuffer[index] = qSamples[i];
                }
                writeIndex = (writeIndex + count) % BUFFER_SIZE; // ← КРИТИЧЕСКИ ВАЖНО
            }
        }

        public void Clear()
        {
            lock (bufferLock)
            {
                writeIndex = 0;
            }
        }

        public (short iPeak, short qPeak) GetPeakAmplitude()
        {
            short iMax = 0, qMax = 0;
            lock (bufferLock)
            {
                for (int i = 0; i < BUFFER_SIZE; i++)
                {
                    if (Math.Abs(iBuffer[i]) > iMax) iMax = Math.Abs(iBuffer[i]);
                    if (Math.Abs(qBuffer[i]) > qMax) qMax = Math.Abs(qBuffer[i]);
                }
            }
            return (iMax, qMax);
        }

        public void SetStatistics(float? iPeakDb, float? qPeakDb, float? iPeakVolts, float? qPeakVolts, float? iRmsVolts, float? qRmsVolts)
        {
            this.iPeakDb = iPeakDb;
            this.qPeakDb = qPeakDb;
            this.iPeakVolts = iPeakVolts;
            this.qPeakVolts = qPeakVolts;
            this.iRmsVolts = iRmsVolts;
            this.qRmsVolts = qRmsVolts;
            Invalidate();
        }

        #endregion

        #region Триггерная синхронизация

        // Поиск точки триггера с гистерезисом для устранения дрожания на шуме
        // Возвращает смещение от начала поиска (положительное = вперёд по буферу)
        private static int FindTriggerPoint(ReadOnlySpan<short> buffer, short triggerLevel, int startIndex, int searchDepth)
        {
            if (searchDepth <= 0) return -1;

            // Гистерезис: ±1% от полной шкалы (32768) для подавления шума
            const short HYSTERESIS = 327; // ~1%
            short upperThreshold = (short)(triggerLevel + HYSTERESIS);
            short lowerThreshold = (short)(triggerLevel - HYSTERESIS);

            // Начинаем в состоянии "ниже нижнего порога"
            bool wasBelow = buffer[startIndex] < lowerThreshold;

            for (int i = 1; i < searchDepth; i++)
            {
                int idx = (startIndex + i) % BUFFER_SIZE;
                short sample = buffer[idx];

                // Обнаружение перехода СНИЗУ ВВЕРХ через ВЕРХНИЙ порог (нарастающий фронт с гистерезисом)
                if (wasBelow && sample >= upperThreshold)
                {
                    return i; // Возвращаем смещение от startIndex
                }

                // Обновляем состояние (только при переходе через НИЖНИЙ порог вниз)
                if (sample < lowerThreshold)
                    wasBelow = true;
                else if (sample > upperThreshold)
                    wasBelow = false;
            }

            return -1; // Триггер не найден
        }

        #endregion

        #region Отрисовка осциллограммы

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 1 || height <= 1) return;

            int centerY = height / 2;
            const float MAX_AMPLITUDE = 32768.0f;
            float scale = (height / 2.0f) / (MAX_AMPLITUDE / sensitivity);

            // Безопасное копирование буферов
            short[] iBuf, qBuf;
            int currentWriteIndex;
            lock (bufferLock)
            {
                iBuf = new short[BUFFER_SIZE];
                qBuf = new short[BUFFER_SIZE];
                Array.Copy(iBuffer, iBuf, BUFFER_SIZE);
                Array.Copy(qBuffer, qBuf, BUFFER_SIZE);
                currentWriteIndex = writeIndex;
            }

            // === СТАБИЛЬНАЯ СИНХРОНИЗАЦИЯ: поиск триггера от фиксированной точки ===
            int startIndex;
            int samplesToDraw = Math.Min(timebase, BUFFER_SIZE);

            if (triggerMode != TriggerMode.None && currentWriteIndex >= samplesToDraw)
            {
                // Выбираем буфер в зависимости от режима триггера
                ReadOnlySpan<short> triggerBuffer = triggerMode == TriggerMode.IChannel
                    ? iBuf.AsSpan()
                    : qBuf.AsSpan();

                // Корректируем уровень триггера на смещение канала
                short adjustedLevel = (short)(triggerLevel + (triggerMode == TriggerMode.IChannel ? iOffset : qOffset));

                // Ищем триггер НАЧИНАЯ С ПОЗИЦИИ (currentWriteIndex - timebase)
                // Это даёт нам окно из 'timebase' отсчётов ПЕРЕД текущей позицией записи
                int searchStart = (currentWriteIndex - timebase + BUFFER_SIZE) % BUFFER_SIZE;
                int triggerOffset = FindTriggerPoint(triggerBuffer, adjustedLevel, searchStart, timebase);

                if (triggerOffset != -1)
                {
                    // Рассчитываем смещение так, чтобы точка триггера оказалась на позиции triggerPosition%
                    int triggerX = (triggerPosition * width) / 100;
                    int samplesBeforeTrigger = (int)(triggerX * timebase / (float)width);

                    // Начинаем отрисовку ЗА триггером (чтобы триггер был в позиции triggerPosition%)
                    startIndex = (searchStart + triggerOffset - samplesBeforeTrigger + BUFFER_SIZE) % BUFFER_SIZE;
                }
                else
                {
                    // Триггер не найден — свободная развёртка от конца буфера
                    startIndex = (currentWriteIndex - samplesToDraw + BUFFER_SIZE) % BUFFER_SIZE;
                }
            }
            else
            {
                // Свободная развёртка
                startIndex = (currentWriteIndex - samplesToDraw + BUFFER_SIZE) % BUFFER_SIZE;
            }
            // ======================================================================

            // Рисуем сетку и центральную линию
            DrawGridAndCenterLine(e.Graphics, width, height, centerY);

            // Отрисовка уровня триггера (если включена синхронизация)
            if (triggerMode != TriggerMode.None)
            {
                int triggerY = centerY - (int)((triggerLevel) * scale);
                using var triggerPen = new Pen(triggerMode == TriggerMode.IChannel ? Color.Yellow : Color.Cyan, 1.0f);
                triggerPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(triggerPen, 0, triggerY, width, triggerY);

                // Маркер триггера слева (треугольник)
                int markerX = (triggerPosition * width) / 100;
                using var markerBrush = new SolidBrush(triggerMode == TriggerMode.IChannel ? Color.Yellow : Color.Cyan);

                Point[] triggerMarker =
                [
                    new Point(markerX - 6, triggerY - 4),
                    new Point(markerX - 6, triggerY + 4),
                    new Point(markerX + 2, triggerY)
                ];
                e.Graphics.FillPolygon(markerBrush, triggerMarker);
            }

            // Рисуем ломаную линию по точкам для обоих каналов
            using var penI = new Pen(Color.Yellow, 1.5f);
            using var penQ = new Pen(Color.Cyan, 1.5f);

            for (int i = 1; i < samplesToDraw; i++)
            {
                int idx1 = (startIndex + i - 1) % BUFFER_SIZE;
                int idx2 = (startIndex + i) % BUFFER_SIZE;

                int x1 = (int)(width * (i - 1) / (float)samplesToDraw);
                int x2 = (int)(width * i / (float)samplesToDraw);

                // Канал I
                float y1_I_raw = (iBuf[idx1] - iOffset) * scale;
                float y2_I_raw = (iBuf[idx2] - iOffset) * scale;
                int y1_I = centerY - (int)y1_I_raw;
                int y2_I = centerY - (int)y2_I_raw;

                // Канал Q
                float y1_Q_raw = (qBuf[idx1] - qOffset) * scale;
                float y2_Q_raw = (qBuf[idx2] - qOffset) * scale;
                int y1_Q = centerY - (int)y1_Q_raw;
                int y2_Q = centerY - (int)y2_Q_raw;

                // Хак для видимости слабых сигналов
                if (Math.Abs(y1_I_raw) < 0.5f && Math.Abs(y2_I_raw) < 0.5f)
                {
                    y1_I -= 1; y2_I -= 1;
                }
                if (Math.Abs(y1_Q_raw) < 0.5f && Math.Abs(y2_Q_raw) < 0.5f)
                {
                    y1_Q -= 1; y2_Q -= 1;
                }

                e.Graphics.DrawLine(penI, x1, y1_I, x2, y2_I);
                e.Graphics.DrawLine(penQ, x1, y1_Q, x2, y2_Q);
            }

            // Подпись нулевого уровня
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);
            e.Graphics.DrawString("Zero level", font, brush, 5, centerY - 15);

            // Отрисовка статистики
            DrawStatistics(e.Graphics, triggerMode, triggerLevel);
        }

        private static void DrawGridAndCenterLine(Graphics g, int width, int height, int centerY)
        {
            using var gridPen = new Pen(Color.FromArgb(40, 40, 40), 1);
            using var centerPen = new Pen(Color.DarkGray, 1);

            int divisions = 8;
            for (int i = 1; i < divisions; i++)
            {
                int y = centerY + (i - divisions / 2) * (height / divisions);
                g.DrawLine(gridPen, 0, y, width, y);
            }
            g.DrawLine(centerPen, 0, centerY, width, centerY);
        }

        private void DrawStatistics(Graphics g, TriggerMode triggerMode, short triggerLevel)
        {
            if (!iPeakDb.HasValue || !iPeakVolts.HasValue || !iRmsVolts.HasValue ||
                !qPeakDb.HasValue || !qPeakVolts.HasValue || !qRmsVolts.HasValue)
                return;

            using var statFont = new Font("Consolas", 9, FontStyle.Bold);
            int y = 8;
            int x = 8;

            using var brushI = new SolidBrush(Color.Yellow);
            string iText = $"I Peak: {iPeakDb:F1} dB, {FormatVolts(iPeakVolts.Value)}, RMS {FormatVolts(iRmsVolts.Value)}";
            g.DrawString(iText, statFont, brushI, x, y);
            y += 18;

            using var brushQ = new SolidBrush(Color.Cyan);
            string qText = $"Q Peak: {qPeakDb:F1} dB, {FormatVolts(qPeakVolts.Value)}, RMS {FormatVolts(qRmsVolts.Value)}";
            g.DrawString(qText, statFont, brushQ, x, y);

            y += 18;
            string triggerText = triggerMode switch
            {
                TriggerMode.None => "Trigger: None",
                TriggerMode.IChannel => $"Trigger: I {FormatVolts(triggerLevel * 3.3f / 32768.0f)}",
                TriggerMode.QChannel => $"Trigger: Q {FormatVolts(triggerLevel * 3.3f / 32768.0f)}",
                _ => ""
            };
            using var brushTrigger = new SolidBrush(triggerMode == TriggerMode.None ? Color.Gray : (triggerMode == TriggerMode.IChannel ? Color.Yellow : Color.Cyan));
            g.DrawString(triggerText, statFont, brushTrigger, x, y);
        }

        #endregion
    }
}