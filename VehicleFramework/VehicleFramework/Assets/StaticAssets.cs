using UnityEngine;
using System.Collections.Generic;
using VehicleFramework.Engines;

namespace VehicleFramework.Assets
{
    public static class StaticAssets
    {
        public static Sprite ModVehicleIcon { get; private set; } = null!;
        public static Sprite UpgradeIcon { get; private set; } = null!;
        public static Sprite DepthIcon { get; private set; } = null!;
        public static Sprite ArmIcon { get; private set; } = null!;
        public static Sprite DefaultPingSprite { get; private set; } = null!;
        public static Sprite DefaultSaveFileSprite { get; private set; } = null!;
        public static Dictionary<TechType, int> DefaultRecipe { get; private set; } = null!;
        public static VFEngine DefaultEngine { get; private set; } = null!;
        internal static void GetSprites()
        {
            ModVehicleIcon = Assets.SpriteHelper.GetSpriteInternal("ModVehicleIcon.png") ?? throw Admin.SessionManager.Fatal("Failed to load ModVehicleIcon.png. Please ensure the sprite is correctly set up. Try downloading the assets again.");
            ArmIcon = Assets.SpriteHelper.GetSpriteInternal("ArmUpgradeIcon.png") ?? throw Admin.SessionManager.Fatal("Failed to load ArmUpgradeIcon.png. Please ensure the sprite is correctly set up. Try downloading the assets again.");
            UpgradeIcon = Assets.SpriteHelper.GetSpriteInternal("UpgradeIcon.png") ?? throw Admin.SessionManager.Fatal("Failed to load UpgradeIcon.png. Please ensure the sprite is correctly set up. Try downloading the assets again.");
            DepthIcon = Assets.SpriteHelper.GetSpriteInternal("DepthIcon.png") ?? throw Admin.SessionManager.Fatal("Failed to load DepthIcon.png. Please ensure the sprite is correctly set up. Try downloading the assets again.");
            Assets.VehicleAssets DSAssets = Assets.AssetBundleInterface.GetVehicleAssetsFromBundle("modvehiclepingsprite");
            if(DSAssets.abi == null)
            {
                throw Admin.SessionManager.Fatal("Failed to load the asset bundle for ModVehiclePingSprite. Please ensure the asset bundle is correctly set up. Try downloading the assets again.");
            }
            DefaultPingSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite") ?? throw Admin.SessionManager.Fatal("Failed to load default ping sprite from the asset bundle. Please ensure the asset bundle is correctly set up. Try downloading the assets again.");
            DefaultSaveFileSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite") ?? throw Admin.SessionManager.Fatal("Failed to load default save file sprite from the asset bundle. Please ensure the asset bundle is correctly set up. Try downloading the assets again.");
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
