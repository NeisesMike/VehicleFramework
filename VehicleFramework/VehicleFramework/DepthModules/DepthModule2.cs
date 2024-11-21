using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.UpgradeTypes;

namespace VehicleFramework.DepthModules
{
    public class DepthModule2 : ModVehicleUpgrade
    {
        public override string ClassId => "ModVehicleDepthModule2";
        public override string DisplayName => LocalizationManager.GetString(EnglishString.Depth2FriendlyString);
        public override string Description => LocalizationManager.GetString(EnglishString.Depth2Description);
        public override List<Assets.Ingredient> Recipe => new List<Assets.Ingredient>()
                {
                    new Assets.Ingredient(TechType.TitaniumIngot, 3),
                    new Assets.Ingredient(TechType.Lithium, 3),
                    new Assets.Ingredient(TechType.EnameledGlass, 3),
                    new Assets.Ingredient(TechType.AluminumOxide, 5)
                };
        public override string TabName => "MVDM";
        public override string TabDisplayName => "Depth Modules";
        public override void OnAdded(AddActionParams param)
        {
            VehicleFramework.Admin.Utils.EvaluateDepthModules(param);
        }
        public override void OnRemoved(AddActionParams param)
        {
            VehicleFramework.Admin.Utils.EvaluateDepthModules(param);
        }
    }
}
