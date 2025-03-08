using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace ADT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string itemPath = Program.pt;
        public bool isBusy = false;
        public string source, destination;
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Helper.FileIntegrityCorrect())
            {
                MessageBox.Show("Application Will Exit");
                Application.Exit();
            }
            else
            {
                EnvironmentPathModifier.AddDirectoryToUserPath(@"C:\adb");
                Helper.ContextMenuAdded();
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;
                button3.Enabled = true;
                if (!string.IsNullOrEmpty(itemPath))
                {
                    textBox2.Text = itemPath;
                    textBox1.Text = "/sdcard";
                    button3.Enabled = false;
                }
            }
        }
        private void Push()
        {
            ADB($"push \"{source}\" {destination}");
            isBusy = false;
        }
        private void ADB( string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = @"C:\adb\adb.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow= true,
                RedirectStandardOutput= true
            };
            Process p = new Process();
            p.StartInfo = psi;
            p.OutputDataReceived += ADBHandler;
            p.Start();            
            p.BeginOutputReadLine();
        }
        
        
        private void ADBHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                string s = e.Data;
                if(s.Contains("adb: error:"))
                {
                    MessageBox.Show(s);
                }
                if (s.Contains("%"))
                {
                    int percentage1, percentage2;
                    if (Helper.ExtractPercentages(s,out percentage1,out percentage2))
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            progressBar1.Value= percentage1;
                        }));
                        if (percentage2 != -1)
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                progressBar2.Value = percentage2;
                            }));
                        }
                        else
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                progressBar2.Value = percentage1;
                            }));
                        }
                    }
                    string rst= Helper.GetStringBetweenPercentages(s);
                    if (rst != null)
                    {
                        rst = rst.Replace("]","");
                        rst = rst.Replace(":", "");
                        rst = rst.Replace(destination, "");
                        string[] drs = rst.Split('/');
                        int rl=drs.Length;
                        Invoke(new MethodInvoker(delegate ()
                        {
                            label1.Text = drs[rl-1];
                        }));
                    }
                }
                if (s.Contains(" pushed"))
                {
                    string[]t=s.Split(':');
                    Invoke(new MethodInvoker(delegate ()
                    {
                        label1.Text = t[t.Length-1];
                    }));
                }
                if(s.Contains("daemon not running"))
                {
                    label1.Text = "Starting Service. Wait...";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isBusy)
            {
                source = itemPath;
                destination = textBox1.Text;
                Thread p = new Thread(Push);
                p.IsBackground = true;
                isBusy = true;
                p.Start();
            }
        }
    }
    public class EnvironmentPathModifier
    {
        public static void AddDirectoryToUserPath(string directoryToAdd)
        {
            try
            {
                string pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (pathVariable != null && !pathVariable.Contains(directoryToAdd))
                {
                    string newPath = pathVariable + ";" + directoryToAdd;
                    Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);                    
                    Console.WriteLine($"Added '{directoryToAdd}' to the user PATH.");                    
                    BroadcastEnvironmentChange();
                }
                else
                {
                    if (pathVariable != null && pathVariable.Contains(directoryToAdd))
                    {
                        Console.WriteLine($"'{directoryToAdd}' is already in the user PATH.");
                    }
                    else
                    {                        
                        Environment.SetEnvironmentVariable("PATH", directoryToAdd, EnvironmentVariableTarget.User);
                        Console.WriteLine($"Created user PATH and added '{directoryToAdd}'.");
                        BroadcastEnvironmentChange();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding directory to PATH: {ex.Message}");
            }
        }

        private static void BroadcastEnvironmentChange()
        {
            try
            {
                //Send WM_SETTINGCHANGE to all top level windows.
                IntPtr HWND_BROADCAST = (IntPtr)0xffff;
                int WM_SETTINGCHANGE = 0x001A;
                int result = SendMessage(HWND_BROADCAST, WM_SETTINGCHANGE, 0, "Environment");

                if (result == 0)
                {
                    Console.WriteLine("Warning: Environment change broadcast may have failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error broadcasting environment change: {ex.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);
    }
}
