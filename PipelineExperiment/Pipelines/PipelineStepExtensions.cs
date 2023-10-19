namespace PipelineExperiment.Pipelines;
public static class PipelineStepExtensions
{
    public static PipelineStep<TInput, TBoundOutput> Bind<TInput, TOutput, TBoundOutput>(this PipelineStep<TInput, TOutput> step, Func<TOutput, PipelineStep<TInput, TBoundOutput>> binding)
    {
        return new PipelineStep<TInput, TBoundOutput>(async input =>
        {
            var computedStep = await step.RunAsync(input);
            if (!computedStep.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return binding(computedResult);
        });
    }

    public static PipelineStep<TInput, TBoundOutput> Bind<TInput, TOutput, TBoundOutput>(this PipelineStep<TInput, TOutput> step, Func<TOutput, ValueTask<PipelineStep<TInput, TBoundOutput>>> binding)
    {
        return new PipelineStep<TInput, TBoundOutput>(async input =>
        {
            var computedStep = await step.RunAsync(input);
            if (!computedStep.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return await binding(computedResult).ConfigureAwait(false);
        });
    }
}
