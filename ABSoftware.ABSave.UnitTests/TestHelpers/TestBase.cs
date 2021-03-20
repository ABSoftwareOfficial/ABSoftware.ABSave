﻿using ABSoftware.ABSave.Converters;
using ABSoftware.ABSave.Deserialization;
using ABSoftware.ABSave.Helpers;
using ABSoftware.ABSave.Mapping;
using ABSoftware.ABSave.Mapping.Generation;
using ABSoftware.ABSave.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.ABSave.UnitTests.TestHelpers
{
    public abstract class TestBase
    {
        public ABSaveMap CurrentMap;
        public MapGenerator CurrentGenerator;
        public MapItemInfo CurrentMapItem;

        public MemoryStream Stream;
        public ABSaveSerializer Serializer;
        public ABSaveDeserializer Deserializer;

        public void Initialize() => Initialize(ABSaveSettings.GetSpeedFocus(true));
        public void Initialize(ABSaveSettings template)
        {
            var settingsBuilder = new ABSaveSettingsBuilder
            {
                // Add "SubTypeConverter" as a converter.
                CustomConverters = new List<Converter>()
                {
                    new SubTypeConverter(false),
                    new SubTypeConverter(true)
                },
                BypassDangerousTypeChecking = true
            };

            CurrentMap = ABSaveMap.Get<object>(settingsBuilder.CreateSettings(template));

            CurrentGenerator = new MapGenerator();
            CurrentGenerator.Initialize(CurrentMap);

            Stream = new MemoryStream();
            Serializer = new ABSaveSerializer();
            Serializer.Initialize(Stream, CurrentMap);

            Deserializer = new ABSaveDeserializer();
            Deserializer.Initialize(Stream, CurrentMap);

            // NOTE: To make testing easier, add some test types as saved so they have a consistent single byte for type writing.
            Serializer.SavedTypes.Add(typeof(SubWithHeader), 0);
            Deserializer.SavedTypes.Add(typeof(SubWithHeader));

            Serializer.SavedTypes.Add(typeof(SubWithoutHeader), 1);
            Deserializer.SavedTypes.Add(typeof(SubWithoutHeader));

            Serializer.SavedTypes.Add(typeof(SubNoConverter), 2);
            Deserializer.SavedTypes.Add(typeof(SubNoConverter));

            Serializer.SavedTypes.Add(typeof(GeneralClass), 3);
            Deserializer.SavedTypes.Add(typeof(GeneralClass));
        }

        public void GoToStart() => Stream.Position = 0;

        public void ResetOutputWithConverter<T>(Converter converter) => ResetOutputWithConverter(typeof(T), converter);

        public void ResetOutputWithConverter(Type type, Converter converter)
        {
            ResetOutput();

            var pos = CurrentGenerator.CreateItem(type, CurrentMap.GenInfo.AllTypes);

            ref MapItem item = ref CurrentGenerator.FillItemWith(MapItemType.Converter, pos);
            ref ConverterMapItem convItm = ref MapItem.GetConverterData(ref item);
            convItm.Converter = converter;

            var genContext = new ContextGen(type, CurrentGenerator);

            item.IsGenerating = false;
            convItm.Context = converter.TryGenerateContext(ref genContext);

            CurrentMapItem = pos;
        }

        public void ResetOutputWithMapFor(Type type)
        {
            ResetOutput();
            CurrentMapItem = CurrentGenerator.GetMap(type);
        }

        public void ResetOutput()
        {
            GoToStart();
            Stream.SetLength(0);
        }

        public void AssertAndGoToStart(params byte[] data)
        {
            AssertOutput(data);
            GoToStart();
        }
        
        public void AssertOutput(params byte[] expected)
        {
            var actual = Stream.ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        public byte[] GetByteArr(params short[] data) => GetByteArr(null, data);

        public byte[] GetByteArr(object[] itms, params short[] data)
        {
            List<byte> bytes = new List<byte>(64);
            ABSaveSerializer serializer = null;
            int currentItmsPos = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] <= 255)
                    bytes.Add((byte)data[i]);
                else
                {
                    switch ((GenType)data[i])
                    {
                        case GenType.Numerical:
                            bytes.AddRange(BitConverter.GetBytes((dynamic)itms[currentItmsPos++]));
                            break;
                        case GenType.String:
                            bytes.AddRange(Encoding.UTF8.GetBytes((string)itms[currentItmsPos++]));
                            break;
                        case GenType.ByteArr:
                            bytes.AddRange((byte[])itms[currentItmsPos++]);
                            break;
                        case GenType.Action:

                            SetupSerializer();
                            ((Action<ABSaveSerializer>)itms[currentItmsPos++])(serializer);
                            bytes.AddRange(((MemoryStream)serializer.Output).ToArray());

                            break;
                        case GenType.MapItem:

                            SetupSerializer();

                            var genMap = (GenMap)itms[currentItmsPos++];
                            serializer.SerializeExactNonNullItem(genMap.Obj, genMap.Item);
                            bytes.AddRange(((MemoryStream)serializer.Output).ToArray());

                            break;
                        case GenType.Size:

                            SetupSerializer();
                            serializer.WriteCompressed((ulong)itms[currentItmsPos++]);

                            bytes.AddRange(((MemoryStream)serializer.Output).ToArray());
                            break;
                    }
                }
            }

            return bytes.ToArray();

            void SetupSerializer()
            {
                if (serializer == null)
                {
                    serializer = new ABSaveSerializer();
                    serializer.Initialize(new MemoryStream(), CurrentMap);
                }
                else
                {
                    serializer.Reset();
                    serializer.Output.Position = 0;
                } 
            }
        }

        public static byte[] Concat(byte[] first, params byte[] second)
        {
            var res = new byte[first.Length + second.Length];
            first.CopyTo(res, 0);
            second.CopyTo(res, first.Length);
            return res;
        }
    }

    public enum GenType : short
    {
        Numerical = 256,
        String = 257,
        ByteArr = 258,
        MapItem = 259,
        Action = 260,
        Size = 261
    }

    public struct GenMap
    {
        public object Obj;
        public MapItemInfo Item;

        public GenMap(object obj, MapItemInfo item) => (Obj, Item) = (obj, item);
    }
}