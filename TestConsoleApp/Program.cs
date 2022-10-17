using System.Text.Json;
using plitiri.Data.DataAccess;

/*
 * Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite
 * MySqlConnector.MySqlConnection, MySqlConnector
 * Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient
 * Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess
 * Npgsql.NpgsqlConnection, Npgsql
 */
var helper = new DataAccessHelper("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection", "Data Source=Application.db; Cache=Shared");
var exists = await helper.ExecuteScalarAsync<long>("SELECT EXISTS (SELECT name FROM sqlite_master WHERE type='table' AND name='mytable');");
if (exists == 0)
{
    Console.WriteLine("* Create Table...");
    await helper.ExecuteNonQueryAsync("CREATE TABLE IF NOT EXISTS mytable (id int);");
    Console.WriteLine();

    Console.WriteLine("* Insert Data...");
    for (var i = 0; i < 10; i++)
    {
        await helper.ExecuteNonQueryAsync($"INSERT INTO mytable (id) VALUES ({i});");
    }
    Console.WriteLine();
}
Console.WriteLine("* Select Data...");
Console.Write("  ");
var list = await helper.ExecuteListAsync("SELECT * FROM mytable where id = @id;", new ParameterCollection { { "id", "2" } });
Console.WriteLine(JsonSerializer.Serialize(list));
Console.WriteLine();

Console.WriteLine("* Finish");
