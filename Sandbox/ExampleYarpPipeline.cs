namespace Sandbox;

using Microsoft.AspNetCore.Http;
using PipelineExperiment;
using PipelineExperiment.Handlers;

public static class ExampleYarpPipeline
{
    private static PipelineStep<YarpPipelineState> InnerPipelineInstance { get; } =
    YarpPipeline.Build(
        static state => state.RequestTransformContext.Path == "/fizz"
                    ? state.TerminateAndForward()
                    : state.Continue(),
        static state => state.RequestTransformContext.Path == "/buzz"
                    ? state.TerminateAndForward()
                    : state.Continue()
        );

    private static PipelineStep<HandlerState<PathString, string?>> MessageHandlerPipelineInstance { get; } =
        HandlerPipeline.Build<PathString, string?>(
            static state => state.Input == "/foo"
                        ? state.Handled("We're looking at a foo")
                        : state.NotHandled(),
            static state => state.Input == "/bar"
                        ? state.Handled(null)
                        : state.NotHandled());

    private static readonly PipelineStep<YarpPipelineState> AddMessageToHttpContext =
        MessageHandlerPipelineInstance
            .Bind(
                wrap: static (YarpPipelineState inputState) => HandlerState<PathString, string?>.For(inputState.RequestTransformContext.Path),
                unwrap: static async (inputState, outputState) =>
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    if (outputState.WasHandled(out string? message))
                    {
                        if (message is string msg)
                        {
                            inputState.RequestTransformContext.HttpContext.Items["Message"] = msg;
                            return inputState.Continue();
                        }
                        else
                        {
                            return inputState.TerminateWith(new(400));
                        }
                    }

                    return inputState.Continue();
                });

    private static readonly Func<YarpPipelineState, PipelineStep<YarpPipelineState>> ChooseMessageContextHandler =
            static state => state.RequestTransformContext.Query.QueryString.HasValue
                                ? state => ValueTask.FromResult(state.TerminateWith(new(400)))
                                : AddMessageToHttpContext;

    public static PipelineStep<YarpPipelineState> Instance { get; } =
        YarpPipeline.Build(
            static state => state.RequestTransformContext.Path == "/"
                ? ValueTask.FromResult(state.TerminateAndForward())
                : InnerPipelineInstance(state),
            YarpPipeline.Current.Choose(ChooseMessageContextHandler),
            static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(new(404))));
}