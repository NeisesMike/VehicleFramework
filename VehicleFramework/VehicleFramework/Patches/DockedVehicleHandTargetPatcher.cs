using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(DockedVehicleHandTarget))]
    public static class DockedVehicleHandTargetPatch
    {
        /*
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnPlayerCinematicModeEnd))]
        public static void OnPlayerCinematicModeEndPostfix(DockedVehicleHandTarget __instance)
        {
            ModVehicle mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
            if(mv != null)
            {
                mv.OnVehicleUndocked();
            }
        }
        */

        /* This transpiler makes one part of OnHandHover more generic
         * Optionally change GetComponent to GetComponentInChildren
         * Simple as
         */
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        public static void OnHandHoverPostfix(DockedVehicleHandTarget __instance)
        {
            ModVehicle mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
            if (mv != null)
            {
                string text = "Enter " + mv.name.Replace("(Clone)", "");
                float energyActual = 0;
                float energyMax = 0;
                foreach (var battery in mv.energyInterface.sources)
                {
                    energyActual += battery.charge;
                    energyMax += battery.capacity;
                }
                float energyFraction = energyActual / energyMax;
                if (energyFraction == 1)
                {
                    string format2 = Language.main.GetFormat<float>("VehicleStatusChargedFormat", mv.liveMixin.GetHealthFraction());
                    HandReticle.main.SetText(HandReticle.TextType.Hand, text, true, GameInput.Button.LeftHand);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format2, false, GameInput.Button.None);
                }
                else
                {
                    string format = Language.main.GetFormat<float, float>("VehicleStatusFormat", mv.liveMixin.GetHealthFraction(), energyFraction);
                    HandReticle.main.SetText(HandReticle.TextType.Hand, text, true, GameInput.Button.LeftHand);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format, false, GameInput.Button.None);
                }
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
        }
    }
}
