namespace Address;

using System.Text.Json;
using Db;
using Mappers;
using Microsoft.Data.SqlClient;

public static class AddressEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/address");

        // READ ALL
        group.MapGet("/", async (Sql db) =>
        {
            using var c = db.Open();
            using var cmd = new SqlCommand("""
                SELECT AddressID, AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode, rowguid, ModifiedDate
                FROM SalesLT.Address
            """, c);
            using var r = await cmd.ExecuteReaderAsync();
            return Results.Ok(RowMappers.MapAll(r));
        });

        // READ ONE
        group.MapGet("/{id:int}", async (int id, Sql db) =>
        {
            using var c = db.Open();
            using var cmd = new SqlCommand("""
                SELECT AddressID, AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode, rowguid, ModifiedDate
                FROM SalesLT.Address
                WHERE AddressID = @id
            """, c);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = await cmd.ExecuteReaderAsync();
            if (!r.HasRows) return Results.NotFound();
            r.Read();
            return Results.Ok(RowMappers.MapRow(r));
        });

        // CREATE
        group.MapPost("/", async (JsonElement body, Sql db) =>
        {
            // required minimal set:
            string addressLine1 = body.GetProperty("AddressLine1").GetString()!;
            string city = body.GetProperty("City").GetString()!;
            string stateProv = body.GetProperty("StateProvince").GetString()!;
            string countryReg = body.GetProperty("CountryRegion").GetString()!;
            string postalCode = body.GetProperty("PostalCode").GetString()!;
            string? addressLine2 = body.TryGetProperty("AddressLine2", out var al2) ? al2.GetString() : null;

            using var c = db.Open();
            using var cmd = new SqlCommand("""
                INSERT INTO SalesLT.Address
                (AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode, rowguid, ModifiedDate)
                OUTPUT INSERTED.AddressID, INSERTED.AddressLine1, INSERTED.AddressLine2, INSERTED.City, INSERTED.StateProvince,
                       INSERTED.CountryRegion, INSERTED.PostalCode, INSERTED.rowguid, INSERTED.ModifiedDate
                VALUES (@a1, @a2, @city, @sp, @cr, @pc, NEWID(), SYSUTCDATETIME());
            """, c);

            cmd.Parameters.AddWithValue("@a1", addressLine1);
            cmd.Parameters.AddWithValue("@a2", (object?)addressLine2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@sp", stateProv);
            cmd.Parameters.AddWithValue("@cr", countryReg);
            cmd.Parameters.AddWithValue("@pc", postalCode);

            using var r = await cmd.ExecuteReaderAsync();
            r.Read();
            var created = RowMappers.MapRow(r);
            return Results.Created($"/api/address/{created["AddressID"]}", created);
        });

        // UPDATE
        group.MapPut("/{id:int}", async (int id, JsonElement body, Sql db) =>
        {
            using var c = db.Open();
            using var cmd = new SqlCommand("""
                UPDATE SalesLT.Address
                SET AddressLine1   = COALESCE(@a1, AddressLine1),
                    AddressLine2   = @a2,
                    City           = COALESCE(@city, City),
                    StateProvince  = COALESCE(@sp, StateProvince),
                    CountryRegion  = COALESCE(@cr, CountryRegion),
                    PostalCode     = COALESCE(@pc, PostalCode),
                    ModifiedDate   = SYSUTCDATETIME()
                WHERE AddressID = @id;

                SELECT AddressID, AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode, rowguid, ModifiedDate
                FROM SalesLT.Address WHERE AddressID = @id;
            """, c);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@a1", body.TryGetProperty("AddressLine1", out var a1) ? a1.GetString() : (object?)DBNull.Value);
            cmd.Parameters.AddWithValue("@a2", body.TryGetProperty("AddressLine2", out var a2) ? (object?)a2.GetString() ?? DBNull.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@city", body.TryGetProperty("City", out var city) ? city.GetString() : (object?)DBNull.Value);
            cmd.Parameters.AddWithValue("@sp", body.TryGetProperty("StateProvince", out var sp) ? sp.GetString() : (object?)DBNull.Value);
            cmd.Parameters.AddWithValue("@cr", body.TryGetProperty("CountryRegion", out var cr) ? cr.GetString() : (object?)DBNull.Value);
            cmd.Parameters.AddWithValue("@pc", body.TryGetProperty("PostalCode", out var pc) ? pc.GetString() : (object?)DBNull.Value);

            using var r = await cmd.ExecuteReaderAsync();
            if (!r.HasRows) return Results.NotFound();
            r.Read();
            return Results.Ok(RowMappers.MapRow(r));
        });

        // DELETE
        group.MapDelete("/{id:int}", async (int id, Sql db) =>
        {
            using var c = db.Open();
            using var cmd = new SqlCommand("""
                DELETE FROM SalesLT.Address WHERE AddressID = @id;
                SELECT @@ROWCOUNT;
            """, c);
            cmd.Parameters.AddWithValue("@id", id);
            var affected = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return affected == 0 ? Results.NotFound() : Results.NoContent();
        });
    }
}
