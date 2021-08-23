using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(uGUI_PDA))]
    public class uGUI_PDAPatcher
    {
        public static GameObject coroutineHelper;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            Logger.Log("uGUI_PDA awake!");
            initModuleSlots();
        }


        public static void initModuleSlots()
        {
            uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();

            List<GameObject> moduleList = new List<GameObject>();

            AtramaManager.atramaModule1 = new GameObject("AtramaModule1");
            AtramaManager.atramaModule2 = new GameObject("AtramaModule2");
            AtramaManager.atramaModule3 = new GameObject("AtramaModule3");
            AtramaManager.atramaModule4 = new GameObject("AtramaModule4");
            AtramaManager.atramaModule5 = new GameObject("AtramaModule5");
            AtramaManager.atramaModule6 = new GameObject("AtramaModule6");
            AtramaManager.atramaArmLeft = new GameObject("AtramaArmLeft");
            AtramaManager.atramaArmRight = new GameObject("AtramaArmRight");

            moduleList.Add(AtramaManager.atramaModule1);
            moduleList.Add(AtramaManager.atramaModule2);
            moduleList.Add(AtramaManager.atramaModule3);
            moduleList.Add(AtramaManager.atramaModule4);
            moduleList.Add(AtramaManager.atramaModule5);
            moduleList.Add(AtramaManager.atramaModule6);
            moduleList.Add(AtramaManager.atramaArmLeft);
            moduleList.Add(AtramaManager.atramaArmRight);


            foreach (GameObject module in moduleList)
            {
                module.transform.parent = equipment.transform;
                module.transform.localScale = Vector3.one;
                module.SetActive(false);
            }


            List<uGUI_EquipmentSlot> moduleSlotList = new List<uGUI_EquipmentSlot>();

            AtramaManager.atramaModuleSlot1 = AtramaManager.atramaModule1.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot2 = AtramaManager.atramaModule2.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot3 = AtramaManager.atramaModule3.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot4 = AtramaManager.atramaModule4.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot5 = AtramaManager.atramaModule5.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot6 = AtramaManager.atramaModule6.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaArmSlotLeft = AtramaManager.atramaArmLeft.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaArmSlotRight = AtramaManager.atramaArmRight.EnsureComponent<uGUI_EquipmentSlot>();

            moduleSlotList.Add(AtramaManager.atramaModuleSlot1);
            moduleSlotList.Add(AtramaManager.atramaModuleSlot2);
            moduleSlotList.Add(AtramaManager.atramaModuleSlot3);
            moduleSlotList.Add(AtramaManager.atramaModuleSlot4);
            moduleSlotList.Add(AtramaManager.atramaModuleSlot6);
            moduleSlotList.Add(AtramaManager.atramaModuleSlot6);
            moduleSlotList.Add(AtramaManager.atramaArmSlotLeft);
            moduleSlotList.Add(AtramaManager.atramaArmSlotRight);

            AtramaManager.atramaModuleSlot1.slot = "AtramaModule1";
            AtramaManager.atramaModuleSlot2.slot = "AtramaModule2";
            AtramaManager.atramaModuleSlot3.slot = "AtramaModule3";
            AtramaManager.atramaModuleSlot4.slot = "AtramaModule4";
            AtramaManager.atramaModuleSlot5.slot = "AtramaModule5";
            AtramaManager.atramaModuleSlot6.slot = "AtramaModule6";
            AtramaManager.atramaArmSlotLeft.slot = "AtramaArmLeft";
            AtramaManager.atramaArmSlotRight.slot = "AtramaArmRight";

            foreach (uGUI_EquipmentSlot moduleSlot in moduleSlotList)
            {
                moduleSlot.manager = equipment;
            }

            Logger.Log("done initing module slots");

            coroutineHelper = new GameObject("CoroutineHelper");
            coroutineHelper.EnsureComponent<CoroutineHelper>().grabComponents();
        }
    }
}
