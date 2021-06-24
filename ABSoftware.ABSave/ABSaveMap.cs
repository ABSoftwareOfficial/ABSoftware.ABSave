﻿using ABCo.ABSave.Configuration;
using ABCo.ABSave.Converters;
using ABCo.ABSave.Mapping.Generation;
using ABCo.ABSave.Mapping.Generation.General;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ABCo.ABSave.Mapping
{
    public class ABSaveMap
    {
        internal static ThreadLocal<MapGenerator> GeneratorPool = new ThreadLocal<MapGenerator>(() => new MapGenerator());
        internal MapItemInfo RootItem;

        /// <summary>
        /// All the types present throughout the map, and their respective map item.
        /// </summary>
        internal Dictionary<Type, MapItem?> AllTypes;

        /// <summary>
        /// The configuration this map uses.
        /// </summary>
        public ABSaveSettings Settings { get; set; }

        // Internal for use with unit tests.
        internal ABSaveMap(ABSaveSettings settings)
        {
            Settings = settings;
            AllTypes = new Dictionary<Type, MapItem?>();
        }

        public static ABSaveMap Get<T>(ABSaveSettings settings) => GetNonGeneric(typeof(T), settings);

        public static ABSaveMap GetNonGeneric(Type type, ABSaveSettings settings)
        {
            var map = new ABSaveMap(settings);
            MapGenerator? generator = map.GetGenerator();

            map.RootItem = generator.GetMap(type);
            ReleaseGenerator(generator);
            return map;
        }

        internal VersionInfo GetVersionInfo(Converter converter, uint version)
        {
            // Try to get the version if it already exists.
            VersionInfo? existing = VersionCacheHandler.GetVersionOrAddNull(converter, version);
            if (existing != null) return existing;

            // If it doesn't, generate it.
            MapGenerator? gen = GetGenerator();
            VersionInfo? res = VersionCacheHandler.AddNewVersion(converter, version, gen);
            ReleaseGenerator(gen);
            return res;
        }

        internal MapItemInfo GetRuntimeMapItem(Type type)
        {
            MapGenerator? gen = GetGenerator();
            MapItemInfo map = gen.GetRuntimeMap(type);
            ReleaseGenerator(gen);
            return map;
        }

        internal MapGenerator GetGenerator()
        {
            MapGenerator res = GeneratorPool.Value!;
            res.Initialize(this);
            return res;
        }

        // NOTE: It's generally not a good idea to call this from a "finally" just in case it pools
        // a generator that's been left in an invalid state.
        internal static void ReleaseGenerator(MapGenerator gen) => gen.FinishGeneration();
    }
}
