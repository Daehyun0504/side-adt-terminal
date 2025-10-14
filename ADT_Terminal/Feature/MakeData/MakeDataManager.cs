using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EmbedBox.Utils;

namespace EmbedBox.Features.MakeData
{
    public class MakeDataManager
    {
        private readonly Form mainForm;

        public MakeDataManager(Form form)
        {
            mainForm = form;
        }

        /** ------------------------------------------------------------------
         *  [MakeData] Record 생성 (Record Only / Record + Boot)
         *  ------------------------------------------------------------------ */
        public void MakeRecord(string pageSizeText, string imageOffsetText, string hostAddrText, bool includeBoot)
        {
            try
            {
                /** 1) 입력 파싱 */
                ushort pageSize = Convert.ToUInt16(pageSizeText);                // 10진수
                ulong imageOffset = FileHelper.ParseHex(imageOffsetText);        // 16진수
                ulong hostAddress = FileHelper.ParseHex(hostAddrText);           // 16진수

                /** 2) 기본 유효성 검사 */
                if (imageOffset > 0xFFFFFFFFFFFFUL) { MessageBox.Show("imageOffset must not exceed 48 bits"); return; }
                if (hostAddress > 0xFFFFFFFFFFFFFFFF) { MessageBox.Show("hostAddress must not exceed 64 bits"); return; }
                if (pageSize > 0xFFFF) { MessageBox.Show("pageSize must not exceed 16 bits"); return; }

                // Record + Boot: 헤더(32B)와 겹치지 않도록 최소 0x20 보장
                if (includeBoot && imageOffset < 0x20)
                {
                    MessageBox.Show(
                        "imageOffset must be at least 0x20 (32) to avoid overlapping the configuration record.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                /** 3) 파일 선택 전 안내 */
                string preOpenMsg = includeBoot
                    ? "Select Boot Code Binary\n\n" +
                      "• RAW .bin 파일을 선택하세요.\n" +
                      "• 선택한 파일은 저장 파일에 실제로 포함됩니다.\n" +
                      "• 헤더(32B) 뒤에 0xFF 패딩을 imageOffset까지 채운 다음 부트 코드가 이어집니다."
                    : "Select Boot Code Binary (Record Only - size only)\n\n" +
                      "• RAW .bin 파일을 선택하세요.\n" +
                      "• 선택한 파일의 크기만 헤더에 기록되고, 본문은 저장 파일에 포함되지 않습니다.";
                MessageBox.Show(preOpenMsg, "Open Boot Code", MessageBoxButtons.OK, MessageBoxIcon.Information);

                /** 4) 부트코드 선택 */
                string bootPath;
                uint mainDataSize;
                byte[] bootBytes = Array.Empty<byte>();

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = includeBoot ? "Select Boot Code Binary (Record + Boot)" : "Select Boot Code Binary (Record Only - size only)";
                    ofd.Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";

                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("No file selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    bootPath = ofd.FileName;
                    FileInfo fi = new FileInfo(bootPath);
                    mainDataSize = (uint)fi.Length;

                    if (includeBoot)
                        bootBytes = File.ReadAllBytes(bootPath);
                }

                /** 5) 선택 요약 안내 */
                {
                    string summary = includeBoot
                        ? $"Selected file will be EMBEDDED.\n\n" +
                          $"• Path : {bootPath}\n" +
                          $"• Size : {mainDataSize} bytes\n" +
                          $"• imageOffset : 0x{imageOffset:X}\n" +
                          $"• Header : 32 bytes (fixed)\n\n" +
                          "다음 단계에서 저장 전 전체 용량(헤더+패딩+부트)을 보여줍니다."
                        : $"Selected file SIZE will be recorded ONLY.\n\n" +
                          $"• Path : {bootPath}\n" +
                          $"• Size : {mainDataSize} bytes\n" +
                          $"• Header : 32 bytes (fixed)\n\n" +
                          "다음 단계에서 헤더만 저장합니다.";
                    MessageBox.Show(summary, "Selection Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                /** 6) 헤더 생성 (data size 포함) */
                // XspiHeaderBuilder.CreateHeader(uint dataSize, ulong imageOffset, ulong hostAddr, ushort pageSize)
                byte[] header = XspiHeaderBuilder.CreateHeader(mainDataSize, imageOffset, hostAddress, pageSize);

                /** 7) 저장 전 용량/레이아웃 안내 및 확인 */
                const ulong CONFIG_SIZE = 32; // 헤더 고정 32바이트
                ulong paddingSize = 0;
                ulong outputSize;

                if (includeBoot)
                {
                    if (imageOffset < CONFIG_SIZE)
                    {
                        MessageBox.Show("imageOffset is smaller than header size (32 bytes).",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    paddingSize = imageOffset - CONFIG_SIZE;
                    outputSize = (ulong)header.Length + paddingSize + mainDataSize;

                    string sizeMsg =
                        "The file will be saved as:\n" +
                        "  [Header 32B] + [0xFF padding up to imageOffset] + [Boot]\n\n" +
                        $"• Header : {header.Length} bytes\n" +
                        $"• Padding : {paddingSize} bytes (0xFF)\n" +
                        $"• Boot : {mainDataSize} bytes\n" +
                        $"• TOTAL : {outputSize} bytes\n" +
                        $"• imageOffset : 0x{imageOffset:X}\n\n" +
                        "Continue to save?";
                    var dr = MessageBox.Show(sizeMsg, "Save Summary", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dr != DialogResult.OK) return;

                    // 큰 패딩 경고(선택)
                    const ulong BIG_PADDING = 16UL * 1024 * 1024; // 16 MiB
                    if (paddingSize >= BIG_PADDING)
                    {
                        var warn = MessageBox.Show(
                            "Padding size is very large (>= 16 MiB). This will create a large output file.\n\nContinue?",
                            "Large Padding Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (warn != DialogResult.OK) return;
                    }
                }
                else
                {
                    outputSize = (ulong)header.Length;
                    string sizeMsg =
                        "Record Only will be saved as:\n" +
                        "  [Header 32B only]\n\n" +
                        $"• Header : {header.Length} bytes\n" +
                        $"• TOTAL : {outputSize} bytes\n\n" +
                        "Continue to save?";
                    var dr = MessageBox.Show(sizeMsg, "Save Summary", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dr != DialogResult.OK) return;
                }

                /** 8) 저장 동작 */
                if (!includeBoot)
                {
                    string savePath = FileHelper.SaveFileDialog("Save Record", "record_only.bin");
                    if (string.IsNullOrEmpty(savePath)) return;

                    File.WriteAllBytes(savePath, header);
                    MessageBox.Show("✅ Record created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string savePath = FileHelper.SaveFileDialog("Save Record + Boot", "record_with_boot.bin");
                    if (string.IsNullOrEmpty(savePath)) return;

                    if (paddingSize > int.MaxValue)
                    {
                        MessageBox.Show("Padding size too large. imageOffset is too far.",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    byte[] padding = (paddingSize == 0)
                        ? Array.Empty<byte>()
                        : Enumerable.Repeat((byte)0xFF, (int)paddingSize).ToArray();

                    using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(header, 0, header.Length);
                        if (padding.Length > 0) fs.Write(padding, 0, padding.Length);
                        fs.Write(bootBytes, 0, bootBytes.Length);
                    }

                    MessageBox.Show("✅ Record + Boot created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
