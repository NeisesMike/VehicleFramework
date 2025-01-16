using System;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(ConstructorInput))]
    public class ConstructorInputPatcher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ConstructorInput.Craft))]
        public static IEnumerable<CodeInstruction> ConstructorInputCraftranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch ReturnValidCraftingPositionMatch = new CodeMatch(i => i.opcode == OpCodes.Call && i.operand.ToString().Contains("ReturnValidCraftingPosition"));

            var newInstructions = new CodeMatcher(instructions)
                .MatchStartForward(ReturnValidCraftingPositionMatch)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
                .Insert(Transpilers.EmitDelegate<Func<ConstructorInput, Vector3, TechType, bool>>(ReturnValidCraftingPositionSpecial));

            return newInstructions.InstructionEnumeration();
        }

        public static bool ReturnValidCraftingPositionSpecial(ConstructorInput instance, Vector3 pollPosition, TechType craftTechType)
        {
            foreach(var marty in VehicleManager.vehicleTypes)
            {
                if(marty.techType == craftTechType)
                {
                    // do something different
                    return true;
                }
            }
            float num = Mathf.Clamp01((pollPosition.x + 2048f) / 4096f);
            float num2 = Mathf.Clamp01((pollPosition.z + 2048f) / 4096f);
            int x = (int)(num * (float)instance.validCraftPositionMap.width);
            int y = (int)(num2 * (float)instance.validCraftPositionMap.height);
            return instance.validCraftPositionMap.GetPixel(x, y).g > 0.5f;
        }
    }
}
