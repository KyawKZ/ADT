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
                Helper.ContextMenuAdded();
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;     
                if (!string.IsNullOrEmpty(itemPath))
                {
                    textBox2.Text = itemPath;
                    textBox1.Text = "/sdcard";                    
                }
            }
        }
        private void Push()
        {
            ADB($"push \"{source}\" {destination}");          
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
            p.WaitForExit();
            isBusy = false;
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

        private void button3_Click(object sender, EventArgs e)
        {
            Form f = new AdvancedTransfer();
            this.Hide();
            f.Show();
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
}
