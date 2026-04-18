using Microsoft.Data.SqlClient;
using Truyen.Models;

namespace Truyen.Controllers;

public partial class TruyenController
{
    private void LoadDuLieuNguoiDung(int maTruyen, int maTK, ChiTietTruyenViewModel model, SqlConnection conn)
    {
        using (var cmd = new SqlCommand("SELECT MaChuong FROM MuaChuong WHERE MaTK = @tk", conn))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.DaMuaChuong.Add(reader.GetInt32(0));
            }
        }

        using (var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM TheoDoi WHERE MaTK = @tk AND MaTruyen = @truyen", conn))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            cmd.Parameters.AddWithValue("@truyen", maTruyen);
            model.DangTheoDoi = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        using (var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM YeuThichTruyen WHERE MaTK = @tk AND MaTruyen = @truyen", conn))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            cmd.Parameters.AddWithValue("@truyen", maTruyen);
            model.DaYeuThich = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        using (var cmd = new SqlCommand(
            "SELECT TOP 1 SoSao, ISNULL(NoiDung, '') AS NoiDung FROM DanhGiaTruyen WHERE MaTK = @tk AND MaTruyen = @truyen", conn))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            cmd.Parameters.AddWithValue("@truyen", maTruyen);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.SoSaoDaChon = Convert.ToInt32(reader["SoSao"]);
                model.NoiDungDanhGia = reader["NoiDung"]?.ToString() ?? "";
            }
        }
    }

    private void LoadThongTinDanhGia(int maTruyen, ChiTietTruyenViewModel model, SqlConnection conn)
    {
        using (var cmd = new SqlCommand(
            "SELECT ISNULL(AVG(CAST(SoSao AS FLOAT)), 0), COUNT(*) FROM DanhGiaTruyen WHERE MaTruyen = @truyen", conn))
        {
            cmd.Parameters.AddWithValue("@truyen", maTruyen);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.DiemTrungBinh = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                model.TongDanhGia = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
            }
        }

        const string sql = @"
            SELECT DG.MaDG, DG.MaTK, DG.SoSao, ISNULL(DG.NoiDung, '') AS NoiDung,
                   DG.NgayDanhGia, ISNULL(TK.HoTen, TK.TenDangNhap) AS HoTen
            FROM DanhGiaTruyen DG
            INNER JOIN TaiKhoan TK ON DG.MaTK = TK.MaTK
            WHERE DG.MaTruyen = @truyen
            ORDER BY DG.NgayDanhGia DESC";

        using var cmdDanhGia = new SqlCommand(sql, conn);
        cmdDanhGia.Parameters.AddWithValue("@truyen", maTruyen);

        using var readerDanhGia = cmdDanhGia.ExecuteReader();
        while (readerDanhGia.Read())
        {
            model.DanhGias.Add(new DanhGiaTruyenViewModel
            {
                MaDG = Convert.ToInt32(readerDanhGia["MaDG"]),
                MaTK = Convert.ToInt32(readerDanhGia["MaTK"]),
                HoTen = readerDanhGia["HoTen"]?.ToString() ?? "",
                SoSao = Convert.ToInt32(readerDanhGia["SoSao"]),
                NoiDung = readerDanhGia["NoiDung"]?.ToString() ?? "",
                NgayDanhGia = Convert.ToDateTime(readerDanhGia["NgayDanhGia"])
            });
        }
    }

    private void LoadTruyenGoiY(int maTruyen, ChiTietTruyenViewModel model, SqlConnection conn)
    {
        const string sql = @"
            SELECT TOP 18 MaTruyen, TenTruyen, TheLoai, MoTa, AnhBia,
                   ISNULL(LuotXem, 0) AS LuotXem,
                   ISNULL(LuotThich, 0) AS LuotThich,
                   TrangThai, NgayDang
            FROM Truyen
            WHERE MaTruyen <> @id
            ORDER BY
                CASE
                    WHEN @theLoai <> '' AND ISNULL(TheLoai, '') = @theLoai THEN 0
                    ELSE 1
                END,
                ISNULL(LuotThich, 0) DESC,
                ISNULL(LuotXem, 0) DESC,
                MaTruyen DESC";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", maTruyen);
        cmd.Parameters.AddWithValue("@theLoai", model.Truyen.TheLoai ?? string.Empty);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            model.GoiYTruyens.Add(MapTruyen(reader));
        }
    }

    private void GhiNhanChuongDaDoc(int maTK, int maChuong, SqlConnection conn)
    {
        using var cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM ChuongDaDoc WHERE MaTK = @tk AND MaChuong = @chuong)
            BEGIN
                INSERT INTO ChuongDaDoc(MaTK, MaChuong) VALUES(@tk, @chuong)
            END", conn);

        cmd.Parameters.AddWithValue("@tk", maTK);
        cmd.Parameters.AddWithValue("@chuong", maChuong);
        cmd.ExecuteNonQuery();
    }

    private void EnsureInteractionTables(SqlConnection conn)
    {
        const string sql = @"
            IF OBJECT_ID(N'TheoDoi', N'U') IS NULL
            BEGIN
                CREATE TABLE TheoDoi
                (
                    MaTK INT NOT NULL,
                    MaTruyen INT NOT NULL,
                    NgayTheoDoi DATETIME NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT PK_TheoDoi PRIMARY KEY (MaTK, MaTruyen)
                );
            END;

            IF OBJECT_ID(N'YeuThichTruyen', N'U') IS NULL
            BEGIN
                CREATE TABLE YeuThichTruyen
                (
                    MaTK INT NOT NULL,
                    MaTruyen INT NOT NULL,
                    NgayYeuThich DATETIME NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT PK_YeuThichTruyen PRIMARY KEY (MaTK, MaTruyen)
                );
            END;

            IF OBJECT_ID(N'DanhGiaTruyen', N'U') IS NULL
            BEGIN
                CREATE TABLE DanhGiaTruyen
                (
                    MaDG INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaTK INT NOT NULL,
                    MaTruyen INT NOT NULL,
                    SoSao INT NOT NULL,
                    NoiDung NVARCHAR(500) NULL,
                    NgayDanhGia DATETIME NOT NULL DEFAULT GETDATE()
                );

                CREATE UNIQUE INDEX UX_DanhGiaTruyen_MaTK_MaTruyen
                ON DanhGiaTruyen(MaTK, MaTruyen);
            END;

            IF OBJECT_ID(N'ChuongDaDoc', N'U') IS NULL
            BEGIN
                CREATE TABLE ChuongDaDoc
                (
                    MaTK INT NOT NULL,
                    MaChuong INT NOT NULL,
                    NgayDoc DATETIME NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT PK_ChuongDaDoc PRIMARY KEY (MaTK, MaChuong)
                );
            END;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private TuSachCaNhanViewModel LoadTuSach(string tieuDe, string moTa, string tableName)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        var model = new TuSachCaNhanViewModel
        {
            TieuDe = tieuDe,
            MoTa = moTa
        };

        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            EnsureInteractionTables(conn);

            string sql = $@"
                SELECT T.MaTruyen, T.TenTruyen, T.TheLoai, T.MoTa, T.AnhBia,
                       ISNULL(T.LuotXem,0) AS LuotXem,
                       ISNULL(T.LuotThich,0) AS LuotThich,
                       T.TrangThai, T.NgayDang
                FROM {tableName} X
                INNER JOIN Truyen T ON X.MaTruyen = T.MaTruyen
                WHERE X.MaTK = @tk
                ORDER BY X.MaTruyen DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tk", maTK ?? throw new InvalidOperationException("Người dùng chưa đăng nhập."));

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.Truyens.Add(MapTruyen(reader));
                }
            }

            LoadTruyenGoiYChoTuSach(
                model,
                conn,
                tableName,
                maTK ?? throw new InvalidOperationException("Người dùng chưa đăng nhập."));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return model;
    }

    private void LoadTruyenGoiYChoTuSach(TuSachCaNhanViewModel model, SqlConnection conn, string tableName, int maTK)
    {
        string sql = $@"
            SELECT TOP 18 T.MaTruyen, T.TenTruyen, T.TheLoai, T.MoTa, T.AnhBia,
                   ISNULL(T.LuotXem, 0) AS LuotXem,
                   ISNULL(T.LuotThich, 0) AS LuotThich,
                   T.TrangThai, T.NgayDang
            FROM Truyen T
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM {tableName} X
                WHERE X.MaTK = @tk AND X.MaTruyen = T.MaTruyen
            )
            ORDER BY ISNULL(T.LuotThich, 0) DESC,
                     ISNULL(T.LuotXem, 0) DESC,
                     T.MaTruyen DESC";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tk", maTK);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            model.GoiYTruyens.Add(MapTruyen(reader));
        }
    }

    private bool DaDangNhap()
    {
        return HttpContext.Session.GetInt32("MaTK").HasValue;
    }

    private int GetSoDuXu(int tkId, SqlConnection conn)
    {
        using var cmd = new SqlCommand(
            "SELECT ISNULL(SoDuXu,0) FROM TaiKhoan WHERE MaTK = @tk", conn);

        cmd.Parameters.AddWithValue("@tk", tkId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private int GetGiaChuong(int maChuong, SqlConnection conn)
    {
        using var cmd = new SqlCommand(
            "SELECT ISNULL(GiaXu,0) FROM Chuong WHERE MaChuong = @id", conn);

        cmd.Parameters.AddWithValue("@id", maChuong);
        return Convert.ToInt32(cmd.ExecuteScalar());
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
            LuotXem = reader["LuotXem"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LuotXem"]),
            LuotThich = reader["LuotThich"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LuotThich"]),
            TrangThai = reader["TrangThai"]?.ToString() ?? "",
            NgayDang = reader["NgayDang"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["NgayDang"])
        };
    }
}
