namespace PipelineExperiment.Handlers;

using System.Diagnostics.CodeAnalysis;

public readonly struct HandlerState<TInput, TResult>
{
    private readonly TResult? result;
    private readonly bool handled;

    private HandlerState(TInput input, TResult? result, bool handled)
    {
        this.result = result;
        this.handled = handled;
        Input = input;
    }

    internal bool Terminate => handled;

    public static HandlerState<TInput, TResult> For(TInput input)
    {
        return new(input, default, false);
    }

    public HandlerState<TInput, TResult> Handled(TResult result)
    {
        return new(Input, result, true);
    }

    public HandlerState<TInput, TResult> NotHandled()
    {
        return this;
    }

    public TInput Input { get; }

    public bool WasHandled([MaybeNullWhen(false)] out TResult? result)
    {
        result = this.result;
        return handled;
    }
}