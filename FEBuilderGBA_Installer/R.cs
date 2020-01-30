using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace FEBuilderGBA_Installer
{
    //string.Format(Properties.Resources,,,)と書くのが面倒なので短縮形をつくる.
    static class R
    {
        public static string _(string str, params object[] args)
        {
            return String.Format(str, args);
        }
        public static string Error(string str, params object[] args)
        {
            return String.Format(str, args);
        }
        public static string ExceptionToString(System.Runtime.InteropServices.ExternalException e)
        {
            return R._("GDI+ Exceptionが発生しました。\r\nもう一度試してください。\r\n何度も再発する場合は、report7zを送ってください。\r\nErrorCode:{0} {1}\r\nMessage:\r\n{2}\r\n{3}", e.ErrorCode, U.HRESULTtoString(e.ErrorCode), e.ToString(), e.StackTrace);
        }
        public static string ExceptionToString(System.UnauthorizedAccessException e)
        {
            return R._("System.UnauthorizedAccessExceptionが発生しました。\r\n何度も再発する場合は、report7zを送ってください。\r\nMessage:\r\n{0}\r\n{1}", e.ToString(), e.StackTrace);
        }
        public static string ExceptionToString(System.OutOfMemoryException e)
        {
            return R._("System.OutOfMemoryExceptionが発生しました。\r\n何度も再発する場合は、report7zを送ってください。\r\nMessage:\r\n{0}\r\n{1}", e.ToString(), e.StackTrace);
        }
        public static string ExceptionToString(System.Exception e)
        {
            return R._("例外が発生しました。\r\n何度も再発する場合は、report7zを送ってください。\r\nMessage:\r\n{0}\r\n{1}", e.ToString(), e.StackTrace);
        }

        //エラーメッセージ OKだけ
        public static void ShowStopError(string str, params object[] args)
        {
            string message = _(str, args);

            try
            {
                string title = "Error";
                MessageBox.Show(message
                    , title
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Error);
            }
            catch (Exception )
            {
            }
        }
        public static void ShowStopError(string str)
        {
            try
            {
                string title = "Error";
                MessageBox.Show(str
                    , title
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Error);
            }
            catch (Exception)
            {
            }
        }

        public static void ShowStopError(string str,Exception ex)
        {
            ShowStopError("{0}\r\n\r\n{1}:\r\n{2}", ex.GetType().ToString(), ex.ToString());
        }

        public static DialogResult ShowOK(string str)
        {
            string message = R._(str);
            return MessageBox.Show(message
                , ""
                , MessageBoxButtons.OK
                , MessageBoxIcon.Information);
        }
    }
}
