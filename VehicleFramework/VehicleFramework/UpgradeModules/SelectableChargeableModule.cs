using System;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Handlers;

namespace VehicleFramework.UpgradeModules
{
    public static partial class ModuleManager
    {
        public static TechType AddSelectableChargeableModule(List<Tuple<TechType, int>> inputRecipe, string classId, string displayName, string description, Action<Vehicle, int, float, float> onSelected, float maxCharge, float energyCost, Atlas.Sprite icon=null, string tabName = "")
        {
            if (tabName == "")
            {
                tabName = Admin.Utils.UpgradePathToString(Admin.Utils.UpgradePath.ModVehicle);
            }
            List<CraftData.Ingredient> recipe = inputRecipe.Select(x => new CraftData.Ingredient(x.Item1, x.Item2)).ToList();
            TechType tt = ModulePrepper.RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.SelectableChargeable, icon, tabName: tabName);
            CraftDataHandler.SetMaxCharge(tt, maxCharge);
            CraftDataHandler.SetEnergyCost(tt, energyCost);
            void WrappedOnSelected(Vehicle mv, int slotID, TechType moduleTechType, float charge, float slotCharge)
            {
                if (moduleTechType == tt)
                {
                    onSelected(mv, slotID, charge, slotCharge);
                }
            }
            ModulePrepper.upgradeOnUseChargeableActions.Add(new Tuple<Action<Vehicle, int, TechType, float, float>, float, float>(WrappedOnSelected, maxCharge, energyCost));
            return tt;
        }
    }
}
