namespace PipelineExperiment.Pipelines;

using System.Diagnostics.CodeAnalysis;

public readonly struct PipelineStep<TInput, TOutput>
{
    private readonly bool isComputed;
    private readonly Func<TInput, ValueTask<PipelineStep<TInput, TOutput>>> computeResult;
    private readonly TOutput? computedResult;

    public PipelineStep(Func<TInput, ValueTask<PipelineStep<TInput, TOutput>>> computeResult)
    {
        this.computeResult = computeResult;
        computedResult = default;
        isComputed = false;
    }

    private PipelineStep(TOutput computedResult)
    {
        computeResult = static input => default;
        this.computedResult = computedResult;
        isComputed = true;
    }

    public static PipelineStep<TInput, TOutput> FromResult(TOutput result)
    {
        return new(result);
    }

    public async ValueTask<PipelineStep<TInput, TOutput>> RunAsync(TInput input)
    {
        if (isComputed)
        {
            return this;
        }

        // Compute the result
        var computedStep = await computeResult(input).ConfigureAwait(false);

        // Recurse into the returned step if we have not yet run the resulting step.
        // The check above will ensure we drop out fast if we have already run.
        return await computedStep.RunAsync(input);
    }

    public bool TryGetResult([MaybeNullWhen(false)] out TOutput computedResult)
    {
        if (isComputed)
        {
            computedResult = this.computedResult;
        }
        else
        {
            computedResult = default;
        }

        return isComputed;
    }
}
