﻿// <copyright file="HandlerPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment.Handlers;

/// <summary>
/// A <see cref="Pipeline"/> for a handler pattern.
/// </summary>
/// <remarks>
/// This will pass an instance of the <see cref="HandlerState{TInput, TResult}"/> to each step in the pipeline in turn,
/// and returns the result of the first step for which <see cref="HandlerState{TInput, TResult}.WasHandled"/> is true.
/// </remarks>
public static class HandlerPipeline
{
    /// <summary>
    /// The identity operator. This returns a <see cref="PipelineStep{THandlerState}"/> which, when executed, produces the current state of the pipeline.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which, when executed, produces the current state of the pipeline.</returns>
    public static PipelineStep<HandlerState<TInput, TResult>> GetCurrent<TInput, TResult>() => Pipeline.Current<HandlerState<TInput, TResult>>();

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params PipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            steps);
    }

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params SyncPipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            steps);
    }
}