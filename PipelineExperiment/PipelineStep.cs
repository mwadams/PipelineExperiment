namespace PipelineExperiment;

public delegate ValueTask<TState> PipelineStep<TState>(TState state);
public delegate TState SyncPipelineStep<TState>(TState state);

public static class PipelineStepExtensions
{
    public static PipelineStep<TState> Choose<TState>(this PipelineStep<TState> step, Func<TState, PipelineStep<TState>> selector)
    {
        return step.Bind(state => selector(state)(state));
    }

    public static PipelineStep<(TValue1, TValue2)> CombineSteps<TValue1, TValue2>(
        this PipelineStep<TValue1> pipeline1,
        PipelineStep<TValue2> pipeline2)
    {
        return async ((TValue1 v1, TValue2 v2) input) =>
        {
            var value1 = await pipeline1(input.v1).ConfigureAwait(false);
            var value2 = await pipeline2(input.v2).ConfigureAwait(false);
            return (value1, value2);
        };
    }

    public static PipelineStep<(TValue1, TValue2, TValue3)> CombineSteps<TValue1, TValue2, TValue3>(
        this PipelineStep<TValue1> pipeline1,
        PipelineStep<TValue2> pipeline2,
        PipelineStep<TValue3> pipeline3)
    {
        return async ((TValue1 v1, TValue2 v2, TValue3 v3) input) =>
        {
            var value1 = await pipeline1(input.v1).ConfigureAwait(false);
            var value2 = await pipeline2(input.v2).ConfigureAwait(false);
            var value3 = await pipeline3(input.v3).ConfigureAwait(false);
            return (value1, value2, value3);
        };
    }

    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState, TValue1)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        Func<TState, TValue1> defaultValue1)
    {
        return stepWith.Bind(
            async (TState state) => (state, await value1Accessor(GetDefaultValue(defaultValue1, state))),
            (TState state, (TState, TValue1) result) => ValueTask.FromResult(result.Item1));
    }

    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState, TValue1, TValue2)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        PipelineStep<TValue2> value2Accessor,
        Func<TState, TValue1>? defaultValue1 = null,
        Func<TState, TValue2>? defaultValue2 = null)
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1Accessor(GetDefaultValue(defaultValue1, state)),
                 await value2Accessor(GetDefaultValue(defaultValue2, state))),
            (TState state, (TState, TValue1, TValue2) result) => ValueTask.FromResult(result.Item1));
    }

    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState, TValue1, TValue2, TValue3)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        PipelineStep<TValue2> value2Accessor,
        PipelineStep<TValue3> value3Accessor,
        Func<TState, TValue1>? defaultValue1 = null,
        Func<TState, TValue2>? defaultValue2 = null,
        Func<TState, TValue3>? defaultValue3 = null)
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1Accessor(GetDefaultValue(defaultValue1, state)),
                 await value2Accessor(GetDefaultValue(defaultValue2, state)),
                 await value3Accessor(GetDefaultValue(defaultValue3, state))),
            (TState state, (TState, TValue1, TValue2, TValue3) result) => ValueTask.FromResult(result.Item1));
    }

    public static PipelineStep<TBoundState> Bind<TState, TBoundState>(
        this PipelineStep<TState> step,
        Func<TBoundState, TState> wrap,
        Func<TBoundState, TState, TBoundState> unwrap)
    {
        return async boundState =>
        {
            var result = await step(wrap(boundState)).ConfigureAwait(false);
            return unwrap(boundState, result);
        };
    }

    public static PipelineStep<TBoundState> Bind<TState, TBoundState>(
        this PipelineStep<TState> step,
        Func<TBoundState, ValueTask<TState>> wrap,
        Func<TBoundState, TState, TBoundState> unwrap)
    {
        return async boundState =>
        {
            var result = await step(await wrap(boundState).ConfigureAwait(false)).ConfigureAwait(false);
            return unwrap(boundState, result);
        };
    }

    public static PipelineStep<TBoundState> Bind<TState, TBoundState>(
        this PipelineStep<TState> step,
        Func<TBoundState, ValueTask<TState>> wrap,
        Func<TBoundState, TState, ValueTask<TBoundState>> unwrap)
    {
        return async boundState =>
        {
            var result = await step(await wrap(boundState).ConfigureAwait(false)).ConfigureAwait(false);
            return await unwrap(boundState, result).ConfigureAwait(false);
        };
    }

    public static PipelineStep<TState> Bind<TState>(
        this PipelineStep<TState> step,
        PipelineStep<TState> binding)
    {
        return async state =>
        {
            var result = await step(state).ConfigureAwait(false);
            return await binding(result).ConfigureAwait(false);
        };
    }

    public static PipelineStep<TBoundState> Bind<TState, TBoundState>(
        this PipelineStep<TState> step,
        Func<TBoundState, TState> wrap,
        Func<TBoundState, TState, ValueTask<TBoundState>> unwrap)
    {
        return async boundState =>
        {
            var result = await step(wrap(boundState)).ConfigureAwait(false);
            return await unwrap(boundState, result).ConfigureAwait(false);
        };
    }

    // We should do something better than coercing this into shutting up about the potential null-ness of a default value
    // The problem comes if TValue is a reference type that is not explicitly nullable.
    private static TValue GetDefaultValue<TState, TValue>(Func<TState, TValue>? defaultValueProvider, TState state) =>
        defaultValueProvider is Func<TState, TValue> provider ? provider(state) : default!;
}
