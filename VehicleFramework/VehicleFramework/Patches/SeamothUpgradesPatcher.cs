using HarmonyLib;
using VehicleFramework.Admin;

// PURPOSE: permits the SeaMoth to react to VF Upgrades
// VALUE: Critical

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(SeaMoth))]
    public class SeaMothUpgradesPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleToggle))]
        public static void SeaMothOnUpgradeModuleTogglePostfix(SeaMoth __instance, int slotID, bool active)
        {
            TechType techType = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            UpgradeTypes.ToggleActionParams param = new()
            {
                active = active,
                vehicle = __instance,
                slotID = slotID,
                techType = techType
            };
            UpgradeRegistrar.OnToggleActions.ForEach(x => x(param));
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleUse))]
        public static void SeaMothOnUpgradeModuleUsePostfix(SeaMoth __instance, TechType techType, int slotID)
        {
            UpgradeTypes.SelectableActionParams param = new()
            {
                vehicle = __instance,
                slotID = slotID,
                techType = techType
            };
            UpgradeRegistrar.OnSelectActions.ForEach(x => x(param));

            UpgradeTypes.SelectableChargeableActionParams param2 = new()
            {
                vehicle = __instance,
                slotID = slotID,
                techType = techType,
                charge = param.vehicle.quickSlotCharge[param.slotID],
                slotCharge = param.vehicle.GetSlotCharge(param.slotID)
            };
            UpgradeRegistrar.OnSelectChargeActions.ForEach(x => x(param2));
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleChange))]
        public static void SeaMothOnUpgradeModuleChangePostfix(SeaMoth __instance, int slotID, TechType techType, bool added)
        {
            UpgradeTypes.AddActionParams addedParams = new()
            {
                vehicle = __instance,
                slotID = slotID,
                techType = techType,
                isAdded = added
            };
            UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
        }
    }
}
