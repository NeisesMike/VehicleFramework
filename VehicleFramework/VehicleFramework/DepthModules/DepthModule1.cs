using System.Collections.Generic;
using VehicleFramework.UpgradeTypes;
//using VehicleFramework.Localization;

namespace VehicleFramework.DepthModules
{
    public class DepthModule1 : ModVehicleUpgrade
    {
        public override string ClassId => "ModVehicleDepthModule1";
        public override string DisplayName => Language.main.Get("VFDepth1FriendlyString");
        public override string Description => Language.main.Get("VFDepth1Description");
        public override List<Assets.Ingredient> Recipe => new List<Assets.Ingredient>()
                {
                    new Assets.Ingredient(TechType.TitaniumIngot, 1),
                    new Assets.Ingredient(TechType.Magnetite, 3),
                    new Assets.Ingredient(TechType.Glass, 3),
                    new Assets.Ingredient(TechType.AluminumOxide, 3)
                };
        public override Atlas.Sprite Icon => Assets.SpriteHelper.GetSprite("Sprites/DepthIcon.png");
        public override Atlas.Sprite TabIcon => Assets.SpriteHelper.GetSprite("Sprites/DepthIcon.png");
        public override string TabName => "MVDM";
        public override string TabDisplayName => Language.main.Get("VFMVDepthModules");
        public override void OnAdded(AddActionParams param)
        {
            Admin.Utils.EvaluateDepthModules(param);
        }
        public override void OnRemoved(AddActionParams param)
        {
            Admin.Utils.EvaluateDepthModules(param);
        }
    }
}
