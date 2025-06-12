using System;
using System.Linq;
using System.Reflection;

namespace RoA.Core.Utility;

static class ReflectionExtentions {
    public static bool HasMethodOverride(this Type t, string methodName, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public) {
        foreach (MethodInfo method in t.GetMethods(bindingFlags).Where(x => x.Name == methodName)) {
            if (method.GetBaseDefinition().DeclaringType != method.DeclaringType) {
                return true;
            }
        }

        return false;
    }
}
