using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlBenchmark.Extensions;
using System.IO;
using System.Linq;

namespace NpgsqlBenchmark.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70)]
    [HideColumns("Error", "StdDev", "Median", "RatioSD")]
    public class BinaryImportMap
    {
        [Params(5, 10, 20)]
        public int Operations;

        private NpgsqlConnection _connection;

        [GlobalSetup]
        public void Setup()
        {
            var root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false)
                .Build()
                ;

            _connection = new NpgsqlConnection(root.GetConnectionString("SqlConnection"));
            _connection.Open();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _connection?.Dispose();
        }

        [Benchmark(Description = $"NpgsqlQuery")]
        public void NpgsqlQuery()
        {
            for (int i = 0; i < Operations; i++)
            {
                var persons = _connection.NpgsqlQueryTable().ToList();
            }
        }

        [Benchmark(Baseline = true, Description = "NpgsqlBinaryImport")]
        public void NpgsqlBinaryImport()
        {
            for (int i = 0; i < Operations; i++)
            {
                var persons = _connection.NpgsqlBinaryImportTable().ToList();
            }
        }
    }
}