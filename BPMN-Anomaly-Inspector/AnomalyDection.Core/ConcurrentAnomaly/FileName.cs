using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.ConcurrentAnomaly
{
    [MemoryDiagnoser]
    [MinColumn, MaxColumn]
    public class YourBenchmark
    {
        [Benchmark(Baseline = true)]

        public void PrevApproach()
        {
            // Your method invocation here
        }

        [Benchmark]
        public void OurApproach()
        {
            // Your method invocation here
        }
    }

}
