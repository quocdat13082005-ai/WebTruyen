namespace Truyen.Models
{
    public class HomeIndexViewModel
    {
        public List<TruyenViewModel> TruyenHot { get; set; } = new();
        public List<TruyenViewModel> TruyenHay { get; set; } = new();
        public List<TruyenViewModel> TruyenFull { get; set; } = new();
    }
}

