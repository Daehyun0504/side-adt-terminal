using System.Windows.Forms;

namespace EmbedBox.UI
{
    public partial class MainForm : Form
    {
        /** 공통 Panel 생성 **/
        private Panel CreatePanel(int x, int y, int width, int height)
        {
            return new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height)
            };
        }

        /** 공통 GroupBox 생성 **/
        private GroupBox CreateGroup(string title, int x, int y, int width, int height)
        {
            return new GroupBox
            {
                Text = title,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height)
            };
        }

        /** Label + ComboBox 생성 **/
        private ComboBox CreateCombo(string labelText, int cx, int cy, Control parent, int lx, int ly, string[]? items = null)
        {
            var label = new Label { Text = labelText, Location = new System.Drawing.Point(lx, ly) };
            var combo = new ComboBox { Location = new System.Drawing.Point(cx, cy), Width = 160 };
            if (items != null) combo.Items.AddRange(items);
            parent.Controls.Add(label);
            parent.Controls.Add(combo);
            return combo;
        }

        /** Label + TextBox 생성 **/
        private TextBox CreateText(string labelText, string defaultValue, int tx, int ty, Control parent, int lx, int ly)
        {
            var label = new Label { Text = labelText, Location = new System.Drawing.Point(lx, ly) };
            var text = new TextBox { Location = new System.Drawing.Point(tx, ty), Width = 200, Text = defaultValue };
            parent.Controls.Add(label);
            parent.Controls.Add(text);
            return text;
        }
    }
}
