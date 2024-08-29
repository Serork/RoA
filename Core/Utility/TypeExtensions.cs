using System;
using System.Reflection;

namespace RoA.Common.Utilities.Extensions;

static class TypeExtensions {	
	public static object GetFieldValue(this Type type, string fieldName, object obj = null, BindingFlags? flags = null) {
		if (!flags.HasValue) {
			flags = new BindingFlags?(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		return type.GetField(fieldName, flags.Value).GetValue(obj);
	}

	public static T GetFieldValue<T>(this Type type, string fieldName, object obj = null, BindingFlags? flags = null) {
		if (!flags.HasValue) {
			flags = new BindingFlags?(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		return (T)type.GetField(fieldName, flags.Value).GetValue(obj);
	}
}
