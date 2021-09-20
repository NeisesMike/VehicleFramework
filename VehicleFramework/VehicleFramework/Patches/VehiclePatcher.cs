using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(Vehicle))]
    public class VehiclePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        public static bool AwakePrefix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            ___energyInterface = mv.GetComponent<EnergyInterface>();
            /*
            Logger.Log("adding equipment");
            __instance.modules = new Equipment(mv.gameObject, mv.modulesRoot.transform);
            __instance.modules.SetLabel("VehicleUpgradesStorageLabel");
            __instance.upgradesInput.equipment = __instance.modules;
            */
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool StartPrefix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandHover")]
        public static bool OnHandHoverPrefix(Vehicle __instance)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv != null)
            {
                return false;
            }
            return true;
            /*
            Logger.Log("Hover!");
            Logger.Log(__instance.GetPilotingMode().ToString());
            Logger.Log((__instance.liveMixin == null).ToString());
            Logger.Log((HandReticle.main == null).ToString());
            Logger.Log((__instance.handLabel == null).ToString());
            */
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(Vehicle __instance, EnergyInterface ___energyInterface)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv != null)
            {
                return false;
            }
            return true;
            /*
            Logger.Log("Click!");
            Logger.Log((___energyInterface == null).ToString());
            Logger.Log((__instance.mainAnimator == null).ToString());
            Logger.Log((__instance.noPowerWelcomeNotification == null).ToString());
            Logger.Log((__instance.welcomeNotification == null).ToString());
            Logger.Log((__instance.enterSound == null).ToString());
            */
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyPhysicsMove")]
        private static bool ApplyPhysicsMovePrefix(Vehicle __instance, ref bool ___wasAboveWater, ref VehicleAccelerationModifier[] ___accelerationModifiers)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }

            // TODO
            return false;

            Transform baseTransform = __instance.transform;
            if (__instance.GetPilotingMode())
            {
                if (__instance.worldForces.IsAboveWater() != ___wasAboveWater)
                {
                    __instance.PlaySplashSound();
                    ___wasAboveWater = __instance.worldForces.IsAboveWater();
                }
                bool flag = baseTransform.position.y < Ocean.main.GetOceanLevel() && baseTransform.position.y < __instance.worldForces.waterDepth && !__instance.precursorOutOfWater;
                if (__instance.moveOnLand || flag)
                {
                    if (__instance.controlSheme == Vehicle.ControlSheme.Submersible)
                    {
                        Vector3 vector = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                        vector.Normalize();
                        float d = Mathf.Abs(vector.x) * __instance.sidewardForce + Mathf.Max(0f, vector.z) * __instance.forwardForce + Mathf.Max(0f, -vector.z) * __instance.backwardForce + Mathf.Abs(vector.y * __instance.verticalForce);
                        Vector3 force = baseTransform.rotation * (d * vector) * Time.deltaTime;
                        for (int i = 0; i < ___accelerationModifiers.Length; i++)
                        {
                            ___accelerationModifiers[i].ModifyAcceleration(ref force);
                        }
                        __instance.useRigidbody.AddForce(force, ForceMode.VelocityChange);
                        return false;
                    }
                    /*
                    if (__instance.controlSheme == Vehicle.ControlSheme.Submarine || __instance.controlSheme == Vehicle.ControlSheme.Mech)
                    {
                        Vector3 vector2 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                        Vector3 vector3 = new Vector3(vector2.x, 0f, vector2.z);
                        float num = Mathf.Abs(vector3.x) * __instance.sidewardForce + Mathf.Max(0f, vector3.z) * __instance.forwardForce + Mathf.Max(0f, -vector3.z) * __instance.backwardForce;
                        vector3 = baseTransform.rotation * vector3;
                        vector3.y = 0f;
                        vector3 = Vector3.Normalize(vector3);
                        if (__instance.onGround)
                        {
                            vector3 = Vector3.ProjectOnPlane(vector3, __instance.surfaceNormal);
                            vector3.y = Mathf.Clamp(vector3.y, -0.5f, 0.5f);
                            num *= __instance.onGroundForceMultiplier;
                        }
                        if (Application.isEditor)
                        {
                            Debug.DrawLine(baseTransform.position, baseTransform.position + vector3 * 4f, Color.white);
                        }
                        Vector3 b = new Vector3(0f, vector2.y, 0f);
                        b.y *= __instance.verticalForce * Time.deltaTime;
                        Vector3 force2 = num * vector3 * Time.deltaTime + b;
                        __instance.OverrideAcceleration(ref force2);
                        for (int j = 0; j < ___accelerationModifiers.Length; j++)
                        {
                            ___accelerationModifiers[j].ModifyAcceleration(ref force2);
                        }
                        __instance.useRigidbody.AddForce(force2, ForceMode.VelocityChange);
                    }
                    */
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdateEnergyRecharge")]
        public static bool UpdateEnergyRechargePrefix(Vehicle __instance)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            // TODO
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("LazyInitialize")]
        public static bool LazyInitializePrefix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }

            ___energyInterface = __instance.gameObject.GetComponent<EnergyInterface>();
            /*
            Logger.Log("LazyInitialize!");
            Logger.Log((___energyInterface == null).ToString());
            Logger.Log((mv.useRigidbody == null).ToString());
            Logger.Log((mv.upgradesInput == null).ToString());
            Logger.Log((mv.modulesRoot == null).ToString());
            */
            return true;
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch("GetStorageInSlot")]
        public static bool GetStorageInSlotPrefix(Vehicle __instance, int slotID, TechType techType, ref ItemsContainer __result)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return true;
            }
            InventoryItem slotItem = __instance.GetSlotItem(slotID);
            if (slotItem == null)
            {
                __result = null;
                return false;
            }
            Pickupable item = slotItem.item;
            if (item.GetTechType() != techType)
            {
                __result = null;
                return false;
            }
            VehicleStorageContainer vsc = item.GetComponent<VehicleStorageContainer>();
            SeamothStorageContainer ssc = item.GetComponent<SeamothStorageContainer>();
            if (vsc == null && ssc == null)
            {
                __result = null;
                return false;
            }
            else if(vsc != null)
            {
                __result = vsc.container;
            }
            else if(ssc != null)
            {
                __result = ssc.container;
            }
            return false;
        }
        */

        [HarmonyPostfix]
        [HarmonyPatch("GetAllStorages")]
        public static void GetAllStoragesPostfix(Vehicle __instance, ref List<IItemsContainer> containers)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return;
            }

            foreach(var tmp in ((ModVehicle)__instance).InnateStorages)
            {
                containers.Add(tmp.Container.GetComponent<InnateStorageContainer>().container);
            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch("UnlockDefaultModuleSlots")]
        public static bool UnlockDefaultModuleSlotsPrefix(Vehicle __instance)
        {
            if (__instance.modules == null)
            {
                Logger.Log("skip");
                return false;
            }
            Logger.Log("don't skip!");

            return true;
        }
        */
        /*
        [HarmonyPostfix]
        [HarmonyPatch("modules", MethodType.Getter)]
        public static void modulesGetterPostfix(Vehicle __instance, ref Equipment __result)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv != null)
            {
                Logger.Log("shimming equipment");
                __result = mv.upgradesEquipment;
            }
        }
        */
    }
}
