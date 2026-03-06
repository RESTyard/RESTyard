namespace RESTyard.Generator.Test;

public record ActionDisposable(Action OnDispose) : IDisposable
{
    public void Dispose() => OnDispose();
}