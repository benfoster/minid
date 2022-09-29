using System.ComponentModel;
using System.Globalization;

namespace Minid;

public class IdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is null || !Id.TryParse(value as string, out Id id))
        {
            return default;
        }
        
        return id;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context,
        CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is not null)
            return ((Id)value).ToString();

        return default;
    }
}