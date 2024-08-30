using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.UpgradeTypes;

namespace VehicleFramework.DepthModules
{
    public class DepthModule1 : ModVehicleUpgrade
    {
        public override string ClassId => "ModVehicleDepthModule1";
        public override string DisplayName => LocalizationManager.GetString(EnglishString.Depth1FriendlyString);
        public override string Description => LocalizationManager.GetString(EnglishString.Depth1Description);
        public override List<Assets.Ingredient> Recipe => new List<Assets.Ingredient>()
                {
                    new Assets.Ingredient(TechType.TitaniumIngot, 1),
                    new Assets.Ingredient(TechType.Magnetite, 3),
                    new Assets.Ingredient(TechType.Glass, 3),
                    new Assets.Ingredient(TechType.AluminumOxide, 3)
                };
        public override string TabName => "MVDM";
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
