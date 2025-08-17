using HarmonyLib;
using UnityEngine;

// PURPOSE: ensures building ghosts truly attach to Submarines, and in a non-problematic way
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(BuilderTool))]
    public class BuilderToolPatcher
    {
        // This patch ensures that building ghosts truly attach to the Submarine they are built in
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BuilderTool.Construct))]
#pragma warning disable IDE0060 // Remove unused parameter
        public static void ConstructPostfix(BuilderTool __instance, Constructable c, bool state, bool start)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            SubRoot? subroot = Player.main?.currentSub;
            if (subroot is not null && subroot.GetComponent<VehicleTypes.Submarine>() != null && c != null)
            {
                if (c.gameObject.GetComponent<LargeWorldEntity>() != null)
                {
                    c.gameObject.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
                }
                c.gameObject.transform.SetParent(subroot.gameObject.transform);
                if(c.gameObject.GetComponent<Rigidbody>())
                {
                    // The architect fabricator from RotA, for example, has a rigidbody for some reason.
                    Component.DestroyImmediate(c.gameObject.GetComponent<Rigidbody>());
                }
            }
        }
    }
}
