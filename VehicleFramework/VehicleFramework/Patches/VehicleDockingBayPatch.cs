using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.Patches
{
    // This set of patches modulates the moonpool docking bay behavior
    // It governs:
    // - accepting a ModVehicle for docking
    // - animating the docking bay arms
    // - alerting the vehicle it has been docked
    // See DockedVehicleHandTargetPatcher for undocking actions
    [HarmonyPatch(typeof(VehicleDockingBay))]
    class VehicleDockingBayPatch
    {
        private static bool HandleMoonpool(ModVehicle mv)
        {
            Vector3 boundingDimensions = mv.GetBoundingDimensions();
            if (boundingDimensions == Vector3.zero)
            {
                return false;
            }
            float mpx = 8.6f;
            float mpy = 8.6f;
            float mpz = 12.0f;
            if (boundingDimensions.x > mpx)
            {
                return false;
            }
            else if (boundingDimensions.y > mpy)
            {
                return false;
            }
            else if (boundingDimensions.z > mpz)
            {
                return false;
            }
            return true;
        }
        private static bool HandleCyclops(ModVehicle mv)
        {
            Vector3 boundingDimensions = mv.GetBoundingDimensions();
            if (boundingDimensions == Vector3.zero)
            {
                return false;
            }
            float mpx = 4.5f;
            float mpy = 6.0f;
            float mpz = 4.5f;
            if (boundingDimensions.x > mpx)
            {
                return false;
            }
            else if (boundingDimensions.y > mpy)
            {
                return false;
            }
            else if (boundingDimensions.z > mpz)
            {
                return false;
            }
            return true;
        }
        public static bool IsThisVehicleSmallEnough(VehicleDockingBay bay, GameObject nearby)
        {
            ModVehicle mv = UWE.Utils.GetComponentInHierarchy<ModVehicle>(nearby.gameObject);
            if (mv == null || Player.main.GetVehicle() != mv)
            {
                return true;
            }
            if (!mv.CanMoonpoolDock)
            {
                return false;
            }
            string subRootName = bay.subRoot.name.ToLower();
            if (subRootName.Contains("base"))
            {
                return HandleMoonpool(mv);
            }
            else if (subRootName.Contains("cyclops"))
            {
                return HandleCyclops(mv);
            }
            else
            {
                Logger.Warn("Trying to dock in something that is neither a moonpool nor a cyclops. What is this?");
                return false;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.LateUpdate))]
        // This patch animates the docking bay arms as if a seamoth is docked
        public static void LateUpdatePostfix(VehicleDockingBay __instance)
        {
            ModVehicle mv = __instance.dockedVehicle as ModVehicle;
            if (mv != null)
            {
                mv.AnimateMoonPoolArms(__instance);
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.DockVehicle))]
        // This patch ensures a modvehicle docks correctly
        public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle, bool rebuildBase)
        {
            if (vehicle is ModVehicle)
            {
                (vehicle as ModVehicle).OnVehicleDocked();
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.SetVehicleDocked))]
        // This patch ensures a modvehicle docks correctly
        public static void SetVehicleDockedPostfix(VehicleDockingBay __instance, Vehicle vehicle)
        {
            if (vehicle is ModVehicle)
            {
                (vehicle as ModVehicle).OnVehicleDocked();
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.OnTriggerEnter))]
        // This patch controls whether to dock a ModVehicle. Only small Submersibles are accepted.
        public static bool OnTriggerEnterPrefix(VehicleDockingBay __instance, Collider other)
        {
            ModVehicle mv = UWE.Utils.GetComponentInHierarchy<ModVehicle>(other.gameObject);
            if(mv == null)
            {
                return true;
            }
            if(!mv.CanMoonpoolDock)
            {
                return false;
            }
            string subRootName = __instance.subRoot.name.ToLower();
            if (subRootName.Contains("base"))
            {
                 return HandleMoonpool(mv);
            }
            else if (subRootName.Contains("cyclops"))
            {
                // When we can handle cyclops docking,
                // not only can this line be uncommented,
                // but this entire block can be replaced with 
                // a call to IsThisVehicleSmallEnough
                return HandleCyclops(mv);
            }
            else
            {
                Logger.Warn("Trying to dock in something that is neither a moonpool nor a cyclops. What is this?");
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.UpdateDockedPosition))]
        public static bool UpdateDockedPositionPrefix(VehicleDockingBay __instance, Vehicle vehicle, float interpfraction)
        {
            ModVehicle mv = vehicle as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            Transform endingTransform;
            string subRootName = __instance.subRoot.name.ToLower();
            if (subRootName.Contains("base"))
            {
                endingTransform = __instance.dockingEndPos.parent.parent;
            }
            else if (subRootName.Contains("cyclops"))
            {
                endingTransform = __instance.dockingEndPos;
            }
            else
            {
                Logger.Warn("Trying to dock in something that is neither a moonpool nor a cyclops. What is this?");
                return true;
            }
            if (!mv.IsUndockingAnimating)
            {
                vehicle.transform.position = Vector3.Lerp(__instance.startPosition, endingTransform.position, interpfraction) - mv.GetDifferenceFromCenter();
                vehicle.transform.rotation = Quaternion.Lerp(__instance.startRotation, endingTransform.rotation, interpfraction);
            }
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaEnter))]
        public static bool LaunchbayAreaEnterPrefix(VehicleDockingBay __instance, GameObject nearby)
        {
            return IsThisVehicleSmallEnough(__instance, nearby);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaExit))]
        public static bool LaunchbayAreaExitPrefix(VehicleDockingBay __instance, GameObject nearby)
        {
            return IsThisVehicleSmallEnough(__instance, nearby);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.OnUndockingComplete))]
        public static bool OnUndockingCompletePrefix(VehicleDockingBay __instance, Player player)
        {
            ModVehicle mv = __instance.dockedVehicle as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            mv.OnUndockingComplete();
            string subRootName = __instance.subRoot.name.ToLower();
            if (subRootName.Contains("cyclops"))
            {
                __instance.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(true);
            }
            SkyEnvironmentChanged.Broadcast(mv.gameObject, (GameObject)null);
            __instance.dockedVehicle = null;
            mv.OnVehicleUndocked();
            return false;
        }

        public static IEnumerator UndockHelper(VehicleDockingBay db, ModVehicle mv)
        {
            float timeToWaitForAnimationSweetspot = 2.5f;
            string subRootName = db.subRoot.name.ToLower();
            if (subRootName.Contains("cyclops"))
            {
                timeToWaitForAnimationSweetspot = 3.6f;
            }
            yield return new WaitForSeconds(timeToWaitForAnimationSweetspot);
            UWE.CoroutineHost.StartCoroutine(mv.Undock(Player.main, db.transform.position.y)); // this releases the vehicle model into the water
            yield break;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.OnUndockingStart))]
        public static void OnUndockingStartPostfix(VehicleDockingBay __instance)
        {
            ModVehicle mv = __instance.dockedVehicle as ModVehicle;
            if (mv != null)
            {
                mv.OnUndockingStart();
                string subRootName = __instance.subRoot.name.ToLower();
                if (subRootName.Contains("cyclops"))
                {
                    __instance.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(false);
                }
                Player.main.SetCurrentSub(null, false);
                UWE.CoroutineHost.StartCoroutine(UndockHelper(__instance, mv));
                //__instance.StartCoroutine(__instance.dockedVehicle.Undock(player, __instance.transform.position.y - 4f)); // this releases the vehicle model into the water
            }
        }
    }
}
