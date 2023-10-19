using Microsoft.AspNetCore.Http;
using PipelineExperiment;
using Sandbox;
using Yarp.ReverseProxy.Transforms;

RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/foo" } }, Path = "/foo" };

var computedResult = await ExampleYarpPipeline.Instance.RunAsync(ctx).ConfigureAwait(false);

if (computedResult.TryGetResult(out YarpPipelineResult result))
{
    if (result.ShouldForward(out NonForwardedResponseDetails responseDetails))
    {
        Console.WriteLine($"Forwarding, message is: {ctx.HttpContext.Items["Message"] ?? "not set"}");
    }
    else
    {
        Console.WriteLine($"Not forwarding: Status Code {responseDetails.StatusCode}");
    }
}
else
{
    Console.WriteLine("Shouldn't get here.");
}