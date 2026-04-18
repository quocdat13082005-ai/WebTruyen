namespace Truyen.Models
{
    public class TuSachCaNhanViewModel
    {
        public string TieuDe { get; set; } = "";
        public string MoTa { get; set; } = "";
        public List<TruyenViewModel> Truyens { get; set; } = new();
        public List<TruyenViewModel> GoiYTruyens { get; set; } = new();
    }
}
