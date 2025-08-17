using System.Collections;
using HarmonyLib;
using UnityEngine;

// PURPOSE: Resolve an out-of-time error
// VALUE: Dubious. Maybe it doesn't matter, but it seems like it would have an effect on the ProtoSerialization methods. So it could be important.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(CellManager))]
    public static class CellManagerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CellManager.RegisterGlobalEntity))]
        public static bool RegisterGlobalEntityPostfix(CellManager __instance, GameObject ent)
        {
            if (ent.GetComponent<ModVehicle>() == null) return true;
            
            if (__instance.streamer == null || __instance.streamer.globalRoot == null)
            {
                // Sometimes this function is called when streamer.globalRoot is null.
                // Not sure why or by whom.
                // All it does is set the parent, so we'll do that as soon as we possibly can.
                MainPatcher.Instance.StartCoroutine(SetParentEventually(__instance, ent));
                return false;
            }
            return true;
        }

        private static IEnumerator SetParentEventually(CellManager cellManager, GameObject ent)
        {
            yield return new WaitUntil(() => cellManager.streamer != null && cellManager.streamer.globalRoot != null);
            ent.transform.parent = cellManager.streamer.globalRoot.transform;
        }
    }
}
