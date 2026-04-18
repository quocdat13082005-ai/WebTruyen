using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Truyen.Controllers;

public partial class TruyenController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TheoDoi(int maTruyen, string? returnUrl = null)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
        {
            TempData["Error"] = "Đăng nhập trước khi theo dõi truyện.";
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            EnsureInteractionTables(conn);

            bool daTheoDoi;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM TheoDoi WHERE MaTK = @tk AND MaTruyen = @truyen", conn))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                daTheoDoi = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }

            string sql = daTheoDoi
                ? "DELETE FROM TheoDoi WHERE MaTK = @tk AND MaTruyen = @truyen"
                : "INSERT INTO TheoDoi(MaTK, MaTruyen) VALUES(@tk, @truyen)";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = daTheoDoi ? "Đã bỏ theo dõi truyện." : "Đã thêm vào danh sách theo dõi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(returnUrl ?? Url.Action("ChiTiet", new { id = maTruyen })!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult YeuThich(int maTruyen, string? returnUrl = null)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
        {
            TempData["Error"] = "Đăng nhập trước khi yêu thích truyện.";
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            EnsureInteractionTables(conn);

            using var tran = conn.BeginTransaction();

            bool daYeuThich;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM YeuThichTruyen WHERE MaTK = @tk AND MaTruyen = @truyen", conn, tran))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                daYeuThich = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }

            using (var cmd = new SqlCommand(
                daYeuThich
                    ? "DELETE FROM YeuThichTruyen WHERE MaTK = @tk AND MaTruyen = @truyen"
                    : "INSERT INTO YeuThichTruyen(MaTK, MaTruyen) VALUES(@tk, @truyen)",
                conn, tran))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand(@"
                UPDATE Truyen
                SET LuotThich =
                    CASE
                        WHEN @boThich = 1 THEN CASE WHEN ISNULL(LuotThich, 0) > 0 THEN LuotThich - 1 ELSE 0 END
                        ELSE ISNULL(LuotThich, 0) + 1
                    END
                WHERE MaTruyen = @truyen", conn, tran))
            {
                cmd.Parameters.AddWithValue("@boThich", daYeuThich ? 1 : 0);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                cmd.ExecuteNonQuery();
            }

            tran.Commit();
            TempData["Success"] = daYeuThich ? "Đã bỏ yêu thích truyện." : "Đã thêm vào truyện yêu thích.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(returnUrl ?? Url.Action("ChiTiet", new { id = maTruyen })!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DanhGia(int maTruyen, int soSao, string? noiDung, string? returnUrl = null)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
        {
            TempData["Error"] = "Đăng nhập trước khi đánh giá truyện.";
            return RedirectToAction("Login", "Auth");
        }

        if (soSao < 1 || soSao > 5)
        {
            TempData["Error"] = "Số sao phải trong khoảng từ 1 đến 5.";
            return Redirect(returnUrl ?? Url.Action("ChiTiet", new { id = maTruyen })!);
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            EnsureInteractionTables(conn);

            bool daDanhGia;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM DanhGiaTruyen WHERE MaTK = @tk AND MaTruyen = @truyen", conn))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                daDanhGia = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }

            const string updateSql = @"
                UPDATE DanhGiaTruyen
                SET SoSao = @soSao,
                    NoiDung = @noiDung,
                    NgayDanhGia = GETDATE()
                WHERE MaTK = @tk AND MaTruyen = @truyen";

            const string insertSql = @"
                INSERT INTO DanhGiaTruyen(MaTK, MaTruyen, SoSao, NoiDung)
                VALUES(@tk, @truyen, @soSao, @noiDung)";

            using (var cmd = new SqlCommand(daDanhGia ? updateSql : insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@tk", maTK.Value);
                cmd.Parameters.AddWithValue("@truyen", maTruyen);
                cmd.Parameters.AddWithValue("@soSao", soSao);
                cmd.Parameters.AddWithValue("@noiDung", (object?)noiDung?.Trim() ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = daDanhGia ? "Đã cập nhật đánh giá." : "Đã gửi đánh giá.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(returnUrl ?? Url.Action("ChiTiet", new { id = maTruyen })!);
    }

    public IActionResult TheoDoi()
    {
        if (!DaDangNhap())
        {
            TempData["Error"] = "Đăng nhập trước để xem tủ sách của bạn.";
            return RedirectToAction("Login", "Auth");
        }

        return View("TuSachCaNhan", LoadTuSach(
            "Truyện đang theo dõi",
            "Danh sách các truyện bạn đã theo dõi.",
            "TheoDoi"));
    }

    public IActionResult YeuThich()
    {
        if (!DaDangNhap())
        {
            TempData["Error"] = "Đăng nhập trước để xem tủ sách của bạn.";
            return RedirectToAction("Login", "Auth");
        }

        return View("TuSachCaNhan", LoadTuSach(
            "Truyện yêu thích",
            "Danh sách các truyện bạn đã đánh dấu yêu thích.",
            "YeuThichTruyen"));
    }
}
