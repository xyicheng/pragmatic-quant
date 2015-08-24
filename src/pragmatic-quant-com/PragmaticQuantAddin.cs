using System.Runtime.InteropServices;
using ExcelDna.Integration;

namespace pragmatic_quant_com
{
    [ComVisible(false)]
    class PragmaticQuantAddin : IExcelAddIn
    {
        private LoggerForm loggerForm;
        public void AutoOpen()
        {
            //ComServer.DllRegisterServer();

            loggerForm = new LoggerForm();
            loggerForm.Show();
        }
        public void AutoClose()
        {
            //ComServer.DllUnregisterServer();

            loggerForm.Close();
        }
    }
}