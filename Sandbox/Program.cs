using Microsoft.AspNetCore.Http;
using PipelineExperiment;
using Sandbox;
using Yarp.ReverseProxy.Transforms;

string[] paths = ["/foo", "/bar", "/fizz", "/", "/baz"];

foreach (var path in paths)
{
    Console.WriteLine($"Handling: {path}");
    RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = path } }, Path = path };

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
}