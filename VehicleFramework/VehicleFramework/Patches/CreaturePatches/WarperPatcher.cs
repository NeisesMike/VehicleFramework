using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches.CreaturePatches
{
    [HarmonyPatch(typeof(Creature))]
    class WarperPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Creature.ChooseBestAction))]
        public static void ChooseBestActionPostfix(Creature __instance, ref CreatureAction __result)
        {
            if(__instance.name.Contains("Warper") && __result.GetType().ToString().Contains("RangedAttackLastTarget"))
            {
                VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
                VehicleTypes.Submarine sub = Player.main.GetVehicle() as VehicleTypes.Submarine;
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
        public static bool WarpBallWarpPrefix(WarpBall __instance, GameObject target, ref Vector3 position)
        {
            // Warp balls shouldn't effect players in Submarines

            Player myPlayer = target.GetComponent<Player>();
            VehicleTypes.Submarine mySub = target.GetComponent<VehicleTypes.Submarine>()
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
