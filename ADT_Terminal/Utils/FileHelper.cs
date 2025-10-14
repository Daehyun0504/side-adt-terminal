using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EmbedBox.Utils
{
    public class FileHelper
    {
        public string? SelectSendFile()  
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select file to send",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public static string SaveFileDialog(string title, string defaultFileName)
        {
            using var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                FileName = defaultFileName
            };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }

        public static ulong ParseHex(string input)
        {
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return Convert.ToUInt64(input.Substring(2), 16);
            return Convert.ToUInt64(input);
        }
    }

    public static class XspiHeaderBuilder
    {
        public static byte[] CreateHeader(uint mainDataSize, ulong imageOffset, ulong hostAddress, ushort pageSize)
        {
            byte[] record = new byte[32];
            Array.Clear(record, 0, record.Length);

            Buffer.BlockCopy(BitConverter.GetBytes(mainDataSize), 0, record, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(imageOffset), 0, record, 8, 6);
            Buffer.BlockCopy(BitConverter.GetBytes(hostAddress), 0, record, 16, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(pageSize), 0, record, 24, 2);
            return record;
        }
    }

    public static class UIHelper
    {
        public static void ShowHelp()
        {
            string info =
                "Guide For Alpine Project\n\n" +
                "1. Translating Binary\n" +
                "- Set combo boxes\n" +
                "- Click button (Translate)\n" +
                "- Choose Origin File and Save File\n\n" +
                "2. Sending Binary\n" +
                "- Set combo boxes\n" +
                "- Click (Send)\n" +
                "- Choose Binary File\n\n" +
                "Help & Support\n" +
                "Call: 010-6234-8812\n" +
                "Email: yoondh@adtek.co.kr";
            MessageBox.Show(info, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
