using System;
using System.Windows.Forms;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        private Panel panelDownload;
        private GroupBox groupPortConfig, groupProtocolConfig, groupDownload;
        private ComboBox comboBoxPort, comboBoxBaudrate, comboBoxDataBits,
                         comboBoxParity, comboBoxStopBits;
        private ComboBox comboBoxProtocol, comboBoxBlockSize, comboBoxError;
        private ProgressBar progressBarDownload;
        private Label labelDownloadStatus;

        /** Download 패널 초기화 **/
        private void InitializeDownloadPanel()
        {
            panelDownload = CreatePanel(190, 10, 690, 400);
            InitializePortGroup();
            InitializeProtocolGroup();
            InitializeDownloadGroup();

            panelDownload.Controls.AddRange(new Control[]
            {
                groupPortConfig, groupProtocolConfig, groupDownload
            });
        }

        /** Port Configuration **/
        private void InitializePortGroup()
        {
            groupPortConfig = CreateGroup("Port Configuration", 10, 10, 320, 180);

            comboBoxPort = CreateCombo("Port", 120, 27, groupPortConfig, 15, 30);
            comboBoxBaudrate = CreateCombo("Baudrate", 120, 57, groupPortConfig, 15, 60, new[] { "9600", "115200", "921600" });
            
            comboBoxDataBits = CreateCombo("Data Bits", 120, 87, groupPortConfig, 15, 90, new[] { "8", "7", "6", "5", });
            comboBoxParity = CreateCombo("Parity", 120, 117, groupPortConfig, 15, 120, new[] { "None", "Odd", "Even" });
            comboBoxStopBits = CreateCombo("Stop Bits", 120, 147, groupPortConfig, 15, 150, new[] { "1", "1.5", "2" });

            comboBoxBaudrate.SelectedItem = "115200";
            comboBoxDataBits.SelectedIndex = comboBoxParity.SelectedIndex = comboBoxStopBits.SelectedIndex = 0;
        }

        /** Protocol Configuration **/
        private void InitializeProtocolGroup()
        {
            groupProtocolConfig = CreateGroup("Protocol Configuration", 350, 10, 320, 180);

            comboBoxProtocol = CreateCombo("Protocol", 120, 27, groupProtocolConfig, 15, 30, new[] { "XMODEM" });
            comboBoxBlockSize = CreateCombo("Block Size", 120, 67, groupProtocolConfig, 15, 70, new[] { "128", "1024" });
            comboBoxError = CreateCombo("Error Type", 120, 107, groupProtocolConfig, 15, 110, new[] { "CRC16" });

            comboBoxProtocol.SelectedIndex = comboBoxError.SelectedIndex = 0;
            comboBoxBlockSize.SelectedIndex = 1;
        }

        /** Download Group **/
        private void InitializeDownloadGroup()
        {
            groupDownload = CreateGroup("Download", 10, 210, 660, 160);

            progressBarDownload = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(620, 20)
            };

            labelDownloadStatus = new Label
            {
                Text = "0 / 0 bytes (0 bps)",
                Location = new System.Drawing.Point(20, 80),
                Width = 400
            };

            var buttonDownload = new Button
            {
                Text = "Download",
                Location = new System.Drawing.Point(20, 110),
                Size = new System.Drawing.Size(100, 30)
            };
            buttonDownload.Click += new EventHandler(buttonSend_Click);

            groupDownload.Controls.AddRange(new Control[]
            {
                progressBarDownload, labelDownloadStatus, buttonDownload
            });
        }
    }
}
