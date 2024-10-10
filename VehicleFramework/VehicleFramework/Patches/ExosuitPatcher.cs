using HarmonyLib;
using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace VehicleFramework.Patches
{
    /* This set of patches is meant to only effect Exosuits.
     * For whatever reason, Exosuit does not implement
     * OnUpgradeModuleUse or
     * OnUpgradeModuleToggle
     * So we patch those in Vehicle here.
     * 
     * The purpose of these patches is to let our
     * ModVehicleUpgrades be usable.
     */
    [HarmonyPatch(typeof(Vehicle))]
    public class VehicleExosuitPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.OnUpgradeModuleToggle))]
        public static void VehicleOnUpgradeModuleTogglePostfix(Vehicle __instance, int slotID, bool active)
        {
            Exosuit exo = __instance as Exosuit;
            if (exo != null)
            {
                TechType techType = exo.modules.GetTechTypeInSlot(exo.slotIDs[slotID]);
                if (active)
                {
                    var moduleToggleAction = UpgradeModules.ModulePrepper.upgradeToggleActions.Where(x => x.Item2 == techType).FirstOrDefault();
                    if (moduleToggleAction != null)
                    {
                        Admin.UpgradeRegistrar.toggledActions.Add(new Tuple<Vehicle, int, Coroutine>(exo, slotID, exo.StartCoroutine(DoToggleAction(exo, slotID, techType, moduleToggleAction.Item3, moduleToggleAction.Item4, moduleToggleAction.Item5))));
                    }
                    IEnumerator DoToggleAction(Vehicle thisMV, int thisSlotID, TechType tt, float timeToFirstActivation, float repeatRate, float energyCostPerActivation)
                    {
                        yield return new WaitForSeconds(timeToFirstActivation);
                        while (true)
                        {
                            if (!thisMV.GetPilotingMode())
                            {
                                exo.ToggleSlot(thisSlotID, false);
                                yield break;
                            }
                            moduleToggleAction.Item1(thisMV, thisSlotID);
                            exo.energyInterface.TotalCanProvide(out int whatWeGot);
                            if (whatWeGot < energyCostPerActivation)
                            {
                                exo.ToggleSlot(thisSlotID, false);
                                yield break;
                            }
                            exo.energyInterface.ConsumeEnergy(energyCostPerActivation);
                            yield return new WaitForSeconds(repeatRate);
                        }
                    }
                }
                else
                {
                    Admin.UpgradeRegistrar.toggledActions.Where(x => x.Item1 == exo).Where(x => x.Item2 == slotID).Where(x => x.Item3 != null).ToList().ForEach(x => exo.StopCoroutine(x.Item3));
                }
                UpgradeTypes.ToggleActionParams param = new UpgradeTypes.ToggleActionParams
                {
                    active = active,
                    vehicle = exo,
                    slotID = slotID,
                    techType = techType
                };
                Admin.UpgradeRegistrar.OnToggleActions.ForEach(x => x(param));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.OnUpgradeModuleUse))]
        public static void VehicleOnUpgradeModuleUsePostfix(Vehicle __instance, TechType techType, int slotID)
        {
            Exosuit exo = __instance as Exosuit;
            if (exo != null)
            {
                UpgradeTypes.SelectableActionParams param = new UpgradeTypes.SelectableActionParams
                {
                    vehicle = __instance,
                    slotID = slotID,
                    techType = techType
                };
                Admin.UpgradeRegistrar.OnSelectActions.ForEach(x => x(param));

                UpgradeTypes.SelectableChargeableActionParams param2 = new UpgradeTypes.SelectableChargeableActionParams
                {
                    vehicle = __instance,
                    slotID = slotID,
                    techType = techType,
                    charge = param.vehicle.quickSlotCharge[param.slotID],
                    slotCharge = param.vehicle.GetSlotCharge(param.slotID)
                };
                Admin.UpgradeRegistrar.OnSelectChargeActions.ForEach(x => x(param2));
            }
        }
    }

    [HarmonyPatch(typeof(Exosuit))]
    public class ExosuitPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyDown))]
        public static void ExosuitSlotKeyDownPostfix(Exosuit __instance, int slotID)
        {
            QuickSlotType quickSlotType = __instance.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.Selectable)
            {
                if (__instance.ConsumeEnergy(techType))
                {
                    __instance.OnUpgradeModuleUse(techType, slotID);
                }
            }
            else if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                __instance.quickSlotCharge[slotID] = 0f;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyHeld))]
        public static void ExosuitSlotKeyHeldPostfix(Exosuit __instance, int slotID)
        {
            QuickSlotType quickSlotType = __instance.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                __instance.ChargeModule(techType, slotID);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyUp))]
        public static void ExosuitSlotKeyUpPostfix(Exosuit __instance, int slotID)
        {
            QuickSlotType quickSlotType = __instance.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                if (__instance.ConsumeEnergy(techType))
                {
                    __instance.OnUpgradeModuleUse(techType, slotID);
                    __instance.quickSlotCharge[slotID] = 0f;
                }
            }
        }
    }
}
