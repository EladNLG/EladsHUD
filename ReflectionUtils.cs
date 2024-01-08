using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CustomHUD
{
    static class ReflectionUtils
    {
        public static T GetPrivateField<T>(this object obj, string field)
        {
            return (T)obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }
    }
}
