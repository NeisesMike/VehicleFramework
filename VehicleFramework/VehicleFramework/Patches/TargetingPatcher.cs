using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Targeting))]
    public static class TargetingPatcher
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(Targeting).GetMethod("GetTarget", new Type[] { typeof(GameObject), typeof(float), typeof(GameObject).MakeByRefType(), typeof(float).MakeByRefType() });
        }

        [HarmonyPrefix]
        public static bool GetTargetPrefix(Targeting __instance, GameObject ignoreObj, float maxDistance, out GameObject result, out float distance, ref bool __result)
        {
            if (ignoreObj != Player.main.gameObject)
            {
                goto exit;
            }
            var list = Admin.GameObjectManager<VehicleTypes.Drone>.Where(x => x.IsPlayerDry);
            if (list.Count() == 0)
            {
                goto exit;
            }
            VehicleTypes.Drone drone = list.First();
            Targeting.AddToIgnoreList(drone.transform);
            __result = Targeting.GetTarget(maxDistance, out result, out distance);
            return false;
        exit:
            result = null;
            distance = 0;
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    public static class InventoryPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.Pickup))]
        public static bool PickupPrefix(Inventory __instance, Pickupable pickupable, ref bool __result)
        {
            var list = Admin.GameObjectManager<VehicleTypes.Drone>.Where(x => x.IsPlayerDry);
            if (list.Count() == 0)
            {
                return true;
            }
            VehicleTypes.Drone drone = list.First();
            foreach (VehicleParts.VehicleStorage vs in drone.InnateStorages)
            {
                var cont = vs.Container.GetComponent<InnateStorageContainer>().container;
                if (cont.HasRoomFor(pickupable))
                {
                    if (cont.AddItem(pickupable) != null)
                    {
                        pickupable.Pickup(true);
                        __result = true;
                        return false;
                    }
                }
            }
            __result = false;
            return false;
        }
    }
}