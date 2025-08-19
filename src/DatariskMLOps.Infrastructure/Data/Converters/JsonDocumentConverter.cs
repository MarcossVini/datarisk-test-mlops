using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DatariskMLOps.Infrastructure.Data.Converters;

public class JsonDocumentConverter : ValueConverter<JsonDocument, string>
{
    public JsonDocumentConverter() : base(
        v => v.RootElement.GetRawText(),
        v => JsonDocument.Parse(v, new JsonDocumentOptions { AllowTrailingCommas = true }))
    {
    }
}

public class NullableJsonDocumentConverter : ValueConverter<JsonDocument?, string?>
{
    public NullableJsonDocumentConverter() : base(
        v => v != null ? v.RootElement.GetRawText() : null,
        v => v != null ? JsonDocument.Parse(v, new JsonDocumentOptions { AllowTrailingCommas = true }) : null)
    {
    }
}
