// <copyright file="ILoggable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace PipelineExperiment;

/// <summary>
/// State that supports logging.
/// </summary>
/// <typeparam name="TState">The type implementing the interface.</typeparam>
public interface ILoggable<TState>
    where TState : ILoggable<TState>
{
    /// <summary>
    /// Gets the logger for the state.
    /// </summary>
    ILogger<TState> Logger { get; }
}