using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WindowsFormsApp1
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string username { get; set; }
        public string pass { get; set; }
        public string AdSoyad { get; set; }
        public string OgrenciNo { get; set; }
        public string Sinif { get; set; }
        public int Durum { get; set; }
        public int Izin { get; set; }
        public DateTime SonGuncelleme { get; set; }
        virtual public List<BrowserLog> Logs { get; set; }
    }
}