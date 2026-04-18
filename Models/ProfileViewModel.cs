using System.ComponentModel.DataAnnotations;

namespace Truyen.Models
{
    public class ProfileViewModel
    {
        public string TenDangNhap { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public int SoDuXu { get; set; }
        public DateTime NgayTao { get; set; }
        public string VaiTro { get; set; } = "";
        
        public string VaiTroDisplay => VaiTro switch
        {
            "NguoiDoc" => "Người đọc",
            "TacGia" => "Tác giả",
            "Admin" => "Quản trị viên",
            _ => VaiTro
        };
    }
}

