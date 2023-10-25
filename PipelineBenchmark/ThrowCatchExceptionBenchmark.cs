﻿// <copyright file="ThrowCatchExceptionBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable CA1822 // Mark members as static - not helpful in benchmarks

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using PipelineExample;
using PipelineExperiment;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.UriTemplates.Benchmarking;

/// <summary>
/// Matches tables.
/// </summary>
[MemoryDiagnoser]
public class ThrowCatchExceptionBenchmark
{
    private static readonly RequestTransformContext[] Contexts =
        [
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/buzz" } }, Path = "/buzz" },
        ];

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark(Baseline = true)]
    public async Task<bool> RunPipeline()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpPipelineState result = await ExampleYarpPipeline.Instance(YarpPipelineState.For(context)).ConfigureAwait(false);
            shouldForward &= result.ShouldForward(out NonForwardedResponseDetails responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithLogging()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpPipelineState result = await ExampleYarpPipelineWithLogging.Instance(YarpPipelineState.For(context)).ConfigureAwait(false);
            shouldForward &= result.ShouldForward(out NonForwardedResponseDetails responseDetails);
        }

        return shouldForward;
    }
}