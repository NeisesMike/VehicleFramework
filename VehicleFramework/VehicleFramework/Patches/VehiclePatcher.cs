using System.Collections.Generic;
using System.Collections;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;
using VehicleFramework.StorageComponents;

// PURPOSE: generally ensures ModVehicles behave like normal Vehicles
// VALUE: Very high.

namespace VehicleFramework.Patches
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
            if (VehicleTypes.Drone.MountedDrone != null || (__instance as ModVehicle) != null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.OnHandHover))]
        public static bool OnHandHoverPrefix(Vehicle __instance)
        {
            ModVehicle? mv = __instance as ModVehicle;
            if (mv == null)
            {
                if (VehicleTypes.Drone.MountedDrone != null)
                {
                    return false;
                }
            }
            else
            {
                if (mv.IsScuttled)
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
        private static bool ApplyPhysicsMovePrefix(Vehicle __instance)
        {
            return __instance is not ModVehicle;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.LazyInitialize))]
        public static bool LazyInitializePrefix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            ModVehicle? mv = __instance as ModVehicle;
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
            ModVehicle? mv = __instance as ModVehicle;
            if (mv == null || mv.InnateStorages == null)
            {
                return;
            }
            foreach (var tmp in mv.InnateStorages)
            {
                containers.Add(tmp.Container.GetComponent<InnateStorageContainer>().Container);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.IsPowered))]
        public static void IsPoweredPostfix(Vehicle __instance, ref bool __result)
        {
            ModVehicle? mv = __instance as ModVehicle;
            if (mv == null)
            {
                return;
            }
            if (!mv.IsPoweredOn)
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
            List<CodeInstruction> codes = new(instructions);
            List<CodeInstruction> newCodes = new(codes.Count + 2);
            CodeInstruction myNOP = new(OpCodes.Nop);
            for (int i = 0; i < codes.Count + 2; i++)
            {
                newCodes.Add(myNOP);
            }
            // push reference to vehicle
            // Call a static function which takes a vehicle and ControlsRotation if it's a ModVehicle
            newCodes[0] = new(OpCodes.Ldarg_0);
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
            VehicleTypes.Drone? mv = __instance as VehicleTypes.Drone;
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
                VehicleDockingBay[]? thisBasesBays = baseCell.GetAllComponentsInChildren<VehicleDockingBay>();
                const float expectedMaxDistance = 5f;
                if(thisBasesBays == null)
                {
                    yield break;
                }
                foreach (VehicleDockingBay bay in thisBasesBays)
                {
                    if(Vector3.Distance(__instance.transform.position, bay.transform.position) < expectedMaxDistance)
                    {
                        bay?.DockVehicle(__instance, false);
                    }
                }
            }
            Admin.SessionManager.StartCoroutine(NotifyDockingBay(__instance.transform.parent.Find("BaseCell(Clone)")));
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Vehicle.Awake))]
        public static void VehicleAwakeHarmonyPostfix(Vehicle __instance)
        {
            Admin.GameObjectManager<Vehicle>.Register(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Vehicle.UpdateEnergyRecharge))]
        public static bool VehicleUpdateEnergyRechargePrefix(Vehicle __instance)
        {
            ModVehicle? mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;

            }
            bool flag = false;
            mv.energyInterface.GetValues(out float num, out float num2);
            if (mv.docked && mv.timeDocked + 4f < Time.time && num < num2)
            {
                float amount = Mathf.Min(num2 - num, num2 * 0.0025f);
                // Can't check mv for GetComponentInParent<PowerRelay> because mv itself has a PowerRelay component
                // We need to make sure we draw power from the PowerRelay above us
                PowerRelay componentInParent = mv.transform.parent.GetComponentInParent<PowerRelay>();
                if (componentInParent == null)
                {
                    Debug.LogError("vehicle is docked but can't access PowerRelay component");
                }
                componentInParent.ConsumeEnergy(amount, out float num3);
                if (!GameModeUtils.RequiresPower() || num3 > 0f)
                {
                    mv.energyInterface.AddEnergy(num3);
                    flag = true;
                }
            }
            if (flag)
            {
                mv.chargingSound.Play();
                return false;
            }
            mv.chargingSound.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            return false;
        }
    }
}
