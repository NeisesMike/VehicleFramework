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
                // TODO
                // does this work?
                // What should the screen say?
                // We should be able to change our name...
                __instance.dockedVehicleType = CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth;
                __instance.usingModulesUIHolder = __instance.seamothModulesUIHolder;
                __instance.currentScreen = __instance.seamothVehicleScreen;

                IEnumerator doit()
                {
                    for (int i = 0; i < 100; i++)
                    {
                        yield return null;
                        Player.main.SetCurrentSub(__instance.dockingBay.GetSubRoot(), false);
                    }
                }
                UWE.CoroutineHost.StartCoroutine(doit());
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
    }
}
