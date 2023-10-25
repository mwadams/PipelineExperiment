// <copyright file="NamedSyncPipelineStep.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace PipelineExperiment;

/// <summary>
/// Represents a synchronous step with an explicit name and ID.
/// </summary>
/// <typeparam name="TState">The type of the step.</typeparam>
/// <param name="EventId">The Event Id for the step.</param>
/// <param name="Step">The instance of the step.</param>
public readonly record struct NamedSyncPipelineStep<TState>(in EventId EventId, in SyncPipelineStep<TState> Step)
    where TState : struct
{
    /// <summary>
    /// Conversion from tuple of <see cref="EventId"/> and <see cref="PipelineStep{TState}"/>.
    /// </summary>
    /// <param name="step">The tuple from which to convert.</param>
    public static implicit operator NamedSyncPipelineStep<TState>((EventId EventId, SyncPipelineStep<TState> Step) step)
    {
        return new(step.EventId, step.Step);
    }

    /// <summary>
    /// Conversion to tuple of <see cref="EventId"/> and <see cref="PipelineStep{TState}"/>.
    /// </summary>
    /// <param name="step">The <see cref="NamedPipelineStep{TState}"/> from which to create the tuple.</param>
    public static implicit operator (EventId EventId, SyncPipelineStep<TState> Step)(in NamedSyncPipelineStep<TState> step)
    {
        return (step.EventId, step.Step);
    }
}