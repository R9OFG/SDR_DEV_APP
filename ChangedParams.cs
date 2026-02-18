/*
 *  ChangedParams.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 06-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

namespace SDR_DEV_APP
{
    public class ChangedParams
    {
        // Размер FFT (в отсчётах)
        public int FftSize { get; set; } = 8192;

        // Верхний уровень спектра в дБ (например, +10 dB)
        public float RefLevelDB { get; set; } = 10.0f;

        // Диапазон отображения в дБ (например, 190 dB → от −180 до +10 dB)
        public float DisplayRangeDB { get; set; } = 190.0f;

        // Диапазон цветовых параметров водопада
        public float WaterfallColorRefDB { get; set; } = -50.0f;
        public float WaterfallColorRangeDB { get; set; } = 60.0f;

        // Направление отрисовки водопада: true — новая строка снизу, false — новая строка сверху
        public bool WaterfallScrollDown { get; set; } = true;

        // Цвета графиков комплексного спектра и спектров Real I/Q
        public Color FullSpectrumColor { get; set; } = Color.LightSeaGreen;
        public Color IChannelColor { get; set; } = Color.Lime;
        public Color QChannelColor { get; set; } = Color.Lime;

        // Инверсия I/Q
        public bool SwapIQ { get; set; } = false;

        // === Коррекции ===
        public bool DcCorrectionEnabled { get; set; } = false;
        public bool GainBalanceEnabled { get; set; } = false;
        public float GainRatio { get; set; } = 1.0f;
        public bool PhaseCorrectionEnabled { get; set; } = false;
        public float PhaseCoeff { get; set; } = 0.0f;

        // === Фильтрация ===
        public bool DigitalLpfEnabled { get; set; } = false;

        // === Демодуляция ===
        public DemodulationType DemodType { get; set; } = DemodulationType.USB;
        public float DemodBandwidthHz { get; set; } = 2700.0f;

        // === Аудиовывод ===
        public bool AGCEnabled { get; set; } = false;
        public float AGCThreshold { get; set; } = 0.7f;
        public float AGCAttackTimeMs { get; set; } = 1.0f;
        public float AGCDecayTimeMs { get; set; } = 500.0f;
        public float VolumePercent { get; set; } = 10.0f;

        // Параметры по умолчанию
        public void SetDefaults()
        {
            FftSize = 8192;
            RefLevelDB = 10.0f;
            DisplayRangeDB = 190.0f;
            WaterfallColorRefDB = -50.0f;
            WaterfallColorRangeDB = 60.0f;
            WaterfallScrollDown = true;
            FullSpectrumColor = Color.LightSeaGreen;
            IChannelColor = Color.Lime;
            QChannelColor = Color.Lime;
            SwapIQ = false;

            // Коррекции
            DcCorrectionEnabled = false;
            GainBalanceEnabled = false;
            GainRatio = 1.0f;
            PhaseCorrectionEnabled = false;
            PhaseCoeff = 0.0f;

            // Фильтрация
            DigitalLpfEnabled = false;

            // Демодуляция
            DemodType = DemodulationType.USB;
            DemodBandwidthHz = 2700.0f;

            // Аудиовывод
            AGCEnabled = false;
            AGCThreshold = 0.7f;
            AGCAttackTimeMs = 1.0f;
            AGCDecayTimeMs = 500.0f;
            VolumePercent = 10.0f;
        }
    }
}