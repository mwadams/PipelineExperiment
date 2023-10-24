// <copyright file="AllSortsBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

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
public class AllSortsBenchmark
{
    private static readonly RequestTransformContext[] Contexts =
        [
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/foo" } }, Path = "/foo" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/bar" } }, Path = "/bar" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/fizz" } }, Path = "/fizz" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/" } }, Path = "/" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/baz" } }, Path = "/baz" },
        ];

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
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
}