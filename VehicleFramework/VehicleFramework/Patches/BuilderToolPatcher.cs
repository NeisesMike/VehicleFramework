using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

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
                if (c != null && !c.constructed && __instance.HasEnergyOrInBase())
                {
                    c.gameObject.transform.SetParent(subroot.gameObject.transform);
                }
            }
        }
    }
}
