using System.ComponentModel;
using System.Reflection;

namespace SocketTest.Common;

public static class EnumExtensions
{
    public static string Description(this Enum value)
    {
        var enumType = value.GetType();
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var descriptions = new List<string>();

        foreach (var field in fields)
        {
            var enumValue = (Enum)Enum.Parse(enumType, field.Name);
            if (!value.HasFlag(enumValue)) continue;
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                descriptions.Add(attribute.Description);
            }
            else
            {
                descriptions.Add(field.Name);
            }
        }

        // 你可以选择以不同的方式组合这些描述，这里使用逗号加空格连接  
        return string.Join(", ", descriptions);
    }
}