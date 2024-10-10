using System;
using System.Collections.Generic;
using System.Linq;

namespace VehicleFramework.UpgradeModules
{
    public static partial class ModuleManager
    {
        public static TechType AddPassiveModule(List<Tuple<TechType, int>> inputRecipe, string classId, string displayName, string description, Action<Vehicle, List<string>, int, bool> onAdded, Atlas.Sprite icon=null, string tabName="")
        {
            if(tabName == "")
            {
                tabName = Admin.Utils.UpgradePathToString(Admin.Utils.UpgradePath.ModVehicle);
            }
            List<CraftData.Ingredient> recipe = inputRecipe.Select(x => new CraftData.Ingredient(x.Item1, x.Item2)).ToList();
            TechType tt = ModulePrepper.RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.Passive, icon, tabName:tabName);
            void WrappedOnAdded(Vehicle mv, List<string> currentUpgrades, int slotID, TechType moduleTechType, bool added)
            {
                if (moduleTechType == tt)
                {
                    onAdded(mv, currentUpgrades, slotID, added);
                }
            }
            ModulePrepper.upgradeOnAddedActions.Add(WrappedOnAdded);
            return tt;
        }
    }
}
