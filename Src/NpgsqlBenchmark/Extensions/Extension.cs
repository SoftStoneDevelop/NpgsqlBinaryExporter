using Npgsql;
using NpgsqlBenchmark.Model;
using System.Collections.Generic;
using System.Data;

namespace NpgsqlBenchmark.Extensions
{
    public static class Extension
    {
        public static IEnumerable<Person> NpgsqlQueryTable(this NpgsqlConnection connection)
        {
            bool needClose = connection.State == ConnectionState.Closed;
            if (needClose)
            {
                connection.Open();
            }

            NpgsqlCommand command = null;
            NpgsqlDataReader reader = null;
            try
            {
                command = connection.CreateCommand();
                command.CommandText = @"
SELECT 
    id,
    firstname,
    identification_id,
    middlename,
    lastname
FROM person
";
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = new Person();
                    if (!reader.IsDBNull(0))
                    {
                        item.Id = reader.GetFieldValue<System.Int32>(0);
                    }

                    if (!reader.IsDBNull(1))
                    {
                        item.FirstName = reader.GetFieldValue<System.String>(1);
                    }

                    if (!reader.IsDBNull(3))
                    {
                        item.MiddleName = reader.GetFieldValue<System.String>(3);
                    }

                    if (!reader.IsDBNull(4))
                    {
                        item.LastName = reader.GetFieldValue<System.String>(4);
                    }

                    if (!reader.IsDBNull(2))
                    {
                        var item1 = new Identification();
                        if (!reader.IsDBNull(2))
                        {
                            item1.Id = reader.GetFieldValue<System.Int32>(2);
                        }

                        item.Identification = item1;
                    }

                    yield return item;
                }

                while (reader.NextResult())
                {
                }

                reader.Dispose();
                reader = null;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            command.Cancel();
                        }
                        catch { /* ignore */ }
                    }

                    reader.Dispose();
                }

                if (needClose)
                {
                    connection.Close();
                }

                if (command != null)
                {
                    command.Parameters.Clear();
                    command.Dispose();
                }
            }
        }

        public static IEnumerable<Person> NpgsqlBinaryImportTable(this NpgsqlConnection connection)
        {
            NpgsqlBinaryExporter export = null;
            try
            {
                export = connection.BeginBinaryExport(@"
COPY person
(
    id,
    firstname,
    middlename,
    lastname,
    identification_id
) TO STDOUT (FORMAT BINARY)
");
                while (export.StartRow() != -1)
                {
                    var item = new Person();
                    if (!export.IsNull)
                    {
                        item.Id = export.Read<System.Int32>(NpgsqlTypes.NpgsqlDbType.Integer);
                    }
                    else
                    {
                        export.Skip();
                    }

                    if (!export.IsNull)
                    {
                        item.FirstName = export.Read<System.String>(NpgsqlTypes.NpgsqlDbType.Text);
                    }
                    else
                    {
                        export.Skip();
                    }

                    if (!export.IsNull)
                    {
                        item.MiddleName = export.Read<System.String>(NpgsqlTypes.NpgsqlDbType.Text);
                    }
                    else
                    {
                        export.Skip();
                    }

                    if (!export.IsNull)
                    {
                        item.LastName = export.Read<System.String>(NpgsqlTypes.NpgsqlDbType.Text);
                    }
                    else
                    {
                        export.Skip();
                    }

                    if (!export.IsNull)
                    {
                        var item1 = new Identification();
                        if (!export.IsNull)
                        {
                            item1.Id = export.Read<System.Int32>(NpgsqlTypes.NpgsqlDbType.Integer);
                        }
                        else
                        {
                            export.Skip();
                        }

                        item.Identification = item1;
                    }
                    else
                    {
                        export.Skip();
                    }

                    yield return item;
                }

                export.Dispose();
                export = null;
            }
            finally
            {
                if (export != null)
                {
                    try { export.Cancel(); } catch { }
                    export.Dispose();
                }
            }
        }
    }
}