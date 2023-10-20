namespace PipelineExperiment.Pipelines;

public static class PipelineStepExtensions
{
    public static PipelineStep<TBoundInput, TOutput> BindInput<TInput, TOutput, TBoundInput>(this PipelineStep<TInput, TOutput> step, Func<TBoundInput, TInput> inputBinding)
    {
        return PipelineStep<TBoundInput, TOutput>.MakeStep(async input =>
        {
            var boundInput = inputBinding(input);
            var result = await step.RunAsync(boundInput).ConfigureAwait(false);
            if (!result.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return PipelineStep<TBoundInput, TOutput>.FromResult(computedResult);
        });
    }

    public static PipelineStep<TBoundInput, TOutput> BindInput<TInput, TOutput, TBoundInput>(this PipelineStep<TInput, TOutput> step, Func<TBoundInput, ValueTask<TInput>> inputBinding)
    {
        return PipelineStep<TBoundInput, TOutput>.MakeStep(async input =>
        {
            var boundInput = await inputBinding(input).ConfigureAwait(false);
            var result = await step.RunAsync(boundInput).ConfigureAwait(false);
            if (!result.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return PipelineStep<TBoundInput, TOutput>.FromResult(computedResult);
        });
    }

    public static PipelineStep<TInput, TBoundOutput> Bind<TInput, TOutput, TBoundOutput>(this PipelineStep<TInput, TOutput> step, Func<TOutput, PipelineStep<TInput, TBoundOutput>> binding)
    {
        return PipelineStep<TInput, TBoundOutput>.MakeStep(async input =>
        {
            var computedStep = await step.RunAsync(input).ConfigureAwait(false);
            if (!computedStep.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return binding(computedResult);
        });
    }

    public static PipelineStep<TInput, TBoundOutput> Bind<TInput, TOutput, TBoundOutput>(this PipelineStep<TInput, TOutput> step, Func<TOutput, ValueTask<PipelineStep<TInput, TBoundOutput>>> binding)
    {
        return PipelineStep<TInput, TBoundOutput>.MakeStep(async input =>
        {
            var computedStep = await step.RunAsync(input).ConfigureAwait(false);
            if (!computedStep.TryGetResult(out TOutput? computedResult))
            {
                throw new InvalidOperationException("The computed result must exist after RunAsync()");
            }

            return await binding(computedResult).ConfigureAwait(false);
        });
    }
}
