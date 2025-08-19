namespace DatariskMLOps.Domain.Interfaces;

public interface IJavaScriptEngine
{
    Task<object> ExecuteAsync(string script, object inputData, CancellationToken cancellationToken = default);
}
