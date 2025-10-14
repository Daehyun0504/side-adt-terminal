using System;
using System.Windows.Forms;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        private Panel panelMakeData;
        private GroupBox groupMakeDataConfig;
        private ComboBox comboBoxMakeOption;
        private TextBox textBoxDataSize, textBoxImageOffset, textBoxHostAddr, textBoxPageSize;

        /** MakeData 패널 초기화 **/
        private void InitializeMakeDataPanel()
        {
            panelMakeData = CreatePanel(190, 10, 690, 400);
            panelMakeData.Visible = false;

            groupMakeDataConfig = CreateGroup("MakeData Configuration", 10, 10, 660, 260);

            comboBoxMakeOption = CreateCombo("Make Option", 160, 37, groupMakeDataConfig, 20, 40,
                new[] { "CDNS_XSPI_HEADER", "CDNS_XSPI_HEADER_BOOTCODE" });
            comboBoxMakeOption.SelectedIndex = 0;

            textBoxDataSize = CreateText("Data Size (byte)", "0", 160, 77, groupMakeDataConfig, 20, 80);
            textBoxImageOffset = CreateText("Image Offset (hex)", "0x0", 160, 117, groupMakeDataConfig, 20, 120);
            textBoxHostAddr = CreateText("Host Address (hex)", "0x0", 160, 157, groupMakeDataConfig, 20, 160);
            textBoxPageSize = CreateText("Page Size (byte)", "512", 160, 197, groupMakeDataConfig, 20, 200);

            var buttonMakeData = new Button
            {
                Text = "MakeData",
                Location = new System.Drawing.Point(160, 230),
                Size = new System.Drawing.Size(120, 30)
            };
            buttonMakeData.Click += new EventHandler(buttonMakeRecord_Click);

            groupMakeDataConfig.Controls.Add(buttonMakeData);
            panelMakeData.Controls.Add(groupMakeDataConfig);
        }
    }
}
