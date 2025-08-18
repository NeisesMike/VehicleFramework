using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using VehicleFramework.MiscComponents;

// PURPOSE: allow ModVehicles to use in-game docking bays
// VALUE: High.

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
        public static bool IsThisDockable(VehicleDockingBay bay, GameObject nearby)
        {
            return
                !IsThisASubmarineWithStandingPilot(nearby)
                && IsThisVehicleSmallEnough(bay, nearby);
        }
        public static bool IsThisASubmarineWithStandingPilot(GameObject nearby)
        {
            VehicleTypes.Submarine mv = UWE.Utils.GetComponentInHierarchy<VehicleTypes.Submarine>(nearby.gameObject);
            if(mv == null)
            {
                return false;
            }    
            if(mv.IsUnderCommand && !mv.IsPlayerPiloting())
            {
                return true;
            }
            return false;
        }
        public static bool IsThisVehicleSmallEnough(VehicleDockingBay bay, GameObject nearby)
        {
            ModVehicle mv = UWE.Utils.GetComponentInHierarchy<ModVehicle>(nearby.gameObject);
            if (mv == null) return true;
            if (!mv.CanMoonpoolDock || mv.docked) return false;
            DockingBayBounds bounds = bay.gameObject.GetComponent<DockingBayBounds>();
            if (bounds == null) return false;
            return bounds.IsVehicleSmallEnough(bay, mv);
        }
        private static void HandleMVDocked(Vehicle vehicle, VehicleDockingBay dock)
        {
            ModVehicle? mv = vehicle as ModVehicle;
            if (mv != null)
            {
                Moonpool moonpool = dock.GetComponentInParent<Moonpool>();
                CyclopsMotorMode cmm = dock.GetComponentInParent<CyclopsMotorMode>();
                if (mv.IsUnderCommand)
                {
                    Player.main.SetCurrentSub(dock.GetSubRoot(), true);
                    Player.main.ToNormalMode(false);
                }
                if (moonpool != null || cmm != null)
                {
                    Transform playerSpawn = dock.transform.Find("playerSpawn");
                    mv.OnVehicleDocked(playerSpawn.position);
                }
                else
                {
                    Logger.Warn("Vehicle Framework is not aware of this dock. The player is probably in a weird position now.");
                    mv.OnVehicleDocked(Vector3.zero);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.LateUpdate))]
        // This patch animates the docking bay arms as if a seamoth is docked
        public static void LateUpdatePostfix(VehicleDockingBay __instance)
        {
            ModVehicle? mv = __instance.dockedVehicle as ModVehicle;
            mv?.AnimateMoonPoolArms(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.DockVehicle))]
        // This patch ensures a modvehicle docks correctly
        public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle, bool rebuildBase)
        {
            HandleMVDocked(vehicle, __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.SetVehicleDocked))]
        // This patch ensures a modvehicle docks correctly
        public static void SetVehicleDockedPostfix(VehicleDockingBay __instance, Vehicle vehicle)
        {
            HandleMVDocked(vehicle, __instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.OnTriggerEnter))]
        // This patch controls whether to dock a ModVehicle. Only small ModVehicles are accepted.
        public static bool OnTriggerEnterPrefix(VehicleDockingBay __instance, Collider other)
        {
            return IsThisDockable(__instance, other.gameObject);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.UpdateDockedPosition))]
        public static bool UpdateDockedPositionPrefix(VehicleDockingBay __instance, Vehicle vehicle, float interpfraction)
        {
            ModVehicle? mv = vehicle as ModVehicle;
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
                //vehicle.transform.rotation = Quaternion.Lerp(__instance.startRotation, mv.CyclopsDockRotation * endingTransform.rotation, interpfraction);
                vehicle.transform.rotation = Quaternion.Lerp(__instance.startRotation, endingTransform.rotation, interpfraction);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaEnter))]
        public static bool LaunchbayAreaEnterPrefix(VehicleDockingBay __instance, GameObject nearby)
        {
            return IsThisDockable(__instance, nearby);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaExit))]
        public static bool LaunchbayAreaExitPrefix(VehicleDockingBay __instance, GameObject nearby)
        {
            return IsThisDockable(__instance, nearby);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.Start))]
        public static void VehicleDockingBayStartPostfix(VehicleDockingBay __instance)
        {
            dockingBays.Add(__instance);

            string subRootName = __instance.subRoot.name.ToLower();
            if (string.Equals(subRootName, "base(clone)", StringComparison.OrdinalIgnoreCase))
            {
                __instance.gameObject.EnsureComponent<DockingBayBounds>()
                   .WithX(8.6f)
                   .WithY(8.6f)
                   .WithZ(12.0f);
            }
            else if (string.Equals(subRootName, "cyclops-mainprefab(clone)", StringComparison.OrdinalIgnoreCase))
            {
                __instance.gameObject.EnsureComponent<DockingBayBounds>()
                   .WithX(4.5f)
                   .WithY(6.0f)
                   .WithZ(4.5f);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleDockingBay.OnDestroy))]
        public static void VehicleDockingBayOnDestroyPostfix(VehicleDockingBay __instance)
        {
            dockingBays.Remove(__instance);
        }
        public static List<VehicleDockingBay> dockingBays = new();
    }


    [HarmonyPatch(typeof(VehicleDockingBay))]
    public static class VehicleDockingBayPatch2
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleDockingBay.LateUpdate))]
        public static IEnumerable<CodeInstruction> VehicleDockingBayLateUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch startCinematicMatch = new(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains("StartCinematicMode"));

            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, startCinematicMatch)
                .Repeat(x =>
                    x.RemoveInstruction()
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                    .Insert(Transpilers.EmitDelegate<Action<PlayerCinematicController, Player, Vehicle>>(MaybeStartCinematicMode))
                );

            return newInstructions.InstructionEnumeration();
        }

        public static void MaybeStartCinematicMode(PlayerCinematicController cinematic, Player player, Vehicle vehicle)
        {
            if(vehicle as ModVehicle == null)
            {
                cinematic.StartCinematicMode(player);
            }
        }
    }
}
