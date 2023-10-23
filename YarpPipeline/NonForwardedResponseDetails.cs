// <copyright file="NonForwardedResponseDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PipelineExperiment;

/// <summary>
/// The information required to build a local response for a non-forwarded pipeline result.
/// </summary>
/// <remarks><see cref="YarpPipelineState.ShouldForward(out PipelineExperiment.NonForwardedResponseDetails)"/> for more details.</remarks>
public readonly record struct NonForwardedResponseDetails(int StatusCode); // We will expand this for real use.