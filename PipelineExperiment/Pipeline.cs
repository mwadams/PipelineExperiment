// <copyright file="Pipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace PipelineExperiment;

/// <summary>
/// Build a pipeline of sequentially executed steps from an array of <see cref="PipelineStep{TState}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each step operates on the instance of the <c>TState</c>, and returns an instance of the <c>TState</c>. That result is fed as the input
/// to the next step in the pipeline.
/// </para>
/// <para>
/// It is usual for the <c>TState</c> to be an immutable type, but side-effects are permitted.
/// </para>
/// <para>
/// A function that returns an instance of a step, rather than the result of a step, is called an <b>operator</b>. These operators
/// implement common patterns, that are resolved when the pipeline is executed.
/// </para>
/// <para>
/// (For the mathematically minded, we use "operator" broadly in the sense of a function that maps one function space to another,
/// rather than the dotnet <c>operator</c>s, such as addition, subtraction and instance type conversion).
/// </para>
/// <para>
/// The <see cref="PipelineStep{TState}"/> is an asynchronous function, which
/// returns a <see cref="ValueTask{TState}"/>.
/// </para>
/// <para>
/// For purely synchronous pipelines, you can use the overloads <see cref="Build{TState}(PipelineExperiment.SyncPipelineStep{TState}[])"/>
/// and <see cref="Build{TState}(Predicate{TState}, PipelineExperiment.SyncPipelineStep{TState}[])"/> that take a <see cref="SyncPipelineStep{TState}"/>
/// and optimize for that case.
/// </para>
/// <para>
/// For mixed sync and async pipelines, you should wrap your <see cref="SyncPipelineStep{TState}"/> instances in a call to
/// <see cref="ValueTask.FromResult{TResult}(TResult)"/>.
/// </para>
/// </remarks>
public static class Pipeline
{
    /// <summary>
    /// The identity operator. An operator that provides current value of the state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A pipeline step which, when executed, provides the current version of the state.</returns>
    public static PipelineStep<TState> Current<TState>()
        where TState : struct
    {
        return static state => ValueTask.FromResult(state);
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(params PipelineStep<TState>[] steps)
        where TState : struct
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
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(params SyncPipelineStep<TState>[] steps)
        where TState : struct
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
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
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
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params PipelineStep<TState>[] steps)
        where TState : struct
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
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
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
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params SyncPipelineStep<TState>[] steps)
        where TState : struct
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

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with entry and exit logging.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> BuildWithStepLogging<TState>(LogLevel level, params NamedPipelineStep<TState>[] steps)
        where TState : struct, ILoggable<TState>
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (NamedPipelineStep<TState> step in steps)
            {
                LogEntry(level, state, step.EventId);

                currentResult = await step.Step(currentResult).ConfigureAwait(false);

                LogExit(level, state, step.EventId);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with entry and exit logging.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> BuildWithStepLogging<TState>(LogLevel level, params NamedSyncPipelineStep<TState>[] steps)
        where TState : struct, ILoggable<TState>
    {
        return state =>
        {
            TState currentResult = state;
            foreach (NamedSyncPipelineStep<TState> step in steps)
            {
                LogEntry(level, state, step.EventId);

                currentResult = step.Step(currentResult);

                LogExit(level, state, step.EventId);
            }

            return ValueTask.FromResult(currentResult);
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true. It also logs on entry, and
    /// exit or termination.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> BuildWithStepLogging<TState>(Predicate<TState> shouldTerminate, LogLevel level, params NamedPipelineStep<TState>[] steps)
        where TState : struct, ILoggable<TState>
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (NamedPipelineStep<TState> step in steps)
            {
                LogEntry(level, state, step.EventId);

                currentResult = await step.Step(currentResult).ConfigureAwait(false);

                if (shouldTerminate(currentResult))
                {
                    LogTerminated(level, state, step.EventId);
                    return currentResult;
                }

                LogExit(level, state, step.EventId);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> BuildWithStepLogging<TState>(Predicate<TState> shouldTerminate, LogLevel level, params NamedSyncPipelineStep<TState>[] steps)
        where TState : struct, ILoggable<TState>
    {
        return state =>
        {
            TState currentResult = state;
            foreach (NamedSyncPipelineStep<TState> step in steps)
            {
                LogEntry(level, state, step.EventId);

                currentResult = step.Step(currentResult);
                if (shouldTerminate(currentResult))
                {
                    LogTerminated(level, state, step.EventId);
                    return ValueTask.FromResult(currentResult);
                }

                LogExit(level, state, step.EventId);
            }

            return ValueTask.FromResult(currentResult);
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogTerminated<TState>(LogLevel level, in TState state, in EventId eventId)
        where TState : struct, ILoggable<TState>
    {
        Log(level, state, eventId, "Terminated");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogEntry<TState>(LogLevel level, in TState state, in EventId eventId)
        where TState : struct, ILoggable<TState>
    {
        Log(level, state, eventId, "Entered");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogExit<TState>(LogLevel level, in TState state, in EventId eventId)
        where TState : struct, ILoggable<TState>
    {
        Log(level, state, eventId, "Exited");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Log<TState>(LogLevel level, TState state, in EventId eventId, string message)
        where TState : struct, ILoggable<TState>
    {
        if (state.Logger.IsEnabled(level))
        {
            switch (level)
            {
                case LogLevel.Debug:
                    state.Logger.LogDebug(eventId, message);
                    break;
                case LogLevel.Trace:
                    state.Logger.LogTrace(eventId, message);
                    break;
                case LogLevel.Information:
                    state.Logger.LogInformation(eventId, message);
                    break;
                case LogLevel.Warning:
                    state.Logger.LogWarning(eventId, message);
                    break;
                case LogLevel.Error:
                    state.Logger.LogError(eventId, message);
                    break;
                case LogLevel.Critical:
                    state.Logger.LogCritical(eventId, message);
                    break;
            }
        }
    }
}