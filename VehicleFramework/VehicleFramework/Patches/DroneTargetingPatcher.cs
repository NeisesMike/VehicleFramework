using System;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

// PURPOSE: Allow drones to target, hover, and pick things up in an intuitive way.
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Targeting))]
    public static class DroneTargetingPatcher
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
            VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
            if (drone == null)
            {
                goto exit;
            }
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
            VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
            if(drone == null)
            {
                return true;
            }
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

    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Pickupable.OnHandHover))]
        public static void PickupableOnHandHoverPostfix(Pickupable __instance, GUIHand hand)
        {
            VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
            if (drone == null || !hand.IsFreeToInteract() || !__instance.isPickupable)
            {
                return;
            }
            bool hasRoomFor = false;
            foreach (VehicleParts.VehicleStorage vs in drone.InnateStorages)
            {
                var cont = vs.Container.GetComponent<InnateStorageContainer>().container;
                if (cont.HasRoomFor(__instance))
                {
                    hasRoomFor = true;
                    break;
                }
            }
            TechType techType = __instance.GetTechType();
            if (hasRoomFor)
            {
                string text2 = string.Empty;
                ISecondaryTooltip component = __instance.gameObject.GetComponent<ISecondaryTooltip>();
                if (component != null)
                {
                    text2 = component.GetSecondaryTooltip();
                }
                string text = (__instance.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType));
                HandReticle.main.SetIcon(__instance.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand, 1f);
                HandReticle.main.SetText(HandReticle.TextType.Hand, text, false, GameInput.Button.LeftHand);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false, GameInput.Button.None);
            }
            else
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(false), true, GameInput.Button.None);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true, GameInput.Button.None);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pickupable.OnHandClick))]
        public static bool PickupableOnHandClickPostfix(Pickupable __instance, GUIHand hand)
        {
            VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
            if (drone == null || !hand.IsFreeToInteract() || !__instance.isPickupable)
            {
                return true;
            }
            bool hasRoomFor = false;
            foreach (VehicleParts.VehicleStorage vs in drone.InnateStorages)
            {
                var cont = vs.Container.GetComponent<InnateStorageContainer>().container;
                if (cont.HasRoomFor(__instance))
                {
                    hasRoomFor = true;
                    break;
                }
            }
            if (hasRoomFor)
            {
                Inventory.Get().Pickup(__instance, false);
            }
            else
            {
                ErrorMessage.AddWarning(Language.main.Get("InventoryFull"));
            }
            return false;
        }
    }
}