namespace PipelineExperiment.Handlers;

using System.Diagnostics.CodeAnalysis;

public readonly struct HandlerResult<T>
{
    private readonly T? result;
    private readonly bool handled;

    internal HandlerResult(T? result, bool handled)
    {
        this.result = result;
        this.handled = handled;
    }

    internal bool Terminate => handled;

    public bool WasHandled([MaybeNullWhen(false)] out T? result)
    {
        result = this.result;
        return handled;
    }
}