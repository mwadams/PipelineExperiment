namespace PipelineExperiment;

public static class YarpPipeline
{
    public static PipelineStep<YarpPipelineState> Current { get; } = Pipeline.Current<YarpPipelineState>();

    public static PipelineStep<YarpPipelineState> Build(params PipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    public static PipelineStep<YarpPipelineState> Build(params SyncPipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }
}
