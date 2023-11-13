using System;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Handlers;

namespace VehicleFramework.UpgradeModules
{
    public static partial class ModuleManager
    {
        public static TechType AddSelectableChargeableModule(List<Tuple<TechType, int>> inputRecipe, string classId, string displayName, string description, Action<ModVehicle, int, float, float> onSelected, float maxCharge, float energyCost, Atlas.Sprite icon=null, string tabName = "MVCM")
        {
            List<CraftData.Ingredient> recipe = inputRecipe.Select(x => new CraftData.Ingredient(x.Item1, x.Item2)).ToList();
            TechType tt = ModulePrepper.RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.SelectableChargeable, icon, tabName: tabName);
            CraftDataHandler.SetMaxCharge(tt, maxCharge);
            CraftDataHandler.SetEnergyCost(tt, energyCost);
            void WrappedOnSelected(ModVehicle mv, int slotID, TechType moduleTechType, float charge, float slotCharge)
            {
                if (moduleTechType == tt)
                {
                    onSelected(mv, slotID, charge, slotCharge);
                }
            }
            ModulePrepper.upgradeOnUseChargeableActions.Add(new Tuple<Action<ModVehicle, int, TechType, float, float>, float, float>(WrappedOnSelected, maxCharge, energyCost));
            return tt;
        }
    }
}
