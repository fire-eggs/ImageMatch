using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace howto_image_hash
{
    public class Logger
    {
        private string _logPath;
        private readonly object _lock = new object();

        public Logger()
        {
            var folder = Path.GetDirectoryName(Application.ExecutablePath) ?? @"E:\";
            _logPath = Path.Combine(folder, "imgComp.log");
        }


        public void log(string msg)
        {
            try
            {
                lock (_lock)
                {
                    using (StreamWriter file = new StreamWriter(_logPath, true))
                    {
                        file.WriteLine(DateTime.Now.ToString("yyyy-M-d HH:mm:ss") + ":" + msg);
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        private static int tick;

        public void logTimer(string msg, bool first=false)
        {
            int delta = 0;
            if (first)
                tick = Environment.TickCount;
            else
                delta = Environment.TickCount - tick;
            log(string.Format("{0}|{1}|", msg, delta));
        }

        public void log(Exception ex)
        {
            string msg = ex.Message + Environment.NewLine + ex.StackTrace;
            log(msg);
        }

        public void ShowLog()
        {
            Process.Start("notepad.exe", _logPath);
        }
    }
}
