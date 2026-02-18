/*
 *  ISignalSource.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 07-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

namespace SDR_DEV_APP
{
    // Интерфейс источника I/Q-сигнала, обеспечивающий единый способ получения
    // временных отсчётов для последующего спектрального и временного анализа.
    // Реализации могут работать с аудиоустройствами, WAV-файлами, сетевыми потоками и т.д.
    public interface ISignalSource
    {
        // Событие, вызываемое при поступлении новых блоков I и Q отсчётов (в нормализованном float-формате)
        event Action<float[], float[]>? SamplesAvailable;

        // Текущая частота дискретизации источника (Гц)
        double SampleRate { get; }

        // Признак активности захвата/воспроизведения
        bool IsRunning { get; }

        // Запускает источник (начинает захват или воспроизведение)
        void Start();

        // Останавливает источник и освобождает ресурсы
        void Stop();
        void Dispose();

        // Событие ошибки захвата
        event Action<string>? CaptureError;
    }
}