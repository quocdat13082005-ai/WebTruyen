using System.ComponentModel.DataAnnotations;

namespace Truyen.Models
{
public class ChiTietTruyenViewModel
{
    public TruyenViewModel Truyen { get; set; } = new();
    public List<ChuongViewModel> Chuongs { get; set; } = new();
    public List<int> DaMuaChuong { get; set; } = new();
}
}