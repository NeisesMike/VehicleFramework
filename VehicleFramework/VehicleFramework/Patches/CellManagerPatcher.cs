using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(CellManager))]
    public static class CellManagerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CellManager.RegisterGlobalEntity))]
        public static bool RegisterGlobalEntityPostfix(CellManager __instance, GameObject ent)
        {
            if (ent.GetComponent<ModVehicle>() != null && __instance.streamer?.globalRoot is null)
            {
                // Sometimes this function is called when streamer.globalRoot is null
                // So if we're a ModVehicle, we simple will not use this function.
                // It sets the parent of the ModVehicle, but in-game we don't want it to have a parent.
                // So it seems like skipping this function can do no harm.

                // Then again, I'm still not sure why this function is called "out of time."
                // And I'm worried that reason may cause other issues...
                return false;
            }
            return true;
        }
    }
}
