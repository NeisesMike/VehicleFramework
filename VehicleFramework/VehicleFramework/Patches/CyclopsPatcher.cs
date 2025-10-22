using System.Collections.Generic;
using System.Collections;
using System;
using HarmonyLib;
using System.Reflection.Emit;
using VehicleFramework.StorageComponents;

// PURPOSE: allow the Cyclops dock terminal to display ModVehicle data. 
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(CyclopsVehicleStorageTerminalManager))]
    public static class CyclopsPatcher
    {
        /* This transpiler makes one part of OnDockedChanged more generic
         * Optionally change GetComponent to GetComponentInChildren
         * Simple as
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            List<CodeInstruction> newCodes = new(codes.Count);
            CodeInstruction myNOP = new(OpCodes.Nop);
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
         */

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        public static IEnumerable<CodeInstruction> CyclopsVehicleStorageTerminalManagerVehicleDockedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch GetEnergyMixinMatch = new(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains("EnergyMixin"));

            CodeMatcher newInstructions = new CodeMatcher(instructions)
                .MatchForward(true, GetEnergyMixinMatch)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Insert(Transpilers.EmitDelegate<Func<EnergyMixin, Vehicle, EnergyMixin?>>(ModVehicle.GetLeastChargedModVehicleEnergyMixinIfNull));

            return newInstructions.InstructionEnumeration();
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
                    Admin.SessionManager.StartCoroutine(EnsureSubRootSet());
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.VehicleDocked))]
        static void VehicleDockedPostfix(CyclopsVehicleStorageTerminalManager __instance, Vehicle vehicle)
        {
            if (vehicle is ModVehicle mv)
            {
                __instance.dockedVehicleType = CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth;
                __instance.usingModulesUIHolder = __instance.seamothModulesUIHolder;
                __instance.currentScreen = __instance.seamothVehicleScreen;
                __instance.vehicleUpgradeConsole = mv.upgradesInput;
                if (__instance.vehicleUpgradeConsole && __instance.vehicleUpgradeConsole.equipment != null)
                {
                    //__instance.vehicleUpgradeConsole.equipment.onEquip += __instance.OnEquip;
                    //__instance.vehicleUpgradeConsole.equipment.onUnequip += __instance.OnUneqip;
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.StorageButtonClick))]
        public static void StorageButtonClickPostfix(CyclopsVehicleStorageTerminalManager __instance, int slotID)
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
