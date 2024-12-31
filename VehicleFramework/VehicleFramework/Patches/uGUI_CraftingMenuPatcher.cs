using HarmonyLib;
using System.Collections.Generic;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_CraftingMenu))]
    public class uGUI_CraftingMenuPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(uGUI_CraftingMenu.Expand))]
        public static void uGUI_CraftingMenuExpandPostfix(uGUI_CraftingMenu __instance, uGUI_CraftingMenu.Node node)
        {
            if(!MainPatcher.VFConfig.isCraftingMenuFix)
            {
                return;
            }
            uGUI_CraftingMenu.Node parent = node.parent as uGUI_CraftingMenu.Node;
            if (node.parent == null)
            {
                return;
            }
            if (__instance.IsGrid(node))
            {
                using (IEnumerator<uGUI_CraftingMenu.Node> enumerator2 = parent.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        uGUI_CraftingMenu.Node thisChild = enumerator2.Current;
                        if (thisChild == node)
                        {
                            continue;
                        }
                        if (thisChild.action == TreeAction.Expand)
                        {
                            thisChild.icon.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
