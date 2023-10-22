namespace PipelineExperiment.Handlers;

public static class HandlerPipeline
{
    public static PipelineStep<HandlerState<TInput, TResult>> GetCurrent<TInput, TResult>() => Pipeline.Current<HandlerState<TInput, TResult>>();

    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params PipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.Terminate,
            steps);
    }

    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params SyncPipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.Terminate,
            steps);
    }
}
