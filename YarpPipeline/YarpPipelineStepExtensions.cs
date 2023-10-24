// <copyright file="YarpPipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment;

/// <summary>
/// Extensions for <see cref="PipelineStep{YarpPipelineState}"/>.
/// </summary>
public static class YarpPipelineStepExtensions
{
    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<YarpPipelineState> OnError(
        this PipelineStep<YarpPipelineState> step,
        PipelineStep<YarpPipelineState> onError)
    {
        return PipelineStepExtensions.OnError<YarpPipelineState, YarpPipelineError>(step, onError);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<YarpPipelineState> OnError(
        this PipelineStep<YarpPipelineState> step,
        SyncPipelineStep<YarpPipelineState> onError)
    {
        return PipelineStepExtensions.OnError<YarpPipelineState, YarpPipelineError>(step, onError);
    }
}