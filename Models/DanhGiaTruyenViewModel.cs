namespace Truyen.Models
{
    public class DanhGiaTruyenViewModel
    {
        public int MaDG { get; set; }
        public int MaTK { get; set; }
        public string HoTen { get; set; } = "";
        public int SoSao { get; set; }
        public string NoiDung { get; set; } = "";
        public DateTime NgayDanhGia { get; set; }
    }
}
