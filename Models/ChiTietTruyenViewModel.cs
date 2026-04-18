using System.ComponentModel.DataAnnotations;

namespace Truyen.Models
{
public class ChiTietTruyenViewModel
{
    public TruyenViewModel Truyen { get; set; } = new();
    public List<ChuongViewModel> Chuongs { get; set; } = new();
    public List<TruyenViewModel> GoiYTruyens { get; set; } = new();
    public List<int> DaMuaChuong { get; set; } = new();
    public bool DangTheoDoi { get; set; }
    public bool DaYeuThich { get; set; }
    public int SoSaoDaChon { get; set; }
    public string NoiDungDanhGia { get; set; } = "";
    public double DiemTrungBinh { get; set; }
    public int TongDanhGia { get; set; }
    public List<DanhGiaTruyenViewModel> DanhGias { get; set; } = new();
}
}
