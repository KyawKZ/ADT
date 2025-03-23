using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ADT
{
    internal class Helper
    {
        public static ProgressBar progressBar1,progressBar2;
        public static string ImportFileDialog()
        {
            string file = null;
            Thread ifd = new Thread(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Multiselect =false,
                };
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    file = openFileDialog.FileName;
                }
            });
            ifd.SetApartmentState(ApartmentState.STA);
            ifd.Start();
            ifd.Join();
            return file;
        }
        public static string ImportFolderDialog()
        {
            string file = null;
            Thread ifd = new Thread(() =>
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog()                ;
                if (FBD.ShowDialog() == DialogResult.OK)
                {
                    file = FBD.SelectedPath;
                }
            });
            ifd.SetApartmentState(ApartmentState.STA);
            ifd.Start();
            ifd.Join();
            return file;
        }
        public static string GetStringBetweenPercentages(string input)
        {
            Regex regex = new Regex(@"(\d+)%.*?(\d+)%");
            Match match = regex.Match(input);

            if (match.Success)
            {
                int startIndex = match.Groups[1].Index + match.Groups[1].Length + 1; // +1 to skip the '%'
                int endIndex = match.Groups[2].Index - 1; // -1 to exclude the '%'

                return input.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                return null;
            }
        }
        public static bool ExtractPercentages(string input, out int percentage1, out int percentage2)
        {
            percentage1 = -1;
            percentage2 = -1;

            Regex regex = new Regex(@"(\d+)%");
            MatchCollection matches = regex.Matches(input);

            if (matches.Count >= 1)
            {
                if (int.TryParse(matches[0].Groups[1].Value, out percentage1))
                {
                    if (matches.Count >= 2)
                    {
                        if (int.TryParse(matches[1].Groups[1].Value, out percentage2))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        return true;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool CheckRegistryKeyExists(string keyPath)
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(keyPath))
                {
                    return key != null;
                }
            }
            catch
            {                      
                return false;
            }
        }
        public static void ContextMenuAdded()
        {
            if (!CheckRegistryKeyExists(@"*\shell\ADT"))
            {
                MessageBox.Show("Need to add to Right-Click Menu");
                File.WriteAllText(@"C:\ProgramData\File.reg", Properties.Resources.File);
                string bt = "regedit /s C:\\ProgramData\\File.reg";
                File.WriteAllText(@"C:\ProgramData\menu.bat", bt);
                Process p = new Process();
                p.StartInfo.FileName = @"C:\ProgramData\menu.bat";
                p.Start();
                p.WaitForExit();
                File.Delete(@"C:\ProgramData\File.reg");
            }
        }
        public static bool FileIntegrityCorrect()
        {
            bool flag = false;
            if (Application.StartupPath.ToLower() == "c:\\adb")
            {
                flag = true;
                if (Directory.Exists(@"C:\adb"))
                {
                    if (!File.Exists(@"C:\adb\adb.exe"))
                    {
                        MessageBox.Show("adb.exe not found in C:\\adb");
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Application must be in C:\\adb");
            }
            return flag;
        }
    }
}
