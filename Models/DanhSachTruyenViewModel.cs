namespace Truyen.Models
{
    public class DanhSachTruyenViewModel
    {
        public List<TruyenViewModel> Truyens { get; set; } = new();
        public List<TruyenViewModel> TatCaTruyens { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string TuKhoa { get; set; } = "";
        public string TheLoai { get; set; } = "";
        public bool LaKetQuaTimKiem { get; set; }
    }
}
