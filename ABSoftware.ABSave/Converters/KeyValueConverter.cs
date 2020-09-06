﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ABSoftware.ABSave.Converters
{
    public class KeyValueConverter : ABSaveTypeConverter
    {
        public static readonly KeyValueConverter Instance = new KeyValueConverter();

        private KeyValueConverter() { }

        public override bool HasExactType => false;
        public override bool CheckCanConvertType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

        public override void Serialize(object obj, Type type, ABSaveWriter writer)
        {
            var genericArgs = type.GetGenericArguments();
            var keySpecifiedType = genericArgs[0];
            var valueSpecifiedType = genericArgs[1];

            dynamic pair = (dynamic)obj;

            var keyVal = pair.Key;
            ABSaveItemConverter.SerializeWithAttribute(keyVal, keySpecifiedType, writer);

            var valueVal = pair.Value;
            ABSaveItemConverter.SerializeWithAttribute(valueVal, valueSpecifiedType, writer);
        }

        public override object Deserialize(Type type, ABSaveReader reader)
        {
            var keySpecifiedType = type.GetGenericArguments()[0];
            var valueSpecifiedType = type.GetGenericArguments()[1];

            return Activator.CreateInstance(type, ABSaveItemConverter.DeserializeWithAttribute(keySpecifiedType, reader), ABSaveItemConverter.DeserializeWithAttribute(valueSpecifiedType, reader));
        }
    }
}