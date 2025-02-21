using HarmonyLib;
using VehicleFramework.Assets;
using TMPro;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_MainMenu))]
    public class FontReferencesPatcher1
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(uGUI_MainMenu.Start))]
        public static void uGUI_MainMenuStartHarmonyPostfix(uGUI_MainMenu __instance)
        {
            FontUtils.Aller_Rg = __instance.transform.Find("Panel/MainMenu/GraphicsDeviceName").GetComponent<TextMeshProUGUI>().font;
        }
    }
    [HarmonyPatch(typeof(uGUI))]
    public class FontReferencesPatcher2
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(uGUI.Awake))]
        public static void uGUIAwakeHarmonyPostfix(uGUI_MainMenu __instance)
        {
            FontUtils.Aller_W_Bd = __instance.transform.Find("ScreenCanvas/HUD/Content/DepthCompass/Compass/NW").GetComponent<TextMeshProUGUI>().font;
        }
    }
}
