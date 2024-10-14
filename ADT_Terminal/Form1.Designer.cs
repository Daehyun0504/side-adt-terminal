using System;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;
using System.Resources;


namespace ADT_Terminal
{
    public partial class Form1 : Form
    {
        private const int BLOCK_SIZE = 1024;
        private const byte SOH = 0x01;  // Start of Header for XMODEM
        private const byte STX = 0x02;  // Start of Header for XMODEM
        private const byte EOT = 0x04;  // End of Transmission
        private const byte ACK = 0x06;  // Acknowledge
        private const byte NAK = 0x15;  // Negative Acknowledge
        private const byte CAN = 0x18;  // Cancel Transmission
        private const byte C = 0x43;    // 'C' for CRC-16 mode

        private System.Windows.Forms.Button buttonFile;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.ComboBox comboBoxPort;
        private System.Windows.Forms.ComboBox comboBoxBaudrate;
        private System.Windows.Forms.ComboBox comboBoxDataBits;
        private System.Windows.Forms.ComboBox comboBoxParity;
        private System.Windows.Forms.ComboBox comboBoxStopBits;
        private System.Windows.Forms.ComboBox comboBoxFlowControl;
        private SerialPort serialPort1;
        private OpenFileDialog openFileDialog1;
        private string selectedFilePath;

        // 새로 추가할 ProgressBar
        private ProgressBar progressBar;
        private Label labelPort;         
        private Label labelBaudrate;     
        private Label labelDataBits;     
        private Label labelParity;       
        private Label labelStopBits;     
        private Label labelFlowControl;

        // 폼 종료 시 시리얼 포트 닫는 메서드
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                MessageBox.Show("Serial Port Closed");
            }
            base.OnFormClosing(e);
        }

        // Port가 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // Baudrate가 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxBaudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // Data Bits가 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // Parity가 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxParity_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // Stopbits가 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // Flow Control이 설정됐을 때 포트를 재오픈하는 메서드
        private void comboBoxFlowControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenSerialPort();
        }

        // 포트를 설정하고 여는 메서드
        private void OpenSerialPort()
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();  // 이미 열린 포트가 있으면 닫기
                }

                // 포트 설정 및 열기
                serialPort1.PortName = comboBoxPort.Text;
                serialPort1.BaudRate = int.Parse(comboBoxBaudrate.Text);
                serialPort1.DataBits = int.Parse(comboBoxDataBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBoxParity.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBoxStopBits.Text);
                serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), comboBoxFlowControl.Text);

                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
                serialPort1.Open();

                MessageBox.Show("Serial Port Opened");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        // 파일 선택 버튼 클릭 이벤트 메서드
        private void buttonFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Binary Files|*.bin";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog1.FileName;
                MessageBox.Show("File Selected: " + selectedFilePath);
            }
        }

        // Help 버튼 클릭 이벤트 핸들러 메서드
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            string projectInfo =
                "■ Program Guide\n" +
                "1. Configuration 설정\n" +
                "   - Port (첫 번째로 설정)\n" +
                "   - Baudrate\n" +
                "   - Data bits\n" +
                "   - Parity\n" +
                "   - Stop Bits\n" +
                "   - Flow Control\n" +
                "2. Board에서 시작 비트(\"C\") 전송\n\n" +
                "■ Program Info\n" +
                "- Purpose : Board Boot Code Download\n" +
                "- Platform : \n" +  // Platform에 해당하는 내용을 추가하세요
                "- Board Code : TF-A\n" +
                "- Bootcode : Flash Boot code\n" +
                "- Protocol : XMODEM\n" +
                "- Protocol Custom Specification:\n" +
                "  <- 1K MODEM\n" +
                "  <- Timeout : 3초씩 10번 이상 타임아웃시 통신 종료";

            MessageBox.Show(projectInfo, "Project Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // XMODEM 방식으로 파일 전송 (CRC-16 사용) 메서드
        private void SendFileUsingXmodem(byte[] fileBytes)
        {
            int totalBlocks = (fileBytes.Length + BLOCK_SIZE - 1) / BLOCK_SIZE;  // 1024바이트로 나누기

            for (int blockNumber = 1; blockNumber <= totalBlocks; blockNumber++)
            {
                // 데이터 블록 준비
                byte[] dataBlock = new byte[BLOCK_SIZE];
                int offset = (blockNumber - 1) * BLOCK_SIZE;
                int bytesToSend = Math.Min(BLOCK_SIZE, fileBytes.Length - offset);
                Array.Copy(fileBytes, offset, dataBlock, 0, bytesToSend);

                // 나머지 공간을 0x1A로 패딩
                for (int i = bytesToSend; i < BLOCK_SIZE; i++)
                {
                    dataBlock[i] = 0x1A;
                }

                bool blockSentSuccessfully = false;

                // 블록 전송 및 재전송 루프
                while (!blockSentSuccessfully)
                {
                    // 블록을 구성
                    byte[] block = new byte[BLOCK_SIZE + 5];  // STX + 블록 번호 + 보수 + 데이터 + CRC-16
                    block[0] = STX;  // Start of Header
                    block[1] = (byte)blockNumber;  // 블록 번호
                    block[2] = (byte)(255 - blockNumber);  // 블록 번호의 보수

                    // 데이터 복사
                    Array.Copy(dataBlock, 0, block, 3, BLOCK_SIZE);

                    // CRC-16 계산
                    ushort crc = CalculateCRC16(dataBlock);
                    block[BLOCK_SIZE + 3] = (byte)((crc >> 8) & 0xFF);  // CRC 상위 바이트
                    block[BLOCK_SIZE + 4] = (byte)(crc & 0xFF);         // CRC 하위 바이트

                    // 블록 전송
                    serialPort1.Write(block, 0, block.Length);

                    // ACK 또는 NAK 대기 (최대 10번 3초 대기)
                    bool ackOrNakReceived = false;
                    for (int attempt = 0; attempt < 10; attempt++)
                    {
                        // 3초 대기
                        System.Threading.Thread.Sleep(3000);

                        if (serialPort1.BytesToRead > 0) // 수신된 데이터가 있는 경우
                        {
                            byte response = (byte)serialPort1.ReadByte();
                            if (response == ACK)
                            {
                                blockSentSuccessfully = true;
                                ackOrNakReceived = true;
                                break; // ACK를 받으면 대기 종료 및 다음 블록으로 진행
                            }
                            else if (response == NAK)
                            {
                                // NAK를 받으면 블록을 재전송
                                MessageBox.Show("NAK received, resending block...");
                                ackOrNakReceived = true;
                                break; // 블록을 재전송할 수 있도록 루프를 종료
                            }
                        }
                    }

                    if (!ackOrNakReceived)
                    {
                        MessageBox.Show("Error: Transmission not acknowledged after 10 attempts.");
                        return;  // 전송 중단
                    }
                }

                // **진행 상황 업데이트**: ProgressBar 갱신
                int progressPercentage = (blockNumber * 100) / totalBlocks;
                progressBar.Invoke(new Action(() => progressBar.Value = progressPercentage)); // UI 스레드에서 ProgressBar 업데이트
            }

            // 전송 완료 시 EOT 전송
            serialPort1.Write(new byte[] { EOT }, 0, 1);

            // 마지막으로 ACK 확인 (최대 10번 3초 대기)
            bool eotAckReceived = false;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // 3초 대기
                System.Threading.Thread.Sleep(3000);

                if (serialPort1.BytesToRead > 0) // 수신된 데이터가 있는 경우
                {
                    byte finalResponse = (byte)serialPort1.ReadByte();
                    if (finalResponse == ACK)
                    {
                        eotAckReceived = true;
                        break; // ACK를 받으면 대기 종료
                    }
                    else
                    {
                        // ACK가 아니면 대기하고 재시도 (다른 데이터를 무시할 수도 있음)
                        MessageBox.Show($"Unexpected response received: {finalResponse}");
                    }
                }
                else
                {
                    // 3초 대기 후에도 ACK를 받지 못했을 때, EOT를 다시 전송
                    serialPort1.Write(new byte[] { EOT }, 0, 1);
                }
            }

            if (!eotAckReceived)
            {
                MessageBox.Show("Error: EOT not acknowledged after 10 attempts.");
                return;  // 전송 중단
            }

            MessageBox.Show("Download Complete");
        }

        // CRC-16 계산 메서드
        private ushort CalculateCRC16(byte[] data)
        {
            ushort crc = 0x0000;
            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);  // 상위 바이트와 XOR
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ 0x1021);  // CRC-16 다항식 0x1021
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }

        // 데이터 수신 메서드
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 수신된 데이터 읽기
                string receivedData = serialPort1.ReadExisting();
                if (receivedData.Contains(((char)C).ToString()))  // 수신된 값이 'C'라면
                {
                    // 파일이 선택되어 있는지 확인
                    if (!string.IsNullOrEmpty(selectedFilePath))
                    {
                        byte[] fileBytes = File.ReadAllBytes(selectedFilePath);
                        SendFileUsingXmodem(fileBytes);  // XMODEM 전송 시작 (CRC-16)
                    }
                    else
                    {
                        MessageBox.Show("Please select a file first.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving data: {ex.Message}");
            }
        }

        // 초기화 메서드  
        private void InitializeComponent()
        {
            this.buttonFile = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button(); 
            this.comboBoxPort = new System.Windows.Forms.ComboBox();
            this.comboBoxBaudrate = new System.Windows.Forms.ComboBox();
            this.comboBoxDataBits = new System.Windows.Forms.ComboBox();
            this.comboBoxParity = new System.Windows.Forms.ComboBox();
            this.comboBoxStopBits = new System.Windows.Forms.ComboBox();
            this.comboBoxFlowControl = new System.Windows.Forms.ComboBox();
            this.serialPort1 = new SerialPort();
            this.openFileDialog1 = new OpenFileDialog();

            // 새로 추가된 ProgressBar
            this.progressBar = new ProgressBar();
            this.labelPort = new Label();
            this.labelBaudrate = new Label();
            this.labelDataBits = new Label();
            this.labelParity = new Label();
            this.labelStopBits = new Label();
            this.labelFlowControl = new Label();
            this.SuspendLayout();

            // Help 버튼 설정
            this.buttonHelp.Location = new System.Drawing.Point(100, 170);  // Select File 버튼 옆에 위치
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(75, 23);
            this.buttonHelp.TabIndex = 2;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click); // 클릭 이벤트 연결

            // ComboBox와 Label 설정
            this.comboBoxPort.FormattingEnabled = true;
            this.comboBoxPort.Location = new System.Drawing.Point(20, 40);  // 콤보박스 위치 조정
            this.comboBoxPort.Name = "comboBoxPort";
            this.comboBoxPort.Size = new System.Drawing.Size(121, 23);
            this.comboBoxPort.Items.AddRange(SerialPort.GetPortNames());
            comboBoxPort.SelectedIndexChanged += comboBoxPort_SelectedIndexChanged;

            // Port Label
            this.labelPort.Location = new System.Drawing.Point(20, 20);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(121, 20);
            this.labelPort.Text = "Port";

            // Baudrate ComboBox
            this.comboBoxBaudrate.FormattingEnabled = true;
            this.comboBoxBaudrate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            this.comboBoxBaudrate.Location = new System.Drawing.Point(160, 40);
            this.comboBoxBaudrate.Name = "comboBoxBaudrate";
            this.comboBoxBaudrate.Size = new System.Drawing.Size(121, 23);
            this.comboBoxBaudrate.SelectedIndex = 4;
            this.comboBoxBaudrate.SelectedIndexChanged += new System.EventHandler(this.comboBoxPort_SelectedIndexChanged);

            // Baudrate Label
            this.labelBaudrate.Location = new System.Drawing.Point(160, 20);
            this.labelBaudrate.Name = "labelBaudrate";
            this.labelBaudrate.Size = new System.Drawing.Size(121, 20);
            this.labelBaudrate.Text = "Baudrate";

            // DataBits ComboBox
            this.comboBoxDataBits.FormattingEnabled = true;
            this.comboBoxDataBits.Items.AddRange(new object[] { "7", "8" });
            this.comboBoxDataBits.Location = new System.Drawing.Point(300, 40);
            this.comboBoxDataBits.Name = "comboBoxDataBits";
            this.comboBoxDataBits.Size = new System.Drawing.Size(121, 23);
            this.comboBoxDataBits.SelectedIndex = 1;
            this.comboBoxDataBits.SelectedIndexChanged += new System.EventHandler(this.comboBoxPort_SelectedIndexChanged);

            // DataBits Label
            this.labelDataBits.Location = new System.Drawing.Point(300, 20);
            this.labelDataBits.Name = "labelDataBits";
            this.labelDataBits.Size = new System.Drawing.Size(121, 20);
            this.labelDataBits.Text = "Data Bits";

            // Parity ComboBox
            this.comboBoxParity.FormattingEnabled = true;
            this.comboBoxParity.Items.AddRange(new object[] { "None", "Odd", "Even" });
            this.comboBoxParity.Location = new System.Drawing.Point(20, 90);
            this.comboBoxParity.Name = "comboBoxParity";
            this.comboBoxParity.Size = new System.Drawing.Size(121, 23);
            this.comboBoxParity.SelectedIndex = 0;
            this.comboBoxParity.SelectedIndexChanged += new System.EventHandler(this.comboBoxPort_SelectedIndexChanged);

            // Parity Label
            this.labelParity.Location = new System.Drawing.Point(20, 70);
            this.labelParity.Name = "labelParity";
            this.labelParity.Size = new System.Drawing.Size(121, 20);
            this.labelParity.Text = "Parity";

            // StopBits ComboBox
            this.comboBoxStopBits.FormattingEnabled = true;
            this.comboBoxStopBits.Items.AddRange(new object[] { "1", "2" });
            this.comboBoxStopBits.Location = new System.Drawing.Point(160, 90);
            this.comboBoxStopBits.Name = "comboBoxStopBits";
            this.comboBoxStopBits.Size = new System.Drawing.Size(121, 33);
            this.comboBoxStopBits.SelectedIndex = 0;
            this.comboBoxStopBits.SelectedIndexChanged += new System.EventHandler(this.comboBoxPort_SelectedIndexChanged);

            // StopBits Label
            this.labelStopBits.Location = new System.Drawing.Point(160, 70);
            this.labelStopBits.Name = "labelStopBits";
            this.labelStopBits.Size = new System.Drawing.Size(121, 20);
            this.labelStopBits.Text = "Stop Bits";

            // FlowControl ComboBox
            this.comboBoxFlowControl.FormattingEnabled = true;
            this.comboBoxFlowControl.Items.AddRange(new object[] { "None", "XOnXOff", "RTS/CTS", "DTR/DSR" });
            this.comboBoxFlowControl.Location = new System.Drawing.Point(300, 90);
            this.comboBoxFlowControl.Name = "comboBoxFlowControl";
            this.comboBoxFlowControl.Size = new System.Drawing.Size(121, 23);
            this.comboBoxFlowControl.SelectedIndex = 0;
            this.comboBoxFlowControl.SelectedIndexChanged += new System.EventHandler(this.comboBoxPort_SelectedIndexChanged);

            // FlowControl Label
            this.labelFlowControl.Location = new System.Drawing.Point(300, 70);
            this.labelFlowControl.Name = "labelFlowControl";
            this.labelFlowControl.Size = new System.Drawing.Size(121, 20);
            this.labelFlowControl.Text = "Flow Control";

            // ProgressBar 설정
            this.progressBar.Location = new System.Drawing.Point(20, 130);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(400, 23);
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;

            // 버튼 설정
            this.buttonFile.Location = new System.Drawing.Point(20, 170);
            this.buttonFile.Name = "buttonFile";
            this.buttonFile.Size = new System.Drawing.Size(75, 23);
            this.buttonFile.TabIndex = 1;
            this.buttonFile.Text = "Select File";
            this.buttonFile.UseVisualStyleBackColor = true;
            this.buttonFile.Click += new System.EventHandler(this.buttonFile_Click);

            // Form 설정
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 200);
            this.Controls.Add(this.labelPort);  // Label 추가
            this.Controls.Add(this.comboBoxPort);
            this.Controls.Add(this.labelBaudrate);  // Label 추가
            this.Controls.Add(this.comboBoxBaudrate);
            this.Controls.Add(this.labelDataBits);  // Label 추가
            this.Controls.Add(this.comboBoxDataBits);
            this.Controls.Add(this.labelParity);  // Label 추가
            this.Controls.Add(this.comboBoxParity);
            this.Controls.Add(this.labelStopBits);  // Label 추가
            this.Controls.Add(this.comboBoxStopBits);
            this.Controls.Add(this.labelFlowControl);  // Label 추가
            this.Controls.Add(this.comboBoxFlowControl);
            this.Controls.Add(this.progressBar);  // ProgressBar 추가
            this.Controls.Add(this.buttonFile);
            this.Controls.Add(this.buttonHelp);  // Help 버튼 추가


            this.Icon = ADT_Code_Downloader.Properties.Resources.ad_logo;
            
            this.Name = "Form1";
            this.Text = "ADT Code Downloader";
            this.ResumeLayout(false);
        }
    }
}
