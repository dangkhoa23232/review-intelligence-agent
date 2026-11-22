using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ReviewIntelligence.Api.Data;

public static class DbUpdateExceptionExtensions
{
    /// <summary>Vi phạm ràng buộc UNIQUE / chỉ mục duy nhất (SQL Server lỗi 2601, 2627).</summary>
    public static bool LaViPhamDuyNhat(this DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
