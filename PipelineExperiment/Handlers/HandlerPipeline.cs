namespace PipelineExperiment.Handlers;

using PipelineExperiment.Pipelines;

public static class HandlerPipeline
{
    public static PipelineStep<TInput, HandlerResult<TOutput>> Build<TInput, TOutput>(
        params Func<TInput, PipelineStep<TInput, HandlerResult<TOutput>>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline<TOutput>, steps.Select(s => Pipeline.MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, HandlerResult<TOutput>> Build<TInput, TOutput>(
        params Func<TInput, ValueTask<PipelineStep<TInput, HandlerResult<TOutput>>>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps.Select(s => Pipeline.MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, HandlerResult<TOutput>> Build<TInput, TOutput>(
        params PipelineStep<TInput, HandlerResult<TOutput>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Pipeline.Build(ShouldTerminatePipeline, steps);
    }

    public static PipelineStep<TInput, HandlerResult<TOutput>> NotHandled<TInput, TOutput>()
    {
        return PipelineStep<TInput, HandlerResult<TOutput>>.FromResult(new HandlerResult<TOutput>(default, false));
    }

    public static PipelineStep<TInput, HandlerResult<TOutput>> Handled<TInput, TOutput>(TOutput result)
    {
        return PipelineStep<TInput, HandlerResult<TOutput>>.FromResult(new HandlerResult<TOutput>(result, true));
    }

    private static bool ShouldTerminatePipeline<TOutput>(HandlerResult<TOutput> context)
    {
        return context.Terminate;
    }
}
