// <copyright file="PipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Threading.Tasks;

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
        where TState : struct
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
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2}"/> for the sequential case.</remarks>
    public static PipelineStep<(TState1 State1, TState2 State2)> ParallelCombineSteps<TState1, TState2>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2)
        where TState1 : struct
        where TState2 : struct
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            ValueTask<TState1> task1 = step1(input.State1);
            ValueTask<TState2> task2 = step2(input.State2);

            return (await task1.ConfigureAwait(false), await task2.ConfigureAwait(false));
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
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2, TState3}"/> for the sequential case.</remarks>
    public static PipelineStep<(TState1 State1, TState2 State2, TState3 State3)> ParallelCombineSteps<TState1, TState2, TState3>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2,
        PipelineStep<TState3> step3)
        where TState1 : struct
        where TState2 : struct
        where TState3 : struct
    {
        return async ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            ValueTask<TState1> task1 = step1(input.State1);
            ValueTask<TState2> task2 = step2(input.State2);
            ValueTask<TState3> task3 = step3(input.State3);

            return (await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false));
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
        where TState1 : struct
        where TState2 : struct
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            TState1 value1 = await step1(input.State1).ConfigureAwait(false);
            TState2 value2 = await step2(input.State2).ConfigureAwait(false);
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
        where TState1 : struct
        where TState2 : struct
        where TState3 : struct
    {
        return async ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            TState1 value1 = await step1(input.State1).ConfigureAwait(false);
            TState2 value2 = await step2(input.State2).ConfigureAwait(false);
            TState3 value3 = await step3(input.State3).ConfigureAwait(false);
            return (value1, value2, value3);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the default value of <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState State, TValue1 Value1)> stepWith,
        PipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
        => BindWith(stepWith, null, value1ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the value provided by the <paramref name="initialValue1FromState"/>
    /// function. This wraps the input state instance to return the appropriate input state for the accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState State, TValue1 Value1)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
    {
        return stepWith.Bind(
            async (TState state) => (state, await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState))),
            (TState _, (TState State, TValue1 Value1) result) => ValueTask.FromResult(result.State));
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the default values for <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        PipelineStep<TValue1> value1ProviderStep,
        PipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the values provided by the <paramref name="initialValue1FromState"/>, and <paramref name="initialValue2FromState"/>
    /// functions (respectively). These wrap the input state instance to return the appropriate input state for each accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        PipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2) result) => ValueTask.FromResult(result.State));
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on the default values of <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="value3ProviderStep">A <see cref="PipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        PipelineStep<TValue1> value1ProviderStep,
        PipelineStep<TValue2> value2ProviderStep,
        PipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep, null, value3ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on values provided by the <paramref name="initialValue1FromState"/>,
    /// <paramref name="initialValue2FromState"/>, and <paramref name="initialValue3FromState"/> functions (respectively). These wrap the input state to
    /// return the appropriate input state for each value accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="initialValue3FromState">A function which returns the initial value for executing the
    /// <paramref name="value3ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value3ProviderStep">A <see cref="PipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        PipelineStep<TValue2> value2ProviderStep,
        Func<TState, TValue3>? initialValue3FromState,
        PipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState)),
                 await value3ProviderStep(GetValueOrDefault(state, initialValue3FromState))),
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
        where TInternalState : struct
        where TState : struct
    {
        return async state =>
        {
            TInternalState internalState = await step(wrap(state)).ConfigureAwait(false);
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
        where TInternalState : struct
        where TState : struct
    {
        return async state =>
        {
            TInternalState internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
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
        where TInternalState : struct
        where TState : struct
    {
        return async state =>
        {
            TInternalState internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
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
        where TInternalState : struct
        where TState : struct
    {
        return async state =>
        {
            TInternalState internalState = await step(wrap(state)).ConfigureAwait(false);
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
        where TState : struct
    {
        return async state =>
        {
            TState result = await step(state).ConfigureAwait(false);
            return await binding(result).ConfigureAwait(false);
        };
    }

    private static TValue GetValueOrDefault<TState, TValue>(TState state, Func<TState, TValue>? defaultValueProvider)
        where TState : struct
        where TValue : struct
        => defaultValueProvider is Func<TState, TValue> provider ? provider(state) : default;
}