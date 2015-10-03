using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ExcelDna.Integration;

namespace pragmatic_quant_com
{
    [ComVisible(false)]
    internal class PragmaticQuantAddin : IExcelAddIn
    {
        private Thread loggerThread;
        public void AutoOpen()
        {
            if (loggerThread == null)
            {
                loggerThread = new Thread(() =>
                {
                    var loggerForm = new LoggerForm();
                    loggerForm.Show();
                    System.Windows.Forms.Application.Run(loggerForm);
                });
                loggerThread.Start();
            }
        }
        public void AutoClose()
        {
            if (loggerThread != null)
                loggerThread.Abort();
        }
    }

    public static class XlFunctionRunner
    {
        public static object Run(string functionName, Func<object> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return string.Format("ERROR while running {0}", functionName);
            }
        }
    }
}