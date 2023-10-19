namespace PipelineExperiment;

using YarpPipelineStep = Pipelines.PipelineStep<Yarp.ReverseProxy.Transforms.RequestTransformContext, YarpPipelineResult>;

// The result of a YARP processing pipeline step
public readonly struct YarpPipelineResult
{
    private readonly NonForwardedResponseDetails responseDetails;
    private readonly TransformState pipelineState;

    private YarpPipelineResult(NonForwardedResponseDetails responseDetails, TransformState pipelineState)
    {
        this.responseDetails = responseDetails;
        this.pipelineState = pipelineState;
    }

    internal static YarpPipelineStep TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        return YarpPipelineStep.FromResult(
            new(responseDetails, TransformState.Terminate));
    }

    internal static YarpPipelineStep TerminateAndForward()
    {
        return YarpPipelineStep.FromResult(
            new(default, TransformState.TerminateAndForward));
    }

    internal static YarpPipelineStep Continue()
    {
        return YarpPipelineStep.FromResult(
            new(default, TransformState.Continue));
    }

    /// <summary>
    /// Used by the terminating pipeline predicate to determine if we should stop processing
    /// the pipeline
    /// </summary>
    internal bool ShouldTerminatePipeline => pipelineState != TransformState.Continue;

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
