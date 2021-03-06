﻿using ABCo.ABSave.Deserialization;
using ABCo.ABSave.Mapping;
using ABCo.ABSave.Mapping.Description.Attributes;
using ABCo.ABSave.Mapping.Generation.Converters;
using ABCo.ABSave.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ABCo.ABSave.Converters
{
    public abstract class Converter
    {
        public Type ItemType { get; internal set; } = null!;
        public bool IsValueItemType { get; internal set; }

        internal volatile bool _isGenerating;
        internal bool _hasOneVersion;

        internal uint _instanceId;
        internal uint _highestVersion;

        public uint HighestVersion => _highestVersion;

        internal SaveInheritanceAttribute[]? _allInheritanceAttributes = null;

        internal ConverterVersionCache _versionCache;

        [StructLayout(LayoutKind.Explicit)]
        internal struct ConverterVersionCache
        {
            [FieldOffset(0)]
            public VersionInfo OneVersion;

            [FieldOffset(0)]
            public Dictionary<uint, VersionInfo?> MultipleVersions;
        }

        /// <summary>
        /// Initializes a given converter for a given type.
        /// </summary>
        public virtual uint Initialize(InitializeInfo info) => 0;

        /// <summary>
        /// Check whether the converter supports a given type, used for non-exact types.
        /// This method is allowed to modify variables, however it is nt.
        /// </summary>
        public virtual bool CheckType(CheckTypeInfo info) => throw new Exception("Converter says it also converts non-exact but does not override 'CheckType' to check for one.");

        /// <summary>
        /// Gets information that can be used by the converter and varies depending on the version number in the source.
        /// This info will be cached and may be used across many threads so ensure it does not change once created.
        /// </summary>
        public virtual (VersionInfo?, bool) GetVersionInfo(InitializeInfo info, uint version) => (null, false);

        /// <summary>
        /// Called when all of the different possible versions right up to the highest version have been generated.
        /// Can be used to free resources that aren't needed if all versions are generated.
        /// </summary>
        protected virtual void DoHandleAllVersionsGenerated() { }

        public void HandleAllVersionsGenerated()
        {
            _allInheritanceAttributes = null;
            DoHandleAllVersionsGenerated();
        }

        public struct SerializeInfo
        {
            public object Instance { get; }
            public Type ActualType { get; }
            public VersionInfo VersionInfo { get; }

            internal SerializeInfo(object instance, Type actualType, VersionInfo versionInfo) =>
                (Instance, ActualType, VersionInfo) = (instance, actualType, versionInfo);
        }

        public abstract void Serialize(in SerializeInfo info, ref BitTarget header);

        public struct DeserializeInfo
        {
            public Type ActualType { get; }
            internal VersionInfo VersionInfo { get; }

            internal DeserializeInfo(Type actualType, VersionInfo versionInfo) =>
                (ActualType, VersionInfo) = (actualType, versionInfo);
        }

        public abstract object Deserialize(in DeserializeInfo info, ref BitSource header);
    }
}