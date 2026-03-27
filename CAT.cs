/*
 *  CAT.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 27-03-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/    
 *  
 */
using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace SDR_DEV_APP
{
    public class CAT
    {
        private SerialPort? _serialPort;
        private bool _isConnected;
        private readonly StringBuilder _lineBuffer = new();

        public event Action<string>? MessageReceived;
        public event Action<bool>? ConnectionStateChanged;

        public bool IsConnected => _isConnected;

        public void Connect(string portName, int baudRate = 115200)
        {
            if (_isConnected) Disconnect();

            try
            {
                _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 100,
                    WriteTimeout = 100,
                    DtrEnable = true,
                    RtsEnable = true
                };
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                System.Threading.Thread.Sleep(300);

                ReadAllAvailable();

                _isConnected = true;
                ConnectionStateChanged?.Invoke(true);

                // Запрос начальных статусов от устройства
                RequestInitialStates();
            }
            catch (UnauthorizedAccessException)
            {
                _isConnected = false;
                MessageReceived?.Invoke($"[CAT Error] Port {portName} is busy.");
                ConnectionStateChanged?.Invoke(false);
            }
            catch (IOException ex) when (ex.Message.Contains("sem"))
            {
                _isConnected = false;
                MessageReceived?.Invoke($"[CAT Error] STM32 not responding on {portName}.");
                ConnectionStateChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                MessageReceived?.Invoke($"[CAT Error] {ex.Message}");
                ConnectionStateChanged?.Invoke(false);
            }
        }

        public void Disconnect()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Dispose();
                }
                catch { }
                _serialPort = null;
            }
            _isConnected = false;
            ConnectionStateChanged?.Invoke(false);
        }

        public void SendCommand(string cmd)
        {
            if (!_isConnected || _serialPort == null) return;
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(cmd + "\r\n");
                _serialPort.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageReceived?.Invoke($"[TX Error] {ex.Message}");
            }
        }

        private void ReadAllAvailable()
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;
            try
            {
                var start = DateTime.Now;
                var timeout = TimeSpan.FromMilliseconds(200);

                while (_serialPort.BytesToRead > 0 || (DateTime.Now - start) < timeout)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        byte b = (byte)_serialPort.ReadByte();
                        ProcessByte(b);
                        start = DateTime.Now;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(5);
                    }
                }
            }
            catch { }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null) return;
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    byte b = (byte)_serialPort.ReadByte();
                    ProcessByte(b);
                }
            }
            catch { }
        }

        private void ProcessByte(byte b)
        {
            if (b == 13) // CR
            {
                if (_lineBuffer.Length > 0)
                {
                    string line = _lineBuffer.ToString();
                    _lineBuffer.Clear();
                    MessageReceived?.Invoke(line);
                }
            }
            else if (b != 10) // Игнорируем LF
            {
                _lineBuffer.Append((char)b);
            }
        }

        #region Команды STM32 (Обертки)
        public void CmdTim8Pwm(bool on) => SendCommand(on ? "tim8_pwm_on" : "tim8_pwm_off");
        public void CmdTim15Pwm(bool on) => SendCommand(on ? "tim15_pwm_on" : "tim15_pwm_off");
        public void CmdSwapIQUAC(bool on) => SendCommand(on ? "swap_iq_uac_on" : "swap_iq_uac_off");
        public void CmdSwapRotateENC(bool on) => SendCommand(on ? "swap_rt_enc_on" : "swap_rt_enc_off");
        public void CmdDcCorrection(bool on) => SendCommand(on ? "dc_on" : "dc_off");
        public void CmdAmpCorrection(bool on) => SendCommand(on ? "amp_on" : "amp_off");
        public void CmdAmpSet(float ratio) => SendCommand($"amp_set {ratio.ToString("F3", CultureInfo.InvariantCulture)}");
        public void CmdPhaseCorrection(bool on) => SendCommand(on ? "phase_on" : "phase_off");
        public void CmdPhaseSet(float deg) => SendCommand($"phase_set {deg.ToString("F4", CultureInfo.InvariantCulture)}");
        public void CmdAgcOutput(bool on) => SendCommand(on ? "agc_on" : "agc_off");
        public void CmdXtallFreqSet(uint hz) => SendCommand($"xt_freq_set {hz}");
        public void CmdSiDriverSet(uint value) => SendCommand($"si_driver_set 0x{value:X}");
        public void CmdLoFreqSet(uint hz) => SendCommand($"lo_freq_set {hz}");
        public void CmdModeSet(string mode) => SendCommand($"mode_set {mode.ToLower()}");
        public void CmdStatus(string type) => SendCommand($"{type}_status");
       
        // Запрашивает текущие статусы всех управляемых параметров.
        public void RequestInitialStates()
        {
            SendCommand("tim8_pwm_status");
            SendCommand("tim15_pwm_status");
            SendCommand("swap_iq_uac_status");
            SendCommand("swap_rt_enc_status");
            SendCommand("dc_status");
            SendCommand("amp_status");
            SendCommand("phase_status");
            SendCommand("agc_status");
            SendCommand("xt_freq_status");
            SendCommand("si_driver_status");
            SendCommand("lo_freq_status");            
            SendCommand("mode_status");
        }
        #endregion
    }
}