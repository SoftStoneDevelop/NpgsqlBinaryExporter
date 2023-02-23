﻿using BenchmarkDotNet.Attributes;
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

        [Benchmark(Description = $"NpgsqlQuery", OperationsPerInvoke = 5)]
        public void NpgsqlQuery()
        {
            var persons = _connection.NpgsqlQueryTable().ToList();
        }

        [Benchmark(Baseline = true, Description = "NpgsqlBinaryImport", OperationsPerInvoke = 5)]
        public void NpgsqlBinaryImport()
        {
            var persons = _connection.NpgsqlBinaryImportTable().ToList();
        }
    }
}