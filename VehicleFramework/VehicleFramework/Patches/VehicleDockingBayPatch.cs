using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using VehicleFramework.MiscComponents;
using VehicleFramework.VehicleTypes;

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
        public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle)
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
    public static class VehicleDockingBayTranspiler
    {
        // This transpiler ensures that StartCinematicMode is not called for newly docked ModVehicles.
        // This way, we can skip the animation that almost always makes the player jump through the vehicle wall.
        // To accomplish this, we make the following edit to the original code:
        /*
         * 
			if (dockedVehicle is Exosuit)
			{
				this.exosuitDockPlayerCinematic.StartCinematicMode(player);
			}
			else
			{
				this.dockPlayerCinematic.StartCinematicMode(player);
			}
         */
        // Becomes this:
        /*
			if (dockedVehicle is ModVehicle)
			{
			}
			else if (dockedVehicle is Exosuit)
			{
				this.exosuitDockPlayerCinematic.StartCinematicMode(player);
			}
			else
			{
				this.dockPlayerCinematic.StartCinematicMode(player);
			}
         */
        // But more accurately, this IL code:
        /*
         *  //If is Exosuit...
       One  IL_00B7: ldloc.0
       two  IL_00B8: isinst    Exosuit
            IL_00BD: brfalse.s IL_00CD // If not an exosuit, jump to "otherwise, do these actions"
            
            // do Exosuit actions
            IL_00BF: ldarg.0
            IL_00C0: ldfld     class PlayerCinematicController VehicleDockingBay::exosuitDockPlayerCinematic
            IL_00C5: ldloc.3
            IL_00C6: callvirt  instance void PlayerCinematicController::StartCinematicMode(class Player)
      eight IL_00CB: br.s      IL_00D9 // Exit the conditional block

            // otherwise, do these actions
            IL_00CD: ldarg.0
            IL_00CE: ldfld     class PlayerCinematicController VehicleDockingBay::dockPlayerCinematic
            IL_00D3: ldloc.3
            IL_00D4: callvirt  instance void PlayerCinematicController::StartCinematicMode(class Player)

            // this is the first line after the conditional block
            IL_00D9: ldarg.0
         */
        // becomes this IL code. The rest is preserved, but the Exosuit check is scooted down to make room for this check.
        // This check skips the whole conditional branch when the vehicle is a ModVehicle, so it does not call StartCinematicMode.
        /*
            IL_00B7: ldloc.0
            IL_00B8: isinst    ModVehicle
            IL_00BD: brfalse.s IL_00D9  // If is a ModVehicle, Exit the conditional block
         */


        private static (bool, object?) FindMatchingPattern(CodeInstruction one, CodeInstruction two, CodeInstruction eight)
        {
            bool oneRight = one.opcode == OpCodes.Ldloc_0;
            if(!oneRight) return (false, null);

            bool twoRight = two.opcode == OpCodes.Isinst && two.operand.ToString().Contains("exosuit");
            if (!twoRight) return (false, null);

            bool eightRight = eight.opcode == OpCodes.Br_S;
            if(!eightRight) return (false, null);

            return (true, eight.operand);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleDockingBay.LateUpdate))]
        public static IEnumerable<CodeInstruction> VehicleDockingBayLateUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
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
                if (FindMatchingPattern(codes[i], codes[i + 1], codes[i + 2]) is (true, object operand))
                {
                    newCodes[i] = new CodeInstruction(OpCodes.Ldloc_0);
                    newCodes[i + 1] = new CodeInstruction(OpCodes.Isinst, typeof(ModVehicle));
                    newCodes[i + 2] = new CodeInstruction(OpCodes.Brfalse_S, operand);
                    i += 2;
                    continue;
                }
            }
            return newCodes.AsEnumerable();
        }
    }
}
