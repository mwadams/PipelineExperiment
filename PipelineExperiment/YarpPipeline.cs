namespace PipelineExperiment;

using PipelineExperiment.Pipelines;

using Yarp.ReverseProxy.Transforms;
using YarpPipelineStep = Pipelines.PipelineStep<YarpProcessingContext, YarpProcessingContext>;

public static class YarpPipeline
{
    public static YarpPipelineStep Build(
        params Func<YarpProcessingContext, YarpPipelineStep>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps.Select(s => Pipeline.MakeStep(s)).ToArray());
    }

    public static YarpPipelineStep Build(
        params Func<YarpProcessingContext, ValueTask<YarpPipelineStep>>[] steps)
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

    public static YarpPipelineStep TerminateWith(YarpProcessingContext context, NonForwardedResponseDetails responseDetails)
    {
        return YarpPipelineResult.TerminateWith(context.RequestTransformContext, responseDetails);
    }

    public static YarpPipelineStep TerminateAndForward(YarpProcessingContext context)
    {
        return YarpPipelineResult.TerminateAndForward(context.RequestTransformContext);
    }

    public static YarpPipelineStep Continue(YarpProcessingContext context)
    {
        return YarpPipelineResult.Continue(context.RequestTransformContext);
    }


    private static bool ShouldTerminatePipeline(YarpProcessingContext context)
    {
        return context.StepResult.ShouldTerminatePipeline;
    }
}
