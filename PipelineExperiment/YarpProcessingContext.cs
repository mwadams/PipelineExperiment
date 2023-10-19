namespace PipelineExperiment;

using Yarp.ReverseProxy.Transforms;

public readonly struct YarpProcessingContext
{
    internal YarpProcessingContext(RequestTransformContext transformContext, YarpPipelineResult stepResult)
    {
        RequestTransformContext = transformContext;
        StepResult = stepResult;
    }

    // Available to the steps
    public RequestTransformContext RequestTransformContext { get; }

    // Only accessible to the Yarp pipeline
    internal YarpPipelineResult StepResult { get; }
}
