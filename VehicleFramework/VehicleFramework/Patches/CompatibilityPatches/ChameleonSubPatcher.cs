using HarmonyLib;

// PURPOSE: ensures ModVehicles are built normally despite the presence of the Chameleon sub
// VALUE: High, unfortunately. As you can see, the prefix is empty. Odd!

namespace VehicleFramework.Patches.CompatibilityPatches
{
    [HarmonyPatch(typeof(ConstructorInput))]
    public static class ChameleonSubPatcher
    {
        /*
         * This patch is specifically for the Chameleon mod.
         * It ensures ModVehicles are built normally;
         * without it, Submarines don't finish being constructed correctly,
         * and mostly obviously, they don't get fabricators.
         * 
         * The purpose of this "empty" patch is to bump the transpilation chain on this method.
         * I don't know why it fixes the problem.
         * I don't even know why the original transpiler is a problem.
         */
        [HarmonyPatch(nameof(ConstructorInput.OnCraftingBeginAsync)), HarmonyPatch(MethodType.Enumerator), HarmonyPostfix]
        public static void OnCraftingBeginAsyncPostfix()
        {
        }
    }
}
