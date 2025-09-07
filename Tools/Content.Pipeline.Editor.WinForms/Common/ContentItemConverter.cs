// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Content.Pipeline.Editor
{
    public class ContentItemConverter : TypeConverter
    {
        private static Attribute[] CategoryProcessorParams = new Attribute[] { new CategoryAttribute("3.ProcessorParams") };

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(value, attributes, true))
                props.Add(prop);

            ContentItem contentItem = value as ContentItem;

            if (contentItem.Processor == PipelineTypes.MissingProcessor)
            {
                foreach (var p in contentItem.ProcessorParams)
                {
                    PropertyDescriptor desc = new ReadonlyProcessorParamsPropertyDescriptor(p.Key,
                                                                                            p.Value.GetType(),
                                                                                            CategoryProcessorParams);
                    props.Add(desc);
                }
            }
            else
            {
                foreach (var p in contentItem.Processor.Properties)
                {
                    PropertyDescriptor desc = new ProcessorParamsPropertyDescriptor(p.Name,
                                                                                    p.Type,
                                                                                    CategoryProcessorParams);
                    props.Add(desc);
                } 
            }

            return props;
        }
    }

    public class ProcessorParamsPropertyDescriptor : PropertyDescriptor
    {
        private static Type _componentType = typeof(ContentItem);

        private readonly Type _propertyType;

        public ProcessorParamsPropertyDescriptor(string propertyName, Type propertyType, Attribute[] attrs)
            : base(propertyName, attrs)
        {
            _propertyType = propertyType;
        }

        public override object GetValue(object component)
        {
            ContentItem contentItem = component as ContentItem;

            if (contentItem.ProcessorParams.ContainsKey(base.Name))
                return contentItem.ProcessorParams[base.Name];

            if (contentItem.Processor.Properties.Contains(base.Name))
                return contentItem.Processor.Properties[base.Name].DefaultValue;

            return string.Empty;
        }

        public override void SetValue(object component, object value)
        {
            ContentItem contentItem = component as ContentItem;

            contentItem.ProcessorParams[base.Name] = value;
        }

        public override void ResetValue(object component)
        {
            SetValue(component, null);
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return _componentType; } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return _propertyType; } }
        public override bool ShouldSerializeValue(object component) { return true; }
    }

    public class ReadonlyProcessorParamsPropertyDescriptor : ProcessorParamsPropertyDescriptor
    {

        public ReadonlyProcessorParamsPropertyDescriptor(string propertyName, Type propertyType, Attribute[] attrs)
            : base(propertyName, propertyType, attrs)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override void ResetValue(object component)
        {
        }

        public override bool CanResetValue(object component) { return false; }
        public override bool IsReadOnly { get { return true; } }
    }
}