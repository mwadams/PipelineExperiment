namespace Sandbox;

using PipelineExperiment.Handlers;
using Yarp.ReverseProxy.Transforms;

using MessageHandlerPipelineStep = PipelineExperiment.Pipelines.PipelineStep<Yarp.ReverseProxy.Transforms.RequestTransformContext, PipelineExperiment.Handlers.HandlerResult<string>>;

// Strongly typed version of a handler pipeline, in this case finding someone who can return a string from a request transform context.
public static class MessageHandlerPipeline
{
    public static MessageHandlerPipelineStep Build(params Func<RequestTransformContext, MessageHandlerPipelineStep>[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep Build(params Func<RequestTransformContext, ValueTask<MessageHandlerPipelineStep>>[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep Build(params MessageHandlerPipelineStep[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep MakeStep(
        Func<RequestTransformContext, MessageHandlerPipelineStep> step)
    {
        return new MessageHandlerPipelineStep(input => ValueTask.FromResult(step(input)));
    }

    public static MessageHandlerPipelineStep MakeStep(
        Func<RequestTransformContext, ValueTask<MessageHandlerPipelineStep>> step)
    {
        return new MessageHandlerPipelineStep(step);
    }

    public static MessageHandlerPipelineStep Handled(string result) =>
        HandlerPipeline.Handled<RequestTransformContext, string>(result);

    public static MessageHandlerPipelineStep NotHandled() =>
        HandlerPipeline.NotHandled<RequestTransformContext, string>();
}