using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleComponents;
using VehicleFramework.UpgradeTypes;

// PURPOSE: allow VF upgrades for the Prawn to be used as expected
// VALUE: High.

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
        public static void DoSlotDown(Exosuit exo, int slotID, bool isJank)
        {
            if (!exo.playerFullyEntered || exo.ignoreInput)
            {
                return;
            }
            if(slotID < 0)
            {
                // Usually this means slotID was -1,
                // which means there was no active slot.
                return;
            }
            if (exo.GetQuickSlotCooldown(slotID) != 1f)
            {
                // cooldown isn't finished!
                return;
            }
            int slotIDToTest = isJank ? slotID - 2 : slotID;
            if (!Player.main.GetQuickSlotKeyDown(slotIDToTest) && !Player.main.GetLeftHandDown())
            {
                // we didn't actually hit the slot button!
                // (or hit the left mouse button!)
                // this prevents activation on SlotNext and SlotPrev
                return;
            }
            QuickSlotType quickSlotType = exo.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.Selectable)
            {
                if (exo.ConsumeEnergy(techType))
                {
                    exo.OnUpgradeModuleUse(techType, slotID);
                }
            }
            else if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                exo.quickSlotCharge[slotID] = 0f;
            }
        }
        public static void DoSlotHeld(Exosuit exo, int slotID)
        {
            if (!exo.playerFullyEntered || exo.ignoreInput)
            {
                return;
            }
            QuickSlotType quickSlotType = exo.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                exo.ChargeModule(techType, slotID);
            }
        }
        public static void DoSlotUp(Exosuit exo, int slotID)
        {
            if (!exo.playerFullyEntered || exo.ignoreInput)
            {
                return;
            }
            QuickSlotType quickSlotType = exo.GetQuickSlotType(slotID, out TechType techType);
            if (quickSlotType == QuickSlotType.SelectableChargeable)
            {
                if (exo.ConsumeEnergy(techType))
                {
                    exo.OnUpgradeModuleUse(techType, slotID);
                }
                exo.quickSlotCharge[slotID] = 0f;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.GetArmPrefab))]
        public static void ExosuitGetArmPrefabPostfix(Exosuit __instance, TechType techType, ref GameObject __result)
        {
            if (__result == null)
            {
                __result = VFArm.GetArmPrefab(techType);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Exosuit.SpawnArm))]
        public static bool ExosuitSpawnArmPrefix(Exosuit __instance, TechType techType, Transform parent, ref IExosuitArm __result)
        {
            ModVehicleArm armLogic = VFArm.GetModVehicleArm(techType);
            if (armLogic != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.GetArmPrefab(techType));
                gameObject.transform.parent = parent.transform;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localPosition = Vector3.zero;
                __result = gameObject.GetComponent<IExosuitArm>();
                gameObject.GetComponent<VFArm>().SetArmDecl(armLogic);
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.OnUpgradeModuleChange))]
        public static void ExosuitOnUpgradeModuleChangePostfix(Exosuit __instance, int slotID, TechType techType, bool added)
        {
            // only work on arms here. Other types of modules are covered in VanillaUpgradeMaker.
            ModVehicleArm armLogic = VFArm.GetModVehicleArm(techType);
            if (armLogic != null)
            {
                UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
                {
                    vehicle = __instance,
                    slotID = slotID,
                    techType = techType,
                    isAdded = added
                };
                Admin.UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
                __instance.MarkArmsDirty();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyDown))]
        public static void ExosuitSlotKeyDownPostfix(Exosuit __instance, int slotID)
        {
            DoSlotDown(__instance, slotID, true);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyHeld))]
        public static void ExosuitSlotKeyHeldPostfix(Exosuit __instance, int slotID)
        {
            DoSlotHeld(__instance, slotID);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotKeyUp))]
        public static void ExosuitSlotKeyUpPostfix(Exosuit __instance, int slotID)
        {
            DoSlotUp(__instance, slotID);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotLeftDown))]
        public static void ExosuitSlotLeftDownPostfix(Exosuit __instance)
        {
            if (!AvatarInputHandler.main.IsEnabled())
            {
                return;
            }
            int slotID = __instance.activeSlot;
            DoSlotDown(__instance, slotID, false);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotLeftHeld))]
        public static void ExosuitSlotLeftHeldPostfix(Exosuit __instance)
        {
            if (!AvatarInputHandler.main.IsEnabled())
            {
                return;
            }
            int slotID = __instance.activeSlot;
            DoSlotHeld(__instance, slotID);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Exosuit.SlotLeftUp))]
        public static void ExosuitSlotLeftUpPostfix(Exosuit __instance)
        {
            if (!AvatarInputHandler.main.IsEnabled())
            {
                return;
            }
            int slotID = __instance.activeSlot;
            DoSlotUp(__instance, slotID);
        }
    }
}
