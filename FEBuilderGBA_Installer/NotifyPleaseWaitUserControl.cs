using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace FEBuilderGBA_Installer
{
    public partial class NotifyPleaseWaitUserControl : UserControl
    {
        public NotifyPleaseWaitUserControl()
        {
            InitializeComponent();
        }
        public void Message(string message)
        {
            MessageLabel.Text = message;
        }
        public string GetMessage()
        {
            return MessageLabel.Text;
        }
    }
}
