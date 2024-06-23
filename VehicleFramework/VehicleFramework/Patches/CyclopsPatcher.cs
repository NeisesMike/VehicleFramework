using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(CyclopsVehicleStorageTerminalManager))]
    public static class CyclopsPatcher
    {
        /* This transpiler makes one part of OnDockedChanged more generic
         * Optionally change GetComponent to GetComponentInChildren
         * Simple as
         */
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodes = new List<CodeInstruction>(codes.Count);
            CodeInstruction myNOP = new CodeInstruction(OpCodes.Nop);
            for (int i = 0; i < codes.Count; i++)
            {
                newCodes.Add(myNOP);
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().ToLower().Contains("energymixin"))
                {
                    newCodes[i] = CodeInstruction.Call(typeof(ModVehicle), nameof(ModVehicle.GetEnergyMixinFromVehicle));
                }
                else
                {
                    newCodes[i] = codes[i];
                }
            }
            return newCodes.AsEnumerable();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        static void VehicleDockedPrefix(CyclopsVehicleStorageTerminalManager __instance, Vehicle vehicle)
        {
            if (vehicle is ModVehicle)
            {
                // TODO: make custom DockedVehicleType and associated HUD
                __instance.dockedVehicleType = CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth;
                __instance.usingModulesUIHolder = __instance.seamothModulesUIHolder;
                __instance.currentScreen = __instance.seamothVehicleScreen;

                IEnumerator EnsureSubRootSet()
                {
                    for (int i = 0; i < 100; i++)
                    {
                        yield return null;
                        Player.main.SetCurrentSub(__instance.dockingBay.GetSubRoot(), false);
                    }
                }
				
                if((vehicle as VehicleTypes.Drone) == null)
                {
                    UWE.CoroutineHost.StartCoroutine(EnsureSubRootSet());
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        static void VehicleDockedPostfix(CyclopsVehicleStorageTerminalManager __instance, Vehicle vehicle)
        {
            if (vehicle is ModVehicle)
            {
                __instance.dockedVehicleType = CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth;
                __instance.usingModulesUIHolder = __instance.seamothModulesUIHolder;
                __instance.currentScreen = __instance.seamothVehicleScreen;
                __instance.vehicleUpgradeConsole = (vehicle as ModVehicle).upgradesInput;
                if (__instance.vehicleUpgradeConsole && __instance.vehicleUpgradeConsole.equipment != null)
                {
                    //__instance.vehicleUpgradeConsole.equipment.onEquip += __instance.OnEquip;
                    //__instance.vehicleUpgradeConsole.equipment.onUnequip += __instance.OnUneqip;
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.StorageButtonClick))]
        public static void StorageButtonClickPostfix(CyclopsVehicleStorageTerminalManager __instance, CyclopsVehicleStorageTerminalManager.VehicleStorageType type, int slotID)
        {
            if (__instance.dockedVehicleType == CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth)
            {
                foreach (StorageInput seamothStorageInput in __instance.currentVehicle.GetAllComponentsInChildren<StorageInput>())
                {
                    if (seamothStorageInput.slotID == slotID)
                    {
                        seamothStorageInput.OpenFromExternal();
                        return;
                    }
                }
                return;
            }
        }
    }
}
