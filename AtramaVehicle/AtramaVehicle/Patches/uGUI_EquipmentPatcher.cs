using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(uGUI_Equipment))]
    public class uGUI_EquipmentPatcher
    {
        public static bool hasInited = false;
        public static Dictionary<string, uGUI_EquipmentSlot> atramaAllSlots = new Dictionary<string, uGUI_EquipmentSlot>();

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(uGUI_Equipment __instance, ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            if (!hasInited)
            {
                buildAllSlots(___allSlots);

                hasInited = true;

                ___allSlots = atramaAllSlots;
            }
        }

        public static void buildAllSlots(Dictionary<string, uGUI_EquipmentSlot> thisAllSlots)
        {
            foreach (KeyValuePair<string, uGUI_EquipmentSlot> pair in thisAllSlots)
            {
                atramaAllSlots.Add(pair.Key, pair.Value);
            }

            if(!atramaAllSlots.ContainsKey("AtramaModule1"))
            {
                atramaAllSlots.Add("AtramaModule1", AtramaManager.atramaModuleSlot1);
            }
            else
            {
                atramaAllSlots["AtramaModule1"] = AtramaManager.atramaModuleSlot1;
            }
            if (!atramaAllSlots.ContainsKey("AtramaModule2"))
            {
                atramaAllSlots.Add("AtramaModule2", AtramaManager.atramaModuleSlot2);
            }
            else
            {
                atramaAllSlots["AtramaModule2"] = AtramaManager.atramaModuleSlot2;
            }
            if (!atramaAllSlots.ContainsKey("AtramaModule3"))
            {
                atramaAllSlots.Add("AtramaModule3", AtramaManager.atramaModuleSlot3);
            }
            else
            {
                atramaAllSlots["AtramaModule3"] = AtramaManager.atramaModuleSlot3;
            }
            if (!atramaAllSlots.ContainsKey("AtramaModule4"))
            {
                atramaAllSlots.Add("AtramaModule4", AtramaManager.atramaModuleSlot4);
            }
            else
            {
                atramaAllSlots["AtramaModule4"] = AtramaManager.atramaModuleSlot4;
            }
            if (!atramaAllSlots.ContainsKey("AtramaArmLeft"))
            {
                atramaAllSlots.Add("AtramaArmLeft", AtramaManager.atramaArmSlotLeft);
            }
            else
            {
                atramaAllSlots["AtramaArmLeft"] = AtramaManager.atramaArmSlotLeft;
            }
            if (!atramaAllSlots.ContainsKey("AtramaArmRight"))
            {
                atramaAllSlots.Add("AtramaArmRight", AtramaManager.atramaArmSlotRight);
            }
            else
            {
                atramaAllSlots["AtramaArmRight"] = AtramaManager.atramaArmSlotRight;
            }
        }
    }
}
