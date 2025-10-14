using System;
using System.Windows.Forms;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        /** 좌측 메뉴 패널 및 버튼 **/
        private Panel panelMenu;
        private Button buttonMenuDownload;
        private Button buttonMenuMakeData;
        private Button buttonMenuHelp;

        /** 좌측 메뉴 초기화 **/
        private void InitializeMenuPanel()
        {
            panelMenu = new Panel
            {
                Location = new System.Drawing.Point(0, 0),
                Size = new System.Drawing.Size(180, 420),
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            buttonMenuDownload = CreateMenuButton("Download\n(PC → Board)", 40);
            buttonMenuDownload.Click += (s, e) => ShowPanel(panelDownload);

            buttonMenuMakeData = CreateMenuButton("MakeData", 110);
            buttonMenuMakeData.Click += (s, e) => ShowPanel(panelMakeData);

            buttonMenuHelp = CreateMenuButton("Help", 180);
            buttonMenuHelp.Click += new EventHandler(buttonHelp_Click);

            panelMenu.Controls.AddRange(new Control[]
            {
                buttonMenuDownload, buttonMenuMakeData, buttonMenuHelp
            });
        }

        /** 메뉴 버튼 생성 헬퍼 **/
        private Button CreateMenuButton(string text, int y)
        {
            return new Button
            {
                Text = text,
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(140, 50),
                FlatStyle = FlatStyle.Standard
            };
        }
    }
}
