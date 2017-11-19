﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using IocPerformance.Benchmarks;

namespace IocPerformance.Output
{
    public class CsvOutput : IOutput
    {
        public void Create(IEnumerable<IBenchmark> benchmarks, IEnumerable<BenchmarkResult> benchmarkResults)
        {
            if (!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }

            using (var fileStream = new FileStream("output\\csv_output.csv", FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.WriteLine("Single thread");

                    writer.Write("Container,Version");

                    foreach (var benchmark in benchmarks)
                    {
                        writer.Write(",{0}", benchmark.Name);
                    }

                    writer.WriteLine();

                    foreach (var container in benchmarkResults.Select(r => r.ContainerInfo).Distinct())
                    {
                        writer.Write("{0},{1}", container.Name, container.Version);

                        foreach (var benchmark in benchmarks)
                        {
                            var resultsOfBenchmark = benchmarkResults.Where(r => r.BenchmarkInfo.Name == benchmark.Name);
                            var time = resultsOfBenchmark.First(r => r.ContainerInfo.Name == container.Name).SingleThreadedResult.Time;

                            writer.Write(
                                ",{0}",
                                time.GetValueOrDefault(0));
                        }

                        writer.WriteLine();
                    }

                    writer.WriteLine();
                    writer.WriteLine("Multiple threads");

                    writer.Write("Container,Version");

                    foreach (var benchmark in benchmarks)
                    {
                        writer.Write(",{0}", benchmark.Name);
                    }

                    writer.WriteLine();

                    foreach (var container in benchmarkResults.Select(r => r.ContainerInfo).Distinct())
                    {
                        writer.Write("{0},{1}", container.Name, container.Version);

                        foreach (var benchmark in benchmarks)
                        {
                            var resultsOfBenchmark = benchmarkResults.Where(r => r.BenchmarkInfo.Name == benchmark.Name);
                            var multithreadedTime = resultsOfBenchmark.First(r => r.ContainerInfo.Name == container.Name).MultiThreadedResult.Time;

                            writer.Write(
                                ",{0}",
                                multithreadedTime.GetValueOrDefault(0));
                        }

                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
