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
    internal class ImporterConverter : TypeConverter
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
            return PipelineTypes.ImportersStandardValuesCollection;
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
                string str = value as string;

                foreach (ImporterTypeDescription i in PipelineTypes.Importers)
                {
                    if (i.DisplayName.Equals(str))
                        return i;
                }
                
                if (string.IsNullOrEmpty(str))
                    return PipelineTypes.NullImporter;
                else
                    return PipelineTypes.MissingImporter;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ImporterTypeDescription importer = (ImporterTypeDescription)value; // contentItem.Importer;
                //System.Diagnostics.Debug.Assert(importer == value);

                if (importer == PipelineTypes.MissingImporter)
                {
                    ContentItem contentItem = (ContentItem)context.Instance;
                    return string.Format("[missing] {0}", contentItem.ImporterName ?? "[null]");
                }

                return importer.DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
