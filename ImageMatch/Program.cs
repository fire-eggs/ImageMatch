using System;
using System.Windows.Forms;

namespace howto_image_hash
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                string path = string.Empty;
                if (args.Length != 0)
                    path = args[0];
                var form = new Form1(path);
                if (string.IsNullOrEmpty(path))
                    Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
