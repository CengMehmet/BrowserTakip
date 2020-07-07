using System;

namespace WindowsFormsApp1
{
    public class BrowserLog
    {
        public int id { get; set; }
        public int UserId { get; set; }
        virtual public User User { get; set; }
        public string Adres { get; set; }
        public DateTime Tarih { get; set; }
    }
}