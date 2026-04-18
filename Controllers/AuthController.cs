using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Truyen.Controllers
{
    public class AuthController : Controller
    {
        private readonly string connectionString =
            @"Server=ADMIN-PC;
              Database=WebTruyen;
              User Id=sa;
              Password=13082005;
              TrustServerCertificate=True;";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Truyen.Models.LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            string sql = @"
                SELECT MaTK, TenDangNhap, HoTen, VaiTro
                FROM TaiKhoan
                WHERE (TenDangNhap = @tk OR Email = @tk)
                AND MatKhau = @mk";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@tk", SqlDbType.NVarChar).Value = model.Username;
            cmd.Parameters.Add("@mk", SqlDbType.NVarChar).Value = model.Password;

            using SqlDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                int maTK = reader.GetInt32("MaTK");
                string tenDangNhap = reader.GetString("TenDangNhap");
                string hoTen = reader.GetString("HoTen");
                string vaiTro = reader.GetString("VaiTro");

                // Validate role match (optional)
                if (!string.IsNullOrEmpty(model.Role) && model.Role != vaiTro)
                {
                    ModelState.AddModelError("Role", "Vai trò chọn không khớp với tài khoản.");
                    return View(model);
                }

                // Lưu Session
                HttpContext.Session.SetInt32("MaTK", maTK);
                HttpContext.Session.SetString("TenDangNhap", tenDangNhap);
                HttpContext.Session.SetString("HoTen", hoTen);
                HttpContext.Session.SetString("VaiTro", vaiTro);

                // Điều hướng theo role
                return vaiTro switch
                {
                    "NguoiDoc" => RedirectToAction("Index", "Home"),
                    "TacGia" => RedirectToAction("Index", "TacGia"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ModelState.AddModelError("", "Sai tên thông tin đăng nhập.");
            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Truyen.Models.RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            // Kiểm tra tồn tại
            string checkSql = @"
                SELECT COUNT(*) FROM TaiKhoan 
                WHERE TenDangNhap = @tdn OR Email = @email";
            using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
            {
                checkCmd.Parameters.Add("@tdn", SqlDbType.NVarChar).Value = model.TenDangNhap;
                checkCmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = model.Email;
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại.");
                    return View(model);
                }
            }

            // Insert mới
            string insertSql = @"
                INSERT INTO TaiKhoan (TenDangNhap, MatKhau, HoTen, Email, VaiTro)
                OUTPUT INSERTED.MaTK
                VALUES (@tdn, @mk, @ht, @email, 'NguoiDoc')";
            using (SqlCommand insertCmd = new SqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.Add("@tdn", SqlDbType.NVarChar).Value = model.TenDangNhap;
                insertCmd.Parameters.Add("@mk", SqlDbType.NVarChar).Value = model.MatKhau;
                insertCmd.Parameters.Add("@ht", SqlDbType.NVarChar).Value = model.HoTen;
                insertCmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = model.Email;
                int maTK = (int)insertCmd.ExecuteScalar();

                // Lấy thông tin để set session (auto-login)
                string selectSql = @"
                    SELECT TenDangNhap, HoTen, VaiTro FROM TaiKhoan WHERE MaTK = @matk";
                using (SqlCommand selectCmd = new SqlCommand(selectSql, conn))
                {
                    selectCmd.Parameters.Add("@matk", SqlDbType.Int).Value = maTK;
                    using SqlDataReader reader = selectCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string tenDangNhap = reader.GetString("TenDangNhap");
                        string hoTen = reader.GetString("HoTen");
                        string vaiTro = reader.GetString("VaiTro");

                        HttpContext.Session.SetInt32("MaTK", maTK);
                        HttpContext.Session.SetString("TenDangNhap", tenDangNhap);
                        HttpContext.Session.SetString("HoTen", hoTen);
                        HttpContext.Session.SetString("VaiTro", vaiTro);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}