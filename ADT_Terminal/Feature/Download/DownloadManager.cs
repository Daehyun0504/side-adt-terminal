using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using EmbedBox.Utils;

namespace EmbedBox.Features.Download
{
    public class DownloadManager
    {
        private readonly Form mainForm;
        private readonly SerialPort serialPort = new SerialPort();
        private string? sendFilePath;
        private DateTime transferStart;

        // Add these near the top of the class (once)
        private const byte SOH = 0x01;
        private const byte STX = 0x02;
        private const byte EOT = 0x04;
        private const byte ACK = 0x06;
        private const byte NAK = 0x15;
        private const byte CAN = 0x18;
        private const byte CHAR_C = 0x43; // 'C'

        public DownloadManager(Form form)
        {
            mainForm = form;
        }

        /** ------------------------------------------------------------------
         *  [Serial] 포트 열기
         *  ------------------------------------------------------------------ */
        public void OpenPort(ComboBox port, ComboBox baud, ComboBox dataBits, ComboBox stopBits, ComboBox parity)
        {
            try
            {
                if (serialPort.IsOpen)
                    serialPort.Close();

                serialPort.PortName = port.Text;
                serialPort.BaudRate = int.Parse(baud.Text);
                serialPort.DataBits = int.Parse(dataBits.Text);
                serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity.Text);
                serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits.Text);
                serialPort.Open();

                MessageBox.Show("✅ Serial Port Opened");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error opening serial port: {ex.Message}");
            }
        }

        /** ------------------------------------------------------------------
         *  [Serial] 포트 닫기
         *  ------------------------------------------------------------------ */
        public void ClosePort()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                MessageBox.Show("🔌 Serial Port Closed");
            }
        }

        /** ------------------------------------------------------------------
         *  [Download] 전송 파일 설정
         *  ------------------------------------------------------------------ */
        public void SetSendFile(string path)
        {
            sendFilePath = path;
            MessageBox.Show($"📂 Selected file: {path}");
        }

        /** Compute CRC16 (XMODEM/CCITT, poly 0x1021, init 0x0000) */
        private static ushort Crc16Xmodem(byte[] buf, int length)
        {
            ushort crc = 0x0000;
            for (int i = 0; i < length; i++)
            {
                crc ^= (ushort)(buf[i] << 8);
                for (int b = 0; b < 8; b++)
                {
                    if ((crc & 0x8000) != 0) crc = (ushort)((crc << 1) ^ 0x1021);
                    else crc <<= 1;
                }
            }
            return crc;
        }

        /** Read one byte with timeout; returns -1 on timeout */
        private static int ReadByteWithTimeout(SerialPort port, int timeoutMs)
        {
            int original = port.ReadTimeout;
            try
            {
                port.ReadTimeout = timeoutMs;
                return port.ReadByte();
            }
            catch (TimeoutException)
            {
                return -1;
            }
            finally
            {
                port.ReadTimeout = original;
            }
        }

        public void SendFile()
        {
            if (string.IsNullOrEmpty(sendFilePath))
            {
                MessageBox.Show("❌ No file selected!");
                return;
            }
            if (!File.Exists(sendFilePath))
            {
                MessageBox.Show("❌ File not found!");
                return;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(sendFilePath);
                int blockSize = 1024; // XMODEM-1K
                int totalBlocks = (fileBytes.Length + blockSize - 1) / blockSize;
                transferStart = DateTime.Now;

                // Ensure port is open and set reasonable timeouts
                if (!serialPort.IsOpen)
                    serialPort.Open();

                int originalReadTimeout = serialPort.ReadTimeout;
                int originalWriteTimeout = serialPort.WriteTimeout;
                serialPort.ReadTimeout = 1000; // 1s per read
                serialPort.WriteTimeout = 2000; // 2s per write

                try
                {
                    // 1) Wait for receiver to request CRC mode ('C') or NAK
                    DateTime waitStart = DateTime.Now;
                    bool receiverReady = false;
                    while ((DateTime.Now - waitStart).TotalSeconds < 20) // 20s window
                    {
                        int b = ReadByteWithTimeout(serialPort, 1000);
                        if (b == -1) continue;
                        if (b == CHAR_C || b == NAK) { receiverReady = true; break; }
                        if (b == CAN)
                        {
                            MessageBox.Show("❌ Transfer canceled by receiver (CAN).");
                            return;
                        }
                    }
                    if (!receiverReady)
                    {
                        MessageBox.Show("❌ Receiver did not request XMODEM (no 'C'/NAK).");
                        return;
                    }

                    // 2) Send data blocks
                    const int MAX_RETRY = 10;
                    byte[] dataBlock = new byte[blockSize];

                    for (int blockNum = 1; blockNum <= totalBlocks; blockNum++)
                    {
                        // Prepare data block with 0x1A padding
                        Array.Clear(dataBlock, 0, dataBlock.Length);
                        int offset = (blockNum - 1) * blockSize;
                        int bytesToSend = Math.Min(blockSize, fileBytes.Length - offset);
                        Array.Copy(fileBytes, offset, dataBlock, 0, bytesToSend);
                        for (int i = bytesToSend; i < blockSize; i++) dataBlock[i] = 0x1A;

                        // Build XMODEM-1K frame: STX | blk | ~blk | data(1024) | CRC(2)
                        byte[] frame = new byte[blockSize + 5];
                        frame[0] = STX;
                        frame[1] = (byte)blockNum;
                        frame[2] = (byte)(255 - blockNum);
                        Array.Copy(dataBlock, 0, frame, 3, blockSize);
                        ushort crc = Crc16Xmodem(dataBlock, blockSize);
                        frame[blockSize + 3] = (byte)((crc >> 8) & 0xFF);
                        frame[blockSize + 4] = (byte)(crc & 0xFF);

                        bool sentOk = false;
                        int tries = 0;

                        while (!sentOk && tries < MAX_RETRY)
                        {
                            tries++;
                            serialPort.Write(frame, 0, frame.Length);

                            // Wait for ACK/NAK/CAN
                            bool gotResponse = false;
                            DateTime respStart = DateTime.Now;
                            while ((DateTime.Now - respStart).TotalMilliseconds < 2000) // 2s for response
                            {
                                int r = ReadByteWithTimeout(serialPort, 200);
                                if (r == -1) continue;

                                if (r == ACK)
                                {
                                    sentOk = true;
                                    gotResponse = true;
                                    break;
                                }
                                else if (r == NAK)
                                {
                                    // resend
                                    gotResponse = true;
                                    break;
                                }
                                else if (r == CAN)
                                {
                                    MessageBox.Show("❌ Transfer canceled by receiver (CAN).");
                                    return;
                                }
                                // Ignore other bytes
                            }

                            if (!gotResponse)
                            {
                                // No response in window → retry
                            }
                        }

                        if (!sentOk)
                        {
                            MessageBox.Show($"❌ Block #{blockNum} failed after {MAX_RETRY} retries.");
                            return;
                        }

                        // (Optional) You can raise progress here if you wire a UI callback.
                        // e.g., mainForm.BeginInvoke(... percent ...)
                    }

                    // 3) Send EOT and wait for final ACK
                    int eotTries = 0;
                    bool eotAck = false;
                    while (eotTries < 10 && !eotAck)
                    {
                        eotTries++;
                        serialPort.Write(new byte[] { EOT }, 0, 1);

                        DateTime waitAckStart = DateTime.Now;
                        while ((DateTime.Now - waitAckStart).TotalMilliseconds < 3000) // up to 3s
                        {
                            int r = ReadByteWithTimeout(serialPort, 300);
                            if (r == -1) continue;
                            if (r == ACK) { eotAck = true; break; }
                            if (r == CAN)
                            {
                                MessageBox.Show("❌ Transfer canceled by receiver (CAN) after EOT.");
                                return;
                            }
                            // Some receivers may NAK EOT; send EOT again on next loop
                        }
                    }

                    if (!eotAck)
                    {
                        MessageBox.Show("❌ EOT not acknowledged after 10 attempts.");
                        return;
                    }

                    TimeSpan total = DateTime.Now - transferStart;
                    MessageBox.Show($"✅ File transfer complete!\n⏱️ {total.TotalMinutes:F0}분 {total.Seconds}초");
                }
                finally
                {
                    // Restore timeouts
                    serialPort.ReadTimeout = originalReadTimeout;
                    serialPort.WriteTimeout = originalWriteTimeout;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Transfer failed: {ex.Message}");
            }
        }


        public SerialPort GetPort() => serialPort;
    }
}
