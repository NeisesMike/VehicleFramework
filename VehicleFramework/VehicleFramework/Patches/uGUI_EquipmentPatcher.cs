using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_Equipment))]
    public class uGUI_EquipmentPatcher
    {
        public static bool hasInited = false;
        public static Dictionary<string, uGUI_EquipmentSlot> vehicleAllSlots = new Dictionary<string, uGUI_EquipmentSlot>();

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(uGUI_Equipment __instance, ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            if (!hasInited)
            {
                buildAllSlots(___allSlots);
                hasInited = true;
                ___allSlots = vehicleAllSlots;
            }
        }

        public static void buildAllSlots(Dictionary<string, uGUI_EquipmentSlot> thisAllSlots)
        {
            foreach (KeyValuePair<string, uGUI_EquipmentSlot> pair in thisAllSlots)
            {
                if (!vehicleAllSlots.ContainsKey(pair.Key))
                {
                    vehicleAllSlots.Add(pair.Key, pair.Value);
                }
                else
                {
                    vehicleAllSlots[pair.Key] = pair.Value;
                }
            }

            if(!vehicleAllSlots.ContainsKey("ModVehicleModule0"))
            {
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                int max_num_modules = 0;
                foreach (VehicleEntry ve in VehicleBuilder.vehicleTypes)
                {
                    if (max_num_modules < ve.modules)
                    {
                        max_num_modules = ve.modules;
                    }
                }
                for (int i = 0; i < max_num_modules; i++)
                {
                    vehicleAllSlots.Add("ModVehicleModule" + i.ToString(), equipment.transform.Find("ModVehicleModule" + i.ToString()).GetComponent<uGUI_EquipmentSlot>());
                }
                vehicleAllSlots.Add("VehicleArmLeft", equipment.transform.Find("VehicleArmLeft").GetComponent<uGUI_EquipmentSlot>());
                vehicleAllSlots.Add("VehicleArmRight", equipment.transform.Find("VehicleArmRight").GetComponent<uGUI_EquipmentSlot>());
            }
            else
            {
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                int max_num_modules = 0;
                foreach (VehicleEntry ve in VehicleBuilder.vehicleTypes)
                {
                    if (max_num_modules < ve.modules)
                    {
                        max_num_modules = ve.modules;
                    }
                }
                for (int i = 0; i < max_num_modules; i++)
                {
                    vehicleAllSlots["ModVehicleModule" + i.ToString()] = equipment.transform.Find("ModVehicleModule" + i.ToString()).GetComponent<uGUI_EquipmentSlot>();
                }
                vehicleAllSlots["VehicleArmLeft"] = equipment.transform.Find("VehicleArmLeft").GetComponent<uGUI_EquipmentSlot>();
                vehicleAllSlots["VehicleArmRight"] = equipment.transform.Find("VehicleArmRight").GetComponent<uGUI_EquipmentSlot>();
            }
        }
    }
}
