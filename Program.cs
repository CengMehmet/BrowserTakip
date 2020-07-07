using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //bool kontrol;

            //Mutex mutex = new Mutex(true, "Program", out kontrol); //Örnek Mutex nesnesi oluşturalım.
            //if (kontrol == false)
            //{
            //    MessageBox.Show("Bu program zaten çalışıyor.");
            //    return;
            //}
            Application.Run(new Form1());
            //GC.KeepAlive(mutex); //Nesneyi kaldırıyoruz.
        }
    }
}