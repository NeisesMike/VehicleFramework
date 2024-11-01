using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    /* This set of patches over BlockSeamoth
     * prevents ModVehicles from entering "moon gates"
     * which are vertical force fields that prevent seamoth entry.
     * The only one I know of is at the entrance to the prison.
     */
    [HarmonyPatch(typeof(BlockSeamoth))]
    public class BlockSeamothPatcher
    {
        private static Rigidbody mvRigidbody;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockSeamoth.FixedUpdate))]
        public static void BlockSeamothFixedUpdatePostfix(BlockSeamoth __instance)
        {
            if (!BlockSeamothPatcher.mvRigidbody)
            {
                return;
            }
            BlockSeamothPatcher.mvRigidbody.AddForce(__instance.transform.forward * 3f, ForceMode.VelocityChange);
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockSeamoth.OnTriggerEnter))]
        public static void BlockSeamothOnTriggerEnterPostfix(BlockSeamoth __instance, Collider other)
        {
            ModVehicle mv = other.GetComponentInParent<ModVehicle>();
            if(mv == null)
            {
                return;
            }
            __instance.enteredCollidersCount++;
            BlockSeamothPatcher.mvRigidbody = mv.useRigidbody;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockSeamoth.OnTriggerExit))]
        public static void BlockSeamothOnTriggerExitPostfix(BlockSeamoth __instance, Collider other)
        {
            ModVehicle mv = other.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            __instance.enteredCollidersCount--;
            if (__instance.enteredCollidersCount <= 0)
            {
                BlockSeamothPatcher.mvRigidbody = null;
            }
        }
    }
}
