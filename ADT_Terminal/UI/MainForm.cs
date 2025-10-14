using System;
using System.Windows.Forms;
using EmbedBox.Features.Download;
using EmbedBox.Features.MakeData;
using EmbedBox.Utils;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        private DownloadManager downloadManager;
        private MakeDataManager makeDataManager;
        private FileHelper fileHelper;

        public MainForm()
        {
            InitializeComponent();

            downloadManager = new DownloadManager(this);
            makeDataManager = new MakeDataManager(this);
            fileHelper = new FileHelper();

            /** 이벤트 연결 **/
            comboBoxPort.SelectedIndexChanged += comboBoxPort_SelectedIndexChanged;
        }

        /** ✅ [1] 폼이 로드될 때 자동으로 호출됨 **/
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshSerialPorts();   // ← 여기서 자동 포트 목록 로드
        }

        /** ✅ [2] 시리얼 포트 목록 갱신 함수 **/
        private void RefreshSerialPorts()
        {
            comboBoxPort.Items.Clear();

            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                comboBoxPort.Items.Add("No Ports Found");
                comboBoxPort.SelectedIndex = 0;
            }
            else
            {
                comboBoxPort.Items.AddRange(ports);
                comboBoxPort.SelectedIndex = 0;
            }
        }

        /** ------------------------------------------------------------------
         *  [Serial Config] - 포트 변경 시 자동 오픈
         *  ------------------------------------------------------------------ */
        private void comboBoxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            downloadManager.OpenPort(
                comboBoxPort,
                comboBoxBaudrate,
                comboBoxDataBits,
                comboBoxStopBits,
                comboBoxParity);
        }
        /** ------------------------------------------------------------------
         *  [Download] - 파일 선택 후 전송
         *  ------------------------------------------------------------------ */
        private void buttonSend_Click(object? sender, EventArgs e)
        {
            string? filePath = fileHelper.SelectSendFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                downloadManager.SetSendFile(filePath);
                downloadManager.SendFile();
            }
        }

        /** ------------------------------------------------------------------
         *  [Help] - 도움말 창
         *  ------------------------------------------------------------------ */
        private void buttonHelp_Click(object? sender, EventArgs e)
        {
            ShowPanel(panelHelp);
        }

        /** ------------------------------------------------------------------
         *  [MakeData] - Record 생성
         *  CDNS_XSPI_HEADER / CDNS_XSPI_HEADER_BOOTCODE
         *  옵션에 따라 includeBoot 자동 결정
         *  ------------------------------------------------------------------ */
        private void buttonMakeRecord_Click(object? sender, EventArgs e)
        {
            bool includeBoot = comboBoxMakeOption.SelectedIndex == 1;  // 0: HEADER, 1: HEADER_BOOTCODE

            makeDataManager.MakeRecord(
                textBoxPageSize.Text,
                textBoxImageOffset.Text,
                textBoxHostAddr.Text,
                includeBoot);

            string message = includeBoot
                ? "✅ MakeData (CDNS_XSPI_HEADER_BOOTCODE) completed!"
                : "✅ MakeData (CDNS_XSPI_HEADER) completed!";
            MessageBox.Show(message, "MakeData", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /** ------------------------------------------------------------------
         *  [Textbox 입력 보정] - "0x" 자동 추가
         *  ------------------------------------------------------------------ */
        private void textBox_TextChanged(object? sender, EventArgs e)
        {
            if (sender is TextBox tb && !tb.Text.StartsWith("0x"))
            {
                tb.Text = "0x" + tb.Text;
                tb.SelectionStart = tb.Text.Length;
            }
        }

        /** ------------------------------------------------------------------
         *  [Form 종료 시 시리얼 포트 닫기]
         *  ------------------------------------------------------------------ */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            downloadManager.ClosePort();
            base.OnFormClosing(e);
        }
    }
}
