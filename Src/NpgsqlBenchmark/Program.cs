using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlBenchmark.Benchmarks;
using System;
using System.IO;

namespace NpgsqlBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //FillTestDatabase();
            BenchmarkRunner.Run<BinaryImportMap>();
        }

        private static void FillTestDatabase()
        {
            var root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false)
                .Build()
                ;

            using (var connection = new NpgsqlConnection(root.GetConnectionString("SqlConnection")))
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS public.identification
(
    id integer NOT NULL,
    typename text COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT identification_pkey PRIMARY KEY (id)
)
";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"
INSERT INTO public.identification(
	id, typename)
	VALUES (
    @id, @typename
);
";
                    var id = cmd.CreateParameter();
                    id.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                    id.ParameterName = "id";
                    cmd.Parameters.Add(id);
                    
                    var typename = cmd.CreateParameter();
                    typename.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                    typename.ParameterName = "typename";
                    cmd.Parameters.Add(typename);
                    cmd.Prepare();

                    id.Value = 1;
                    typename.Value = "sailor's passport";
                    cmd.ExecuteNonQuery();

                    id.Value = 2;
                    typename.Value = "officer's certificate";
                    cmd.ExecuteNonQuery();

                    id.Value = 3;
                    typename.Value = "driver license";
                    cmd.ExecuteNonQuery();

                    id.Value = 4;
                    typename.Value = "citizen's passport";
                    cmd.ExecuteNonQuery();

                    id.Value = 5;
                    typename.Value = "party card";
                    cmd.ExecuteNonQuery();
                }

                //fill person
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS public.person
(
    id integer NOT NULL,
    firstname text COLLATE pg_catalog.""default"" NOT NULL,
    middlename text COLLATE pg_catalog.""default"" NOT NULL,
    lastname text COLLATE pg_catalog.""default"" NOT NULL,
    identification_id integer,
    CONSTRAINT person_pkey PRIMARY KEY (id),
    CONSTRAINT identification_fk FOREIGN KEY (identification_id)
        REFERENCES public.identification (id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"
INSERT INTO public.person(
	id, firstname, middlename, lastname, identification_id)
	VALUES (
    @id, @firstname, @middlename, @lastname, @identification_id
);
";
                    cmd.Parameters.Clear();
                    var id = cmd.CreateParameter();
                    id.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                    id.ParameterName = "id";
                    cmd.Parameters.Add(id);

                    var firstname = cmd.CreateParameter();
                    firstname.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                    firstname.ParameterName = "firstname";
                    cmd.Parameters.Add(firstname);

                    var middlename = cmd.CreateParameter();
                    middlename.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                    middlename.ParameterName = "middlename";
                    cmd.Parameters.Add(middlename);

                    var lastname = cmd.CreateParameter();
                    lastname.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                    lastname.ParameterName = "lastname";
                    cmd.Parameters.Add(lastname);

                    var identificationId = cmd.CreateParameter();
                    identificationId.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                    identificationId.ParameterName = "identification_id";
                    identificationId.IsNullable = true;
                    cmd.Parameters.Add(identificationId);
                    cmd.Prepare();

                    var refId = 0;
                    for (int i = 0; i < 1000000; i++)
                    {
                        id.Value = i;
                        firstname.Value = $"John{i}";
                        middlename.Value = $"Сurly{i}";
                        lastname.Value = $"Doe{i}";

                        if (++refId > 5)
                        {
                            refId = 1;
                        }

                        if (i % 2 == 0)
                        {
                            identificationId.Value = refId;
                        }
                        else
                        {
                            identificationId.Value = DBNull.Value;
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}