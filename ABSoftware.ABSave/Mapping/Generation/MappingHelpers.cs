﻿using ABCo.ABSave.Mapping.Description.Attributes;

namespace ABCo.ABSave.Mapping.Generation
{
    internal static class MappingHelpers
    {
        public static void UpdateHighestVersionFromRange(ref uint highestVersion, uint startVer, uint endVer)
        {
            // If there is no upper we'll only update the highest version based on what the minimum is.
            if (endVer == uint.MaxValue)
            {
                if (startVer > highestVersion)
                    highestVersion = startVer;
            }

            // If not update based on what their custom high is.
            else
            {
                if (endVer > highestVersion)
                    highestVersion = endVer;
            }
        }

        public static T? FindAttributeForVersion<T>(T[] attributes, uint version)
            where T : AttributeWithVersion
        {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Length; i++)
            {
                T currentAttribute = attributes[i];
                if (currentAttribute.FromVer <= version && currentAttribute.ToVer >= version)
                    return currentAttribute;
            }

            return null;
        }
    }
}
