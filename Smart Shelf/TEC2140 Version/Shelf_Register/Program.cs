using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shelf_Register
{
    static class Program
    {
        public static string TcpHost;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //Console.WriteLine("Invalid args");
                //TcpHost = "192.168.1.124:9000";

            }
            else
            {
                TcpHost = args[0];

            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Front());
        }
    }
}
