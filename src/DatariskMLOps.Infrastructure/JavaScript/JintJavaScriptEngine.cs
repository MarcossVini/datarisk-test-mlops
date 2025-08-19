using Jint;
using Jint.Runtime;
using DatariskMLOps.Domain.Interfaces;

namespace DatariskMLOps.Infrastructure.JavaScript;

public class JintJavaScriptEngine : IJavaScriptEngine
{
    public async Task<object> ExecuteAsync(string script, object inputData, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var engine = new Engine(options =>
            {
                options.TimeoutInterval(TimeSpan.FromMinutes(5));
                options.MaxStatements(10000);
                options.LimitRecursion(50);
                options.CancellationToken(cancellationToken);
            });

            try
            {
                // Disponibiliza apenas o que é necessário
                engine.SetValue("data", inputData);

                // Remove acesso a APIs perigosas
                engine.Global.Delete("require");
                engine.Global.Delete("import");
                engine.Global.Delete("eval");
                engine.Global.Delete("Function");

                // Executa o script como uma função que recebe data
                var result = engine.Evaluate($"({script})(data)");

                return result.ToObject() ?? new object();
            }
            catch (JavaScriptException ex)
            {
                throw new InvalidOperationException($"Script execution failed: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException("Script execution timeout", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error during script execution: {ex.Message}", ex);
            }
        }, cancellationToken);
    }
}
