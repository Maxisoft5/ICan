using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ICan.Common.Utils
{
	public static class EnumExtensions
	{
		public static string GetDisplayName(this Enum enumValue)
		{
			return enumValue.GetType()
							.GetMember(enumValue.ToString())
							.First()
							.GetCustomAttribute<DisplayAttribute>()
							.GetName();
		}

		public static T GetValueFromName<T>(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentOutOfRangeException(nameof(name));
			name = name.ToLower();
			var type = typeof(T);
			if (!type.IsEnum) throw new InvalidOperationException();

			foreach (var field in type.GetFields())
			{
				var attribute = Attribute.GetCustomAttribute(field,
					typeof(DisplayAttribute)) as DisplayAttribute;
				if (attribute != null)
				{
					if (attribute.Name.ToLower().Equals(name))
					{
						return (T)field.GetValue(null);
					}
				}
				else
				{
					if (field.Name.ToLower().Equals(name))
						return (T)field.GetValue(null);
				}
			}

			throw new ArgumentOutOfRangeException(nameof(name));
		}
	}
}
