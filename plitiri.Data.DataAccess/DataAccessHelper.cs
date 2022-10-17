using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plitiri.Data.DataAccess;
public class DataAccessHelper
{
    private readonly DbConnectionStringBuilder? _dbConnectionStringBuilder;
    private readonly string? _assemblyName;
    private readonly string? _dbConnectionTypeName;


    public DataAccessHelper(string? assemblyName, string? dbConnectionTypeName, string? connectionString)
    {
        _assemblyName = assemblyName;
        _dbConnectionTypeName = dbConnectionTypeName;

        _dbConnectionStringBuilder = new DbConnectionStringBuilder();
        _dbConnectionStringBuilder.ConnectionString = connectionString;
    }

    /// <summary>
    /// Create DbConnection
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    private DbConnection? CreateDbConnection()
    {
        if (string.IsNullOrWhiteSpace(_assemblyName) == false && string.IsNullOrWhiteSpace(_dbConnectionTypeName) == false)
        {
            var type = Type.GetType($"{_dbConnectionTypeName}, {_assemblyName}", true, true);
            if (type != null)
            {
                var connection = Activator.CreateInstance(type) as IDbConnection;
                if (connection != null)
                {
                    connection.ConnectionString = _dbConnectionStringBuilder?.ConnectionString;
                }
                return (DbConnection?)connection;
            }
            else
            {
                throw new DataAccessException("type 이 null 입니다.");
            }
        }
        else
        {
            throw new DataAccessException("assemblyName 또는 dbConnectionTypeName 이 정의되지 않았습니다.");
        }
    }

    /// <summary>
    /// Returns the result of ExecuteNonQuery.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public async Task<int> ExecuteNonQueryAsync(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        using var connection = CreateDbConnection();
        if (connection != null)
        {
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            if (parameters != null)
            {
                foreach (var keyValuePair in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = keyValuePair.Key;
                    dbParameter.Value = keyValuePair.Value;
                    command.Parameters.Add(dbParameter);
                }
            }

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            throw new DataAccessException("connection 이 null 입니다.");
        }
    }

    /// <summary>
    /// Returns the result of ExecuteNonQuery.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public int ExecuteNonQuery(string commandText, ParameterCollection? parameters = default)
    {
        using var connection = CreateDbConnection();
        if (connection != null)
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            if (parameters != null)
            {
                foreach (var keyValuePair in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = keyValuePair.Key;
                    dbParameter.Value = keyValuePair.Value;
                    command.Parameters.Add(dbParameter);
                }
            }

            return command.ExecuteNonQuery();
        }
        else
        {
            throw new DataAccessException("connection 이 null 입니다.");
        }
    }

    /// <summary>
    /// Returns the result of ExecuteReader as a list of lists of ExpandoObject.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public async Task<IList<IList<ExpandoObject>>> ExecuteListsAsync(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        var response = new List<IList<ExpandoObject>>();
        using (var connection = CreateDbConnection())
        {
            if (connection != null)
            {
                await connection.OpenAsync(cancellationToken);

                using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                if (parameters != null)
                {
                    foreach (var keyValuePair in parameters)
                    {
                        var dbParameter = command.CreateParameter();
                        dbParameter.ParameterName = keyValuePair.Key;
                        dbParameter.Value = keyValuePair.Value;
                        command.Parameters.Add(dbParameter);
                    }
                }
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
                do
                {
                    var list = new List<ExpandoObject>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var obj = new ExpandoObject();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var type = reader.GetFieldType(i);
                            var value = reader.GetValue(i);

                            if (obj.TryAdd(name, value) == false)
                            {
                                throw new DataAccessException("List<ExpandoObject> 에 ExpandObject 를 추가하지 못했습니다.");
                            }
                        }
                        list.Add(obj);
                    }
                    response.Add(list);
                } while (await reader.NextResultAsync(cancellationToken));
            }
            else
            {
                throw new DataAccessException("connection 이 null 입니다.");
            }
        }

        return response;
    }

    /// <summary>
    /// Returns the result of ExecuteReader as a list of lists of ExpandoObject.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public IList<IList<ExpandoObject>> ExecuteLists(string commandText, ParameterCollection? parameters = default)
    {
        var response = new List<IList<ExpandoObject>>();
        using (var connection = CreateDbConnection())
        {
            if (connection != null)
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                if (parameters != null)
                {
                    foreach (var keyValuePair in parameters)
                    {
                        var dbParameter = command.CreateParameter();
                        dbParameter.ParameterName = keyValuePair.Key;
                        dbParameter.Value = keyValuePair.Value;
                        command.Parameters.Add(dbParameter);
                    }
                }
                using var reader = command.ExecuteReader(CommandBehavior.Default);
                do
                {
                    var list = new List<ExpandoObject>();
                    while (reader.Read())
                    {
                        var obj = new ExpandoObject();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var type = reader.GetFieldType(i);
                            var value = reader.GetValue(i);

                            if (obj.TryAdd(name, value) == false)
                            {
                                throw new DataAccessException("List<ExpandoObject> 에 ExpandObject 를 추가하지 못했습니다.");
                            }
                        }
                        list.Add(obj);
                    }
                    response.Add(list);
                } while (reader.NextResult());
            }
            else
            {
                throw new DataAccessException("connection 이 null 입니다.");
            }
        }

        return response;
    }

    /// <summary>
    /// Returns the result of ExecuteReader as a list of lists of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<IList<T>>> ExecuteListsAsync<T>(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        var response = new List<IList<T>>();

        foreach (var list in await ExecuteListsAsync(commandText, parameters, cancellationToken))
        {
            if (list != null)
            {
                var genericList = ConvertHelper.ToGenericList<List<T>>(list);
                if (genericList != null)
                {
                    response.Add(genericList);
                }
            }
        }

        return response;
    }

    /// <summary>
    /// Returns the result of ExecuteReader as a list of lists of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IList<IList<T>> ExecuteLists<T>(string commandText, ParameterCollection? parameters = default)
    {
        var response = new List<IList<T>>();

        foreach (var list in ExecuteLists(commandText, parameters))
        {
            if (list != null)
            {
                var genericList = ConvertHelper.ToGenericList<List<T>>(list);
                if (genericList != null)
                {
                    response.Add(genericList);
                }
            }
        }

        return response;
    }

    /// <summary>
    /// Return the result of ExecuteReader as a list of TExpandoObject.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public async Task<IList<ExpandoObject>> ExecuteListAsync(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        var response = new List<ExpandoObject>();
        using (var connection = CreateDbConnection())
        {
            if (connection != null)
            {
                await connection.OpenAsync(cancellationToken);

                using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                if (parameters != null)
                {
                    foreach (var keyValuePair in parameters)
                    {
                        var dbParameter = command.CreateParameter();
                        dbParameter.ParameterName = keyValuePair.Key;
                        dbParameter.Value = keyValuePair.Value;
                        command.Parameters.Add(dbParameter);

                    }
                }
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var obj = new ExpandoObject();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var type = reader.GetFieldType(i);
                        var value = reader.GetValue(i);

                        if (obj.TryAdd(name, value) == false)
                        {
                            throw new DataAccessException("List<ExpandoObject> 에 ExpandObject 를 추가하지 못했습니다.");
                        }
                    }
                    response.Add(obj);
                }
            }
            else
            {
                throw new DataAccessException("connection 이 null 입니다.");
            }
        }

        return response;
    }

    /// <summary>
    /// Return the result of ExecuteReader as a list of TExpandoObject.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public IList<ExpandoObject> ExecuteList(string commandText, ParameterCollection? parameters = default)
    {
        var response = new List<ExpandoObject>();
        using (var connection = CreateDbConnection())
        {
            if (connection != null)
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                if (parameters != null)
                {
                    foreach (var keyValuePair in parameters)
                    {
                        var dbParameter = command.CreateParameter();
                        dbParameter.ParameterName = keyValuePair.Key;
                        dbParameter.Value = keyValuePair.Value;
                        command.Parameters.Add(dbParameter);

                    }
                }
                using var reader = command.ExecuteReader(CommandBehavior.Default);
                while (reader.Read())
                {
                    var obj = new ExpandoObject();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var type = reader.GetFieldType(i);
                        var value = reader.GetValue(i);

                        if (obj.TryAdd(name, value) == false)
                        {
                            throw new DataAccessException("List<ExpandoObject> 에 ExpandObject 를 추가하지 못했습니다.");
                        }
                    }
                    response.Add(obj);
                }
            }
            else
            {
                throw new DataAccessException("connection 이 null 입니다.");
            }
        }

        return response;
    }

    /// <summary>
    /// Return the result of ExecuteReader as a list of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<T>> ExecuteListAsync<T>(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        return ConvertHelper.ToGenericList<List<T>>(await ExecuteListAsync(commandText, parameters, cancellationToken)) ?? new();
    }

    /// <summary>
    /// Return the result of ExecuteReader as a list of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IList<T> ExecuteList<T>(string commandText, ParameterCollection? parameters = default)
    {
        return ConvertHelper.ToGenericList<List<T>>(ExecuteList(commandText, parameters)) ?? new();
    }

    /// <summary>
    /// Return the result of ExecuteScalar.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public async Task<object?> ExecuteScalarAsync(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        using var connection = CreateDbConnection();
        if (connection != null)
        {
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            if (parameters != null)
            {
                foreach (var keyValuePair in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = keyValuePair.Key;
                    dbParameter.Value = keyValuePair.Value;
                    command.Parameters.Add(dbParameter);

                }
            }

            return await command.ExecuteScalarAsync(cancellationToken);
        }
        else
        {
            throw new DataAccessException("connection 이 null 입니다.");
        }
    }

    /// <summary>
    /// Return the result of ExecuteScalar.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="DataAccessException"></exception>
    public object? ExecuteScalar(string commandText, ParameterCollection? parameters = default)
    {
        using var connection = CreateDbConnection();
        if (connection != null)
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            if (parameters != null)
            {
                foreach (var keyValuePair in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = keyValuePair.Key;
                    dbParameter.Value = keyValuePair.Value;
                    command.Parameters.Add(dbParameter);

                }
            }

            return command.ExecuteScalar();
        }
        else
        {
            throw new DataAccessException("connection 이 null 입니다.");
        }
    }

    /// <summary>
    /// Return the result of ExecuteScalar as T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T?> ExecuteScalarAsync<T>(string commandText, ParameterCollection? parameters = default, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteScalarAsync(commandText, parameters, cancellationToken);

        return (T?)result;
    }

    /// <summary>
    /// Return the result of ExecuteScalar as T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T? ExecuteScalar<T>(string commandText, ParameterCollection? parameters = default)
    {
        var result = ExecuteScalar(commandText, parameters);

        return (T?)result;
    }
}
