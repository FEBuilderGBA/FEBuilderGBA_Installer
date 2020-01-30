using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.Reflection;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security;

namespace FEBuilderGBA_Downloader
{
    //その他、雑多なもの.
    //名前タイプするのが面倒なので Util -> U とする.
    public static class U
    {
        public static bool mkdir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception )
                {
                    //ディレクトリがロックされていて消せない場合があるらしい
                    //その場合、作るという目的は達成しているので、まあいいかなあ。
                   return false;
                }
            }
            Directory.CreateDirectory(dir);
            return true;
        }
        public static void DownloadFileByDirect(string save_filename, string download_url, InputFormRef.AutoPleaseWait pleaseWait)
        {
            U.HttpDownload(save_filename, download_url, Path.GetDirectoryName(download_url), pleaseWait);
        }
        public static void HttpDownload(string savefilename, string url, string referer = "", InputFormRef.AutoPleaseWait pleaseWait = null, System.Net.CookieContainer cookie = null)
        {
            HttpWebRequest request = HttpMakeRequest(url, referer, cookie);

            WebResponse rsp = request.GetResponse();
            using (Stream output = File.OpenWrite(savefilename))
            using (Stream input = rsp.GetResponseStream())
            {
                byte[] buffer = new byte[1024 * 8];
                int totalSize = (int)rsp.ContentLength;
                int readTotalSize = 0;
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);

                    if (pleaseWait != null)
                    {
                        readTotalSize += bytesRead;
                        if (totalSize == -1)
                        {
                            pleaseWait.DoEvents("Download: " + readTotalSize + "/" + "???");
                        }
                        else
                        {
                            pleaseWait.DoEvents("Download: " + readTotalSize + "/" + totalSize);
                        }
                    }
                }
            }

            rsp.Close();

            if (cookie != null)
            {
                System.Net.CookieCollection cookies = request.CookieContainer.GetCookies(request.RequestUri);
                cookie.Add(cookies);
            }
        }
        //https://qiita.com/Takezoh/items/3eff6806a59152656ddc
        //MONOには証明書が入っていないので別処理
        private static bool OnRemoteCertificateValidationCallback(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            //危険だけど継続する
            return true;
        }
        public static uint atoi(String a)
        {
            //C#のTryParseはC言語のatoiと違い、後ろに数字以外があると false が変えるので補正する.
            for (int i = 0; i < a.Length; i++)
            {
                if (!isnum(a[i]))
                {
                    a = a.Substring(0, i);
                    break;
                }
            }

            int ret = 0;
            if (!int.TryParse(a, out ret))
            {
                return 0;
            }
            return (uint)ret;
        }
        public static uint atoh(String a)
        {
            //C#のTryParseはC言語のatoiと違い、後ろに数字以外があると false が変えるので補正する.
            for (int i = 0; i < a.Length; i++)
            {
                if (!ishex(a[i]))
                {
                    a = a.Substring(0, i);
                    break;
                }
            }

            int ret = 0;
            if (!int.TryParse(a, System.Globalization.NumberStyles.HexNumber, null, out ret))
            {
                return 0;
            }
            return (uint)ret;
        }
        public static uint atoi0x(String a)
        {
            if (a.Length >= 2 && a[0] == '0' && a[1] == 'x')
            {
                return atoh(a.Substring(2));
            }
            if (a.Length >= 1 && a[0] == '$')
            {
                return atoh(a.Substring(1));
            }
            return atoi(a);
        }
        static string GenUserAgent()
        {
            System.OperatingSystem os = System.Environment.OSVersion;

            uint seed = U.atoi(DateTime.Now.ToString("yyMMddHH"));

            Random rand = new Random((int)seed);
            int SafariMinorVersion = 537;
            int SafariMajorVersion = 36;
            int Chrome1Version = 65;
            int Chrome2Version = 0;
            int Chrome3Version = 2107;
            int Chrome4Version = 108;

            string UserAgent = string.Format("Mozilla/5.0 (Windows NT {0}.{1}; Win64; x64) AppleWebKit/{2}.{3} (KHTML, like Gecko) Chrome/{4}.{5}.{6}.{7} Safari/{2}.{3}"
                , os.Version.Major//Windows 8では、「6」//OSのメジャーバージョン番号を表示する
                , os.Version.Minor//Windows 8では、「2」//OSのマイナーバージョン番号を表示する
                , SafariMinorVersion
                , SafariMajorVersion
                , Chrome1Version
                , Chrome2Version
                , Chrome3Version
                , Chrome4Version
                );
            return UserAgent;
        }
        static HttpWebRequest HttpMakeRequest(string url, string referer, System.Net.CookieContainer cookie = null)
        {
            ServicePointManager.ServerCertificateValidationCallback = OnRemoteCertificateValidationCallback;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2 

            string UserAgent = GenUserAgent(); //"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            //自動プロキシ検出を利用しない.
            //こちらの方が早くなります.
            request.Proxy = null;

            //貴方の好きなUAを使ってね。
            request.UserAgent = UserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (referer != "")
            {
                request.Referer = referer;
            }
            if (cookie != null)
            {
                request.CookieContainer = new System.Net.CookieContainer();
                request.CookieContainer.Add(cookie.GetCookies(request.RequestUri));
            }
            return request;
        }
        public static long GetFileSize(string filename)
        {
            FileInfo info = new FileInfo(filename);
            return info.Length;
        }
        public static void OpenURLOrFile(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ee)
            {
                R.ShowStopError(ee.ToString());
            }
        }
        //httpでそこそこ怪しまれずに通信する
        public static string HttpGet(string url, string referer = "", System.Net.CookieContainer cookie = null)
        {
            HttpWebRequest request = HttpMakeRequest(url, referer, cookie);
            string r = "";

            WebResponse rsp = request.GetResponse();
            Stream stm = rsp.GetResponseStream();
            if (stm != null)
            {
                StreamReader reader = new StreamReader(stm, Encoding.UTF8);
                r = reader.ReadToEnd();
                stm.Close();
            }
            rsp.Close();

            if (cookie != null)
            {
                System.Net.CookieCollection cookies = request.CookieContainer.GetCookies(request.RequestUri);
                cookie.Add(cookies);
            }

            return r;
        }

        public static String var_dump(object obj, int nest = 0)
        {
            if (obj == null)
            {
                return "null";
            }
            if (obj is uint || obj is int
                || obj is ushort || obj is short
                || obj is byte || obj is byte
                || obj is float || obj is double || obj is bool
                || obj is UInt16 || obj is Int16
                || obj is UInt32 || obj is Int32
                || obj is UInt64 || obj is Int64
                )
            {
                return obj.ToString();
            }
            if (obj is string)
            {
                return "\"" + obj.ToString() + "\"";
            }

            if (nest >= 2)
            {
                return "...";
            }

            StringBuilder sb = new StringBuilder();
            IEnumerable ienum = obj as IEnumerable;
            if (ienum != null)
            {
                sb.Append("{");
                foreach (object o in ienum)
                {
                    sb.Append(var_dump(o, nest + 1) + ",");
                }
                sb.Append("}");
                return sb.ToString();
            }

            sb.Append("{");
            const BindingFlags FINDS_FLAG = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo[] infoArray = obj.GetType().GetFields(FINDS_FLAG);
            foreach (FieldInfo info in infoArray)
            {
                object o = info.GetValue(obj);
                sb.Append(info.Name + ": " + var_dump(o, nest + 1) + ",");
            }
            sb.Append("}");
            return sb.ToString();
        }
        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(
          uint dwFlags, IntPtr lpSource,
          uint dwMessageId, uint dwLanguageId,
          StringBuilder lpBuffer, int nSize,
          IntPtr Arguments);
        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;

        //https://www.atmarkit.co.jp/fdotnet/dotnettips/741win32errmsg/win32errmsg.html
        public static string HRESULTtoString(int errCode)
        {
            StringBuilder message = new StringBuilder(1024);

            FormatMessage(
              FORMAT_MESSAGE_FROM_SYSTEM,
              IntPtr.Zero,
              (uint)errCode,
              0,
              message,
              message.Capacity,
              IntPtr.Zero);

            return message.ToString();
        }

        public static bool isalhpa(char a)
        {
            return isalhpa((byte)a);
        }
        public static bool isalhpa(byte a)
        {
            return ((a >= 'a' && a <= 'z')
                || (a >= 'A' && a <= 'Z')
                );
        }
        public static bool isalhpanum(char a)
        {
            return isalhpanum((byte)a);
        }
        public static bool isalhpanum(byte a)
        {
            return (a >= 'a' && a <= 'z')
                || (a >= 'A' && a <= 'Z')
                || (a >= '0' && a <= '9')
                ;
        }
        public static bool isnum_f(char a)
        {
            return isnum_f((byte)a);
        }
        public static bool isnum_f(byte a)
        {
            return ((a >= '0' && a <= '9') || a == '.');
        }
        public static bool isnum(char a)
        {
            return isnum((byte)a);
        }
        public static bool isnum(byte a)
        {
            return (a >= '0' && a <= '9');
        }
        public static bool ishex(char a)
        {
            return ishex((byte)a);
        }
        public static bool ishex(byte a)
        {
            return (a >= '0' && a <= '9') || (a >= 'a' && a <= 'f') || (a >= 'A' && a <= 'F');
        }
        public static bool isAlphaNumString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] >= 'a' && str[i] <= 'z')
                   || (str[i] >= 'A' && str[i] <= 'Z')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isAsciiString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= 0x7f)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isAlphaString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                     ((str[i] >= 'a' && str[i] <= 'z')
                   || (str[i] >= 'A' && str[i] <= 'Z')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isHexString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] >= 'a' && str[i] <= 'f')
                   || (str[i] >= 'A' && str[i] <= 'F')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isNumString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isAscii(byte a)
        {
            return (a >= 0x20 && a <= 0x7e);
        }

        public static string ToHexString(decimal a)
        {
            return ToHexString((uint)a);
        }
        public static string ToHexString(int a)
        {
            if (a <= 0xff)
            {
                return a.ToString("X02");
            }
            if (a <= 0xffff)
            {
                return a.ToString("X04");
            }
            if (a <= 0x7fffffff)
            {
                return a.ToString("X08");
            }
            return "???";
        }
        public static string ToHexString8(int a)
        {
            return a.ToString("X08");
        }
        public static string ToHexString8(uint a)
        {
            return a.ToString("X08");
        }
        public static string ToHexString2(int a)
        {
            return a.ToString("X02");
        }
        public static string ToHexString2(uint a)
        {
            return a.ToString("X02");
        }

        public static string To0xHexString(uint a)
        {
            return "0x" + ToHexString(a);
        }
        public static string To0xHexString(int a)
        {
            return "0x" + ToHexString(a);
        }
        public static string ToHexString(uint a)
        {
            if (a <= 0xff)
            {
                return a.ToString("X02");
            }
            if (a <= 0xffff)
            {
                return a.ToString("X04");
            }
            if (a <= 0xffffff)
            {
                return a.ToString("X06");
            }
            if (a <= 0xffffffff)
            {
                return a.ToString("X08");
            }
            return "???";
        }
        public static string GetRelativePath(string uri1, string uri2)
        {
            Uri u1 = new Uri(uri1);
            Uri u2 = new Uri(uri2);

            Uri relativeUri = u1.MakeRelativeUri(u2);

            string relativePath = relativeUri.ToString();

            relativePath = relativePath.Replace('/', '\\');
            relativePath = Uri.UnescapeDataString(relativePath);

            return (relativePath);
        }
        public static string UrlDecode(string urlString)
        {
            return Uri.UnescapeDataString(urlString);
        }

        //一時的にカレントディレクトリを移動する.
        public class ChangeCurrentDirectory : IDisposable
        {
            string current_dir;
            public ChangeCurrentDirectory(string dir)
            {
                current_dir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Path.GetDirectoryName(dir));
            }
            public void Dispose()
            {
                Directory.SetCurrentDirectory(current_dir);
            }
        }

    }
}

