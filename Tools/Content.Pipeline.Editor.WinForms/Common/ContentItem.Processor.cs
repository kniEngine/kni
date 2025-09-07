// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Content.Pipeline.Editor
{
    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    internal class ProcessorConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            if (context.Instance is Array)
            {
                Array array = context.Instance as Array;
                foreach (var obj in array)
                {
                    ContentItem item = obj as ContentItem;
                    if (item.BuildAction == BuildAction.Copy)
                        return false;
                }
            }
            else
            {
                ContentItem contentItem = context.Instance as ContentItem;
                if (contentItem.BuildAction == BuildAction.Copy)
                    return false;
            }                
                        
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return PipelineTypes.ProcessorsStandardValuesCollection;            
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                foreach (ProcessorTypeDescription i in PipelineTypes.Processors)
                {
                    if (i.DisplayName.Equals(value))
                        return i;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ProcessorTypeDescription processor = (ProcessorTypeDescription)value;

                if (processor == PipelineTypes.MissingProcessor)
                {
                    ContentItem contentItem = context.Instance as ContentItem;
                    return string.Format("[missing] {0}", contentItem.ProcessorName);
                }

                return processor.DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
