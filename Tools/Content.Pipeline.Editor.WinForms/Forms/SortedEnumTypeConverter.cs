using System;
using System.ComponentModel;
using System.Globalization;

namespace Content.Pipeline.Editor
{
    class SortedEnumTypeConverter : EnumConverter
    {
        private readonly Type _enumType;
        private readonly StandardValuesCollection _values;

        public SortedEnumTypeConverter(Type type) : 
            base(type)
        {
            _enumType = this.EnumType;
            if (_enumType.IsGenericType
            &&  _enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
                _enumType = Nullable.GetUnderlyingType(_enumType);

            string[] values = Enum.GetNames(_enumType);
            Array.Sort(values);
            _values = new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return _values;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)
            ||  sourceType == _enumType)
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                if (value.GetType() == _enumType)
                    return value;

                if (value is string)
                    return Enum.Parse(_enumType, value as string, true);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    if (value is string)
                        return value;

                    return Enum.GetName(_enumType, value);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
