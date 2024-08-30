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
        [HarmonyPatch("ChooseBestAction")]
        public static void ChooseBestActionPostfix(Creature __instance, ref CreatureAction __result)
        {
            if(__instance.name.Contains("Warper") && __result.GetType().ToString().Contains("RangedAttackLastTarget"))
            {
                VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
                if(drone != null)
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
        [HarmonyPatch("Warp")]
        public static void WarpPrefix(WarpBall __instance, GameObject target, ref Vector3 position)
        {
            Player component = target.GetComponent<Player>();
            if (component != null && component.GetMode() == Player.Mode.LockedPiloting)
            {
                ModVehicle mv = component.GetVehicle() as ModVehicle;
                VehicleTypes.Submarine sub = mv as VehicleTypes.Submarine;
                if (mv != null && sub == null)
                {
                    mv.ForceExitLockedMode();
                    mv.PlayerExit();
                    position = mv.transform.position + UnityEngine.Random.onUnitSphere * 15f;
                }
            }
        }
    }
}
