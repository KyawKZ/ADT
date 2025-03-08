using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADT
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string pt;
        static void Main(string[] args)
        {
            try
            {
                if (args.Length<1) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new AdvancedTransfer());
                }
                else
                {
                    pt = args[0];
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());                    
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }          
        }
    }
}
