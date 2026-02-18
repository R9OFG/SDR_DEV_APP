/*
 *  WavSignalSource.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 07-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/    
 *  
 */

using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SDR_DEV_APP
{
    // Источник сигнала из WAV-файла (реализует интерфейс ISignalSource)
    public class WavSignalSource : ISignalSource, IDisposable
    {
        #region Поля и состояние

        private readonly string filePath;                   // Путь к файлу
        private AudioFileReader? audioReader;               // Читатель WAV-файла
        private CancellationTokenSource? cancellationTokenSource; // Токен отмены воспроизведения
        private Task? playbackTask;                         // Задача фонового воспроизведения
        private bool isRunning;                             // Флаг активности источника
        private readonly object lockObj = new();            // Объект синхронизации
        private bool isPaused;                              // Флаг паузы
        private readonly ManualResetEvent pauseEvent = new(true); // Событие управления паузой (true = не в паузе)

        // Событие новых отсчётов I/Q (вызывается при поступлении данных)
        public event Action<float[], float[]>? SamplesAvailable;

        // Событие ошибки захвата/воспроизведения (реализация интерфейса ISignalSource)
        public event Action<string>? CaptureError;

        // Событие изменения позиции (0.0..1.0) — вызывается при воспроизведении и перемотке
        public event Action<double>? PositionChanged;

        // Событие завершения воспроизведения (вызывается при достижении конца файла БЕЗ зацикливания)
        public event Action? PlaybackCompleted;

        // Флаг зацикленного воспроизведения
        public bool Loop { get; set; } = false;

        #endregion

        #region Свойства

        // Общая длительность файла в секундах
        public double TotalDurationSeconds => audioReader?.TotalTime.TotalSeconds ?? 0.0;

        // Текущая позиция в процентах (0.0..1.0)
        public double CurrentPositionPercent
        {
            get => audioReader != null && audioReader.Length > 0
                ? (double)audioReader.Position / audioReader.Length
                : 0.0;
            private set => SeekToPercent(value);
        }

        // Частота дискретизации из файла (Гц)
        public double SampleRate => audioReader?.WaveFormat.SampleRate ?? 48000;

        // Признак активности источника (реализация интерфейса ISignalSource)
        public bool IsRunning => isRunning;

        // Состояние паузы
        public bool IsPaused
        {
            get => isPaused;
            private set
            {
                if (isPaused != value)
                {
                    isPaused = value;
                    if (value)
                        pauseEvent.Reset(); // блокируем поток воспроизведения
                    else
                        pauseEvent.Set();   // разблокируем поток
                }
            }
        }

        #endregion

        #region Конструктор

        // Создание источника из указанного WAV-файла
        public WavSignalSource(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("WAV file not found", filePath);

            this.filePath = filePath;
        }

        #endregion

        #region Управление воспроизведением

        // Запуск воспроизведения файла
        public void Start()
        {
            lock (lockObj)
            {
                if (isRunning) return;

                try
                {
                    audioReader = new AudioFileReader(filePath);
                    var format = audioReader.WaveFormat;

                    // Проверка поддерживаемых форматов
                    if (format.Channels < 1 || format.Channels > 2)
                        throw new InvalidOperationException($"Unsupported channel count: {format.Channels} (need 1 or 2 channels)");

                    if (format.Encoding != WaveFormatEncoding.IeeeFloat &&
                        format.Encoding != WaveFormatEncoding.Pcm &&
                        format.Encoding != WaveFormatEncoding.Extensible)
                        throw new InvalidOperationException($"Unsupported encoding: {format.Encoding}");

                    cancellationTokenSource = new CancellationTokenSource();
                    isRunning = true;

                    // Запуск фоновой задачи воспроизведения
                    playbackTask = Task.Run(() => PlaybackLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);
                }
                catch
                {
                    audioReader?.Dispose();
                    audioReader = null;
                    throw;
                }
            }
        }

        // Остановка воспроизведения
        public void Stop()
        {
            lock (lockObj)
            {
                if (!isRunning) return;

                // Разблокируем поток на случай, если он в паузе
                pauseEvent.Set();
                cancellationTokenSource?.Cancel();
                playbackTask?.Wait(1000);

                audioReader?.Dispose();
                audioReader = null;
                isRunning = false;
                IsPaused = false; // сбрасываем состояние паузы
            }
        }

        // Постановка на паузу
        public void Pause()
        {
            if (IsRunning && !IsPaused)
                IsPaused = true;
        }

        // Возобновление воспроизведения после паузы
        public void Resume()
        {
            if (IsRunning && IsPaused)
                IsPaused = false;
        }

        // Переключение состояния паузы
        public void TogglePause()
        {
            if (IsRunning)
                IsPaused = !IsPaused;
        }

        #endregion

        #region Цикл воспроизведения

        // Фоновый цикл чтения и отправки данных
        private void PlaybackLoop(CancellationToken token)
        {
            try
            {
                const int emitSize = 512; // ≈10 мс при 48 кГц
                var format = audioReader!.WaveFormat;
                bool isStereo = format.Channels == 2;
                int bytesPerSample = format.BitsPerSample / 8;
                double sampleRate = format.SampleRate;

                long targetTicksPerEmit = (long)(Stopwatch.Frequency * emitSize / sampleRate);
                long nextDeadlineTicks = 0;
                bool wasPaused = false; // Флаг для детектирования выхода из паузы

                // Цикл работает до отмены (а не до конца файла) для поддержки зацикливания
                while (!token.IsCancellationRequested)
                {
                    // Ожидание выхода из паузы (с таймаутом для проверки отмены)
                    pauseEvent.WaitOne(100);
                    if (token.IsCancellationRequested || IsPaused)
                    {
                        wasPaused = true;
                        continue;
                    }

                    // Сброс дедлайна после паузы для точной синхронизации
                    if (wasPaused)
                    {
                        nextDeadlineTicks = 0;
                        wasPaused = false;
                    }

                    // === 1. Чтение данных из файла ===
                    int bytesToRead = emitSize * format.Channels * bytesPerSample;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = audioReader.Read(buffer, 0, bytesToRead);

                    // === 2. Обработка конца файла ===
                    if (bytesRead == 0)
                    {
                        if (Loop)
                        {
                            // Зацикливание: перематываем в начало и продолжаем
                            audioReader.Position = 0;
                            PositionChanged?.Invoke(0.0); // уведомляем подписчиков о сбросе позиции
                            continue; // продолжаем цикл без выхода
                        }
                        else
                        {
                            // Без зацикливания: завершаем воспроизведение
                            break;
                        }
                    }

                    int samplesRead = bytesRead / (format.Channels * bytesPerSample);
                    if (samplesRead == 0) break;

                    // === 3. Конвертация в float ===
                    float[] iSamples = new float[samplesRead];
                    float[] qSamples = new float[samplesRead];

                    if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
                    {
                        // Быстрая конвертация float32 через прямое кастование памяти
                        var floats = MemoryMarshal.Cast<byte, float>(new Span<byte>(buffer, 0, bytesRead));
                        for (int i = 0; i < samplesRead; i++)
                        {
                            iSamples[i] = floats[i * format.Channels];
                            qSamples[i] = isStereo ? floats[i * format.Channels + 1] : iSamples[i];
                        }
                    }
                    else
                    {
                        // Конвертация PCM в float для 16/24/32 бит
                        for (int i = 0; i < samplesRead; i++)
                        {
                            int baseIndex = i * format.Channels * bytesPerSample;
                            float iVal = ReadPcmSample(buffer, baseIndex, format.BitsPerSample);
                            iSamples[i] = iVal;
                            qSamples[i] = isStereo
                                ? ReadPcmSample(buffer, baseIndex + bytesPerSample, format.BitsPerSample)
                                : iVal;
                        }
                    }

                    // === 4. Отправка данных подписчикам (КРИТИЧЕСКИ ВАЖНО!) ===
                    SamplesAvailable?.Invoke(iSamples, qSamples); // ← без этой строки нет спектров и звука!

                    // === 5. Обновление позиции ===
                    if (!IsPaused)
                    {
                        double percent = (double)audioReader.Position / audioReader.Length;
                        PositionChanged?.Invoke(percent);
                    }

                    // === 6. Точная синхронизация по времени ===
                    if (nextDeadlineTicks == 0)
                    {
                        // Первый запуск или после паузы — устанавливаем новый дедлайн
                        nextDeadlineTicks = Stopwatch.GetTimestamp() + targetTicksPerEmit;
                    }
                    else
                    {
                        // Следующий дедлайн = предыдущий + интервал
                        nextDeadlineTicks += targetTicksPerEmit;
                        long now = Stopwatch.GetTimestamp();
                        long remainingTicks = nextDeadlineTicks - now;

                        // Активное ожидание без блокировки потока (если осталось >0.5 мс)
                        if (remainingTicks > Stopwatch.Frequency / 2000)
                        {
                            while (Stopwatch.GetTimestamp() < nextDeadlineTicks &&
                                   !token.IsCancellationRequested &&
                                   !IsPaused)
                            {
                                Thread.SpinWait(10);
                            }
                        }
                        // Если remainingTicks <= 0 — пропускаем задержку (отстаём, но не накапливаем долг)
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение при остановке — не ошибка
            }
            catch (Exception ex)
            {
                // КРИТИЧЕСКИ ВАЖНО: логируем ошибку и уведомляем подписчиков БЕЗ блокировки потока
                Debug.WriteLine($"WAV playback error: {ex.Message}");
                CaptureError?.Invoke($"WAV playback error: {ex.Message}");
            }
            finally
            {
                // Вызов события завершения ТОЛЬКО при отключённом зацикливании
                if (!Loop && audioReader != null && !token.IsCancellationRequested)
                {
                    if (audioReader.Position >= audioReader.Length)
                    {
                        PlaybackCompleted?.Invoke();
                    }
                }
                Stop();
            }
        }

        #endregion

        #region Вспомогательные методы

        // Чтение PCM отсчёта разной разрядности в нормализованный float [-1.0, +1.0]
        private static float ReadPcmSample(byte[] buffer, int offset, int bitsPerSample)
        {
            switch (bitsPerSample)
            {
                case 16:
                    short s16 = BitConverter.ToInt16(buffer, offset);
                    return s16 / 32768.0f;

                case 24:
                    int s24 = buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16);
                    if ((s24 & 0x800000) != 0) s24 |= unchecked((int)0xFF000000);
                    return s24 / 8388608.0f;

                case 32:
                    int s32 = BitConverter.ToInt32(buffer, offset);
                    return s32 / 2147483648.0f;

                default:
                    throw new NotSupportedException($"Unsupported PCM bit depth: {bitsPerSample}");
            }
        }

        // Перемотка к заданному проценту (0.0 = начало, 1.0 = конец)
        public void SeekToPercent(double percent)
        {
            if (audioReader == null) return;

            lock (lockObj)
            {
                if (!isRunning) return;

                percent = Math.Clamp(percent, 0.0, 1.0);
                long newPosition = (long)(audioReader.Length * percent);
                audioReader.Position = newPosition;

                // Сброс паузы при перемотке с конца файла
                if (IsPaused && percent < 1.0)
                    IsPaused = false;

                // Уведомление подписчиков об изменении позиции
                PositionChanged?.Invoke(percent);
            }
        }

        #endregion

        #region IDisposable

        // Корректное освобождение ресурсов
        public void Dispose()
        {
            Stop();
            audioReader?.Dispose();
            pauseEvent.Dispose();
        }

        #endregion
    }
}