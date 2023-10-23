// <copyright file="Pipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment;

/// <summary>
/// Build a pipeline of sequentially executed steps from an array of <see cref="PipelineStep{TState}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each step operates on the instance of the TState, and returns an instance of the TState. That result is fed as the input
/// to the next step in the pipeline.
/// </para>
/// <para>
/// It is usual for the TState to be immutable, but side-effects are permitted.
/// </para>
/// <para>
/// A function that returns an instance of a step is called an <b>operator</b>. These operators
/// allow you to build more complex logic into your pipeline at design time, that are resolved
/// when the pipeline is executed.
/// </para>
/// <para>
/// The underlying <see cref="PipelineStep{TState}"/> is an intrinsically asynchronous process, which
/// returns a <see cref="ValueTask{TState}"/>.
/// </para>
/// <para>
/// For purely synchronous pipelines, you can use the overloads <see cref="Build{TState}(PipelineExperiment.SyncPipelineStep{TState}[])"/>
/// and <see cref="Build{TState}(Predicate{TState}, PipelineExperiment.SyncPipelineStep{TState}[])"/> that take a <see cref="SyncPipelineStep{TState}"/>
/// and optimize.
/// </para>
/// <para>
/// For mixed sync and async pipelines, you should wrap your <see cref="SyncPipelineStep{TState}"/> instances in a call to
/// <see cref="ValueTask.FromResult{TResult}(TResult)"/>.
/// </para>
/// </remarks>
public static class Pipeline
{
    /// <summary>
    /// An operator that provides current value of the state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A pipeline step which, when executed, provides the current version of the state.</returns>
    public static PipelineStep<TState> Current<TState>()
    {
        return static state => ValueTask.FromResult(state);
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order as a pipeline.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The state is passed to each <see cref="PipelineStep{TState}"/> in turn, and the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(params PipelineStep<TState>[] steps)
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (PipelineStep<TState> step in steps)
            {
                currentResult = await step(currentResult).ConfigureAwait(false);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order as a pipeline.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The state is passed to each <see cref="PipelineStep{TState}"/> in turn, and the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(params SyncPipelineStep<TState>[] steps)
    {
        return state =>
        {
            TState currentResult = state;
            foreach (SyncPipelineStep<TState> step in steps)
            {
                currentResult = step(currentResult);
            }

            return ValueTask.FromResult(currentResult);
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order as a pipeline, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The state is passed to each <see cref="PipelineStep{TState}"/> in turn until one returns an instance
    /// of the <typeparamref name="TState"/> for which the <paramref name="shouldTerminate"/> predicate returns true.
    /// At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params PipelineStep<TState>[] steps)
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (PipelineStep<TState> step in steps)
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

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order as a pipeline, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The state is passed to each <see cref="PipelineStep{TState}"/> in turn until one returns an instance
    /// of the <typeparamref name="TState"/> for which the <paramref name="shouldTerminate"/> predicate returns true.
    /// At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params SyncPipelineStep<TState>[] steps)
    {
        return state =>
        {
            TState currentResult = state;
            foreach (SyncPipelineStep<TState> step in steps)
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