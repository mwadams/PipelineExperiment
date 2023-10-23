// <copyright file="HandlerState{TInput,TResult}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PipelineExperiment.Handlers;

/// <summary>
/// The state for an event handler.
/// </summary>
/// <typeparam name="TInput">The type of the value to be handled.</typeparam>
/// <typeparam name="TResult">The result of handling the value.</typeparam>
/// <remarks>
/// <para>
/// When you build and execute <see cref="HandlerPipeline"/>, you start with the
/// state provided by <see cref="For(TInput)"/>. The state is passed to the
/// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
/// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
/// next step, until one returns <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>.
/// At this point the pipeline will be terminated.
/// </para>
/// <para>
/// On termination, you can inspect the resulting value using <see cref="WasHandled(out TResult)"/>.
/// </para>
/// </remarks>
public readonly struct HandlerState<TInput, TResult>
{
    private readonly TResult? result;
    private readonly bool handled;

    private HandlerState(TInput input, TResult? result, bool handled)
    {
        this.result = result;
        this.handled = handled;
        this.Input = input;
    }

    /// <summary>
    /// Gets the input value for the handler.
    /// </summary>
    public TInput Input { get; }

    /// <summary>
    /// Creates an instance of the handler state for the given input value.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <returns>The <see cref="HandlerState{TInput, TResult}"/> for the input value.</returns>
    public static HandlerState<TInput, TResult> For(TInput input)
    {
        return new(input, default, false);
    }

    /// <summary>
    /// Creates an instance of the <see cref="HandlerState{TInput, TResult}"/> indicating
    /// that the handler handled the input, and provided the given result.
    /// </summary>
    /// <param name="result">The result of handling.</param>
    /// <returns>The <see cref="HandlerState{TInput, TResult}"/> with the given result value.</returns>
    public HandlerState<TInput, TResult> Handled(TResult result)
    {
        return new(this.Input, result, true);
    }

    /// <summary>
    /// Creates an instance of the <see cref="HandlerState{TInput, TResult}"/> indicating that the
    /// handler did not handle the input.
    /// </summary>
    /// <returns>The unchanged <see cref="HandlerState{TInput, TResult}"/>.</returns>
    public HandlerState<TInput, TResult> NotHandled()
    {
        return this;
    }

    /// <summary>
    /// Indicates whether the input value has been handled by the pipeline. If so,
    /// the <paramref name="result"/> will represent the value produced by the handler.
    /// </summary>
    /// <param name="result">The value produced by the handler.</param>
    /// <returns><see langword="true"/> if the input value has been handled, otherwise <see langword="false"/>.</returns>
    public bool WasHandled([MaybeNullWhen(false)] out TResult? result)
    {
        result = this.result;
        return this.handled;
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="HandlerPipeline"/> should terminate.
    /// </summary>
    /// <returns><see langword="true"/> if the pipeline should terminate.</returns>
    internal bool ShouldTerminate() => this.handled;
}