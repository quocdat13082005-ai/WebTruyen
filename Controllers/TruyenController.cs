using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Truyen.Models;

namespace Truyen.Controllers;

public class TruyenController : Controller
{
    private readonly string connectionString =
        @"Server=ADMIN-PC;Database=WebTruyen;User Id=sa;Password=13082005;TrustServerCertificate=True;";

    // ===========================
    // CHI TIẾT TRUYỆN
    // ===========================
    public IActionResult ChiTiet(int id)
    {
        var model = new ChiTietTruyenViewModel();

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            // tăng lượt xem
            using (var cmd = new SqlCommand(
                "UPDATE Truyen SET LuotXem = ISNULL(LuotXem,0)+1 WHERE MaTruyen=@id", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.ExecuteNonQuery();
            }

            // thông tin truyện
            string sql = @"
            SELECT T.*, ISNULL(TK.HoTen,N'Không rõ') AS TacGia
            FROM Truyen T
            LEFT JOIN TaiKhoan TK ON T.MaTacGia = TK.MaTK
            WHERE T.MaTruyen=@id";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    model.Truyen = new TruyenViewModel
                    {
                        MaTruyen = Convert.ToInt32(reader["MaTruyen"]),
                        TenTruyen = reader["TenTruyen"]?.ToString() ?? "",
                        TheLoai = reader["TheLoai"]?.ToString() ?? "",
                        MoTa = reader["MoTa"]?.ToString() ?? "",
                        AnhBia = reader["AnhBia"]?.ToString() ?? "",
                        LuotXem = Convert.ToInt32(reader["LuotXem"]),
                        LuotThich = Convert.ToInt32(reader["LuotThich"]),
                        TrangThai = reader["TrangThai"]?.ToString() ?? "",
                        TacGia = reader["TacGia"]?.ToString() ?? ""
                    };
                }
                else
                {
                    return NotFound();
                }
            }

            // danh sách chương
            using (var cmd = new SqlCommand(
                "SELECT MaChuong,TenChuong,SoChuong FROM Chuong WHERE MaTruyen=@id ORDER BY SoChuong", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    model.Chuongs.Add(new ChuongViewModel
                    {
                        MaChuong = Convert.ToInt32(reader["MaChuong"]),
                        TenChuong = reader["TenChuong"]?.ToString() ?? "",
                        SoThuTu = Convert.ToInt32(reader["SoChuong"])
                    });
                }
            }

            // Load user's purchased chapters
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK.HasValue)
            {
                using (var cmd = new SqlCommand(
                    "SELECT MaChuong FROM MuaChuong WHERE MaTK=@tk", conn))
                {
                    cmd.Parameters.AddWithValue("@tk", maTK.Value);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        model.DaMuaChuong.Add(reader.GetInt32(0));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return View(model);
    }

    // ===========================
    // ĐỌC CHƯƠNG
    // ===========================
    public IActionResult DocChuong(int id)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");

        if (maTK == null)
        {
            TempData["Error"] = "Đăng nhập trước!";
            return RedirectToAction("Login", "Auth");
        }

        var model = new DocChuongViewModel();

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            int tkId = maTK.Value;

            // check đã mua
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM MuaChuong WHERE MaTK=@tk AND MaChuong=@c", conn))
            {
                cmd.Parameters.AddWithValue("@tk", tkId);
                cmd.Parameters.AddWithValue("@c", id);

                int daMua = (int)cmd.ExecuteScalar();

                if (daMua == 0)
                {
                    ViewBag.MaChuong = id;
                    ViewBag.SoDu = GetSoDuXu(tkId, conn);
                    ViewBag.GiaXu = GetGiaChuong(id, conn);

                    return View("MuaChuong");
                }
            }

            // tăng lượt đọc
            using (var cmd = new SqlCommand(
                "UPDATE Chuong SET LuotDoc = ISNULL(LuotDoc,0)+1 WHERE MaChuong=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // load chương + tên truyện
            string sql = @"
            SELECT C.MaChuong,
                   C.TenChuong,
                   C.NoiDung,
                   C.SoChuong,
                   C.MaTruyen,
                   ISNULL(C.LuotDoc,0) AS LuotDoc,
                   T.TenTruyen
            FROM Chuong C
            INNER JOIN Truyen T ON C.MaTruyen = T.MaTruyen
            WHERE C.MaChuong=@id";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    model.Chuong = new ChiTietChuongViewModel
                    {
                        MaChuong = Convert.ToInt32(reader["MaChuong"]),
                        TenChuong = reader["TenChuong"]?.ToString() ?? "",
                        NoiDung = reader["NoiDung"]?.ToString() ?? "",
                        SoThuTu = Convert.ToInt32(reader["SoChuong"]),
                        MaTruyen = Convert.ToInt32(reader["MaTruyen"]),
                        TenTruyen = reader["TenTruyen"]?.ToString() ?? "",
                        LuotDoc = Convert.ToInt32(reader["LuotDoc"])
                    };
                }
            }

            // chương trước
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 MaChuong
                FROM Chuong
                WHERE MaTruyen=@truyen AND SoChuong < @so
                ORDER BY SoChuong DESC", conn))
            {
                cmd.Parameters.AddWithValue("@truyen", model.Chuong.MaTruyen);
                cmd.Parameters.AddWithValue("@so", model.Chuong.SoThuTu);

                var obj = cmd.ExecuteScalar();

                if (obj != null)
                    model.Chuong.MaChuongTruoc = Convert.ToInt32(obj);
            }

            // chương sau
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 MaChuong
                FROM Chuong
                WHERE MaTruyen=@truyen AND SoChuong > @so
                ORDER BY SoChuong ASC", conn))
            {
                cmd.Parameters.AddWithValue("@truyen", model.Chuong.MaTruyen);
                cmd.Parameters.AddWithValue("@so", model.Chuong.SoThuTu);

                var obj = cmd.ExecuteScalar();

                if (obj != null)
                    model.Chuong.MaChuongSau = Convert.ToInt32(obj);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return View(model);
    }

    // ===========================
    // MUA CHƯƠNG
    // ===========================
[HttpPost]
public IActionResult MuaChuong(int maChuong, string returnUrl = null)
{
    int? maTK = HttpContext.Session.GetInt32("MaTK");

    if (maTK == null)
    {
        if (IsAjaxRequest())
            return Json(new { success = false, message = "Đăng nhập trước!" });
        return RedirectToAction("Login", "Auth");
    }

    string errorMsg = null;
    bool success = false;
    string successMsg = "Mua chương thành công!";

    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        int tkId = maTK.Value;

        using var tran = conn.BeginTransaction();

        try
        {
            // 1. lấy giá chương
            int giaXu = 0;

            using (var cmd = new SqlCommand(
                "SELECT ISNULL(GiaXu,0) FROM Chuong WHERE MaChuong=@id",
                conn, tran))
            {
                cmd.Parameters.AddWithValue("@id", maChuong);
                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    errorMsg = "Chương không tồn tại!";
                    tran.Rollback();
                    goto EndTransaction;
                }
                giaXu = Convert.ToInt32(result);
            }

            // 2. kiểm tra đã mua chưa
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM MuaChuong WHERE MaTK=@tk AND MaChuong=@c",
                conn, tran))
            {
                cmd.Parameters.AddWithValue("@tk", tkId);
                cmd.Parameters.AddWithValue("@c", maChuong);

                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    errorMsg = "Bạn đã mua rồi!";
                    tran.Rollback();
                    goto EndTransaction;
                }
            }

            // 3. trừ xu
            using (var cmd = new SqlCommand(@"
                UPDATE TaiKhoan
                SET SoDuXu = SoDuXu - @gia
                WHERE MaTK=@tk AND SoDuXu >= @gia",
                conn, tran))
            {
                cmd.Parameters.AddWithValue("@gia", giaXu);
                cmd.Parameters.AddWithValue("@tk", tkId);

                if (cmd.ExecuteNonQuery() == 0)
                {
                    errorMsg = "Xu không đủ!";
                    tran.Rollback();
                    goto EndTransaction;
                }
            }

            // 4. lưu mua
            using (var cmd = new SqlCommand(@"
                INSERT INTO MuaChuong(MaTK,MaChuong,SoXuDaTru)
                VALUES(@tk,@c,@gia)",
                conn, tran))
            {
                cmd.Parameters.AddWithValue("@tk", tkId);
                cmd.Parameters.AddWithValue("@c", maChuong);
                cmd.Parameters.AddWithValue("@gia", giaXu);
                cmd.ExecuteNonQuery();
            }

            tran.Commit();
            success = true;
        }
        catch
        {
            tran.Rollback();
            errorMsg = "Lỗi hệ thống!";
        }

    EndTransaction:
        // AJAX response
        if (IsAjaxRequest())
        {
            if (success)
                return Json(new { success = true, message = successMsg });
            return Json(new { success = false, message = errorMsg ?? "Lỗi không xác định!" });
        }

        // Regular form
        if (success)
        {
            TempData["Success"] = successMsg;
            return Redirect(returnUrl ?? "/");
        }
        else
        {
            TempData["Error"] = errorMsg ?? "Lỗi hệ thống!";
            return Redirect(returnUrl ?? "/");
        }
    }
    catch
    {
        if (IsAjaxRequest())
            return Json(new { success = false, message = "Lỗi kết nối!" });
        TempData["Error"] = "Lỗi kết nối!";
        return RedirectToAction("Index", "Home");
    }
}

private bool IsAjaxRequest()
{
    return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
}

    // ===========================
    // HÀM PHỤ
    // ===========================
    private int GetSoDuXu(int tkId, SqlConnection conn)
    {
        using var cmd = new SqlCommand(
            "SELECT ISNULL(SoDuXu,0) FROM TaiKhoan WHERE MaTK=@tk", conn);

        cmd.Parameters.AddWithValue("@tk", tkId);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private int GetGiaChuong(int maChuong, SqlConnection conn)
    {
        using var cmd = new SqlCommand(
            "SELECT ISNULL(GiaXu,0) FROM Chuong WHERE MaChuong=@id", conn);

        cmd.Parameters.AddWithValue("@id", maChuong);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

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
