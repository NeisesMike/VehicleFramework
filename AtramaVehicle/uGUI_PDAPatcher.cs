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
            initModuleSlots();
        }


        public static void initModuleSlots()
        {
            Logger.Log("init module slots");

            uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();

            AtramaManager.atramaModule1 = GameObject.Instantiate(new GameObject("AtramaModule1"), equipment.transform);
            AtramaManager.atramaModule2 = GameObject.Instantiate(new GameObject("AtramaModule2"), equipment.transform);
            AtramaManager.atramaModule3 = GameObject.Instantiate(new GameObject("AtramaModule3"), equipment.transform);
            AtramaManager.atramaModule4 = GameObject.Instantiate(new GameObject("AtramaModule4"), equipment.transform);
            AtramaManager.atramaModule5 = GameObject.Instantiate(new GameObject("AtramaModule5"), equipment.transform);
            AtramaManager.atramaModule6 = GameObject.Instantiate(new GameObject("AtramaModule6"), equipment.transform);
            AtramaManager.atramaArmLeft = GameObject.Instantiate(new GameObject("AtramaArmLeft"), equipment.transform);
            AtramaManager.atramaArmRight = GameObject.Instantiate(new GameObject("AtramaArmRight"), equipment.transform);

            AtramaManager.atramaModule1.SetActive(false);
            AtramaManager.atramaModule2.SetActive(false);
            AtramaManager.atramaModule3.SetActive(false);
            AtramaManager.atramaModule4.SetActive(false);
            AtramaManager.atramaModule5.SetActive(false);
            AtramaManager.atramaModule6.SetActive(false);
            AtramaManager.atramaArmLeft.SetActive(false);
            AtramaManager.atramaArmRight.SetActive(false);

            Logger.Log("adding slots");
            AtramaManager.atramaModuleSlot1 = AtramaManager.atramaModule1.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot2 = AtramaManager.atramaModule2.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot3 = AtramaManager.atramaModule3.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot4 = AtramaManager.atramaModule4.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot5 = AtramaManager.atramaModule5.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaModuleSlot6 = AtramaManager.atramaModule6.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaArmSlotLeft = AtramaManager.atramaArmLeft.EnsureComponent<uGUI_EquipmentSlot>();
            AtramaManager.atramaArmSlotRight = AtramaManager.atramaArmRight.EnsureComponent<uGUI_EquipmentSlot>();

            Logger.Log("naming slots");
            AtramaManager.atramaModuleSlot1.slot = "AtramaModule1";
            AtramaManager.atramaModuleSlot2.slot = "AtramaModule2";
            AtramaManager.atramaModuleSlot3.slot = "AtramaModule3";
            AtramaManager.atramaModuleSlot4.slot = "AtramaModule4";
            AtramaManager.atramaModuleSlot5.slot = "AtramaModule5";
            AtramaManager.atramaModuleSlot6.slot = "AtramaModule6";
            AtramaManager.atramaArmSlotLeft.slot = "AtramaArmLeft";
            AtramaManager.atramaArmSlotRight.slot = "AtramaArmRight";

            Logger.Log("setting managers");
            AtramaManager.atramaModuleSlot1.manager = equipment;
            AtramaManager.atramaModuleSlot2.manager = equipment;
            AtramaManager.atramaModuleSlot3.manager = equipment;
            AtramaManager.atramaModuleSlot4.manager = equipment;
            AtramaManager.atramaModuleSlot5.manager = equipment;
            AtramaManager.atramaModuleSlot6.manager = equipment;
            AtramaManager.atramaArmSlotLeft.manager = equipment;
            AtramaManager.atramaArmSlotRight.manager = equipment;

            Logger.Log("Starting coroutine");
            coroutineHelper = new GameObject("CoroutineHelper");
            coroutineHelper.EnsureComponent<CoroutineHelper>().grabComponents();

        }


    }
}
