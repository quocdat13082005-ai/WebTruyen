using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Truyen.Models;

namespace Truyen.Controllers;

public class DsTimTruyenController : Controller
{
    private readonly string connectionString =
        @"Server=ADMIN-PC;Database=WebTruyen;User Id=sa;Password=13082005;TrustServerCertificate=True;";

    // ===========================
    // CHI TIẾT TRUYỆN
    // ===========================

// ===========================
// DANH SÁCH TRUYỆN
// ===========================
public IActionResult DanhSachTruyen(int page = 1)
{
    const int pageSize = 18;

    var model = new DanhSachTruyenViewModel();

    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        // 1. COUNT tổng truyện
        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Truyen", conn))
        {
            model.TotalCount = (int)cmd.ExecuteScalar();
        }

        model.TotalPages = (int)Math.Ceiling(model.TotalCount / (double)pageSize);
        model.CurrentPage = Math.Clamp(page, 1, model.TotalPages == 0 ? 1 : model.TotalPages);

        int offset = (model.CurrentPage - 1) * pageSize;

        // 2. Lấy danh sách truyện (paging)
        string sql = @"
            SELECT MaTruyen, TenTruyen, TheLoai, AnhBia,
                   ISNULL(LuotXem,0) AS LuotXem,
                   ISNULL(LuotThich,0) AS LuotThich,
                   TrangThai
            FROM Truyen
            ORDER BY MaTruyen DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                model.Truyens.Add(new TruyenViewModel
                {
                    MaTruyen = Convert.ToInt32(reader["MaTruyen"]),
                    TenTruyen = reader["TenTruyen"]?.ToString() ?? "",
                    TheLoai = reader["TheLoai"]?.ToString() ?? "",
                    AnhBia = reader["AnhBia"]?.ToString() ?? "",
                    LuotXem = Convert.ToInt32(reader["LuotXem"]),
                    LuotThich = Convert.ToInt32(reader["LuotThich"]),
                    TrangThai = reader["TrangThai"]?.ToString() ?? ""
                });
            }
        }
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.Message;
    }

    return View(model);
}

public IActionResult TimKiem(string? tuKhoa, string? theLoai, int page = 1)
{
    const int pageSize = 18;

    var model = new DanhSachTruyenViewModel
    {
        TuKhoa = tuKhoa?.Trim() ?? "",
        TheLoai = theLoai?.Trim() ?? "",
        LaKetQuaTimKiem = true
    };

    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        bool coTuKhoa = !string.IsNullOrWhiteSpace(model.TuKhoa);
        bool coTheLoai = !string.IsNullOrWhiteSpace(model.TheLoai);

        string whereClause = "";
        if (coTuKhoa && coTheLoai)
        {
            whereClause = "WHERE (TenTruyen LIKE @tuKhoa OR TheLoai LIKE @tuKhoa) AND TheLoai LIKE @theLoai";
        }
        else if (coTuKhoa)
        {
            whereClause = "WHERE TenTruyen LIKE @tuKhoa OR TheLoai LIKE @tuKhoa";
        }
        else if (coTheLoai)
        {
            whereClause = "WHERE TheLoai LIKE @theLoai";
        }

        using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM Truyen {whereClause}", conn))
        {
            if (coTuKhoa)
                cmd.Parameters.AddWithValue("@tuKhoa", $"%{model.TuKhoa}%");
            if (coTheLoai)
                cmd.Parameters.AddWithValue("@theLoai", $"%{model.TheLoai}%");

            model.TotalCount = Convert.ToInt32(cmd.ExecuteScalar());
        }

        model.TotalPages = (int)Math.Ceiling(model.TotalCount / (double)pageSize);
        model.CurrentPage = Math.Clamp(page, 1, model.TotalPages == 0 ? 1 : model.TotalPages);

        int offset = (model.CurrentPage - 1) * pageSize;

        string sql = $@"
            SELECT MaTruyen, TenTruyen, TheLoai, AnhBia,
                   ISNULL(LuotXem,0) AS LuotXem,
                   ISNULL(LuotThich,0) AS LuotThich,
                   TrangThai
            FROM Truyen
            {whereClause}
            ORDER BY MaTruyen DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        using (var cmd = new SqlCommand(sql, conn))
        {
            if (coTuKhoa)
                cmd.Parameters.AddWithValue("@tuKhoa", $"%{model.TuKhoa}%");
            if (coTheLoai)
                cmd.Parameters.AddWithValue("@theLoai", $"%{model.TheLoai}%");
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                model.Truyens.Add(new TruyenViewModel
                {
                    MaTruyen = Convert.ToInt32(reader["MaTruyen"]),
                    TenTruyen = reader["TenTruyen"]?.ToString() ?? "",
                    TheLoai = reader["TheLoai"]?.ToString() ?? "",
                    AnhBia = reader["AnhBia"]?.ToString() ?? "",
                    LuotXem = Convert.ToInt32(reader["LuotXem"]),
                    LuotThich = Convert.ToInt32(reader["LuotThich"]),
                    TrangThai = reader["TrangThai"]?.ToString() ?? ""
                });
            }
        }

        if (!model.Truyens.Any())
        {
            const string allSql = @"
                SELECT MaTruyen, TenTruyen, TheLoai, AnhBia,
                       ISNULL(LuotXem,0) AS LuotXem,
                       ISNULL(LuotThich,0) AS LuotThich,
                       TrangThai
                FROM Truyen
                ORDER BY MaTruyen DESC
                OFFSET 0 ROWS FETCH NEXT 18 ROWS ONLY";

            using var allCmd = new SqlCommand(allSql, conn);
            using var allReader = allCmd.ExecuteReader();

            while (allReader.Read())
            {
                model.TatCaTruyens.Add(new TruyenViewModel
                {
                    MaTruyen = Convert.ToInt32(allReader["MaTruyen"]),
                    TenTruyen = allReader["TenTruyen"]?.ToString() ?? "",
                    TheLoai = allReader["TheLoai"]?.ToString() ?? "",
                    AnhBia = allReader["AnhBia"]?.ToString() ?? "",
                    LuotXem = Convert.ToInt32(allReader["LuotXem"]),
                    LuotThich = Convert.ToInt32(allReader["LuotThich"]),
                    TrangThai = allReader["TrangThai"]?.ToString() ?? ""
                });
            }
        }
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.Message;
    }

    return View("DanhSachTruyen", model);
}
}
