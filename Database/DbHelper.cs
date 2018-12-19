using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace ExemploJWT.Database
{
    public class DbHelper : IDisposable
    {
        private string _connectionString;

        public string connectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    var strBuilder = new StringBuilder();
                    // strBuilder.AppendFormat("server={0};", ShellParameters.GetInstance().GetDBHost());
                    // strBuilder.AppendFormat("userid={0};", ShellParameters.GetInstance().GetDBUser());
                    // strBuilder.AppendFormat("password={0};", ShellParameters.GetInstance().GetDBPassword());
                    // strBuilder.AppendFormat("database={0};", ShellParameters.GetInstance().GetDBName());
                    // strBuilder.AppendFormat("port={0};", ShellParameters.GetInstance().GetDBPort());

                    strBuilder.Append("Host=localhost;Username=postgres;Password=postgres;Database=aspnet_jwt");

                    _connectionString = strBuilder.ToString();
                }

                return _connectionString;
            }
        }

        public IEnumerable<IDictionary<string, object>> Read(string sql, params object[] parms)
        {
            var _toReturn = new List<IDictionary<string, object>>();

            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dic = Enumerable.Range(0, reader.FieldCount)
                                        .ToDictionary(reader.GetName, reader.GetValue);

                            _toReturn.Add(dic);
                        }
                    }
                }
            }

            return _toReturn;
        }

        public IEnumerable<dynamic> ReadDynamic(string sql, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var fieldNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

                        foreach (IDataRecord row in reader)
                        {
                            var expando = new ExpandoObject() as IDictionary<string, object>;
                            foreach (var fieldName in fieldNames)
                            {
                                expando[fieldName] = row[fieldName];
                            }

                            yield return expando;
                        }
                    }
                }
            }
        }

        public IEnumerable<IDictionary<string, object>> Read(string sql)
        {
            return Read(sql, null);
        }

        public IEnumerable<T> Read<T>(string sql, Func<IDataReader, T> mapper, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return mapper(reader);
                        }
                    }
                }
            }
        }

        public IEnumerable<T> Read<T>(string sql, Func<IDataReader, T> mapper)
        {
            return Read(sql, mapper, null);
        }

        public object Scalar(string sql, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        public void Insert(string sql, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public int Update(string sql, params object[] parms)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CreateCommand(sql, connection, parms))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Delete(string sql, params object[] parms)
        {
            return Update(sql, parms);
        }

        public int Delete(string sql)
        {
            return Update(sql, null);
        }

        private DbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection();

            connection.ConnectionString = connectionString;
            connection.Open();
            return connection;
        }

        private DbCommand CreateCommand(string sql, DbConnection conn, params object[] parms)
        {
            var command = conn.CreateCommand();

            command.CommandText = sql;
            command.AddParameters(parms);
            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: descartar estado gerenciado (objetos gerenciados).
                    // Fechar a connection
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool disposedValue = false; // Para detectar chamadas redundantes
    }

    internal static class DbExtentions
    {
        public static void AddParameters(this DbCommand command, object[] parms)
        {
            if (parms != null && parms.Length > 0)
            {
                for (int i = 0; i < parms.Length; i += 2)
                {
                    string name = parms[i].ToString();

                    if (parms[i + 1] is string && (string)parms[i + 1] == "")
                        parms[i + 1] = null;

                    object value = parms[i + 1] ?? DBNull.Value;

                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = name;
                    dbParameter.Value = value;

                    command.Parameters.Add(dbParameter);
                }
            }
        }
    }

    public static class TypeExtentions
    {
        public static int AsId(this object item, int defaultId = -1)
        {
            if (item == null)
                return defaultId;

            int result;
            if (!int.TryParse(item.ToString(), out result))
                return defaultId;

            return result;
        }

        public static int AsInt(this object item, int defaultInt = default(int))
        {
            if (item == null)
                return defaultInt;

            int result;
            if (!int.TryParse(item.ToString(), out result))
                return defaultInt;

            return result;
        }

        public static double AsDouble(this object item, double defaultDouble = default(double))
        {
            if (item == null)
                return defaultDouble;

            double result;
            if (!double.TryParse(item.ToString(), out result))
                return defaultDouble;

            return result;
        }

        public static string AsString(this object item, string defaultString = default(string))
        {
            if (item == null || item.Equals(DBNull.Value))
                return defaultString;

            return item.ToString().Trim();
        }

        public static DateTime AsDateTime(this object item, DateTime defaultDateTime = default(DateTime))
        {
            if (item == null || string.IsNullOrEmpty(item.ToString()))
                return defaultDateTime;

            DateTime result;
            if (!DateTime.TryParse(item.ToString(), out result))
                return defaultDateTime;

            return result;
        }

        public static bool AsBool(this object item, bool defaultBool = default(bool))
        {
            if (item == null)
                return defaultBool;

            return new List<string>() { "yes", "y", "true" }.Contains(item.ToString().ToLower());
        }

        public static byte[] AsByteArray(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            return Convert.FromBase64String(s);
        }

        public static string AsBase64String(this object item)
        {
            if (item == null)
                return null;

            return Convert.ToBase64String((byte[])item);
        }

        public static Guid AsGuid(this object item)
        {
            try
            {
                return new Guid(item.ToString());
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public static byte[] AsByteArray(this object item)
        {
            if ((item == null) || (item is DBNull))
                return null;

            return (byte[])item;
        }

        public static string OrderBy(this string sql, string sortExpression)
        {
            if (string.IsNullOrEmpty(sortExpression))
                return sql;

            return sql + " ORDER BY " + sortExpression;
        }

        public static string CommaSeparate<T, U>(this IEnumerable<T> source, Func<T, U> func)
        {
            return string.Join(",", source.Select(s => func(s).ToString()).ToArray());
        }
    }
}

//Examples
/*
    private DbHelper dbHelper = new DbHelper();

    public Model LoadById(Guid id)
    {
        var sql = $"SELECT * FROM table WHERE id = @id";

        var parms = new object[]
        {
            "@id", id
        };

        return dbHelper.Read<Model>(sql, Mapper, parms).FirstOrDefault();
    }

    public void Save(Model model)
    {
        var sql = $"UPDATE table SET field_1 = @field_1 WHERE id = @id";

        var parms = new object[]
        {
            "@id", template.Id,
            "@field_1", model.Field1
        };

        dbHelper.Update(sql, parms);
    }

    private static Func<IDataReader, model> Mapper = reader =>
        new Model
        {
            Id = reader["id"].AsGuid(),
            Codigo = reader["codigo"].AsString(),
            Descricao = reader["descricao"].AsString(),
            Stream = reader["stream"].AsByteArray(),
            IdCatalogo = reader["id_catalogo"].AsGuid(),
            IdGrupoEmpresarial = reader["id_grupoempresarial"].AsGuid()
        };
*/
