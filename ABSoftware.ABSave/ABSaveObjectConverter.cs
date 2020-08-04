﻿using ABSoftware.ABSave.Helpers;
using ABSoftware.ABSave.Mapping;
using ABSoftware.ABSave.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ABSoftware.ABSave
{
    public static class ABSaveObjectConverter
    {
        public static void Serialize(object obj, TypeInformation typeInformation, ABSaveWriter writer)
        {
            var info = GetObjectMemberInfos(typeInformation.ActualType, writer.Settings);

            writer.WriteInt32((uint)info.Length);
            for (int i = 0; i < info.Length; i++)
            {
                writer.WriteText(info[i].Name);
                AutoSerializeValue(obj, info[i], writer);
            }
        }

        public static void Serialize(object obj, TypeInformation typeInformation, ABSaveWriter writer, ObjectMapItem item)
        {
            writer.WriteInt32((uint)item.NumberOfItems);
            
            for (int i = 0; i < item.NumberOfItems; i++)
            {
                writer.WriteText(item.Items[i].Name);

                if (item.Items[i].UseReflection)
                    AutoSerializeValue(obj, typeInformation.ActualType.GetField(item.Items[i].Name, writer.Settings.MemberReflectionFlags), writer);
                else
                {
                    var objVal = item.Items[i].Getter(obj);
                    var actualType = objVal.GetType();
                    var specifiedType = item.Items[i].FieldType;

                    ABSaveItemSerializer.Serialize(objVal, new TypeInformation(actualType, Type.GetTypeCode(actualType), specifiedType, Type.GetTypeCode(specifiedType)), writer, item.Items[i]);
                }
            }
        }

        static void AutoSerializeValue(object obj, FieldInfo item, ABSaveWriter writer)
        {
            var val = item.GetValue(obj);
            var actualType = val.GetType();
            var specifiedType = item.FieldType;

            ABSaveItemSerializer.Serialize(val, new TypeInformation(actualType, Type.GetTypeCode(actualType), specifiedType, Type.GetTypeCode(specifiedType)), writer);
        }

        public static FieldInfo[] GetObjectMemberInfos(Type typ, ABSaveSettings settings) => typ.GetFields(settings.MemberReflectionFlags);
    }
}
