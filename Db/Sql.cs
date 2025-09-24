namespace Db;

using Microsoft.Data.SqlClient;

public sealed class Sql
{
    private readonly string _cs;
    public Sql(string connectionString) => _cs = connectionString;
    public SqlConnection Open() { var c = new SqlConnection(_cs); c.Open(); return c; }
}
