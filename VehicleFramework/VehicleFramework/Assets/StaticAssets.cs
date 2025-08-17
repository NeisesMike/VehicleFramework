using UnityEngine;
using System.Collections.Generic;
using VehicleFramework.Engines;

namespace VehicleFramework.Assets
{
    public static class StaticAssets
    {
        public static Sprite ModVehicleIcon { get; private set; }
        public static Sprite UpgradeIcon { get; private set; }
        public static Sprite DepthIcon { get; private set; }
        public static Sprite ArmIcon { get; private set; }
        public static Sprite DefaultPingSprite { get; private set; }
        public static Sprite DefaultSaveFileSprite { get; private set; }
        public static Dictionary<TechType, int> DefaultRecipe { get; private set; }
        public static VFEngine DefaultEngine { get; private set; }
        internal static void GetSprites()
        {
            ModVehicleIcon = Assets.SpriteHelper.GetSpriteInternal("ModVehicleIcon.png");
            ArmIcon = Assets.SpriteHelper.GetSpriteInternal("ArmUpgradeIcon.png");
            UpgradeIcon = Assets.SpriteHelper.GetSpriteInternal("UpgradeIcon.png");
            DepthIcon = Assets.SpriteHelper.GetSpriteInternal("DepthIcon.png");

            Assets.VehicleAssets DSAssets = Assets.AssetBundleInterface.GetVehicleAssetsFromBundle("modvehiclepingsprite");
            DefaultPingSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            DefaultSaveFileSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            DSAssets.Close();
        }
        internal static void SetupDefaultAssets()
        {
            DefaultRecipe = new()
            {
                { TechType.PlasteelIngot, 1 },
                { TechType.Lubricant, 1 },
                { TechType.ComputerChip, 1 },
                { TechType.AdvancedWiringKit, 1 },
                { TechType.Lead, 2 },
                { TechType.EnameledGlass, 2 }
            };

            DefaultEngine = new Engines.AtramaEngine();
        }
    }
}
