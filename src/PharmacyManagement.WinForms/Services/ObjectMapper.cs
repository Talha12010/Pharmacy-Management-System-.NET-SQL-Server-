using System.Reflection;

namespace PharmacyManagement.WinForms.Services;

public static class ObjectMapper
{
    public static TDest Copy<TSource, TDest>(TSource source) where TDest : new()
    {
        var destination = new TDest();
        CopyInto(source, destination);
        return destination;
    }

    public static void CopyInto<TSource, TDest>(TSource source, TDest destination)
    {
        if (source is null || destination is null)
        {
            return;
        }

        var destProperties = typeof(TDest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destProperties.TryGetValue(sourceProperty.Name, out var destProperty))
            {
                continue;
            }

            var value = sourceProperty.GetValue(source);
            if (value is null && Nullable.GetUnderlyingType(destProperty.PropertyType) is null && destProperty.PropertyType.IsValueType)
            {
                continue;
            }

            if (destProperty.PropertyType.IsInstanceOfType(value))
            {
                destProperty.SetValue(destination, value);
            }
            else if (value is IEnumerable<string> stringEnumerable && destProperty.PropertyType == typeof(List<string>))
            {
                destProperty.SetValue(destination, stringEnumerable.ToList());
            }
            else if (value is not null && destProperty.PropertyType.IsEnum && value is string text && Enum.TryParse(destProperty.PropertyType, text, true, out var enumValue))
            {
                destProperty.SetValue(destination, enumValue);
            }
            else if (value is not null)
            {
                var targetType = Nullable.GetUnderlyingType(destProperty.PropertyType) ?? destProperty.PropertyType;
                try
                {
                    var converted = Convert.ChangeType(value, targetType);
                    destProperty.SetValue(destination, converted);
                }
                catch
                {
                    // Ignore conversion failures
                }
            }
        }
    }
}
