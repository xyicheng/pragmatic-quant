using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ExcelDna.Integration;
using pragmatic_quant_model.Basic;

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

    public static class FunctionRunnerUtils
    {
        #region private methods
        private static Exception[] CollectAggregate(Exception exception)
        {
            var aggregateExcept = exception as AggregateException;
            if (aggregateExcept != null)
            {
                var inners = aggregateExcept.InnerExceptions.Map(CollectAggregate);
                return EnumerableUtils.Append(inners);
            }
            return new[] {exception};
        }
        #endregion
        public static object Run(string functionName, Func<object> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                var inners = CollectAggregate(e);
                foreach (var exception in inners)
                    Trace.TraceError(exception.Message);
                return string.Format("ERROR while running {0}", functionName);
            }
        }

        public static Task<T> ComputationTaskWithLog<T>(this TaskFactory taskFactory, string computationName, Func<T> computation, params Task[] requiredTasks)
        {
            Func<T> taskActionWithLog = () =>
            {
                var tasksFailures = requiredTasks.Where(t => t.IsFaulted && t.Exception != null)
                                               .Map(t => t.Exception);
                if (tasksFailures.Any())
                    throw tasksFailures.First();
                
                var timer = new Stopwatch();
                Trace.WriteLine(string.Format("Start {0}...", computationName));
                timer.Restart();

                var result = computation();

                timer.Stop();
                Trace.WriteLine(String.Format("{3} done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds, computationName));
                return result;
            };

            if (requiredTasks.Length > 0)
                return taskFactory.ContinueWhenAll(requiredTasks, r => taskActionWithLog());
            return taskFactory.StartNew(() => taskActionWithLog());
        }

    }

}