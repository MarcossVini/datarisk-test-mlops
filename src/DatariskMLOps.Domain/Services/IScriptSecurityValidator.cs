namespace DatariskMLOps.Domain.Services;

public class ScriptValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public SecurityRisk RiskLevel { get; set; }
    public List<string> Warnings { get; set; } = new();

    public static ScriptValidationResult Valid() => new() { IsValid = true, RiskLevel = SecurityRisk.Low };
    public static ScriptValidationResult Invalid(string error, SecurityRisk risk = SecurityRisk.High)
        => new() { IsValid = false, ErrorMessage = error, RiskLevel = risk };
}

public enum SecurityRisk
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public interface IScriptSecurityValidator
{
    Task<ScriptValidationResult> ValidateScriptAsync(string script);
    Task<ScriptValidationResult> ValidateScriptComplexityAsync(string script);
}

public class ScriptSecurityValidator : IScriptSecurityValidator
{
    private readonly string[] _forbiddenPatterns = {
        "while(true)", "for(;;)", "setInterval", "setTimeout",
        "eval(", "Function(", "require(", "import(",
        "process.", "global.", "__dirname", "__filename",
        "Buffer.", "fs.", "path.", "os.", "crypto.",
        "XMLHttpRequest", "fetch(", "localStorage", "sessionStorage"
    };

    private readonly string[] _suspiciousPatterns = {
        "Math.random", "Date.now", "new Date()",
        "JSON.stringify", "JSON.parse", "Object.keys"
    };

    public async Task<ScriptValidationResult> ValidateScriptAsync(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
            return ScriptValidationResult.Invalid("Script cannot be empty");

        var result = new ScriptValidationResult { IsValid = true, RiskLevel = SecurityRisk.Low };

        // Check forbidden patterns
        foreach (var pattern in _forbiddenPatterns)
        {
            if (script.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptValidationResult.Invalid($"Forbidden pattern detected: {pattern}", SecurityRisk.Critical);
            }
        }

        // Check suspicious patterns
        foreach (var pattern in _suspiciousPatterns)
        {
            if (script.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.Warnings.Add($"Suspicious pattern detected: {pattern}");
                result.RiskLevel = SecurityRisk.Medium;
            }
        }

        // Check for infinite loops
        if (HasPotentialInfiniteLoop(script))
        {
            return ScriptValidationResult.Invalid("Potential infinite loop detected", SecurityRisk.High);
        }

        // Validate complexity
        var complexityResult = await ValidateScriptComplexityAsync(script);
        if (!complexityResult.IsValid)
            return complexityResult;

        return result;
    }

    public async Task<ScriptValidationResult> ValidateScriptComplexityAsync(string script)
    {
        await Task.CompletedTask; // For async interface

        var lines = script.Split('\n');
        if (lines.Length > 1000)
            return ScriptValidationResult.Invalid("Script too long (max 1000 lines)", SecurityRisk.High);

        var nestedLoops = CountNestedLoops(script);
        if (nestedLoops > 3)
            return ScriptValidationResult.Invalid("Too many nested loops (max 3)", SecurityRisk.High);

        var functionCount = CountFunctions(script);
        if (functionCount > 10)
            return ScriptValidationResult.Invalid("Too many functions (max 10)", SecurityRisk.Medium);

        return ScriptValidationResult.Valid();
    }

    private bool HasPotentialInfiniteLoop(string script)
    {
        // Simple heuristic - look for loops without clear termination
        var whileMatches = System.Text.RegularExpressions.Regex.Matches(script, @"while\s*\(",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var forMatches = System.Text.RegularExpressions.Regex.Matches(script, @"for\s*\(",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return whileMatches.Count > 5 || forMatches.Count > 10;
    }

    private int CountNestedLoops(string script)
    {
        int maxNesting = 0;
        int currentNesting = 0;

        var tokens = System.Text.RegularExpressions.Regex.Matches(script,
            @"\b(for|while)\s*\(|\{|\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match token in tokens)
        {
            if (token.Value.StartsWith("for") || token.Value.StartsWith("while"))
                continue;
            else if (token.Value == "{")
                currentNesting++;
            else if (token.Value == "}")
            {
                maxNesting = Math.Max(maxNesting, currentNesting);
                currentNesting = Math.Max(0, currentNesting - 1);
            }
        }

        return maxNesting;
    }

    private int CountFunctions(string script)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(script,
            @"function\s+\w+\s*\(", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return matches.Count;
    }
}
