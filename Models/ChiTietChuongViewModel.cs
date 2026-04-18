namespace Truyen.Models
{
    public class ChiTietChuongViewModel
    {
        public int MaChuong { get; set; }
        public string TenChuong { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public int SoThuTu { get; set; }
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = "";
        public int? MaChuongTruoc { get; set; }
        public int? MaChuongSau { get; set; }
        public int LuotDoc { get; set; }
    }
    
    public class DocChuongViewModel
    {
        public ChiTietChuongViewModel Chuong { get; set; } = new();
    }
}