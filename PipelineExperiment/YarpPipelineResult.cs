namespace PipelineExperiment;

using System.Diagnostics.CodeAnalysis;
using Yarp.ReverseProxy.Transforms;

using YarpPipelineStep = Pipelines.PipelineStep<YarpProcessingContext, YarpProcessingContext>;

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

    internal static YarpPipelineStep TerminateWith(RequestTransformContext context, NonForwardedResponseDetails responseDetails)
    {
        return YarpPipelineStep.FromResult(
            new YarpProcessingContext(
                context,
                new(responseDetails, TransformState.Terminate)));
    }

    internal static YarpPipelineStep TerminateAndForward(RequestTransformContext context)
    {
        return YarpPipelineStep.FromResult(
            new YarpProcessingContext(
                context,
                new(default, TransformState.TerminateAndForward)));
    }

    internal static YarpPipelineStep Continue(RequestTransformContext context)
    {
        return YarpPipelineStep.FromResult(
            new YarpProcessingContext(
                context,
                new(default, TransformState.Continue)));
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
    public bool ShouldForward([NotNullWhen(false)] out NonForwardedResponseDetails? responseDetails)
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
