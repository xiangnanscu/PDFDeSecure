using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFDeSecure
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            var Args = Environment.GetCommandLineArgs();
            if (Args.Length == 2)
            {
                // 设置控制台编码以正确显示中文
                try
                {
                    // 尝试使用 UTF-8
                    Console.OutputEncoding = Encoding.UTF8;
                    Console.InputEncoding = Encoding.UTF8;
                }
                catch
                {
                    try
                    {
                        // 如果 UTF-8 失败，使用系统默认编码（GBK）
                        Console.OutputEncoding = Encoding.GetEncoding(936);
                        Console.InputEncoding = Encoding.GetEncoding(936);
                    }
                    catch { }
                }

                // 单参数模式：递归处理目录下所有PDF并覆盖源文件
                var TargetDir = Args[1];
                if (Directory.Exists(TargetDir))
                {
                    ProcessDirectoryRecursive(TargetDir);
                    Environment.Exit(0);
                }
                else if (File.Exists(TargetDir) && TargetDir.ToLower().EndsWith(".pdf"))
                {
                    // 单个文件模式
                    ProcessSingleFile(TargetDir, TargetDir);
                    Environment.Exit(0);
                }
            }
            else if (Args.Length > 2)
            {
                //Auto Processing Mode
                PdfDocument pdf = new PdfDocument();
                PdfDocument outpdf = new PdfDocument();
                //First Argu Is INPUT Dir, Second Is OUTPUT Dir
                var Input = Args[1];
                var Output = Args[2];
                DirectoryInfo di = new DirectoryInfo(Input);
                var aryFi = di.GetFiles("*.pdf");
                var counter = 0;
                var error = 0;
                foreach (FileInfo fi in aryFi)
                {
                    //Skip file with errors
                    try
                    {
                        outpdf = new PdfDocument();
                        Stream fileStream = fi.OpenRead();
                        pdf = PdfReader.Open(fileStream, PdfDocumentOpenMode.Import);
                        foreach (PdfPage page in pdf.Pages)
                        {
                            outpdf.AddPage(page);
                        }
                        outpdf.Save(new FileInfo(Output + "\\" + fi.Name).OpenWrite(), true);
                        counter++;
                        pdf.Dispose();
                        fileStream.Close();
                        outpdf.Dispose();
                    }
                    catch (Exception ex)
                    {
                        error++;
                        File.WriteAllText(Output + "\\Error-" + fi.Name + ".log", ex.ToString());
                    }
                }
                MessageBox.Show("Unlocked " + counter + " files" + Environment.NewLine + "Failed " + error + " files" + Environment.NewLine + "Percentage " + counter + "/" + (counter + error) + " = " + ((float)counter / (float)(counter + error) * 100).ToString("f2") + "%, Cheers!", "PDF file Unlocked! and Saved!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PDFDeSecure());
            }
        }

        /// <summary>
        /// 递归处理目录下所有PDF文件并覆盖源文件
        /// </summary>
        static void ProcessDirectoryRecursive(string directory)
        {
            var counter = 0;
            var error = 0;
            var pdfFiles = Directory.GetFiles(directory, "*.pdf", SearchOption.AllDirectories);

            Console.WriteLine($"找到 {pdfFiles.Length} 个PDF文件，开始处理...");
            Console.WriteLine();

            foreach (var pdfFile in pdfFiles)
            {
                try
                {
                    Console.WriteLine($"[{counter + error + 1}/{pdfFiles.Length}] 处理: {pdfFile}");
                    ProcessSingleFile(pdfFile, pdfFile);
                    counter++;
                    Console.WriteLine($"      [成功] 已覆盖源文件");
                }
                catch (Exception ex)
                {
                    error++;
                    var errorLog = Path.Combine(Path.GetDirectoryName(pdfFile), $"Error-{Path.GetFileName(pdfFile)}.log");
                    File.WriteAllText(errorLog, ex.ToString());
                    Console.WriteLine($"      [失败] {ex.Message}");
                }
                Console.WriteLine();
            }

            var message = $"处理完成！\n\n总计: {pdfFiles.Length} 个文件\n成功: {counter} 个文件\n失败: {error} 个文件";
            if (counter + error > 0)
            {
                message += $"\n成功率: {((float)counter / (float)(counter + error) * 100):F2}%";
            }

            MessageBox.Show(message, "PDF处理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 处理单个PDF文件
        /// </summary>
        static void ProcessSingleFile(string inputFile, string outputFile)
        {
            PdfDocument pdf = null;
            PdfDocument outpdf = null;
            Stream fileStream = null;
            FileStream outputStream = null;
            string tempFile = null;

            try
            {
                // 创建临时文件
                tempFile = outputFile + ".tmp";

                // 读取源文件
                fileStream = File.OpenRead(inputFile);
                pdf = PdfReader.Open(fileStream, PdfDocumentOpenMode.Import);

                // 创建新文档
                outpdf = new PdfDocument();
                foreach (PdfPage page in pdf.Pages)
                {
                    outpdf.AddPage(page);
                }

                // 保存到临时文件
                outputStream = File.OpenWrite(tempFile);
                outpdf.Save(outputStream);
                outputStream.Close();
                outputStream = null;

                // 关闭源文件流
                fileStream.Close();
                fileStream = null;

                // 释放资源
                pdf.Dispose();
                pdf = null;
                outpdf.Dispose();
                outpdf = null;

                // 备份原文件
                // string backupFile = outputFile + ".bak";
                // if (File.Exists(outputFile))
                // {
                //     File.Copy(outputFile, backupFile, true);
                // }

                // 替换原文件
                File.Delete(outputFile);
                File.Move(tempFile, outputFile);

                // 删除备份（可选，如果需要保留备份可以注释掉这行）
                // File.Delete(backupFile);
            }
            catch
            {
                // 清理临时文件
                if (tempFile != null && File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }

                // 恢复备份
                string backupFile = outputFile + ".bak";
                if (File.Exists(backupFile) && !File.Exists(outputFile))
                {
                    try { File.Move(backupFile, outputFile); } catch { }
                }

                throw;
            }
            finally
            {
                // 确保所有资源都被释放
                if (fileStream != null)
                {
                    try { fileStream.Close(); } catch { }
                }
                if (outputStream != null)
                {
                    try { outputStream.Close(); } catch { }
                }
                if (pdf != null)
                {
                    try { pdf.Dispose(); } catch { }
                }
                if (outpdf != null)
                {
                    try { outpdf.Dispose(); } catch { }
                }
            }
        }
    }
}
