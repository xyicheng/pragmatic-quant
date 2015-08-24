using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace pragmatic_quant_com
{
    public partial class LoggerForm : Form
    {
        #region private fields
        private TextBoxTraceListener textBoxListener;
        #endregion
        public LoggerForm()
        {
            InitializeComponent();
        }
        private void LoggerForm_Load_1(object sender, EventArgs e)
        {
            textBoxListener = new TextBoxTraceListener(logDisplayTrace);
            Trace.Listeners.Add(textBoxListener);
        }
    }

    public class TextBoxTraceListener : TraceListener
    {
        #region private fields
        private readonly TextBox target;
        private readonly Action<string> invokeWrite; 
        #endregion
        #region private methods
        private void SendString(string message)
        {
            // No need to lock text box as this function will only 
            // ever be executed from the UI thread
            target.Text += message;
        }
        #endregion

        public TextBoxTraceListener(TextBox target)
        {
            this.target = target;
            invokeWrite = SendString;
        }

        public override void Write(string message)
        {
            target.Invoke(invokeWrite, message);
        }

        public override void WriteLine(string message)
        {
            target.Invoke(invokeWrite, message + Environment.NewLine);
        }
    }
}
