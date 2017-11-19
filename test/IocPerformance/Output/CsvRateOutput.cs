﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using IocPerformance.Adapters;
using IocPerformance.Benchmarks;

namespace IocPerformance.Output
{
    /// <summary>
    /// Returns csv_Rate.csv, each value is a rate of comparing with the base value in NoContainerAdapter. 
    /// It makes the measurements to be independent of the hardware installed.
    /// </summary>
    public class CsvRateOutput : IOutput
    {
        public void Create(IEnumerable<IBenchmark> benchmarks, IEnumerable<BenchmarkResult> benchmarkResults)
        {
            if (!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }

            using (var fileStream = new FileStream("output\\csv_Rate.csv", FileMode.Create))
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
                            var basetime = resultsOfBenchmark.First(r => r.ContainerInfo.Name == "No").SingleThreadedResult.Time;

                            writer.Write(
                                ",{0}",
                                CalcRate(time, basetime));
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
                            var basetime = resultsOfBenchmark.First(r => r.ContainerInfo.Name == "No").MultiThreadedResult.Time;

                            writer.Write(
                                ",{0}",
                                CalcRate(multithreadedTime, basetime));
                        }

                        writer.WriteLine();
                    }
                }
            }
        }

        private static string CalcRate(long? val, long? baseValue)
        {
            if (!val.HasValue || !baseValue.HasValue)
            {
                return "-";
            }

            if (baseValue.Value == 0)
            {
                return "NaN";
            }

            double rate = (double)val.Value / baseValue.Value;
            return rate.ToString("0.000");
        }
    }
}
