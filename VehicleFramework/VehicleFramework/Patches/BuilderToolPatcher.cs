using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(BuilderTool))]
    public class BuilderToolPatcher
    {
        // This patch ensures that building ghosts truly attach to the Submarine they are built in
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BuilderTool.Construct))]
        public static void ConstructPostfix(BuilderTool __instance, Constructable c, bool state, bool start)
        {
            SubRoot subroot = Player.main.currentSub;
            if (subroot != null && subroot.GetComponent<VehicleTypes.Submarine>())
            {
                if (c != null && __instance.HasEnergyOrInBase())
                {
                    if (!c.constructed)
                    {
                        if (c.gameObject.GetComponent<LargeWorldEntity>() != null)
                        {
                            c.gameObject.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
                        }
                        c.gameObject.transform.SetParent(subroot.gameObject.transform);
                    }
                }
            }
        }
    }
}
