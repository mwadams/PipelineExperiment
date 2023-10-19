namespace Sandbox;

using PipelineExperiment;
using PipelineExperiment.Handlers;
using PipelineExperiment.Pipelines;
using Yarp.ReverseProxy.Transforms;

public static class ExampleYarpPipeline
{
    public static PipelineStep<YarpProcessingContext, YarpProcessingContext> Instance { get; } =
        YarpPipeline.Build(
            ctx => ctx.RequestTransformContext.Path == "/"
                        ? YarpPipeline.TerminateAndForward(ctx)
                        : YarpPipeline.Continue(ctx),
            ctx => ctx.RequestTransformContext.Query.QueryString.HasValue
                        ? YarpPipeline.TerminateWith(ctx, new(400))
                        : ExampleHandlerPipeline.Instance.Bind(
                            result =>
                            {
                                return result.WasHandled(out int statusCode)
                                    ? YarpPipeline.TerminateWith(ctx, new(statusCode))
                                    : YarpPipeline.Continue(ctx);
                            }));
}

public static class ExampleHandlerPipeline
{
    public static PipelineStep<YarpProcessingContext, HandlerResult<int>> Instance { get; } =
        HandlerPipeline.Build<YarpProcessingContext, int>(
            ctx => ctx.RequestTransformContext.Path == "/"
                        ? HandlerPipeline.Handled<YarpProcessingContext, int>(1)
                        : HandlerPipeline.NotHandled<YarpProcessingContext, int>(),
            ctx => ctx.RequestTransformContext.Query.QueryString.HasValue
                        ? HandlerPipeline.Handled<YarpProcessingContext, int>(2)
                        : HandlerPipeline.NotHandled<YarpProcessingContext, int>());
}