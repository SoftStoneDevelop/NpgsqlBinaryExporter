﻿using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
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

    public class BoxTest
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
CREATE TABLE IF NOT EXISTS public.boxTable
(
    id serial NOT NULL,
    value box NOT NULL,
    CONSTRAINT boxTable_pkey PRIMARY KEY (id)
);
";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
INSERT INTO public.boxTable
(value)
VALUES
($1)
";
                var box = new NpgsqlBox(
                    upperRight: new NpgsqlPoint(0.1d, 0.5d), 
                    lowerLeft: new NpgsqlPoint(0.4d, 0.3d)
                    );
                var parametr = new NpgsqlParameter<NpgsqlBox>();
                parametr.TypedValue = box;
                cmd.Parameters.Add(parametr);
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = @"
SELECT
    value
FROM public.boxTable
";
                using var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    var readedBox = reader.GetFieldValue<NpgsqlBox>(0);
                    var isSame = readedBox == box;//false
                    //box is {(0.1,0.5),(0.4,0.3)}
                    //readedBox is {(0.4,0.5),(0.1,0.3)}
                }
            }
        }
    }
}