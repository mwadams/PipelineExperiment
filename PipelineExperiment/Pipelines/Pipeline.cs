namespace PipelineExperiment.Pipelines;

public static class Pipeline
{
    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        Predicate<TOutput> shouldTerminate,
        params Func<TInput, ValueTask<PipelineStep<TInput, TOutput>>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Build(shouldTerminate, steps.Select(s => MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        Predicate<TOutput> shouldTerminate,
        params Func<TInput, PipelineStep<TInput, TOutput>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Build(shouldTerminate, steps.Select(s => MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        Predicate<TOutput> shouldTerminate,
        params PipelineStep<TInput, TOutput>[] steps)
    {
        if (steps.Length == 0)
        {
            throw new ArgumentException("You must provide at least one step to build a sequential pipeline", nameof(steps));
        }

        // Never terminate
        var sequentialPipeline = new TerminatingPipeline<TInput, TOutput>(shouldTerminate, steps);
        return new PipelineStep<TInput, TOutput>(sequentialPipeline.RunAsync);
    }

    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        params Func<TInput, ValueTask<PipelineStep<TInput, TOutput>>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Build(steps.Select(s => MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        params Func<TInput, PipelineStep<TInput, TOutput>>[] steps)
    {
        // This is the sort of the thing, but clearly we're building the array again
        return Build(steps.Select(s => MakeStep(s)).ToArray());
    }

    public static PipelineStep<TInput, TOutput> Build<TInput, TOutput>(
        params PipelineStep<TInput, TOutput>[] steps)
    {
        if (steps.Length == 0)
        {
            throw new ArgumentException("You must provide at least one step to build a sequential pipeline", nameof(steps));
        }

        // Never terminate
        var sequentialPipeline = Build(s => false, steps);
        return new PipelineStep<TInput, TOutput>(sequentialPipeline.RunAsync);
    }

    public static PipelineStep<TInput, TOutput> MakeStep<TInput, TOutput>(
        Func<TInput, PipelineStep<TInput, TOutput>> step)
    {
        return new PipelineStep<TInput, TOutput>(input => ValueTask.FromResult(step(input)));
    }

    public static PipelineStep<TInput, TOutput> MakeStep<TInput, TOutput>(
        Func<TInput, ValueTask<PipelineStep<TInput, TOutput>>> step)
    {
        return new PipelineStep<TInput, TOutput>(step);
    }

    private readonly record struct TerminatingPipeline<TInput, TOutput>(Predicate<TOutput> ShouldTerminate, PipelineStep<TInput, TOutput>[] Steps)
    {
        internal async ValueTask<PipelineStep<TInput, TOutput>> RunAsync(TInput input)
        {
            PipelineStep<TInput, TOutput> computedStep = default;

            foreach (PipelineStep<TInput, TOutput> step in Steps)
            {
                computedStep = await step.RunAsync(input);

                // This will always be true after RunAsync();
                if (!computedStep.TryGetResult(out TOutput? computedResult))
                {
                    throw new InvalidOperationException("The computed result must exist after RunAsync()");
                }


                if (ShouldTerminate(computedResult))
                {
                    break;
                }
            }

            return computedStep;
        }
    }
}
