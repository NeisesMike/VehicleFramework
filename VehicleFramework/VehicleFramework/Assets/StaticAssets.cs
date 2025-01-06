using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.Assets
{
    public static class StaticAssets
    {
        internal static Atlas.Sprite ModVehicleIcon { get; private set; }
        internal static Atlas.Sprite UpgradeIcon { get; private set; }
        internal static Atlas.Sprite DepthIcon { get; private set; }
        internal static Atlas.Sprite ArmIcon { get; private set; }
        public static void GetSprites()
        {
            ModVehicleIcon = Assets.SpriteHelper.GetSpriteInternal("ModVehicleIcon.png");
            ArmIcon = Assets.SpriteHelper.GetSpriteInternal("ArmUpgradeIcon.png");
            UpgradeIcon = Assets.SpriteHelper.GetSpriteInternal("UpgradeIcon.png");
            DepthIcon = Assets.SpriteHelper.GetSpriteInternal("DepthIcon.png");

            Assets.VehicleAssets DSAssets = Assets.AssetBundleInterface.GetVehicleAssetsFromBundle("modvehiclepingsprite");
            VehicleManager.defaultPingSprite = Assets.AssetBundleInterface.LoadAdditionalSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            VehicleManager.defaultSaveFileSprite = Assets.AssetBundleInterface.LoadAdditionalRawSprite(DSAssets.abi, "ModVehicleSpriteAtlas", "ModVehiclePingSprite");
            DSAssets.Close();
        }
        public static void SetupDefaultAssets()
        {
            VehicleManager.defaultRecipe.Add(TechType.PlasteelIngot, 1);
            VehicleManager.defaultRecipe.Add(TechType.Lubricant, 1);
            VehicleManager.defaultRecipe.Add(TechType.ComputerChip, 1);
            VehicleManager.defaultRecipe.Add(TechType.AdvancedWiringKit, 1);
            VehicleManager.defaultRecipe.Add(TechType.Lead, 2);
            VehicleManager.defaultRecipe.Add(TechType.EnameledGlass, 2);

            VehicleManager.defaultEngine = new Engines.AtramaEngine();
        }
    }
}
