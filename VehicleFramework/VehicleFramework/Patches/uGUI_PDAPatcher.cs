using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_PDA))]
    public class uGUI_PDAPatcher
    {
        public static GameObject moduleBuilder;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            Logger.Log("uGUI_PDA awake!");
            initModuleSlots();
            Logger.Log("done initing module slots");

            moduleBuilder = new GameObject("ModuleBuilder");
            moduleBuilder.EnsureComponent<ModuleBuilder>().grabComponents();
        }

        public static void initModuleSlots()
        {
            uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();

            int max_num_modules = 0;
            foreach (VehicleEntry ve in VehicleBuilder.vehicleTypes)
            {
                if(max_num_modules < ve.modules)
                {
                    max_num_modules = ve.modules;
                }
            }

            for (int i = 0; i < max_num_modules; i++)
            {
                GameObject thisModule = new GameObject("ModVehicleModule" + i.ToString());
                thisModule.transform.parent = equipment.transform;
                thisModule.transform.localScale = Vector3.one;
                thisModule.SetActive(false);
                uGUI_EquipmentSlot thisSlot = thisModule.EnsureComponent<uGUI_EquipmentSlot>();
                thisSlot.slot = "ModVehicleModule" + i.ToString();
                thisSlot.manager = equipment;
            }

            GameObject leftArm = new GameObject("VehicleArmLeft");
            leftArm.transform.parent = equipment.transform;
            leftArm.transform.localScale = Vector3.one;
            leftArm.SetActive(false);
            uGUI_EquipmentSlot leftSlot = leftArm.EnsureComponent<uGUI_EquipmentSlot>();
            leftSlot.slot = "VehicleArmLeft";
            leftSlot.manager = equipment;

            GameObject rightArm = new GameObject("VehicleArmRight");
            rightArm.transform.parent = equipment.transform;
            rightArm.transform.localScale = Vector3.one;
            rightArm.SetActive(false);
            uGUI_EquipmentSlot rightSlot = rightArm.EnsureComponent<uGUI_EquipmentSlot>();
            rightSlot.slot = "VehicleArmRight";
            rightSlot.manager = equipment;
        }
    }
}
