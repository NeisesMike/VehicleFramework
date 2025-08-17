using System.Collections.Generic;
using VehicleFramework.UpgradeTypes;
//using VehicleFramework.Localization;
using UnityEngine;

namespace VehicleFramework.DepthModules
{
    public class DepthModule3 : ModVehicleUpgrade
    {
        public override string ClassId => "ModVehicleDepthModule3";
        public override string DisplayName => Language.main.Get("VFDepth3FriendlyString");
        public override string Description => Language.main.Get("VFDepth3Description");
        public override List<Ingredient> Recipe => new()
                {
                    new(TechType.PlasteelIngot, 3),
                    new(TechType.Nickel, 3),
                    new(TechType.EnameledGlass, 3),
                    new(TechType.Kyanite, 3)
                };
        public override Sprite Icon => Assets.SpriteHelper.GetSprite("Sprites/DepthIcon.png");
        public override Sprite TabIcon => Assets.SpriteHelper.GetSprite("Sprites/DepthIcon.png");
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
