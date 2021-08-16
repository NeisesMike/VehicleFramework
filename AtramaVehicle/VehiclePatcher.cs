using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(Vehicle))]
    public class VehiclePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool StartPretfix(Vehicle __instance, ref EnergyInterface ___energyInterface)
        {
            if (__instance.gameObject == null || __instance.gameObject.name == null || __instance.gameObject.name != "AtramaPilotChair")
            {
                return true;
            }
            ___energyInterface = __instance.GetComponent<EnergyInterface>();
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandHover")]
        public static void OnHandHoverPrefix(Vehicle __instance)
        {
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
        public static void OnHandClickPrefix(Vehicle __instance, EnergyInterface ___energyInterface)
        {
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
            if (__instance.gameObject == null || __instance.gameObject.name == null || __instance.gameObject.name != "AtramaPilotChair")
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
            if (__instance.gameObject == null || __instance.gameObject.name == null || __instance.gameObject.name != "AtramaPilotChair")
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
            if (__instance.gameObject == null || __instance.gameObject.name == null || __instance.gameObject.name != "AtramaPilotChair")
            {
                return true;
            }


            ___energyInterface = __instance.gameObject.GetComponent<EnergyInterface>();
            /*
            Logger.Log("LazyInitialize!");
            Logger.Log((___energyInterface == null).ToString());
            Logger.Log((__instance.useRigidbody == null).ToString());
            Logger.Log((__instance.upgradesInput == null).ToString());
            Logger.Log((__instance.modulesRoot == null).ToString());
            */
            if (__instance.modulesRoot == null)
            {
                __instance.modulesRoot = __instance.transform.parent.Find("ModulesRootObject").GetComponent<ChildObjectIdentifier>();
            }


            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetStorageInSlot")]
        public static bool GetStorageInSlotPrefix(Vehicle __instance, int slotID, TechType techType, ref ItemsContainer __result)
        {
            if (__instance.gameObject == null || __instance.gameObject.name == null || __instance.gameObject.name != "AtramaPilotChair")
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
            AtramaStorageContainer component = item.GetComponent<AtramaStorageContainer>();
            if (component == null)
            {
                __result = null;
                return false;
            }
            __result = component.container;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("IsPowered")]
        public static void IsPoweredPostfix(EnergyInterface ___energyInterface)
        {
            /*
            Logger.Log((___energyInterface == null).ToString());
            Logger.Log((___energyInterface.sources.Count() == 0).ToString());
            foreach (EnergyMixin mixin in ___energyInterface.sources)
            {
                if (mixin == null)
                {
                    Logger.Log("null mixin");
                }
                else
                {
                    Logger.Log("got mixin: " + mixin.name);
                    Logger.Log(mixin.gameObject.name);
                }
            }
            */
        }


    }
}
