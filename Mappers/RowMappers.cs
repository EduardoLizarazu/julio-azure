namespace Mappers;

using System.Data;
using Microsoft.Data.SqlClient;

public static class RowMappers
{
    public static Dictionary<string, object?> MapRow(SqlDataReader r)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < r.FieldCount; i++)
        {
            var name = r.GetName(i);
            var val = r.IsDBNull(i) ? null : r.GetValue(i);
            dict[name] = val;
        }
        return dict;
    }

    public static List<Dictionary<string, object?>> MapAll(SqlDataReader r)
    {
        var list = new List<Dictionary<string, object?>>();
        while (r.Read()) list.Add(MapRow(r));
        return list;
    }
}
