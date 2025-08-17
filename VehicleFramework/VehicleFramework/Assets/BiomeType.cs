using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeData = LootDistributionData.BiomeData;

namespace VehicleFramework.Assets
{
    public enum AbstractBiomeType
    {
        SafeShallows,
        KelpForest,
        GrassyPlateus,
        MushroomForest,
        BulbZone,
        JellyshroomCaves,
        FloatingIslands,
        LavaZone,
        CrashZone,
        SparseReef,
        UnderwaterIslands,
        GrandReef,
        DeepGrandReef,
        BloodKelp,
        Mountains,
        Dunes,
        SeaTreader,
        TreeCove,
        BonesField,
        GhostTree,
        LostRiver1,
        LostRiver2,
        Canyon,
        SkeletonCave,
        CragField,
        PrisonAquarium,
        Mesas
    }

    public static class BiomeTypes
    {
        // I included "CreatureOnly" biomes and some dead (unused) biome numbers
        private static readonly Dictionary<AbstractBiomeType, List<BiomeType>> mapping = new()
        {
            {AbstractBiomeType.SafeShallows, Enumerable.Range(101, 27).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.KelpForest, Enumerable.Range(201, 22).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.GrassyPlateus, Enumerable.Range(301, 26).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.MushroomForest, Enumerable.Range(401, 31).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.BulbZone, Enumerable.Range(501, 21).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.JellyshroomCaves, Enumerable.Range(601, 12).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.FloatingIslands, Enumerable.Range(701, 7).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.LavaZone, Enumerable.Range(801, 40).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.CrashZone, Enumerable.Range(913, 4).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.SparseReef, Enumerable.Range(1005, 20).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.UnderwaterIslands, Enumerable.Range(1200, 19).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.GrandReef, Enumerable.Range(1300, 21).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.DeepGrandReef, Enumerable.Range(1400, 9).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.BloodKelp, Enumerable.Range(1500, 21).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.Mountains, Enumerable.Range(1600, 19).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.Dunes, Enumerable.Range(1700, 19).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.SeaTreader, Enumerable.Range(1800, 13).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.TreeCove, Enumerable.Range(1900, 7).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.BonesField, Enumerable.Range(2000, 24).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.GhostTree, Enumerable.Range(2100, 14).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.LostRiver1, Enumerable.Range(2200, 9).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.LostRiver2, Enumerable.Range(2900, 9).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.Canyon, Enumerable.Range(2300, 6).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.SkeletonCave, Enumerable.Range(2400, 8).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.CragField, Enumerable.Range(2500, 4).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.PrisonAquarium, Enumerable.Range(2600, 9).Select(i => (BiomeType)i).ToList() },
            {AbstractBiomeType.Mesas, Enumerable.Range(2800, 3).Select(i => (BiomeType)i).ToList() }
        };

        public static List<BiomeType> Get(AbstractBiomeType type)
        {
            return mapping[type];
        }

        public static BiomeData GetOneBiomeData(BiomeType type, int Icount = 1, float Iprobability = 0.1f)
        {
            return new BiomeData { biome = type, count = Icount, probability = Iprobability };
        }

        public static List<BiomeData> GetBiomeData(BiomeStruct biomeStruct)
        {
            return Get(biomeStruct.type).Select(biomeType => GetOneBiomeData(biomeType, biomeStruct.count, biomeStruct.probability)).ToList();
        }
    }

    public struct BiomeStruct
    {
        public AbstractBiomeType type;
        public int count;
        public float probability;
        public BiomeStruct(AbstractBiomeType itype, int icount, float iprobability)
        {
            type = itype;
            count = icount;
            probability = iprobability;
        }
    }

    public class AbstractBiomeData
    {
        public List<BiomeStruct> biomes = new();
        public List<BiomeData> ConvertStruct(BiomeStruct biome)
        {
            return BiomeTypes.GetBiomeData(biome);
        }
        public List<BiomeData> Get()
        {
            return biomes.SelectMany(x => ConvertStruct(x)).ToList();
        }
    }

    public static class AbstractBiomeDataExtensions
    {
        public static AbstractBiomeData WithBiome(this AbstractBiomeData data, AbstractBiomeType type, int count = 1, float probability = 0.1f)
        {
            data.biomes.Add(new BiomeStruct(type, count, probability));
            return data;
        }
    }
}
