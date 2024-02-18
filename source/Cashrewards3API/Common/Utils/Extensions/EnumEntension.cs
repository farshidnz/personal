using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Cashrewards3API.Common.Utils.Extensions
{
    public static class EnumEntension
    {

        public static string GetDescription(this System.Enum enumValue)
        {
            return enumValue.GetType()
                       .GetMember(enumValue.ToString())
                       .First()
                       .GetCustomAttribute<DescriptionAttribute>()?
                       .Description ?? string.Empty;
        }
    }
}
