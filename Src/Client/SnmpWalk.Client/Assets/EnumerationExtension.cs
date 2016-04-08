using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace SnmpWalk.Client.Assets
{
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumerationExtension(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }

            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                {
                    return;
                }

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (!enumType.IsEnum)
                {
                    throw new ArgumentException("Type must be Enum.");
                }

                _enumType = value;
            }
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute =
                EnumType.GetField(enumValue.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);

            return (from object enumValue in enumValues
                    select new EnumerationMember
                    {
                        Value = enumValue,
                        Description = Resources.StringResources.ResourceManager.GetString(GetDescription(enumValue)),
                        Key = GetDescription(enumValue)
                    }).ToArray();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public string Key { get; set; }
            public object Value { get; set; }
        }
    }
}