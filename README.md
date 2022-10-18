# DataAccess
A wrapper library that makes it easy to access data from multiple relational databases using ado.net.

## Assembly
| plitiri.Data.DataAccess

## Usage

1. Create
    * `var helper = new DataAccessHelper([AssemblyName], [DbConnectionType], [ConnectionString]);`
2. Execute
```csharp
    var helper = new DataAccessHelper("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection", "Data Source=Application.db; Cache=Shared");
    
    // create table
    await helper.ExecuteNonQueryAsync("CREATE TABLE IF NOT EXISTS mytable (id int);");
    
    // insert to table
    for (var i = 0; i < 10; i++)
    {
        await helper.ExecuteNonQueryAsync($"INSERT INTO mytable (id) VALUES ({i});");
    }
    
    // select from table
    var list = await helper.ExecuteListAsync("SELECT * FROM mytable where id = @id;", new ParameterCollection { { "id", "2" } });
    
    // print
    Console.WriteLine(JsonSerializer.Serialize(list));
```
