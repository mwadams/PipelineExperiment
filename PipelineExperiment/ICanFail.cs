﻿// <copyright file="ICanFail.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PipelineExperiment;

/// <summary>
/// An interface implemented by state for steps that can fail.
/// </summary>
/// <typeparam name="TState">The type that implements the interface.</typeparam>
/// <typeparam name="TError">The type of the error details.</typeparam>
/// <remarks>
/// This is used by retry operators to determine whether a failure is transient or permanent.
/// </remarks>
public interface ICanFail<TState, TError>
    where TState : struct, ICanFail<TState, TError>
    where TError : struct
{
    /// <summary>
    /// Gets the operation status.
    /// </summary>
    PipelineStepStatus ExecutionStatus { get; }

    /// <summary>
    /// Try to get the error details from the state.
    /// </summary>
    /// <param name="errorDetails">The error details, if the execution status does not indicate success.</param>
    /// <returns><see langword="true"/> if the execution status does not indicate success.</returns>
    bool TryGetErrorDetails([NotNullWhen(true)] TError errorDetails);

    /// <summary>
    /// Returns the state with the execution status set to
    /// <see cref="PipelineStepStatus.PermanentFailure"/>.
    /// </summary>
    /// <param name="errorDetails">The details of the error.</param>
    /// <returns>The updated state.</returns>
    TState PermanentFailure(TError errorDetails);

    /// <summary>
    /// Returns the state with the execution status set to
    /// <see cref="PipelineStepStatus.TransientFailure"/>.
    /// </summary>
    /// <param name="errorDetails">The details of the error.</param>
    /// <returns>The updated state.</returns>
    TState TransientFailure(TError errorDetails);

    /// <summary>
    /// Returns the state with the execution status set to
    /// <see cref="PipelineStepStatus.Success"/>.
    /// </summary>
    /// <returns>The updated state.</returns>
    TState Success();
}