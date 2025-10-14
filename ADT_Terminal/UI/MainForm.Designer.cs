using System.Windows.Forms;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        private Panel panelHelp; // ✅ 추가

        private void InitializeComponent()
        {
            InitializeBaseForm();
            InitializeMenuPanel();
            InitializeDownloadPanel();
            InitializeMakeDataPanel();
            InitializeHelpPanel(); // ✅ Help 패널 추가

            Controls.AddRange(new Control[] { panelMenu, panelDownload, panelMakeData, panelHelp });
        }

        /** 폼 기본 설정 **/
        private void InitializeBaseForm()
        {
            Text = "EmbedBox - Embedded Data Utility";
            ClientSize = new System.Drawing.Size(900, 420);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        /** 패널 전환 **/
        private void ShowPanel(Panel target)
        {
            panelDownload.Visible = false;
            panelMakeData.Visible = false;
            panelHelp.Visible = false; // ✅ Help 패널도 숨김
            target.Visible = true;
        }

        /** Help Panel 구성 **/
        private void InitializeHelpPanel()
        {
            panelHelp = CreatePanel(190, 10, 690, 400);
            panelHelp.Visible = false; // 기본 비활성화

            var groupHelp = CreateGroup("Help & Information", 10, 10, 660, 380);

            var labelInfo = new Label()
            {
                Text = "📘 EmbedBox 사용 가이드\n\n" +
                       "1️⃣ [Download]\n" +
                       "   - PC에서 Board로 데이터를 전송합니다.\n" +
                       "   - Protocol: XMODEM\n" +
                       "   - Port 및 Baudrate 설정 후 Download 버튼 클릭\n\n" +
                       "2️⃣ [MakeData]\n" +
                       "   - 이미지 파일을 Header 형식으로 변환합니다.\n" +
                       "   - 옵션: CDNS_XSPI_HEADER / CDNS_XSPI_HEADER_BOOTCODE\n\n" +
                       "3️⃣ [Help]\n" +
                       "   - 이 화면에서는 프로그램 사용법과 옵션 설명을 제공합니다.",
                Location = new System.Drawing.Point(20, 30),
                AutoSize = false,
                Width = 600,
                Height = 300
            };

            var buttonOpenDocs = new Button()
            {
                Text = "Open Documentation",
                Location = new System.Drawing.Point(20, 330),
                Size = new System.Drawing.Size(200, 30)
            };
            buttonOpenDocs.Click += (s, e) =>
            {
                MessageBox.Show("현재는 로컬 문서 링크가 없습니다.\n" +
                                "추후 온라인 매뉴얼 또는 PDF 파일로 연결됩니다.",
                                "EmbedBox Help",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            };

            groupHelp.Controls.Add(labelInfo);
            groupHelp.Controls.Add(buttonOpenDocs);
            panelHelp.Controls.Add(groupHelp);
        }
    }
}
