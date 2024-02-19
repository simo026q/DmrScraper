// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using DmrScraper;
using DmrScraper.Benchmarks;

BenchmarkRunner.Run<HtmlReadBenchmarks>();
//BenchmarkRunner.Run<HtmlLoadBenchmarks>();