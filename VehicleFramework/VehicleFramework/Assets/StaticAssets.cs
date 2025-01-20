using UnityEngine;
using System.Collections.Generic;
using VehicleFramework.Engines;

namespace VehicleFramework.Assets
{
    public static class StaticAssets
    {
        internal static Atlas.Sprite ModVehicleIcon { get; private set; }
        internal static Atlas.Sprite UpgradeIcon { get; private set; }
        internal static Atlas.Sprite DepthIcon { get; private set; }
        internal static Atlas.Sprite ArmIcon { get; private set; }
        internal static Atlas.Sprite DefaultPingSprite { get; private set; }
        internal static Sprite DefaultSaveFileSprite { get; private set; }
        internal static Dictionary<TechType, int> DefaultRecipe { get; private set; }
        internal static VFEngine DefaultEngine { get; private set; }
        public static void GetSprites()
        {
            ModVehicleIcon = Assets.SpriteHelper.GetSpriteInternal("ModVehicleIcon.png");
            ArmIcon = Assets.SpriteHelper.GetSpriteInternal("ArmUpgradeIcon.png");
            UpgradeIcon = Assets.SpriteHelper.GetSpriteInternal("UpgradeIcon.png");
            DepthIcon = Assets.SpriteHelper.GetSpriteInternal("DepthIcon.png");

            Assets.VehicleAssets DSAssets = Assets.AssetBundleInterface.GetVehicleAssetsFromBundle("modvehiclepingsprite");
            DefaultPingSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            DefaultSaveFileSprite = Assets.AssetBundleInterface.LoadAdditionalRawSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            DSAssets.Close();
        }
        public static void SetupDefaultAssets()
        {
            DefaultRecipe = new Dictionary<TechType, int>();
            DefaultRecipe.Add(TechType.PlasteelIngot, 1);
            DefaultRecipe.Add(TechType.Lubricant, 1);
            DefaultRecipe.Add(TechType.ComputerChip, 1);
            DefaultRecipe.Add(TechType.AdvancedWiringKit, 1);
            DefaultRecipe.Add(TechType.Lead, 2);
            DefaultRecipe.Add(TechType.EnameledGlass, 2);

            DefaultEngine = new Engines.AtramaEngine();
        }
    }
}
