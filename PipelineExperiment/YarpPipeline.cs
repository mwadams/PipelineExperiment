namespace PipelineExperiment;

using PipelineExperiment.Pipelines;

using Yarp.ReverseProxy.Transforms;

using YarpPipelineStep = Pipelines.PipelineStep<Yarp.ReverseProxy.Transforms.RequestTransformContext, YarpPipelineResult>;

public static class YarpPipeline
{
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

    private static bool ShouldTerminatePipeline(YarpPipelineResult result)
    {
        return result.ShouldTerminatePipeline;
    }
}
