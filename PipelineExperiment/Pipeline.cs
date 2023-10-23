namespace PipelineExperiment;

public static class Pipeline
{
    public static PipelineStep<TState> Current<TState>()
    {
        return static state => ValueTask.FromResult(state);
    }

    public static PipelineStep<TState> Build<TState>(params PipelineStep<TState>[] steps)
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (var step in steps)
            {
                currentResult = await step(currentResult).ConfigureAwait(false);
            }

            return currentResult;
        };
    }

    public static PipelineStep<TState> Build<TState>(params SyncPipelineStep<TState>[] steps)
    {
        return state =>
        {
            TState currentResult = state;
            foreach (var step in steps)
            {
                currentResult = step(currentResult);
            }

            return ValueTask.FromResult(currentResult);
        };
    }

    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params PipelineStep<TState>[] steps)
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (var step in steps)
            {
                currentResult = await step(currentResult).ConfigureAwait(false);
                if (shouldTerminate(currentResult))
                {
                    return currentResult;
                }
            }

            return currentResult;
        };
    }

    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params SyncPipelineStep<TState>[] steps)
    {
        return state =>
        {
            TState currentResult = state;
            foreach (var step in steps)
            {
                currentResult = step(currentResult);
                if (shouldTerminate(currentResult))
                {
                    return ValueTask.FromResult(currentResult);
                }
            }

            return ValueTask.FromResult(currentResult);
        };
    }
}
