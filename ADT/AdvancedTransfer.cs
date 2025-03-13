using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADT
{
    public partial class AdvancedTransfer : Form
    {
        public AdvancedTransfer()
        {
            InitializeComponent();
        }
        public bool isBusy = false;
        public string ListPath = "sdcard";
        public string File2Copy = "";
        public string F2Save = "";
        public string F2I = "";
        private void AdvancedTransfer_Load(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
        }
        private void ADBPull()
        {
            ADBAsync($"pull \"{File2Copy}\" \"{F2Save}\"");
        }

        private void ADBPush()
        {            
            ADBAsync($"push \"{F2I}\" /sdcard");
        }
        private string ADB(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = @"C:\adb\adb.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            Process p = new Process();
            p.StartInfo = psi;            
            p.Start();
            p.WaitForExit();
            isBusy = false;
            return p.StandardOutput.ReadToEnd();
        }
        private bool isDirectory(string path)
        {
            string m = ADB($"shell ls -ld {path}");
            if (m.Contains("No such file or directory") || m.Contains("error:"))
            {
                MessageBox.Show(m);
                return false;
            }
            else
            {
                if (m.StartsWith("d"))
                {
                    return true;
                }
                else if (m.StartsWith("l"))
                {
                    return true;
                }
                else
                {                
                    return false;
                }
            }
        }
        private void listDirectory()
        {
            if (isDirectory(ListPath))
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    textBox1.Text = ListPath;
                }));
                string m = ADB($"shell ls \"{ListPath}\"");
                listBackend(m);
            }
            else
            {
                MessageBox.Show("This is not a folder");
            }
        }
        private void listfiles()
        {
            string m = ADB("shell ls /sdcard");
            listBackend(m);
        }

        private void PreviousDirectory()
        {
            if (ListPath != null && ListPath.Contains("/"))
            {
                string[] sec = ListPath.Split('/');
                int sl = sec.Length;
                string p = "sdcard";                
                for (int i = 0; i < sl - 1; i++)
                {
                    if (sec[i] != "sdcard")
                    {
                        p += $"/{sec[i]}";
                    }
                }
                ListPath = p;
                listDirectory();
            }
        }        
        private void Import()
        {
            Invoke(new MethodInvoker(delegate () { label1.Text = "Transfering. Wait..."; }));
            string m = ADB("devices");
            string[] ms = m.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in ms)
            {
                if (line.Contains("\tdevice"))
                {
                    string[] d = line.Split('\t');
                    F2Save = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), d[0].Trim());                 
                    ADBPush();
                }
            }
        }
        private void Export()
        {
            Invoke(new MethodInvoker(delegate () { label1.Text = "Transfering. Wait..."; }));
            string m = ADB("devices");
            string[] ms = m.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in ms)
            {
                if (line.Contains("\tdevice"))
                {
                    string[] d = line.Split('\t');
                    F2Save=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),d[0].Trim());
                    if (!Directory.Exists(F2Save))
                    { 
                        Directory.CreateDirectory(F2Save); 
                    }
                    ADBPull();
                }                
            }
        }
        private void listBackend(string list)
        {
            if (!list.Contains("daemon not running.") && !list.Contains("daemon started successfully") && !list.Contains("error:"))
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    listBox1.Items.Clear();
                }));
            }
            string[] lines = list.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.Contains("error:"))
                {
                    Invoke(new MethodInvoker(delegate () {
                        label1.Text = line;
                    }));                    
                }
                if (!line.Contains("daemon not running.") && !line.Contains("* daemon started successfully") && !line.Contains("adb server version"))
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        listBox1.Items.Add(line);
                        textBox1.Text = ListPath;
                    }));
                }         
            }
        }
        private void ADBAsync(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = @"C:\adb\adb.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
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
                if (s.Contains("adb: error:"))
                {
                    MessageBox.Show(s);
                }
                if (s.Contains("%"))
                {
                    int percentage1, percentage2;
                    if (Helper.ExtractPercentages(s, out percentage1, out percentage2))
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            progressBar1.Value = percentage1;
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
                    string rst = Helper.GetStringBetweenPercentages(s);
                    if (rst != null)
                    {
                        rst = rst.Replace("]", "");
                        rst = rst.Replace(":", "");
                        rst = rst.Replace(ListPath, "");
                        string[] drs = rst.Split('/');
                        int rl = drs.Length;
                        Invoke(new MethodInvoker(delegate ()
                        {
                            label1.Text = drs[rl - 1];
                        }));
                    }
                }
                if (s.Contains(" pushed"))
                {
                    string[] t = s.Split(':');
                    Invoke(new MethodInvoker(delegate ()
                    {
                        label1.Text = t[t.Length - 1];
                    }));
                }
                if(s.Contains(" pulled"))
                {
                    string[] t2 = s.Split(':');
                    Invoke(new MethodInvoker(delegate ()
                    {
                        label1.Text = t2[t2.Length - 1];
                    }));
                }
                if (s.Contains("daemon not running"))
                {
                    label1.Text = "Starting Service. Wait...";
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (!isBusy)
            {
                listBox1.Items.Clear();
                Thread t = new Thread(listfiles);
                t.IsBackground = true;
                isBusy = true;
                ListPath = "sdcard";
                t.Start();
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if(listBox1.SelectedItems!=null)
            {
                string lp=listBox1.SelectedItem.ToString();
                if(lp.Contains(" "))
                {
                    lp = lp.Replace(" ", "\\ ");
                }
                ListPath = $"{textBox1.Text}/{lp}";                               
                if (!isBusy)
                {                    
                    Thread ls = new Thread(listDirectory);
                    ls.IsBackground = true;
                    isBusy = true;
                    ls.Start();
                }                
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ListPath = textBox1.Text;
            if (!isBusy)
            {
                Thread ls = new Thread(PreviousDirectory);
                ls.IsBackground = true;
                isBusy = true;
                ls.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }
            if (listBox1.SelectedItem != null) 
            {
                File2Copy = textBox1.Text + "/" + listBox1.SelectedItem.ToString();
                Thread s = new Thread(Export);
                s.IsBackground = true;
                isBusy = true;
                s.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }
            F2I = Helper.ImportFileDialog();
            if(F2I != null)
            {
                Thread im = new Thread(Import);
                im.IsBackground = true;
                isBusy = true;
                im.Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }
            F2I = Helper.ImportFolderDialog();
            if (F2I != null)
            {
                Thread im = new Thread(Import);
                im.IsBackground = true;
                isBusy = true;
                im.Start();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Process[] ps = Process.GetProcessesByName("adb.exe");
            if (ps.Length > 0)
            {
                foreach(Process p in ps)
                {
                    p.Kill();
                }                                        
            }
        }
    }
}
