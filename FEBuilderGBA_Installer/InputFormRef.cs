using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace FEBuilderGBA_Downloader
{
    public class InputFormRef
    {
        //不明Exceptionが発生した時に、forms.ActiveForm は nullになるらしいので独自に記録する.
        static Form LastActiveForm = null;

        public static Type GetControll<Type>(Form self, Type not_found_def)
        {
            foreach (object o in self.Controls)
            {
                if (o.GetType() == typeof(Type))
                {//既に所持.
                    return (Type)o;
                }
            }
            return not_found_def;
        }
        //応答なしにならないようにメッセージループを回す
        public static bool DoEvents(Form self = null, string message = null)
        {
            if (System.Threading.Thread.CurrentThread.IsBackground)
            {//スレッド処理 停止の確認
                return false;
            }

            if (message == null)
            {
                //nop
            }
            else if (LastPleaseWaitStaticCache != null)
            {
                LastPleaseWaitStaticCache.Message(message);
            }
            else if (self != null)
            {
                NotifyPleaseWaitUserControl c = GetControll<NotifyPleaseWaitUserControl>(self, null);

                if (c != null)
                {//表示している.
                    c.Message(message);
                    LastPleaseWaitStaticCache = c;
                }
            }
            else
            {//ウィンドウがない状態
                return false;
            }
            //ループを回す
            Application.DoEvents();
            //停止は受け付けない!
            return false;
        }

        
        //しばらくお待ちください.
        static NotifyPleaseWaitUserControl LastPleaseWaitStaticCache = null;
        public static void ShowPleaseWait(Form self)
        {
            if (GetControll<NotifyPleaseWaitUserControl>(self, null) != null)
            {//既に表示中.
                return;
            }

            NotifyPleaseWaitUserControl notifyControl = new NotifyPleaseWaitUserControl();
            notifyControl.Location = new System.Drawing.Point
            (
              (self.Width - notifyControl.Width) / 2
            , (self.Height - notifyControl.Height) / 2
            );

            notifyControl.Hide();
            self.Controls.Add(notifyControl);
            notifyControl.BringToFront();
            notifyControl.Show();
            notifyControl.Update();

            LastPleaseWaitStaticCache = notifyControl;
        }
        public static void HidePleaseWait(Form self)
        {
            NotifyPleaseWaitUserControl c = GetControll<NotifyPleaseWaitUserControl>(self, null);

            if (c != null)
            {//表示しているなら消す
                self.Controls.Remove(c);
                LastPleaseWaitStaticCache = null;
            }
        }
        public static bool IsPleaseWaitDialog(Form self)
        {
            if (self == null)
            {
                self = LastActiveForm;
                if (self == null)
                {
                    return false;
                }
            }

            NotifyPleaseWaitUserControl c = GetControll<NotifyPleaseWaitUserControl>(self, null);
            return (c != null);
        }

        public class AutoPleaseWait : IDisposable
        {
            Form SelfForm;
            public AutoPleaseWait()
            {
                Init(null);
            }
            public AutoPleaseWait(Form self)
            {
                Init(self);
            }
            void Init(Form self)
            {
                if (self == null)
                {
                    self = LastActiveForm;
                }

                this.SelfForm = self;
                if (this.SelfForm == null)
                {
                    return;
                }
                InputFormRef.ShowPleaseWait(this.SelfForm);
            }
            public void Dispose()
            {
                if (this.SelfForm == null)
                {
                    return;
                }
                InputFormRef.HidePleaseWait(this.SelfForm);
            }
            public void DoEvents(string message = null)
            {
                if (this.SelfForm == null)
                {
                    return;
                }
                InputFormRef.DoEvents(this.SelfForm, message);
            }
        };
    }
}
