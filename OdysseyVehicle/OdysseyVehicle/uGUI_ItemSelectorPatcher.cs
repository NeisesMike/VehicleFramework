using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using VehicleFramework;

namespace OdysseyVehicle
{
    [HarmonyPatch(typeof(uGUI_ItemSelector))]
    class uGUI_ItemSelectorPatcher
    {
        static GameInput.Button[] buttonsCancel = new GameInput.Button[]
        {
        GameInput.Button.RightHand,
        GameInput.Button.Exit,
        GameInput.Button.UICancel
        };

        static bool GetButtonDown(GameInput.Button[] buttons)
        {
            int i = 0;
            int num = buttons.Length;
            while (i < num)
            {
                if (GameInput.GetButtonDown(buttons[i]))
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleInput")]
        public static void OpenPDAPrefix(uGUI_ItemSelector __instance, IItemSelectorManager ___manager)
        {
            // configure the appropriate AllSlots
            if ((___manager as EnergyMixin)?.GetComponentInParent<Odyssey>() != null)
            {
                if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
                {
                    (___manager as EnergyMixin).GetComponentInParent<Odyssey>()?.aiBatterySequence.Set(0.25f, false, null);
                }
                if (GetButtonDown(buttonsCancel))
                {
                    (___manager as EnergyMixin).GetComponentInParent<Odyssey>()?.aiBatterySequence.Set(0.25f, false, null);
                }
            }
        }
    }
}
