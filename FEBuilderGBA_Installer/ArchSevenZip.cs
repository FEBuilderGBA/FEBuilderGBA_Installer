using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FEBuilderGBA_Downloader
{
    public class ArchSevenZip
    {
//        [DllImport("7-zip32.dll", CharSet = CharSet.Ansi)]
//        static extern int SevenZip(
//            IntPtr hwnd,            // ウィンドウハンドル
//            string szCmdLine,       // コマンドライン
//            StringBuilder szOutput, // 処理結果文字列
//            int dwSize);            // 引数szOutputの文字列サイズ
        public delegate int SevenZipDelegate(IntPtr hWnd, string szCmdLine, StringBuilder szOutput, int dwSize);

        //dll埋め込み
        //see http://yasuharu.net/diary/2795
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        class SevenZipDLLInResource : IDisposable
        {
            IntPtr Handle;
            IntPtr SevenZipPtr;
            SevenZipDelegate SevenZipMarshal;
            const string Temp_dll_name = "_7-zip32.dll";
            public SevenZipDLLInResource()
            {
                ExtractResourceToFile(Temp_dll_name);

                this.Handle = LoadLibrary(Temp_dll_name);
                this.SevenZipPtr = GetProcAddress(this.Handle, "SevenZip");
                this.SevenZipMarshal = (SevenZipDelegate)Marshal.GetDelegateForFunctionPointer(SevenZipPtr, typeof(SevenZipDelegate));
            }
            public void Dispose()
            {
                if (this.Handle != IntPtr.Zero)
                {
                    FreeLibrary(this.Handle);
                    this.Handle = IntPtr.Zero;
                    this.SevenZipPtr = IntPtr.Zero;
                    this.SevenZipMarshal = null;
                }
                if (System.IO.File.Exists(Temp_dll_name))
                {
                    File.Delete(Temp_dll_name);
                }
            }
            public int SevenZip(IntPtr hWnd, string szCmdLine, StringBuilder szOutput, int dwSize)
            {
                return this.SevenZipMarshal(hWnd, szCmdLine,szOutput, dwSize);
            }

            static void ExtractResourceToFile(string filename)
            {
                if (!System.IO.File.Exists(filename))
                {
                    File.WriteAllBytes(filename, Properties.Resources._7_zip32);
                }
            }
        }


        public static string Extract(string a7z, string dir)
        {
            try
            {
                string basedir1 = Path.GetDirectoryName(a7z) + "\\";
                string basedir2 = Path.GetDirectoryName(dir) + "\\";
                if (basedir1 == basedir2)
                {
                    string a7z_relativePath = U.GetRelativePath(basedir1, a7z);
                    string dir_relativePath = U.GetRelativePath(basedir2, dir);
                    string errorMessage;
                    using (new U.ChangeCurrentDirectory(basedir1))
                    {
                        errorMessage = ExtractLow(a7z_relativePath, dir_relativePath);
                    }
                    //いくつかの環境では相対パスでうまくいかないことがあるらしい.
                    if (errorMessage.Length <= 0)
                    {//上手くいった
                        return "";
                    }

                    //絶対パスで再取得.
                }
                return ExtractLow(a7z, dir);
            }
            catch (Exception e)
            {
                Debug.Assert(false);
                return R.Error("7z解凍中にエラーが発生しました。\r\nターゲットファイル:{0}\r\n{1}", a7z , e.ToString());
            }
        }
        static string ExtractLow(string a7z, string dir)
        {
            string command = "x -y -hide -o" + "\"" + dir  + "\"" + " " + "\"" + a7z + "\"";
            

            StringBuilder sb = new StringBuilder(1024);
//            int r = SevenZip(IntPtr.Zero, command, sb, 1024);
            int r;
            using (SevenZipDLLInResource dll = new SevenZipDLLInResource())
            {
                r = dll.SevenZip(IntPtr.Zero, command, sb, 1024);
            }
            
            if (r != 0)
            {
                return sb.ToString();
            }
            return "";
        }
        public static string Compress(string a7z, string target, uint checksize = 1024)
        {
            try
            {
                string basedir1 = Path.GetDirectoryName(a7z) + "\\";
                string basedir2 = Path.GetDirectoryName(target) + "\\";

                if (basedir1 == basedir2)
                {
                    string a7z_relativePath = U.GetRelativePath(basedir1, a7z);
                    string target_relativePath = U.GetRelativePath(basedir2, target);
                    string errorMessage;
                    using (new U.ChangeCurrentDirectory(basedir1))
                    {
                        errorMessage = CompressLow(a7z_relativePath, target_relativePath, checksize);
                    }

                    //いくつかの環境では相対パスでうまくいかないことがあるらしい.
                    if (errorMessage.Length <= 0)
                    {//上手くいった
                        return "";
                    }
                }
                return CompressLow(a7z, target, checksize);
            }
            catch (Exception e)
            {
                Debug.Assert(false);
                return R.Error("7z圧縮中にエラーが発生しました。\r\n{0}", e.ToString());
            }
        }
        static string CompressLow(string a7z, string target , uint checksize)
        {
            string command = "a -hide " + "\"" + a7z + "\"" + " " + "\"" + target + "\"";

            StringBuilder sb = new StringBuilder(1024);
//            int r = SevenZip(IntPtr.Zero, command, sb, 1024);
            int r;
            using (SevenZipDLLInResource dll = new SevenZipDLLInResource())
            {
                r = dll.SevenZip(IntPtr.Zero, command, sb, 1024);
            }

            if (r != 0)
            {//エラー発生
                return sb.ToString();
            }

            if (!File.Exists(a7z))
            {
                return "file not found";
            }
            else if (U.GetFileSize(a7z) < checksize)
            {
                File.Delete(a7z);
                return "file size too short";
            }

            return "";
        }
    }
}
