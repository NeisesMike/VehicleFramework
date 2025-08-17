using System.Collections.Generic;
using System.Collections;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;

// PURPOSE: generally ensures ModVehicles behave like normal Vehicles
// VALUE: Very high.

namespace VehicleFramework
{
    [HarmonyPatch(typeof(Vehicle))]
    public class VehiclePatcher
    {
        /*
         * This collection of patches generally ensures our ModVehicles behave like normal Vehicles.
         * Each will be commented if necessary
         */

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.OnHandClick))]
        public static bool OnHandClickPrefix(Vehicle __instance)
        {
            if (VehicleTypes.Drone.mountedDrone != null || (__instance as ModVehicle) != null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.OnHandHover))]
        public static bool OnHandHoverPrefix(Vehicle __instance)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                if (VehicleTypes.Drone.mountedDrone != null)
                {
                    return false;
                }
            }
            else
            {
                if (mv.isScuttled)
                {
                    float now = mv.GetComponent<Sealed>().openedAmount;
                    float max = mv.GetComponent<Sealed>().maxOpenedAmount;
                    string percent = Mathf.CeilToInt(now * 100f / max).ToString();
                    HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("VFDeconstructionHint")}: {percent}", true, GameInput.Button.None);
                }
                else if (mv.IsUnderCommand)
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "", true, GameInput.Button.None);
                }
                else
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, mv.GetName(), true, GameInput.Button.None);
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.ApplyPhysicsMove))]
        private static bool ApplyPhysicsMovePrefix(Vehicle __instance, ref bool ___wasAboveWater, ref VehicleAccelerationModifier[] ___accelerationModifiers)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv != null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.LazyInitialize))]
        public static bool LazyInitializePrefix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }

            ___energyInterface = __instance.gameObject.GetComponent<EnergyInterface>();
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.GetAllStorages))]
        public static void GetAllStoragesPostfix(Vehicle __instance, ref List<IItemsContainer> containers)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return;
            }

            foreach (var tmp in ((ModVehicle)__instance).InnateStorages)
            {
                containers.Add(tmp.Container.GetComponent<InnateStorageContainer>().container);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.IsPowered))]
        public static void IsPoweredPostfix(Vehicle __instance, ref EnergyInterface ___energyInterface, ref bool __result)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return;
            }
            if (!mv.isPoweredOn)
            {
                __result = false;
            }
        }

        [HarmonyPatch(nameof(Vehicle.Update))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /* This is basically a prefix for Vehicle.Update,
             * but we choose to transpile instead,
             * so that our code may be considered "core."
             * That is, it will be skipped if any other Prefix returns false.
             * This is desirable to be as "alike" normal Vehicles as possible;
             * in particular, this ensures compatibility with FreeLook
             * We must control our ModVehicle rotation within the core Vehicle.Update code.
             */
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodes = new List<CodeInstruction>(codes.Count + 2);
            CodeInstruction myNOP = new CodeInstruction(OpCodes.Nop);
            for (int i = 0; i < codes.Count + 2; i++)
            {
                newCodes.Add(myNOP);
            }
            // push reference to vehicle
            // Call a static function which takes a vehicle and ControlsRotation if it's a ModVehicle
            newCodes[0] = new CodeInstruction(OpCodes.Ldarg_0);
            newCodes[1] = CodeInstruction.Call(typeof(ModVehicle), nameof(ModVehicle.MaybeControlRotation));
            for (int i = 0; i < codes.Count; i++)
            {
                newCodes[i + 2] = codes[i];
            }
            return newCodes.AsEnumerable();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.GetPilotingMode))]
        public static bool GetPilotingModePrefix(Vehicle __instance, ref bool __result)
        {
            VehicleTypes.Drone mv = __instance as VehicleTypes.Drone;
            if (mv == null)
            {
                return true;
            }
            __result = Player.main.GetMode() == Player.Mode.LockedPiloting && mv.IsUnderCommand;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.ReAttach))]
        public static bool VehicleReAttachPrefix(Vehicle __instance)
        {
            IEnumerator NotifyDockingBay(Transform baseCell)
            {
                if(baseCell == null)
                {
                    yield break;
                }
                yield return new WaitUntil(() => baseCell.Find("BaseMoonpool(Clone)") != null);
                var thisBasesBays = baseCell?.GetAllComponentsInChildren<VehicleDockingBay>();
                const float expectedMaxDistance = 5f;
                foreach (VehicleDockingBay bay in thisBasesBays)
                {
                    if(Vector3.Distance(__instance.transform.position, bay.transform.position) < expectedMaxDistance)
                    {
                        bay?.DockVehicle(__instance, false);
                    }
                }
            }
            MainPatcher.Instance.StartCoroutine(NotifyDockingBay(__instance.transform.parent.Find("BaseCell(Clone)")));
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.Awake))]
        public static void VehicleAwakeHarmonyPostfix(Vehicle __instance)
        {
            Admin.GameObjectManager<Vehicle>.Register(__instance);
        }
    }

    [HarmonyPatch(typeof(Vehicle))]
    public class VehiclePatcher2
    {
        /* This transpiler makes one part of UpdateEnergyRecharge more generic
         * Optionally change GetComponentInParent to GetComponentInParentButNotInMe
         * Simple as.
         * The purpose is to ensure ModVehicles are recharged while docked.
         */
        [HarmonyPatch(nameof(Vehicle.UpdateEnergyRecharge))]
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
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().ToLower().Contains("powerrelay"))
                {
                    newCodes[i] = CodeInstruction.Call(typeof(VehiclePatcher2), nameof(VehiclePatcher2.GetPowerRelayAboveVehicle));
                }
                else
                {
                    newCodes[i] = codes[i];
                }
            }
            return newCodes.AsEnumerable();
        }
        public static PowerRelay GetPowerRelayAboveVehicle(Vehicle veh)
        {
            if ((veh as ModVehicle) == null)
            {
                return veh.GetComponentInParent<PowerRelay>();
            }
            else
            {
                return veh.transform.parent.gameObject.GetComponentInParent<PowerRelay>();
            }
        }
    }
}
