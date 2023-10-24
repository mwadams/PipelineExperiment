using Microsoft.AspNetCore.Http;
using PipelineExample;
using PipelineExperiment;
using Yarp.ReverseProxy.Transforms;

string[] paths = ["/foo", "/bar", "/fizz", "/buzz", "/", "/baz"];

foreach (string path in paths)
{
    Console.WriteLine($"Handling: {path}");
    RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = path } }, Path = path };

    YarpPipelineState result = await ExampleYarpPipeline.Instance(YarpPipelineState.For(ctx)).ConfigureAwait(false);

    if (result.ShouldForward(out NonForwardedResponseDetails responseDetails))
    {
        Console.WriteLine($"Forwarding, message is: {ctx.HttpContext.Items["Message"] ?? "not set"}");
    }
    else
    {
        Console.WriteLine($"Not forwarding: Status Code {responseDetails.StatusCode}");
    }
}
