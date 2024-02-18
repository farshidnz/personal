using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Extensions
{
    public static class ObjectExtension
    {

        public static T GetPropertyValue<T>(this object source, string propertyName)
        {
            if (source.GetType().GetProperty(propertyName) != null)
                return (T)source.GetType().GetProperty(propertyName).GetValue(source);
            else
                return default(T);
        }
    }
}
