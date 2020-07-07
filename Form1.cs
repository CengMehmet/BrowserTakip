using License;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")] public static extern bool ReleaseCapture();

        private BrowserContext db;
        public User user;
        private string baglanti = "", key = "";

        public Form1()
        {
            InitializeComponent();
            xmlOku();
            RegistryIslemleri();
        }

        public string xmlOku()
        {
            baglanti = ""; key = "";
            XmlTextReader oku = new XmlTextReader("config.xml");
            while (oku.Read())
            {
                if (oku.NodeType == XmlNodeType.Element)
                {
                    switch (oku.Name)
                    {
                        //SQL Bağlantı Ayarları
                        case "SqlBaglanti":
                            baglanti = oku.ReadString().ToString();
                            break;
                    }
                }
            }
            oku.Close();
            return baglanti;
        }

        private void RegistryIslemleri()
        {
            string sistemkey = license.CPUSeriNo() + license.HDDserino();
            Registry.CurrentUser.CreateSubKey("PWR");
            RegistryKey PtsReg = Registry.CurrentUser.OpenSubKey("PWR", true);
            try { key = PtsReg.GetValue("key").ToString(); } catch (Exception e) { }
            try
            {
                //baglanti = PtsReg.GetValue("sql").ToString();
            }
            catch (Exception e) { }

            if (string.IsNullOrEmpty(key))
            {
                try { PtsReg.SetValue("key", "1"); } catch (Exception e) { }
                DialogResult dr = MessageBox.Show("Lisans Hatası", "Lisans", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (dr == DialogResult.OK) Environment.Exit(0);
            }
            else
            {
                if (sistemkey != key)
                {
                    DialogResult dr = MessageBox.Show("Lisans Hatası", "Lisans", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (dr == DialogResult.OK) Environment.Exit(0);
                }
            }
            if (string.IsNullOrEmpty(baglanti))
            {
                //try { PtsReg.SetValue("sql", "Data Source=.; Initial Catalog=BrowserTakip; User Id=sa; Password=Recep123"); } catch (Exception e) { }
            }
            db = new BrowserContext(baglanti);
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(base.Handle, 161, 2, 0);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static string GetChromeUrl(Process process)
        {
            //Process[] procsChrome = Process.GetProcessesByName("chrome");
            //foreach (Process chrome in procsChrome)
            //{
            //    if (chrome.MainWindowHandle == IntPtr.Zero)
            //        continue;

            //    AutomationElement element2 = AutomationElement.FromHandle(chrome.MainWindowHandle);
            //    if (element2 == null)
            //        return null;
            //    Condition conditions = new AndCondition(
            //        new PropertyCondition(AutomationElement.ProcessIdProperty, chrome.Id),
            //        new PropertyCondition(AutomationElement.IsControlElementProperty, true),
            //        new PropertyCondition(AutomationElement.IsContentElementProperty, true),
            //        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

            //    AutomationElement elementx = element2.FindFirst(TreeScope.Descendants, conditions);

            //    return ((ValuePattern)elementx.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            //}
            //return "";

            if (process == null)
                throw new ArgumentNullException("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;

            AutomationElement edit = element.FindFirst(TreeScope.Descendants,
             new AndCondition(
                  new PropertyCondition(AutomationElement.NameProperty, "adres ve arama çubuğu", PropertyConditionFlags.IgnoreCase),
                  new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                  new PropertyCondition(AutomationElement.IsContentElementProperty, true),
                  new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)));
            if (edit == null) return null;
            string a = ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            if (a != null)
            {
                if (Regex.IsMatch(a, @"^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$"))
                {
                    // prepend http:// to the url, because Chrome hides it if it's not SSL
                    if (!a.StartsWith("http"))
                    {
                        a = "http://" + a;
                    }
                    return a;
                }
            }
            if (edit.Current.HasKeyboardFocus == false)
            {
                return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            }
            else
            {
                return null;
            }
        }

        public static string GetInternetExplorerUrl(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;

            AutomationElement rebar = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ReBarWindow32"));
            if (rebar == null)
                return null;

            AutomationElement edit = rebar.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            if (edit == null) return null;
            return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
        }

        public static string GetFirefoxUrl(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;

            //var a = element.Current.Name;
            //AutomationElementCollection desktopChildren =
            //AutomationElement.FocusedElement.FindAll(
            //TreeScope.Children, Condition.TrueCondition);

            var g = AutomationElement.NameProperty.ToString();
            //var doc2 = element.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Document));
            string nameProp =
            element.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
            var h = element.Current;
            Condition conditions = new AndCondition(
                    new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id),
                    new PropertyCondition(AutomationElement.NameProperty, "adres ve arama çubuğu"),
                    new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                    new PropertyCondition(AutomationElement.IsContentElementProperty, true),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

            AutomationElement doc = element.FindFirst(TreeScope.Descendants, conditions);
            if (doc == null)
                return null;

            if (process.MainWindowTitle != "")
            {
                var e = doc.Current;
                string c = doc.Current.Name;
                var d = element.Current.NativeWindowHandle;
                Process b = process;
            }

            return ((ValuePattern)doc.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
        }

        private string sonurl = "";

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Process process in Process.GetProcessesByName("iexplore"))
            {
                string url = GetInternetExplorerUrl(process);
                if (url == null) continue;
                if (url == sonurl) continue;

                richTextBox1.Text += (url) + "\n";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("chrome"))
                {
                    string url = GetChromeUrl(process);
                    if (url == null)
                        continue;

                    richTextBox1.Text += ("CH Url for '" + process.MainWindowTitle + "' is " + url) + "\n";
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("firefox"))
                {
                    string url = GetFirefoxUrl(process);
                    if (url == null)
                        continue;

                    richTextBox1.Text += ("FF Url for '" + process.MainWindowTitle + "' is " + url) + "\n";
                }
            }
            catch (Exception)
            {
            }
        }

        public bool InternetKontrol()
        {
            try
            {
                System.Net.Sockets.TcpClient kontrol_client = new System.Net.Sockets.TcpClient("www.google.com.tr", 80);
                kontrol_client.SendTimeout = 500;
                kontrol_client.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void timer1_Tick(object sender, EventArgs e)
        {
            labelsaat.Text = DateTime.Now.ToString("HH:mm");
            labeltarih.Text = DateTime.Now.ToString("dd-MM-yyyy");

            Process[] AllProcesses = Process.GetProcesses();
            foreach (var process in AllProcesses)
            {
                if (process.MainWindowTitle != "")
                {
                    string s = process.ProcessName.ToLower();
                    if (s == "opera" || s == "firefox" || s == "iexplore" || s == "microsoftedgecp")
                        process.Kill();
                }
            }

            if (user == null) { OturumuKapat(); return; }
            try
            {
                if (!InternetKontrol())
                {
                    OturumuKapat();
                    MessageBox.Show("Lütfen Öncelikle İnternet Bağlantınızı Kontrol Ediniz...", "İnternete Bağlı Değilsiniz");
                    return;
                }
                using (var bdb = new BrowserContext(baglanti))
                {
                    User kullanici = bdb.UserSet.FirstOrDefault(u => u.username == user.username);
                    if (kullanici.Izin == 0) { richTextBox1.Clear(); kullanici.Durum = 0; bdb.SaveChanges(); OturumuKapat(); return; }
                    kullanici.SonGuncelleme = DateTime.Now;
                    kullanici.Durum = 1;
                    foreach (Process process in Process.GetProcessesByName("chrome"))
                    {
                        string url = GetChromeUrl(process);

                        //string url = urlal();
                        if (string.IsNullOrEmpty(url)) continue;
                        if (url == sonurl) continue;
                        if (sonurl.Contains(url)) continue;
                        //if (!url.Contains("http") || !url.Contains("https")) continue;
                        richTextBox1.Text += (url) + "\n";
                        sonurl = url;

                        bdb.BrowserLogSet.Add(new BrowserLog()
                        {
                            UserId = kullanici.UserId,
                            User = kullanici,
                            Adres = url,
                            Tarih = DateTime.Now
                        });
                    }
                    bdb.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                using (var bdb = new BrowserContext(baglanti))
                {
                    User kullanici = bdb.UserSet.FirstOrDefault(u => u.username == user.username);
                    TimeSpan ts = DateTime.Now.Subtract(kullanici.SonGuncelleme);
                    if (ts.TotalSeconds > 30 && ts.TotalMinutes < 10)
                    {
                        using (var bdb2 = new BrowserContext(baglanti))
                        {
                            if (user == null) { OturumuKapat(); return; }
                            User kullanici2 = bdb2.UserSet.FirstOrDefault(u => u.username == user.username);
                            kullanici2.Durum = 0;
                            bdb2.SaveChanges();
                            OturumuKapat();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void OturumuKapat()
        {
            timer1.Enabled = false;
            LoginForm lgnfrm = (LoginForm)Application.OpenForms["LoginForm"];
            if (lgnfrm != null) return;
            LoginForm loginForm = new LoginForm();
            //loginForm.TopMost = true;
            loginForm.ShowDialog();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var bdb = new BrowserContext(baglanti))
            {
                if (user == null) { OturumuKapat(); return; }
                User kullanici = bdb.UserSet.FirstOrDefault(u => u.username == user.username);
                kullanici.Durum = 0;
                bdb.SaveChanges();
                OturumuKapat();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //MessageBox.Show("Programı Kapatamazsınız.");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Restart();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("opera"))
                {
                    string url = GetFirefoxUrl(process);
                    if (url == null)
                        continue;

                    richTextBox1.Text += ("FF Url for '" + process.MainWindowTitle + "' is " + url) + "\n";
                }
            }
            catch (Exception)
            {
            }
        }
    }
}