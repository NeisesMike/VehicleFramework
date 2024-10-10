using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nautilus.Handlers;

namespace VehicleFramework.UpgradeModules
{
    public static partial class ModuleManager
    {
        public static TechType AddToggleableModule(List<Tuple<TechType, int>> inputRecipe, string classId, string displayName, string description, Action<Vehicle, int> OnToggle, float energyCostPerActivation = 1f, float timeToFirstActivation = 1f, float repeatRate = 1f, Atlas.Sprite icon=null, string tabName = "")
        {
            if (tabName == "")
            {
                tabName = Admin.Utils.UpgradePathToString(Admin.Utils.UpgradePath.ModVehicle);
            }
            List<CraftData.Ingredient> recipe = inputRecipe.Select(x => new CraftData.Ingredient(x.Item1, x.Item2)).ToList();
            TechType tt = ModulePrepper.RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.Toggleable, icon, tabName: tabName);
            ModulePrepper.upgradeToggleActions.Add(new Tuple<Action<ModVehicle, int>, TechType, float, float, float>(OnToggle, tt, timeToFirstActivation, repeatRate, energyCostPerActivation));
            return tt;
        }
    }
}
