namespace PipelineExperiment;

using PipelineExperiment.Pipelines;

using Yarp.ReverseProxy.Transforms;

using YarpPipelineStep = Pipelines.PipelineStep<Yarp.ReverseProxy.Transforms.RequestTransformContext, YarpPipelineResult>;

public static class YarpPipeline
{
    // This is pointless right now because the RequestTransformContext would be available as an input anyway.
    // But what if it wasn't...?
    public static PipelineStep<RequestTransformContext, RequestTransformContext> GetRequestTransformContext { get; } =
        PipelineStep<RequestTransformContext, RequestTransformContext>.ReturnContext();

    public static YarpPipelineStep Build(
        params Func<RequestTransformContext, YarpPipelineStep>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps.Select(s => Pipeline.MakeStep(s)).ToArray());
    }

    public static YarpPipelineStep Build(
        params Func<RequestTransformContext, ValueTask<YarpPipelineStep>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps.Select(s => Pipeline.MakeStep(s)).ToArray());
    }

    public static YarpPipelineStep Build(
        params YarpPipelineStep[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps);
    }

    public static YarpPipelineStep TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        return YarpPipelineResult.TerminateWith(responseDetails);
    }

    public static YarpPipelineStep TerminateAndForward()
    {
        return YarpPipelineResult.TerminateAndForward();
    }

    public static YarpPipelineStep Continue()
    {
        return YarpPipelineResult.Continue();
    }

    public static YarpPipelineStep MakeStep(
        Func<RequestTransformContext, YarpPipelineStep> step)
    {
        return YarpPipelineStep.MakeStep(input => ValueTask.FromResult(step(input)));
    }

    public static YarpPipelineStep MakeStep(
        Func<RequestTransformContext, ValueTask<YarpPipelineStep>> step)
    {
        return YarpPipelineStep.MakeStep(step);
    }

    public static YarpPipelineStep BindWithRequestTransformContext<TOutput>(
        this PipelineStep<RequestTransformContext, TOutput> step,
        Func<RequestTransformContext, TOutput, YarpPipelineStep> binding)
    {
        step.BindWith(YarpPipeline.GetRequestTransformContext, binding);
        return step.Bind(binding);
    }

    private static bool ShouldTerminatePipeline(YarpPipelineResult result)
    {
        return result.ShouldTerminatePipeline;
    }
}
