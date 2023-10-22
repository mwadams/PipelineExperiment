namespace PipelineExperiment;

using Yarp.ReverseProxy.Transforms;

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

    public static YarpPipelineState For(RequestTransformContext requestTransformContext)
    {
        return new(requestTransformContext, default, TransformState.Continue);
    }

    public YarpPipelineState TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        return new(this.RequestTransformContext, responseDetails, TransformState.Terminate);
    }

    public YarpPipelineState TerminateAndForward()
    {
        return new(this.RequestTransformContext, default, TransformState.TerminateAndForward);
    }

    public YarpPipelineState Continue()
    {
        return new(this.RequestTransformContext, default, TransformState.Continue);
    }

    /// <summary>
    /// Used by the terminating pipeline predicate to determine if we should stop processing
    /// the pipeline
    /// </summary>
    internal bool ShouldTerminatePipeline => pipelineState != TransformState.Continue;

    public RequestTransformContext RequestTransformContext { get; }

    /// <summary>
    /// Used by whoever executed the pipeline to determine whether we should forward the result,
    /// or build a local response using the resulting response details.
    /// </summary>
    public bool ShouldForward(out NonForwardedResponseDetails responseDetails)
    {
        if (pipelineState == TransformState.Continue || pipelineState == TransformState.TerminateAndForward)
        {
            responseDetails = default;
            return true;
        }

        responseDetails = this.responseDetails;
        return false;
    }

    private enum TransformState
    {
        Continue,
        Terminate,
        TerminateAndForward
    }
}
