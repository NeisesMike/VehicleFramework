using HarmonyLib;
using UnityEngine;

// PURPOSE: protect player's in Submarines against Warper teleport balls.
// VALUE: High.

namespace VehicleFramework.Patches.CreaturePatches
{
    [HarmonyPatch(typeof(Creature))]
    class WarperPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Creature.ChooseBestAction))]
        public static void ChooseBestActionPostfix(Creature __instance, ref CreatureAction __result)
        {
            if(__result == null || __result.GetType() == null)
            {
                return;
            }
            if(__instance.name.Contains("Warper") && __result.GetType().ToString().Contains("RangedAttackLastTarget"))
            {
                VehicleTypes.Drone? drone = VehicleTypes.Drone.MountedDrone;
                VehicleTypes.Submarine? sub = Player.main?.GetVehicle() as VehicleTypes.Submarine;
                if (drone != null || sub != null)
                {
                    __result = new SwimRandom();
                }
            }
        }
    }

    [HarmonyPatch(typeof(WarpBall))]
    class WarperPatcher2
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(WarpBall.Warp))]
        public static bool WarpBallWarpPrefix(GameObject target)
        {
            // Warp balls shouldn't effect players in Submarines

            Player myPlayer = target.GetComponent<Player>();
            VehicleTypes.Submarine? mySub = target.GetComponent<VehicleTypes.Submarine>()
                ?? myPlayer?.GetVehicle() as VehicleTypes.Submarine;

            if(mySub == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
