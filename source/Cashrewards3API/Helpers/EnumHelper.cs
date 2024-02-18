using System.ComponentModel;
using System.Reflection;

namespace Cashrewards3API.Helpers
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<TEnum>(this TEnum enumValue)
            where TEnum : struct
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            
            if (fi == null)
            {
                return enumValue.ToString();
            }
            DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(
            typeof(DescriptionAttribute),
            false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return enumValue.ToString();
        }

    }
}