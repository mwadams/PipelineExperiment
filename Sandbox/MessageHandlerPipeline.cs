namespace Sandbox;

using Microsoft.AspNetCore.Http;
using PipelineExperiment.Handlers;

using MessageHandlerPipelineStep = PipelineExperiment.Pipelines.PipelineStep<Microsoft.AspNetCore.Http.PathString, PipelineExperiment.Handlers.HandlerResult<string?>>;

// Strongly typed version of a handler pipeline, in this case finding someone who can return a string? from a request transform context.
public static class MessageHandlerPipeline
{
    public static MessageHandlerPipelineStep Build(params Func<PathString, MessageHandlerPipelineStep>[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep Build(params Func<PathString, ValueTask<MessageHandlerPipelineStep>>[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep Build(params MessageHandlerPipelineStep[] steps) =>
        HandlerPipeline.Build(steps);

    public static MessageHandlerPipelineStep MakeStep(
        Func<PathString, MessageHandlerPipelineStep> step)
    {
        return MessageHandlerPipelineStep.MakeStep(input => ValueTask.FromResult(step(input)));
    }

    public static MessageHandlerPipelineStep MakeStep(
        Func<PathString, ValueTask<MessageHandlerPipelineStep>> step)
    {
        return MessageHandlerPipelineStep.MakeStep(step);
    }

    public static MessageHandlerPipelineStep Handled(string? result) =>
        HandlerPipeline.Handled<PathString, string?>(result);

    public static MessageHandlerPipelineStep NotHandled() =>
        HandlerPipeline.NotHandled<PathString, string?>();
}