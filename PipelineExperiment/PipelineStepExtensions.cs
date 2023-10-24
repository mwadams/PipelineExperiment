// <copyright file="PipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment;

/// <summary>
/// Operators for <see cref="PipelineStep{TState}"/>.
/// </summary>
public static class PipelineStepExtensions
{
    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// provided by a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step whose output is bound to the selected <see cref="PipelineStep{TState}"/>.</param>
    /// <param name="selector">The selector which takes the output of the <paramref name="step"/> and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> Choose<TState>(this PipelineStep<TState> step, Func<TState, PipelineStep<TState>> selector)
    {
        return step.Bind(state => selector(state)(state));
    }

    /// <summary>
    /// An operator that combines two steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <param name="cancellationTokenProvider">A function which can provide a cancellation token from the state.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2, TState3}"/> for the sequential case.</remarks>
    public static PipelineStep<(TState1 State1, TState2 State2)> ParallelCombineSteps<TState1, TState2>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2,
        Func<TState1, CancellationToken>? cancellationTokenProvider = null)
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            ValueTask<TState1> task1 = step1(input.State1);
            ValueTask<TState2> task2 = step2(input.State2);

            // Optimize for sync completion.
            if (task1.IsCompleted)
            {
                if (task2.IsCompleted)
                {
                    return (task1.Result, task2.Result);
                }
                else
                {
                    return (task1.Result, await task2.ConfigureAwait(false));
                }
            }
            else if (task2.IsCompleted)
            {
                if (task1.IsCompleted)
                {
                    return (task1.Result, task2.Result);
                }
                else
                {
                    return (await task1.ConfigureAwait(false), task2.Result);
                }
            }

            // Otherwise, convert to real tasks and await the result
            await Task.WhenAll(task1.AsTask(), task2.AsTask()).ConfigureAwait(false);
            return (task1.Result, task2.Result);
        };
    }

    /// <summary>
    /// An operator that combines two steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    public static PipelineStep<(TState1 State1, TState2 State2)> CombineSteps<TState1, TState2>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2)
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            TState1? value1 = await step1(input.State1).ConfigureAwait(false);
            TState2? value2 = await step2(input.State2).ConfigureAwait(false);
            return (value1, value2);
        };
    }

    /// <summary>
    /// An operator that combines three steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <typeparam name="TState3">The type of the state of the third step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <param name="step3">The third input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    public static PipelineStep<(TState1 State1, TState2 State2, TState3 State3)> CombineSteps<TState1, TState2, TState3>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2,
        PipelineStep<TState3> step3)
    {
        return async ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            TState1? value1 = await step1(input.State1).ConfigureAwait(false);
            TState2? value2 = await step2(input.State2).ConfigureAwait(false);
            TState3? value3 = await step3(input.State3).ConfigureAwait(false);
            return (value1, value2, value3);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1Accessor"/>
    /// step. The value accessor operates on the value provided by the <paramref name="initialValue1"/>
    /// function. This wraps the input state instance to return the appropriate input state for the accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1Accessor">A <see cref="PipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <param name="initialValue1">A function which returns the initial value for executing the
    /// <paramref name="value1Accessor"/>. It has access to the state to do this.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState State, TValue1 Value1)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        Func<TState, TValue1> initialValue1)
    {
        return stepWith.Bind(
            async (TState state) => (state, await value1Accessor(GetDefaultValue(initialValue1, state))),
            (TState _, (TState State, TValue1 Value1) result) => ValueTask.FromResult(result.State));
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1Accessor"/>, and <paramref name="value2Accessor"/>
    /// steps. The value accessors operate on the values provided by the <paramref name="initialValue1"/>, and <paramref name="initialValue2"/>
    /// functions (respectively). These wrap the input state instance to return the appropriate input state for each accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1Accessor">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2Accessor">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="initialValue1">A function which returns the initial value for executing the
    /// <paramref name="value1Accessor"/>. It has access to the state to do this.</param>
    /// <param name="initialValue2">A function which returns the initial value for executing the
    /// <paramref name="value2Accessor"/>. It has access to the state to do this.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        PipelineStep<TValue2> value2Accessor,
        Func<TState, TValue1>? initialValue1 = null,
        Func<TState, TValue2>? initialValue2 = null)
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1Accessor(GetDefaultValue(initialValue1, state)),
                 await value2Accessor(GetDefaultValue(initialValue2, state))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2) result) => ValueTask.FromResult(result.State));
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1Accessor"/>, <paramref name="value2Accessor"/>,
    /// and <paramref name="value3Accessor"/> steps. The value accessors operate on values provided by the <paramref name="initialValue1"/>,
    /// <paramref name="initialValue2"/>, and <paramref name="initialValue3"/> functions (respectively). These wrap the input state to
    /// return the appropriate input state for each value accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1Accessor">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2Accessor">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="value3Accessor">A <see cref="PipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <param name="initialValue1">A function which returns the initial value for executing the
    /// <paramref name="value1Accessor"/>. It has access to the state to do this.</param>
    /// <param name="initialValue2">A function which returns the initial value for executing the
    /// <paramref name="value2Accessor"/>. It has access to the state to do this.</param>
    /// <param name="initialValue3">A function which returns the initial value for executing the
    /// <paramref name="value3Accessor"/>. It has access to the state to do this.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        PipelineStep<TValue1> value1Accessor,
        PipelineStep<TValue2> value2Accessor,
        PipelineStep<TValue3> value3Accessor,
        Func<TState, TValue1>? initialValue1 = null,
        Func<TState, TValue2>? initialValue2 = null,
        Func<TState, TValue3>? initialValue3 = null)
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1Accessor(GetDefaultValue(initialValue1, state)),
                 await value2Accessor(GetDefaultValue(initialValue2, state)),
                 await value3Accessor(GetDefaultValue(initialValue3, state))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3) result) => ValueTask.FromResult(result.State));
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInternalState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInternalState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInternalState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInternalState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInternalState, TState>(
        this PipelineStep<TInternalState> step,
        Func<TState, TInternalState> wrap,
        Func<TState, TInternalState, TState> unwrap)
    {
        return async state =>
        {
            TInternalState? internalState = await step(wrap(state)).ConfigureAwait(false);
            return unwrap(state, internalState);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInternalState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInternalState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInternalState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInternalState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInternalState, TState>(
        this PipelineStep<TInternalState> step,
        Func<TState, ValueTask<TInternalState>> wrap,
        Func<TState, TInternalState, TState> unwrap)
    {
        return async state =>
        {
            TInternalState? internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
            return unwrap(state, internalState);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInternalState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInternalState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInternalState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInternalState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInternalState, TState>(
        this PipelineStep<TInternalState> step,
        Func<TState, ValueTask<TInternalState>> wrap,
        Func<TState, TInternalState, ValueTask<TState>> unwrap)
    {
        return async state =>
        {
            TInternalState? internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
            return await unwrap(state, internalState).ConfigureAwait(false);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInternalState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInternalState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInternalState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInternalState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInternalState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInternalState, TState>(
        this PipelineStep<TInternalState> step,
        Func<TState, TInternalState> wrap,
        Func<TState, TInternalState, ValueTask<TState>> unwrap)
    {
        return async state =>
        {
            TInternalState? internalState = await step(wrap(state)).ConfigureAwait(false);
            return await unwrap(state, internalState).ConfigureAwait(false);
        };
    }

    /// <summary>
    /// An operator which binds the output of one step to the input of another.
    /// </summary>
    /// <typeparam name="TState">The type of the state of the pipeline step.</typeparam>
    /// <param name="step">The step whose output is provided to the binding step.</param>
    /// <param name="binding">The step whose input is provided by the binding.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes <paramref name="step"/>, and
    /// provides its output as the input of the <paramref name="binding"/> step, returning
    /// the resulting state.</returns>
    /// <remarks>This is equivalent to <see cref="Pipeline.Build{TState}(PipelineStep{TState}[])"/> passing the
    /// two steps in order.</remarks>
    public static PipelineStep<TState> Bind<TState>(
        this PipelineStep<TState> step,
        PipelineStep<TState> binding)
    {
        return async state =>
        {
            TState? result = await step(state).ConfigureAwait(false);
            return await binding(result).ConfigureAwait(false);
        };
    }

    // We should do something better than coercing this into shutting up about the potential null-ness of a default value
    // The problem comes if TState is a reference type that is not explicitly nullable.
    private static TValue GetDefaultValue<TState, TValue>(Func<TState, TValue>? defaultValueProvider, TState state) =>
        defaultValueProvider is Func<TState, TValue> provider ? provider(state) : default!;
}