using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FEBuilderGBA_Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.PathTextBox.Text = Path.Combine(MakeBaseDirectory(), "FEBuilderGBA");
        }

        string URL;
        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (InputFormRef.IsPleaseWaitDialog(this))
            {//2重割り込み禁止
                return;
            }

            string InstallDir = PathTextBox.Text;
            if (!Directory.Exists(InstallDir))
            {
                U.mkdir(InstallDir);
            }

            //少し時間がかかるので、しばらくお待ちください表示.
            using (InputFormRef.AutoPleaseWait pleaseWait = new InputFormRef.AutoPleaseWait(this))
            {
                pleaseWait.DoEvents("Check Lastest Version...");

                string url;
                string r = CheckUpdateURLByGitHub(out url);
                if (r != "")
                {
                    R.ShowStopError(r);
                    return;
                }
                this.URL = url;

                string update7z = Path.GetTempFileName();

                pleaseWait.DoEvents("Download...");
                //ダウンロード
                try
                {
                    U.DownloadFileByDirect(update7z, this.URL, pleaseWait);
                }
                catch (Exception ee)
                {
                    BrokenDownload(ee);
                    return;
                }
                if (!File.Exists(update7z))
                {
                    BrokenDownload("There are no files to download.");
                    return;
                }
                if (U.GetFileSize(update7z) < 2 * 1024 * 1024)
                {
                    BrokenDownload("Downloaded file is too small.");
                    return;
                }

                pleaseWait.DoEvents("Extract...");

                //解凍
                try
                {
                    string _update = InstallDir;
                    U.mkdir(_update);
                    r = ArchSevenZip.Extract(update7z, _update);
                    if (r != "")
                    {
                        BrokenDownload("The downloaded file could not be decompressed." + "\r\n" + r);
                        return;
                    }
                }
                catch (Exception ee)
                {
                    BrokenDownload(ee);
                    return;
                }

                string updateNewVersionFilename = Path.Combine(InstallDir, "FEBuilderGBA.exe");
                if (!File.Exists(updateNewVersionFilename))
                {
                    BrokenDownload("There was no executable file when unzipping the downloaded file.");
                    return;
                }
                if (U.GetFileSize(updateNewVersionFilename) < 2 * 1024 * 1024)
                {
                    BrokenDownload("The executable file was too small when unzipping the downloaded file.");
                    return;
                }

                pleaseWait.DoEvents("GO!");
            }
            R.ShowOK("Installation is completed!!\r\nStart FEBuilderGBA.");

            try
            {
                string updateNewVersionFilename = Path.Combine(InstallDir, "FEBuilderGBA.exe");

                Process p = new Process();
                p.StartInfo.FileName = updateNewVersionFilename;
                //p.StartInfo.UseShellExecute = false;
                p.Start();
            }
            catch (Exception ee)
            {
                BrokenDownload(ee);
                return;
            }

            this.Close();
        }
        void BrokenDownload(Exception e)
        {
            BrokenDownload(e.ToString());
        }
        void BrokenDownload(string errormessage)
        {
            R.ShowStopError("Automatic installation failed due to an error.\r\nDisplay URL in browser instead.\r\nPlease download manually.\r\n{0}", errormessage);
            OpenBrower();
        }
        void OpenBrower()
        {
            U.OpenURLOrFile(this.URL);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog open = new FolderBrowserDialog();
            if (Directory.Exists(this.PathTextBox.Text))
            {
                open.SelectedPath = this.PathTextBox.Text;
            }
            DialogResult dr = open.ShowDialog();
            if (dr != DialogResult.OK)
            {
                return;
            }

            string dir = open.SelectedPath;
            if (dir.IndexOf("FEBuilderGBA") < 0)
            {//パスにFEBuilderGBAが含まれていなければ、ディレクトリを作る.
                dir = Path.Combine(dir, "FEBuilderGBA");
            }
            this.PathTextBox.Text = dir;
        }

        static string MakeBaseDirectory()
        {
            string[] args = Environment.GetCommandLineArgs();

            //コマンドライン引数の最初の引数にはプロセスへのパスが入っています.
            string selfPath = args[0];
            //コマンドライン引数で渡されると、相対パスになるので、一度フルパスに変換します.
            selfPath = Path.GetFullPath(selfPath);
            //ディレクトリ名の取得
            string currentDir = System.IO.Path.GetDirectoryName(selfPath);
            //現在のプロセスがあるディレクトリがベースディレクトリです.
            return Path.GetFullPath(currentDir);
        }


        static string CheckUpdateURLByGitHub(out string out_url)
        {
            out_url = "";

            string url = "https://api.github.com/repos/FEBuilderGBA/FEBuilderGBA/releases/latest";
            string contents;
            try
            {
                contents = U.HttpGet(url);
            }
            catch (Exception e)
            {
#if DEBUG
                R.Error("Cannot access website. URL:{0} Message:{1}", url, e.ToString());
                throw;
#else
                return R.Error("Cannot access website. URL:{0} Message:{1}", url, e.ToString());
#endif
            }

            string downloadurl;
            {
                Match match = Regex.Match(contents
                , "\"browser_download_url\": \"(.+)\""
                );
                if (match.Groups.Count < 2)
                {
                    return R._("Site results were disappointing.\r\n{0}", url) + "\r\n\r\n"
                        + "browser_download_url not found" + "\r\n"
                        + "contents:\r\n" + contents + "\r\n"
                        + "match.Groups:\r\n" + U.var_dump(match.Groups);
                }
                downloadurl = match.Groups[1].Value;
            }

            out_url = downloadurl;
            return "";
        }
    }
}
