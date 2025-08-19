using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// PURPOSE: allow custom save file sprites to be displayed
// VALUE: High.

namespace VehicleFramework.Patches
{
    // See also: SaveLoadManagerPatcher
    [HarmonyPatch(typeof(MainMenuLoadPanel))]
    public class MainMenuLoadPanelPatcher
    {
        public static void AddLoadButtonSprites(MainMenuLoadButton lb)
        {
            foreach (var ve in Admin.VehicleManager.vehicleTypes)
            {
                if (ve.mv != null && ve.mv.SaveFileSprite != Assets.StaticAssets.DefaultSaveFileSprite)
                {
                    string techType = ve.techType.AsString();
                    GameObject imageObject = new(techType);
                    imageObject.transform.SetParent(lb.saveIcons.transform, false);
                    imageObject.AddComponent<UnityEngine.UI.Image>().sprite = ve.mv.SaveFileSprite;
                    imageObject.EnsureComponent<RectTransform>().sizeDelta = new(24, 24);
                    imageObject.SetActive(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MainMenuLoadPanel.UpdateLoadButtonState))]
        public static void MainMenuLoadPanelUpdateLoadButtonStatePostfix(MainMenuLoadButton lb)
        {
            // A SaveIcon should be square
            AddLoadButtonSprites(lb);

            if(SaveLoadManagerPatcher.hasTechTypeGameInfo.TryGetValue(lb.saveGame, out List<string>? hasTechTypes))
            {
                hasTechTypes.ForEach(x => lb.saveIcons.FindChild(x)?.gameObject.SetActive(true));
            }

            lb.saveIcons.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().spacing = 0;

            int count = 0;
            foreach (Transform tr in lb.saveIcons.transform)
            {
                if(tr.gameObject.activeInHierarchy)
                {
                    count++;
                }
            }

            foreach (Transform tr in lb.saveIcons.transform)
            {
                if (count > 6)
                {
                    tr.GetComponent<RectTransform>().sizeDelta *= (6 / (float)count);
                }
            }
        }
    }
}
