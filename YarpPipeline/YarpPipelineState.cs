// <copyright file="YarpPipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Yarp.ReverseProxy.Transforms;

namespace PipelineExperiment;

/// <summary>
/// The state for processing a YARP transform.
/// </summary>
/// <remarks>
/// The steps in the pipe can inspect and modify the <see cref="RequestTransformContext"/>,
/// then choose to either <see cref="Continue()"/> processing, <see cref="TerminateAndForward()"/> - passing
/// the <see cref="RequestTransformContext"/> on to YARP for forwarding to the appropriate endpoint, or
/// <see cref="TerminateWith(NonForwardedResponseDetails)"/> a specific response code, headers and/or body.
/// </remarks>
public readonly struct YarpPipelineState
{
    private readonly NonForwardedResponseDetails responseDetails;
    private readonly TransformState pipelineState;

    private YarpPipelineState(RequestTransformContext requestTransformContext, NonForwardedResponseDetails responseDetails, TransformState pipelineState)
    {
        this.RequestTransformContext = requestTransformContext;
        this.responseDetails = responseDetails;
        this.pipelineState = pipelineState;
    }

    private enum TransformState
    {
        Continue,
        Terminate,
        TerminateAndForward,
    }

    /// <summary>
    /// Gets the <see cref="RequestTransformContext"/> for the current request.
    /// </summary>
    public RequestTransformContext RequestTransformContext { get; }

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="YarpPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.pipelineState != TransformState.Continue;

    /// <summary>
    /// Gets an instance of the <see cref="YarpPipelineState"/> for a particular
    /// <see cref="RequestTransformContext"/>.
    /// </summary>
    /// <param name="requestTransformContext">The <see cref="RequestTransformContext"/> with which to
    /// initialize the <see cref="YarpPipelineState"/>.</param>
    /// <returns>The initialized instance.</returns>
    public static YarpPipelineState For(RequestTransformContext requestTransformContext)
    {
        return new(requestTransformContext, default, TransformState.Continue);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance which will terminate the pipeline
    /// with the given response details. The request will not be forwarded to the endpoint.
    /// </summary>
    /// <param name="responseDetails">The details of the response to return.</param>
    /// <returns>The terminating <see cref="YarpPipelineState"/>.</returns>
    public YarpPipelineState TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        return new(this.RequestTransformContext, responseDetails, TransformState.Terminate);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance which will terminate the pipeline
    /// and allow the request to be forwarded to the appropriate endpoint.
    /// </summary>
    /// <returns>The terminating <see cref="YarpPipelineState"/>.</returns>
    public YarpPipelineState TerminateAndForward()
    {
        return new(this.RequestTransformContext, default, TransformState.TerminateAndForward);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance that will continue processing the pipeline.
    /// </summary>
    /// <returns>The non-terminating <see cref="YarpPipelineState"/>.</returns>
    /// <remarks>
    /// <para>
    /// Note that if this is the last step in the pipeline, it will allow the request to be forwarded to
    /// the appropriate endpoint.
    /// </para>
    /// </remarks>
    public YarpPipelineState Continue()
    {
        return new(this.RequestTransformContext, default, TransformState.Continue);
    }

    /// <summary>
    /// Determines whether the result should be forwarded through YARP,
    /// or whether we should build a local response using the resulting response details.
    /// </summary>
    /// <param name="responseDetails">The response details to use if the result should not be forwarded.</param>
    /// <returns><see langword="true"/> if the result should be forwarded. If <see langword="false"/> then
    /// the <paramref name="responseDetails"/> will be set an can be used to build a local response.</returns>
    public bool ShouldForward(out NonForwardedResponseDetails responseDetails)
    {
        if (this.pipelineState == TransformState.Continue || this.pipelineState == TransformState.TerminateAndForward)
        {
            responseDetails = default;
            return true;
        }

        responseDetails = this.responseDetails;
        return false;
    }
}