﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PipelineExample;
using PipelineExperiment;
using Yarp.ReverseProxy.Transforms;

string[] paths = ["/foo", "/bar", "/fizz", "/buzz", "/", "/baz"];

using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
        }));

ILogger<YarpPipelineState> logger = loggerFactory.CreateLogger<YarpPipelineState>();

foreach (string path in paths)
{
    RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = path } }, Path = path };

    YarpPipelineState result = await ExampleYarpPipelineWithLogging.Instance(YarpPipelineState.For(ctx, logger)).ConfigureAwait(false);

    if (result.ShouldForward(out NonForwardedResponseDetails responseDetails))
    {
        Console.WriteLine($"Forwarding, message is: {ctx.HttpContext.Items["Message"] ?? "not set"}");
    }
    else
    {
        Console.WriteLine($"Not forwarding: Status Code {responseDetails.StatusCode}");
    }
}
