using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(GUIHand))]
    public class GUIHandPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GUIHand.OnUpdate))]
        public static void GUIHandOnUpdatePostfix(GUIHand __instance)
        {
            if (Player.main.mode == Player.Mode.Sitting)
            {
                if (AvatarInputHandler.main.IsEnabled() && !__instance.IsPDAInUse() && __instance.grabMode == GUIHand.GrabMode.None)
                {
                    __instance.UpdateActiveTarget();
                    if (__instance.activeTarget.GetComponentInChildren<DroneStation>())
                    {
                        GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, __instance);
                        if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
                        {
                            GUIHand.Send(__instance.activeTarget, HandTargetEventType.Click, __instance);
                        }
                    }
                }
            }
        }
    }
}
