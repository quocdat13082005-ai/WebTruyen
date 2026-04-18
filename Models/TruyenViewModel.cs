namespace Truyen.Models
{
    public class TruyenViewModel
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = "";
        public string TheLoai { get; set; } = "";
        public string MoTa { get; set; } = "";
        public string? AnhBia { get; set; }
        public int LuotXem { get; set; }
        public int LuotThich { get; set; }
        public string TrangThai { get; set; } = "Đang ra";
        public DateTime NgayDang { get; set; }
        public string? TacGia { get; set; }
    }
}


