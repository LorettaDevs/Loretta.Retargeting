// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
