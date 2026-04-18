using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Truyen.Models;

namespace Truyen.Controllers;

public class MuaChuongController : Controller
{
    private readonly string connectionString =
        @"Server=ADMIN-PC;Database=WebTruyen;User Id=sa;Password=13082005;TrustServerCertificate=True;";

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

}
