// Copyright (C)2024 Nick Kastellanos

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Design
{
    public class ColorConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Color color = (Color)value;

            if (destinationType == typeof(string))
            {
                var terms = new string[4];
                terms[0] = color.R.ToString("R", culture);
                terms[1] = color.G.ToString("R", culture);
                terms[2] = color.B.ToString("R", culture);
                terms[3] = color.A.ToString("R", culture);

                return string.Join(culture.TextInfo.ListSeparator + " ", terms);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Type sourceType = value.GetType();
            Color color = default(Color);

            if (sourceType == typeof(string))
            {
                string str = (string)value;
                string[] values = str.Split(culture.TextInfo.ListSeparator.ToCharArray());

                color.R = (byte)int.Parse(values[0], culture);
                color.G = (byte)int.Parse(values[1], culture);
                color.B = (byte)int.Parse(values[2], culture);
                color.A = (byte)int.Parse(values[3], culture);

                return color;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
    