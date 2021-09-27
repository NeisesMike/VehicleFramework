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
            if (mv != null)
            {
                return false;
            }
            return true;
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


        [HarmonyPostfix]
        [HarmonyPatch("IsPowered")]
        public static void IsPoweredPostfix(Vehicle __instance, ref EnergyInterface ___energyInterface, ref bool __result)
        {
            ModVehicle mv = __instance as ModVehicle;
            if (mv == null)
            {
                return;
            }
            if(mv.IsDisengaged)
            {
                __result = false;
            }
        }

    }
}
