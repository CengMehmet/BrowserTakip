using License;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private bool _altF4Pressed = false;

        //public User user;
        private string baglanti = "", key = "";

        private string yetki = "";

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        //[StructLayout(LayoutKind.Sequential)]
        private IntPtr ptrHook;

        private LowLevelKeyboardProc objKeyboardProcess;

        private struct KeyboardDLLStruct
        {
            public Keys key;

            public int scanCode;

            public int flags;

            public int time;

            public IntPtr extra;
        }

        public LoginForm()
        {
            InitializeComponent();
            xmlOku();
            RegistryIslemleri();
            Taskbar.Gizle();
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
            FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
        }

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KeyboardDLLStruct objKeyInfo = (KeyboardDLLStruct)Marshal.PtrToStructure(lp, typeof(KeyboardDLLStruct));
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin)
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
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

                        case "yetki":
                            yetki = oku.ReadString().ToString();
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
            try
            {
                //baglanti = PtsReg.GetValue("sql").ToString();
            }
            catch (Exception e) { }
            if (string.IsNullOrEmpty(baglanti))
            {
                try
                {
                    //PtsReg.SetValue("sql", "Data Source=.; Initial Catalog=BrowserTakip; User Id=sa; Password=Recep123");
                }
                catch (Exception e) { }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public void SifreKontrol(string username, string pass)
        {
            if (!InternetKontrol()) { return; }
            using (var bdb = new BrowserContext(baglanti))
            {
                User user = bdb.UserSet.FirstOrDefault(u => u.username == username);
                if (user == null) { MessageBox.Show("Kullanıcı Adı Yanlış!"); return; }
                if (user.pass != pass) { MessageBox.Show("Şifre Yanlış!"); return; }
                if (user.Durum == 1 && username != "admin") { MessageBox.Show("Bir Kullanıcı ile Aynı Anda Yalnızca Tek Giriş Yapılabilir."); return; }
                if (user.username == "admin" && user.pass == pass) { } else { if (!InternetKontrol()) { return; } }
                Form1 frm = (Form1)Application.OpenForms["Form1"];
                if (yetki == "admin") { if (user.username != yetki) { MessageBox.Show("Giriş İzniniz Yok!"); return; } }
                if (user.Izin == 0) { MessageBox.Show("Giriş İzniniz Yok!"); return; }
                user.Durum = 1;
                frm.label1.Text = user.AdSoyad + " Hoşgeldiniz";
                bdb.SaveChanges();
                frm.user = user;
                frm.timer1.Enabled = true;
                Taskbar.Goster();
                this.Close();
            }
        }

        private void LoginForm_SizeChanged(object sender, EventArgs e)
        {
            LoginForm frm = (LoginForm)sender;

            panel1.Location = new Point((frm.Width - panel1.Size.Width) / 2, (frm.Height - panel1.Size.Height) / 2);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                _altF4Pressed = false;
                SifreKontrol(textBox1.Text, textBox2.Text);
            }
            else if (e.KeyCode == (Keys)115)
            {
                _altF4Pressed = true;
            }
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == (Keys)115)
            {
                _altF4Pressed = true;
            }
            else
            {
                _altF4Pressed = false;
            }
            //_altF4Pressed = (e.KeyCode.Equals(Keys.F4) && e.Alt == true);
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_altF4Pressed)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            SifreKontrol(textBox1.Text, textBox2.Text);
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
                MessageBox.Show("Lütfen Öncelikle İnternet Bağlantınızı Kontrol Ediniz...", "İnternete Bağlı Değilsiniz");
                return false;
            }
        }
    }
}