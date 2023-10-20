namespace Sandbox;

using Microsoft.AspNetCore.Http;
using PipelineExperiment;
using PipelineExperiment.Handlers;
using PipelineExperiment.Pipelines;
using Yarp.ReverseProxy.Transforms;

public static class ExampleYarpPipeline
{
    public static PipelineStep<RequestTransformContext, YarpPipelineResult> Instance { get; } =
        YarpPipeline.Build(
            YarpPipeline.MakeStep(
                static ctx => ctx.Path == "/"
                    ? YarpPipeline.TerminateAndForward()
                    : InnerPipelineInstance),
            YarpPipeline.MakeStep(static async ctx =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return ctx.Query.QueryString.HasValue
                            ? YarpPipeline.TerminateWith(new(400))
                            : MessageHandlerPipelineInstance
                                .BindInput(static (RequestTransformContext input) => input.Path)
                                .BindWith(
                                    YarpPipeline.GetRequestTransformContext,
                                    static (input, result) =>
                                    {
                                        if (result.WasHandled(out string? message))
                                        {
                                            if (message is string msg)
                                            {
                                                input.HttpContext.Items["Message"] = msg;
                                                return YarpPipeline.Continue();
                                            }
                                            else
                                            {
                                                return YarpPipeline.TerminateWith(new(400));
                                            }
                                        }

                                        return YarpPipeline.Continue();
                                    });
            }),
            YarpPipeline.MakeStep(static ctx => ctx.HttpContext.Items["Message"] is string message
                        ? YarpPipeline.Continue()
                        : YarpPipeline.TerminateWith(new(404))));

    private static PipelineStep<RequestTransformContext, YarpPipelineResult> InnerPipelineInstance { get; } =
        YarpPipeline.Build(
            static ctx => ctx.Path == "/fizz"
                        ? YarpPipeline.TerminateAndForward()
                        : YarpPipeline.Continue(),
            static ctx => ctx.Path == "/buzz"
                        ? YarpPipeline.TerminateAndForward()
                        : YarpPipeline.Continue()
            );

    private static PipelineStep<PathString, HandlerResult<string?>> MessageHandlerPipelineInstance { get; } =
        MessageHandlerPipeline.Build(
            static path => path == "/foo"
                        ? MessageHandlerPipeline.Handled("We're looking at a foo")
                        : MessageHandlerPipeline.NotHandled(),
            static path => path == "/bar"
                        ? MessageHandlerPipeline.Handled(null)
                        : MessageHandlerPipeline.NotHandled());
}