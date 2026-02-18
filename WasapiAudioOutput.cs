/*
 *  WasapiAudioOutput.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 04-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/  
 *  
 */

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace SDR_DEV_APP
{
    // Аудиовывод через WASAPI для стерео I/Q сигнала
    // Обеспечивает стабильный вывод в реальном времени с защитой от переполнения
    public sealed class WasapiAudioOutput : IDisposable
    {
        #region Поля и состояние

        private readonly MMDevice device;                   // Целевое аудиоустройство вывода
        private WasapiOut? player;                          // Проигрыватель WASAPI
        private BufferedWaveProvider? waveProvider;         // Буферизованный источник аудиоданных
        private readonly object lockObj = new();            // Объект синхронизации
        private bool isRunning;                             // Флаг активности вывода
        private byte[]? conversionBuffer;                   // Переиспользуемый буфер конвертации float → byte

        // Порог заполнения буфера для срабатывания смягчённого дропа (95%)
        private const float BUFFER_DROP_THRESHOLD = 0.95f;

        // Доля буфера, удаляемая при переполнении (25%)
        private const float BUFFER_DROP_RATIO = 0.25f;

        #endregion

        #region Конструктор

        // Инициализация аудиовывода для указанного устройства
        public WasapiAudioOutput(MMDevice device)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
        }

        #endregion

        #region Свойства

        // Текущая частота дискретизации вывода (Гц)
        public double SampleRate => waveProvider?.WaveFormat.SampleRate ?? 0;

        // Флаг активности аудиовывода
        public bool IsRunning => isRunning;

        #endregion

        #region Управление воспроизведением

        // Запуск аудиовывода с заданной частотой дискретизации
        // Буфер 200 мс — компромисс между задержкой (приемлемо для радиосвязи) и стабильностью в фоновом режиме
        // Латентность 50 мс — предотвращает дропы при сворачивании окна без излишней задержки
        public void Start(int sampleRate = 48000)
        {
            lock (lockObj)
            {
                if (isRunning) return;

                try
                {
                    // Формат: стерео float32 (I = левый, Q = правый)
                    var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

                    // Буфер 200 мс — достаточно для компенсации фоновых задержек ОС
                    // Формула: sampleRate * bytesPerSample(4) * channels(2) * seconds(0.2)
                    int bufferLength = (int)(sampleRate * 4 * 2 * 0.2);
                    waveProvider = new BufferedWaveProvider(waveFormat)
                    {
                        BufferLength = bufferLength,
                        DiscardOnBufferOverflow = true // ← критично: отбрасывать старые данные вместо клиппинга
                    };

                    // Латентность 50 мс вместо 20 мс — предотвращает дропы при сворачивании окна
                    player = new WasapiOut(device, AudioClientShareMode.Shared, false, 50);
                    player.Init(waveProvider);
                    player.Play();

                    isRunning = true;
                    conversionBuffer = null; // сброс буфера конвертации
                }
                catch
                {
                    // Очистка ресурсов при ошибке инициализации
                    player?.Dispose();
                    player = null;
                    waveProvider = null;
                    throw;
                }
            }
        }

        // Остановка воспроизведения и освобождение ресурсов
        public void Stop()
        {
            lock (lockObj)
            {
                if (!isRunning) return;

                try
                {
                    player?.Stop();
                }
                finally
                {
                    player?.Dispose();
                    player = null;
                    waveProvider = null;
                    conversionBuffer = null; // освобождаем буфер конвертации
                    isRunning = false;
                }
            }
        }

        // Полная очистка буфера вывода
        public void ClearBuffer()
        {
            lock (lockObj)
            {
                waveProvider?.ClearBuffer();
            }
        }

        #endregion

        #region Вывод аудиоданных

        // Отправка I/Q сэмплов на аудиовыход
        public void WriteSamples(ReadOnlySpan<float> iSamples, ReadOnlySpan<float> qSamples, bool swapIQ = false)
        {
            // Локальная копия для потокобезопасности без блокировки
            var provider = waveProvider;
            if (provider == null || !isRunning || iSamples.Length == 0 || qSamples.Length == 0)
                return;

            int count = Math.Min(iSamples.Length, qSamples.Length);
            int byteCount = count * 2 * 4; // 2 канала × 4 байта на float

            // Защита от переполнения буфера
            // При заполнении >95% удаляем 25% старых данных (смягчённый дроп вместо резкой очистки)
            if (provider.BufferedBytes > provider.BufferLength * BUFFER_DROP_THRESHOLD)
            {
                int bytesToDrop = (int)(provider.BufferLength * BUFFER_DROP_RATIO);
                var dummy = new byte[bytesToDrop];
                provider.Read(dummy, 0, bytesToDrop); // частичное удаление старых данных
            }

            // === Конвертация float/byte без аллокаций ===
            // Переиспользуем буфер для минимизации нагрузки на GC
            if (conversionBuffer == null || conversionBuffer.Length < byteCount)
            {
                conversionBuffer = new byte[byteCount * 2]; // запас на будущее
            }

            // Быстрая конвертация через прямое кастование памяти
            Span<float> floatSpan = MemoryMarshal.Cast<byte, float>(conversionBuffer.AsSpan(0, byteCount));
            for (int i = 0; i < count; i++)
            {
                // Применяем свап каналов при необходимости
                float iVal = swapIQ ? qSamples[i] : iSamples[i];
                float qVal = swapIQ ? iSamples[i] : qSamples[i];

                // Жёсткая защита от клиппинга (ограничение амплитуды)
                iVal = Math.Clamp(iVal, -1.0f, 1.0f);
                qVal = Math.Clamp(qVal, -1.0f, 1.0f);

                // Запись в стереоформат: [I0, Q0, I1, Q1, ...]
                floatSpan[i * 2] = iVal;     // левый канал (I)
                floatSpan[i * 2 + 1] = qVal; // правый канал (Q)
            }

            // Отправка данных в аудиобуфер
            provider.AddSamples(conversionBuffer, 0, byteCount);
        }

        #endregion

        #region IDisposable

        // Корректное освобождение ресурсов
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}