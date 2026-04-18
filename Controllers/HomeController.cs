using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Truyen.Models;

namespace Truyen.Controllers;

public class HomeController : Controller
{
    private readonly string connectionString =
        @"Server=ADMIN-PC;Database=WebTruyen;User Id=sa;Password=13082005;TrustServerCertificate=True;";

    public IActionResult Index()
    {
        var model = new HomeIndexViewModel();

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            // ===============================
            // Truyện Hot
            // ===============================
            string hotSql = @"
                SELECT TOP 10 *
                FROM Truyen
                ORDER BY LuotXem DESC";

            using (var cmd = new SqlCommand(hotSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.TruyenHot.Add(MapTruyen(reader));
                }
            }

            // ===============================
            // Truyện Hay
            // ===============================
            string haySql = @"
                SELECT TOP 10 *
                FROM Truyen
                ORDER BY LuotThich DESC";

            using (var cmd = new SqlCommand(haySql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.TruyenHay.Add(MapTruyen(reader));
                }
            }

            // ===============================
            // Truyện Full
            // ===============================
            string fullSql = @"
                SELECT TOP 10 *
                FROM Truyen
                WHERE TrangThai = N'Full'
                ORDER BY LuotXem DESC";

            using (var cmd = new SqlCommand(fullSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.TruyenFull.Add(MapTruyen(reader));
                }
            }
        }
        catch
        {
            // lỗi db thì trả list rỗng
        }

        return View(model);
    }

    public IActionResult TheLoai()
    {
        return View();
    }

    public IActionResult Profile()
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");

        if (maTK == null)
        {
            TempData["Error"] = "Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            string sql = @"
                SELECT *
                FROM TaiKhoan
                WHERE MaTK = @matk";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matk", maTK);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var model = new ProfileViewModel
                {
                    TenDangNhap = reader["TenDangNhap"]?.ToString() ?? "",
                    HoTen = reader["HoTen"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    VaiTro = reader["VaiTro"]?.ToString() ?? "",

                    SoDuXu = reader["SoDuXu"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["SoDuXu"]),

                    NgayTao = reader["NgayTao"] == DBNull.Value
                        ? DateTime.Now
                        : Convert.ToDateTime(reader["NgayTao"])
                };

                return View(model);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    private TruyenViewModel MapTruyen(SqlDataReader reader)
    {
        return new TruyenViewModel
        {
            MaTruyen = Convert.ToInt32(reader["MaTruyen"]),
            TenTruyen = reader["TenTruyen"]?.ToString() ?? "",
            TheLoai = reader["TheLoai"]?.ToString() ?? "",
            MoTa = reader["MoTa"]?.ToString() ?? "",
            AnhBia = reader["AnhBia"]?.ToString(),

            LuotXem = reader["LuotXem"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["LuotXem"]),

            LuotThich = reader["LuotThich"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["LuotThich"]),

            TrangThai = reader["TrangThai"]?.ToString() ?? "",

            NgayDang = reader["NgayDang"] == DBNull.Value
                ? DateTime.Now
                : Convert.ToDateTime(reader["NgayDang"])
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}