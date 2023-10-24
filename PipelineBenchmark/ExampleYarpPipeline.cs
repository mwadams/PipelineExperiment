// <copyright file="ExampleYarpPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using PipelineExperiment;
using PipelineExperiment.Handlers;

namespace PipelineExample;

/// <summary>
/// A sample YARP pipeline illustrating a smorgasbord of capabilities.
/// </summary>
public static class ExampleYarpPipeline
{
    private static readonly PipelineStep<YarpPipelineState> InnerPipelineInstance =
    YarpPipeline.Build(
        static state => state.RequestTransformContext.Path == "/fizz"
                    ? state.TerminateAndForward()
                    : state.Continue(),
        static state => state.RequestTransformContext.Path == "/buzz"
                    ? state.TerminateAndForward()
                    : state.Continue());

    private static readonly PipelineStep<HandlerState<PathString, string?>> MessageHandlerPipelineInstance =
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
                wrap: static (YarpPipelineState state) => HandlerState<PathString, string?>.For(state.RequestTransformContext.Path),
                unwrap: static (state, innerState) =>
                {
                    if (innerState.WasHandled(out string? message))
                    {
                        if (message is string msg)
                        {
                            state.RequestTransformContext.HttpContext.Items["Message"] = msg;
                            return state.Continue();
                        }
                        else
                        {
                            return state.TerminateWith(new(400));
                        }
                    }

                    return state.Continue();
                });

    private static readonly Func<YarpPipelineState, PipelineStep<YarpPipelineState>> ChooseMessageContextHandler =
            static state => state.RequestTransformContext.Query.QueryString.HasValue
                                ? state => ValueTask.FromResult(state.TerminateWith(new(400)))
                                : AddMessageToHttpContext;

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> Instance { get; } =
        YarpPipeline.Build(
            static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? ValueTask.FromResult(state.TerminateAndForward())
                : InnerPipelineInstance(state),
            YarpPipeline.Current.Choose(ChooseMessageContextHandler), // But we prefer this style where we hide away the state
            static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(new(404))));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> ForceAsyncInstance { get; } =
        YarpPipeline.Build(
            static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? ValueTask.FromResult(state.TerminateAndForward())
                : InnerPipelineInstance(state),
            async state =>
            {
                await Task.Delay(0).ConfigureAwait(false);
                return state.Continue();
            },
            YarpPipeline.Current.Choose(ChooseMessageContextHandler), // But we prefer this style where we hide away the state
            static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(new(404))));
}