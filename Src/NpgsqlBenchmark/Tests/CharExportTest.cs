using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;

namespace NpgsqlBenchmark.Tests
{
    public class CharExportTest
    {
        public void Test()
        {
            var root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false)
                .Build()
                ;

            using (var connection = new NpgsqlConnection(root.GetConnectionString("SqlConnection")))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS public.charTable
(
    id serial NOT NULL,
    value char NOT NULL,
    CONSTRAINT charTable_pkey PRIMARY KEY (id)
);
";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
INSERT INTO public.charTable
(value)
VALUES
($1)
";
                var parametr = new NpgsqlParameter<char>();
                parametr.NpgsqlValue = 'd';
                cmd.Parameters.Add(parametr);
                cmd.ExecuteNonQuery();

                parametr.NpgsqlValue = 's';
                cmd.ExecuteNonQuery();

                using (var export = connection.BeginBinaryExport(@"

COPY public.charTable
(
    id,
    value
) TO STDOUT (FORMAT BINARY)

"))
                {
                    while (export.StartRow() != -1)
                    {
                        var id = export.Read<int>();
                        var value = export.Read<char>();
                    }
                }
            }
        }
    }
}