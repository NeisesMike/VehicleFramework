using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using VehicleFramework;
using UnityEngine;

namespace OdysseyVehicle
{

    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    class VehicleUpgradeConsoleInputPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void OpenPDAPrefix(VehicleUpgradeConsoleInput __instance, Sequence ___sequence)
        {
            // control opening the modules hatch
            if (__instance.GetComponentInParent<Odyssey>() != null)
            {
                if (___sequence.active)
                {
                    Quaternion quaternion = Quaternion.Euler(__instance.anglesClosed);
                    Quaternion quaternion2 = Quaternion.Euler(-__instance.anglesOpened);
                    __instance.GetComponentInParent<Odyssey>().transform.Find("Geometry/Exterior Panels/Panel Left/DoorLeftBottomHinge").localRotation = Quaternion.Lerp(quaternion, quaternion2, ___sequence.t);
                }
            }
        }
    }
}
