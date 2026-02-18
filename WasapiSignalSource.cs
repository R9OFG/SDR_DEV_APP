/*
 *  WasapiSignalSource.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 07-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/  
 *  
 */

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace SDR_DEV_APP
{
    // Реализация ISignalSource для захвата аудиосигнала через WASAPI.
    // Поддерживает только стереоустройства с float32 форматом (I — левый канал, Q — правый).
    public class WasapiSignalSource(MMDevice device) : ISignalSource
    {
        // Аудиоустройство Windows, с которого будет производиться захват
        private readonly MMDevice device = device ?? throw new ArgumentNullException(nameof(device));
        // Экземпляр захвата NAudio
        private WasapiCapture? capture;
        // Объект для потокобезопасного запуска/остановки
        private readonly object lockObj = new();

        // Событие, вызываемое при поступлении новых I/Q отсчётов
        public event Action<float[], float[]>? SamplesAvailable;
        // Событие ошибки захвата (для уведомления формы из безопасного потока)
        public event Action<string>? CaptureError;

        // Текущая частота дискретизации (берётся из WaveFormat захвата)
        public double SampleRate => capture?.WaveFormat.SampleRate ?? 0;
        // Флаг активности захвата
        public bool IsRunning { get; private set; }

        // Запускает захват аудиосигнала с указанного устройства
        public void Start()
        {
            lock (lockObj)
            {
                if (IsRunning) return;
                try
                {
                    capture = new WasapiCapture(device, false);
                    capture.DataAvailable += OnDataAvailable;
                    capture.StartRecording();
                    IsRunning = true;
                }
                catch
                {
                    capture?.Dispose();
                    capture = null;
                    throw;
                }
            }
        }

        // Обработчик входящих аудиоданных от NAudio — ЗАЩИЩЁН ОТ ИСКЛЮЧЕНИЙ
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                // Защита от гонки при остановке
                if (capture == null || !IsRunning || e.BytesRecorded <= 0) return;

                // Поддерживается только float32 формат
                if (capture.WaveFormat?.Encoding != WaveFormatEncoding.IeeeFloat) return;

                // Выравниваем длину буфера до кратной 8 байтам (2 float = стерео пара)
                int validBytes = e.BytesRecorded & ~7; // кратно 8
                if (validBytes < 8) return;

                // Безопасное преобразование байтов в float
                ReadOnlySpan<byte> buffer = new(e.Buffer, 0, validBytes);
                ReadOnlySpan<float> floats = MemoryMarshal.Cast<byte, float>(buffer);

                // Разделяем I и Q
                var iList = new List<float>(floats.Length / 2);
                var qList = new List<float>(floats.Length / 2);

                for (int i = 0; i < floats.Length - 1; i += 2)
                {
                    iList.Add(floats[i]);     // левый канал → I
                    qList.Add(floats[i + 1]); // правый канал → Q
                }

                // Передаём данные подписчикам
                if (iList.Count > 0 && SamplesAvailable != null)
                {
                    try
                    {
                        SamplesAvailable([.. iList], [.. qList]);
                    }
                    catch (Exception ex)
                    {
                        // Уведомляем форму через событие — форма покажет MessageBox в своём потоке
                        CaptureError?.Invoke($"Signal processing error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Уведомляем форму через событие — форма покажет MessageBox в своём потоке
                CaptureError?.Invoke($"Audio capture interrupted: {ex.Message}");
            }
        }

        // Останавливает захват и освобождает ресурсы
        public void Stop()
        {
            lock (lockObj)
            {
                if (!IsRunning) return;
                try
                {
                    // Сначала отписываемся — критично для избежания гонки
                    if (capture != null) capture.DataAvailable -= OnDataAvailable;
                    capture?.StopRecording();
                }
                finally
                {
                    capture?.Dispose();
                    capture = null;
                    IsRunning = false;
                }
            }
        }

        // Освобождает ресурсы
        public void Dispose()
        {
            Stop();
        }
    }
}