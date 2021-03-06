﻿using ABCo.ABSave.Mapping.Description;
using System;

namespace ABCo.ABSave.Mapping.Generation.IntermediateObject
{
    /// <summary>
    /// Tracks the current information while creating the intermediate object info.
    /// </summary>
    internal struct IntermediateMappingContext
    {
        public Type ClassType;
        public SaveMembersMode Mode;
        public int TranslationCurrentOrderInfo;

        // Used to count how many unskipped members were present so we know the size for our final array.
        public int UnskippedMemberCount;
        public uint HighestVersion;

        public IntermediateMappingContext(Type classType, SaveMembersMode mode)
        {
            ClassType = classType;
            Mode = mode;
            TranslationCurrentOrderInfo = 0;
            UnskippedMemberCount = 0;
            HighestVersion = 0;
        }
    }
}
