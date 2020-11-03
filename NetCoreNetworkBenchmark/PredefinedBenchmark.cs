﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PredefinedBenchmark.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NetCoreNetworkBenchmark
{
	//[SimpleJob(RuntimeMoniker.CoreRt31, launchCount: 1, warmupCount: 1, targetCount: 10)]
	[SimpleJob(RuntimeMoniker.CoreRt50, launchCount: 1, warmupCount: 1, targetCount: 10)]
	[GcServer(true)]
	[GcConcurrent(false)]
	[RPlotExporter]
	public class PredefinedBenchmark
	{
		[ParamsAllValues]
		public NetworkLibrary Library;

		private int messageTarget;
		private INetworkBenchmark libraryImpl;


		[GlobalSetup(Target = nameof(Benchmark1))]
		public void PrepareBenchmark1()
		{
			Benchmark.ApplyPredefinedConfiguration();
			var config = Benchmark.Config;

			messageTarget = 1000 * 1000;
			config.Clients = 1000;
			config.ParallelMessages = 1;
			config.MessageByteSize = 32;
			PrepareBenchmark();
		}

		[GlobalSetup(Target = nameof(Benchmark2))]
		public void PrepareBenchmark2()
		{
			Benchmark.ApplyPredefinedConfiguration();
			var config = Benchmark.Config;

			messageTarget = 1000 * 1000;
			config.Clients = 1000;
			config.ParallelMessages = 10;
			config.MessageByteSize = 32;
			PrepareBenchmark();
		}

		[GlobalSetup]
		public void PrepareBenchmark()
		{
			var config = Benchmark.Config;
			config.Library = Library;

			libraryImpl = INetworkBenchmark.CreateNetworkBenchmark(Library);
			Benchmark.PrepareBenchmark(libraryImpl);
		}

		[Benchmark]
		public long Benchmark1()
		{
			return RunBenchmark();
		}

		[Benchmark]
		public long Benchmark2()
		{
			return RunBenchmark();
		}

		private long RunBenchmark()
		{
			var benchmarkdata = Benchmark.BenchmarkData;
			Benchmark.StartBenchmark(libraryImpl);
			var receivedMessages = Interlocked.Read(ref benchmarkdata.MessagesClientReceived);

			while (receivedMessages < messageTarget)
			{
				Thread.Sleep(1);
				receivedMessages = Interlocked.Read(ref benchmarkdata.MessagesClientReceived);
			}

			Benchmark.StopBenchmark(libraryImpl);
			return receivedMessages;
		}

		[IterationCleanup]
		public void CleanupIteration()
		{
			// Wait for messages from previous benchmark to be all sent
			// TODO this can be done in a cleaner way
			Thread.Sleep(100);
		}

		[GlobalCleanup]
		public void CleanupBenchmark()
		{
			Benchmark.CleanupBenchmark(libraryImpl);
		}
	}
}
