using AnomalyDection.Core.ConcurrentAnomaly;
using AnomalyDection.Core.SP_Tree;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.Benchmark
{
   // [SimpleJob(RuntimeMoniker.Net80)]
    [Config(typeof(TimeAndSpaceConfig))]

    public class SPTreeBenchmark
    {
        private SpTree_Node sptree; // Assuming SP_Tree is the type of your sptree object

        [GlobalSetup]
        public void Setup()
        {
            // Initialize your sptree object here
            // This method runs once before all benchmarks

            // Populate sptree as necessary
            string filePath = "Benchmark\\BPMNs\\TestCase_1.bpmn";
            sptree = ConvertBpmnIntoSpTree.Parse(filePath);

        }

        [Benchmark()]
        public void PrevApproach()
        {
            Prev_SP_Tree_Approach prevApproach = new();
            prevApproach.Original_sptree_CAD(sptree);
        }

        [Benchmark]
        public void OurApproach()
        {
            OurApproachToConcurrent ourApproach = new();
            ourApproach.Traverse(sptree);
        }
    }


    public class TimeAndSpaceConfig : ManualConfig
    {
        public TimeAndSpaceConfig()
        {
            var job = Job.Dry
              .WithWarmupCount(5) // Number of warm-up iterations
           //   .WithIterationTime(Perfolizer.Horology.TimeInterval.FromMilliseconds(100))
            
             // .WithMaxIterationCount(2)
              .WithIterationCount(20); // Number of target iterations
            

            AddJob(job);

            // Add memory diagnoser to measure memory usage
            AddDiagnoser(MemoryDiagnoser.Default);

            // Keep the default time columns, or specify your own
            // For example, to keep only the mean and error:
            AddColumn(StatisticColumn.Mean, StatisticColumn.Min, StatisticColumn.Max);

            // If you're only interested in Gen 0/1/2 collections, you can specify them explicitly
            //AddColumn(ProviderFactory.CreateColumnProvider(DiagnosersLoader.GetColumns(Diagnoser.Default).OfType<MemoryDiagnoser>().First()));
        }
    }
}
