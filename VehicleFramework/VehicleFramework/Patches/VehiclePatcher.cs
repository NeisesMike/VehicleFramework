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
