// <copyright file="YarpPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment;

/// <summary>
/// A <see cref="Pipeline"/> for handling YARP transforms.
/// </summary>
public static class YarpPipeline
{
    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static PipelineStep<YarpPipelineState> Current { get; } = Pipeline.Current<YarpPipelineState>();

    /// <summary>
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpPipelineState> Build(params PipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpPipelineState> Build(params SyncPipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }
}