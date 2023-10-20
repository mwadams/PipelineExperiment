namespace PipelineExperiment.Pipelines;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public readonly struct PipelineStep<TContext, TOutput>
{
    private readonly bool isContext;
    private readonly bool isComputed;
    private readonly Func<ValueTask<PipelineStep<TContext, TOutput>>> computeResult;
    private readonly TOutput? computedResult;

    private PipelineStep(bool notUsedJustHereToMakeThisCtorDistinct)
    {
        Debug.Assert(typeof(TContext) == typeof(TOutput));
        computeResult = static () => default;
        this.isContext = true;
    }

    private PipelineStep(Func<ValueTask<PipelineStep<TContext, TOutput>>> computeResult)
    {
        this.computeResult = computeResult;
        computedResult = default;
        isComputed = false;
    }

    private PipelineStep(TOutput computedResult)
    {
        computeResult = static () => default;
        this.computedResult = computedResult;
        isComputed = true;
    }

    public static PipelineStep<TContext, TOutput> ReturnContext()
    {
        if (typeof(TContext) == typeof(TOutput))
        {
            throw new ArgumentException("Input and output type must be the same for ReturnContext");
        }

        return new(true);
    }

    public static PipelineStep<TContext, TOutput> FromResult(TOutput result)
    {
        return new(result);
    }

    public static PipelineStep<TContext, TOutput> MakeStep(
        Func<PipelineStep<TContext, TOutput>> step)
    {
        return new PipelineStep<TContext, TOutput>(() => ValueTask.FromResult(step()));
    }

    public static PipelineStep<TContext, TOutput> MakeStep(
        Func<ValueTask<PipelineStep<TContext, TOutput>>> step)
    {
        return new PipelineStep<TContext, TOutput>(step);
    }


    public async ValueTask<PipelineStep<TContext, TOutput>> RunAsync(TContext input)
    {
        if (isContext)
        {
            Debug.Assert(typeof(TContext) == typeof(TOutput));
            return FromResult((TOutput)(object)input!);
        }

        if (isComputed)
        {       
            return this;
        }

        // Compute the result
        var computedStep = await computeResult().ConfigureAwait(false);

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
