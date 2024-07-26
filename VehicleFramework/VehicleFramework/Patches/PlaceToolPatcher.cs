using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(PlaceTool))]
    public class PlaceToolPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlaceTool.OnPlace))]
        public static void OnPlacePostfix(PlaceTool __instance)
        {
            SubRoot subroot = Player.main.currentSub;
            if (subroot != null && subroot.GetComponent<VehicleTypes.Submarine>())
            {
                Transform aimTransform = Builder.GetAimTransform();
                RaycastHit raycastHit = default(RaycastHit);
                bool flag = false;
                int num = UWE.Utils.RaycastIntoSharedBuffer(aimTransform.position, aimTransform.forward, 5f, -5, QueryTriggerInteraction.UseGlobal);
                float num2 = float.PositiveInfinity;
                for (int i = 0; i < num; i++)
                {
                    RaycastHit raycastHit2 = UWE.Utils.sharedHitBuffer[i];
                    //if (!raycastHit2.collider.isTrigger && !UWE.Utils.SharingHierarchy(__instance.gameObject, raycastHit2.collider.gameObject) && num2 > raycastHit2.distance)
                    if (!raycastHit2.collider.isTrigger && num2 > raycastHit2.distance)
                    {
                        flag = true;
                        raycastHit = raycastHit2;
                        num2 = raycastHit2.distance;
                    }
                }
                if (flag)
                {
                    VehicleTypes.Submarine componentInParent = raycastHit.collider.gameObject.GetComponentInParent<VehicleTypes.Submarine>();
                    __instance.transform.SetParent(componentInParent.transform);
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlaceTool.LateUpdate))]
        public static void LateUpdatePrefix(PlaceTool __instance)
        {
            SubRoot subroot = Player.main.currentSub;
            if (subroot != null && subroot.GetComponent<VehicleTypes.Submarine>())
            {
                if (__instance.usingPlayer != null)
                {
                    Transform aimTransform = Builder.GetAimTransform();
                    RaycastHit raycastHit = default(RaycastHit);
                    bool flag = false;
                    int num = UWE.Utils.RaycastIntoSharedBuffer(aimTransform.position, aimTransform.forward, 5f, -5, QueryTriggerInteraction.UseGlobal);
                    float num2 = float.PositiveInfinity;
                    for (int i = 0; i < num; i++)
                    {
                        RaycastHit raycastHit2 = UWE.Utils.sharedHitBuffer[i];
                        ///if (!raycastHit2.collider.isTrigger && !UWE.Utils.SharingHierarchy(__instance.gameObject, raycastHit2.collider.gameObject) && num2 > raycastHit2.distance)
                        if (!raycastHit2.collider.isTrigger && num2 > raycastHit2.distance)
                        {
                            flag = true;
                            raycastHit = raycastHit2;
                            num2 = raycastHit2.distance;
                        }
                    }
                    if (flag)
                    {
                        VehicleTypes.Submarine componentInParent = raycastHit.collider.gameObject.GetComponentInParent<VehicleTypes.Submarine>();
                        __instance.allowedOnRigidBody = componentInParent != null;
                    }
                }
            }
        }
    }
}
