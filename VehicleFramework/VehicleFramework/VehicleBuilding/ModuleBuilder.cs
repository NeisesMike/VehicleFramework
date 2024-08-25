using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

namespace VehicleFramework
{
    public class ModuleBuilder : MonoBehaviour
    {
        public static ModuleBuilder main;
        public static Dictionary<string, uGUI_EquipmentSlot> vehicleAllSlots = new Dictionary<string, uGUI_EquipmentSlot>();
        public bool isEquipmentInit = false;
        public bool areModulesReady = false;
        public static bool haveWeCalledBuildAllSlots = false;
        public static bool slotExtenderIsPatched = false;
        public static bool slotExtenderHasGreenLight = false;
        public static readonly string LeftArmSlotName = "VehicleArmLeft";
        public static readonly string RightArmSlotName = "VehicleArmRight";

        public void Awake()
        {
            main = this;
        }

        public uGUI_Equipment equipment;

        public GameObject genericModuleObject; // parent object of the regular module slot
        public GameObject armModuleObject; // parent object of the arm module slot
        public GameObject genericModuleIconRect;
        public GameObject genericModuleHint;

        public GameObject modulesBackground; // background image parent object
        public Sprite backgroundSprite; // background image "the vehicle"

        public Sprite genericModuleSlotSprite;
        public Sprite leftArmModuleSlotSprite;
        public Sprite rightArmModuleSlotSprite;

        // These two materials might be the same
        public Material genericModuleSlotMaterial;
        public Material armModuleSlotMaterial;

        public Transform topLeftSlot = null;
        public Transform bottomRightSlot = null;
        public Transform leftArmSlot = null;

        private bool haveSlotsBeenInited = false;

        public void BuildAllSlots()
        {
            StartCoroutine(BuildAllSlotsInternal());
        }
        public IEnumerator BuildAllSlotsInternal()
        {
            while (!haveSlotsBeenInited)
            {
                yield return null;
            }

            if (!vehicleAllSlots.ContainsKey("VehicleModule0"))
            {
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                int max_num_modules = 0;
                foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
                {
                    if(ve.mv is null)
                    {
                        continue;
                    }
                    if (max_num_modules < ve.mv.NumModules)
                    {
                        max_num_modules = ve.mv.NumModules;
                    }
                }
                for (int i = 0; i < max_num_modules; i++)
                {
                    vehicleAllSlots.Add("VehicleModule" + i.ToString(), equipment.transform.Find("VehicleModule" + i.ToString()).GetComponent<uGUI_EquipmentSlot>());
                }
                vehicleAllSlots.Add(ModuleBuilder.LeftArmSlotName, equipment.transform.Find(ModuleBuilder.LeftArmSlotName).GetComponent<uGUI_EquipmentSlot>());
                vehicleAllSlots.Add(ModuleBuilder.RightArmSlotName, equipment.transform.Find(ModuleBuilder.RightArmSlotName).GetComponent<uGUI_EquipmentSlot>());
            }
            else
            {
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                int max_num_modules = 0;
                foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
                {
                    if (ve.mv is null)
                    {
                        continue;
                    }
                    if (max_num_modules < ve.mv.NumModules)
                    {
                        max_num_modules = ve.mv.NumModules;
                    }
                }
                for (int i = 0; i < max_num_modules; i++)
                {
                    vehicleAllSlots["VehicleModule" + i.ToString()] = equipment.transform.Find("VehicleModule" + i.ToString()).GetComponent<uGUI_EquipmentSlot>();
                }
                vehicleAllSlots[ModuleBuilder.LeftArmSlotName] = equipment.transform.Find(ModuleBuilder.LeftArmSlotName).GetComponent<uGUI_EquipmentSlot>();
                vehicleAllSlots[ModuleBuilder.RightArmSlotName] = equipment.transform.Find(ModuleBuilder.RightArmSlotName).GetComponent<uGUI_EquipmentSlot>();
            }

            // Now that we've gotten the data we need,
            // we can let slot extender mangle it
            var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
            if (type2 != null)
            {
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                ModuleBuilder.slotExtenderHasGreenLight = true;
                equipment.Awake();
            }
        }
        public void grabComponents()
        {
            StartCoroutine(BuildGenericModulesASAP());
        }
        private IEnumerator BuildGenericModulesASAP()
        {
            // this function is invoked by PDA.Awake,
            // so that we can access the same PDA here
            // Unfortunately this means we must wait for the player to open the PDA.
            // Maybe we can grab equipment from prefab?
            equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab").GetComponentInChildren<uGUI_Equipment>(true);

            while (!main.isEquipmentInit)
            {
                yield return null;
            }

            foreach (KeyValuePair<string, uGUI_EquipmentSlot> pair in vehicleAllSlots)
            {
                switch (pair.Key)
                {
                    case "ExosuitModule1":
                        {
                            // get slot location
                            topLeftSlot = pair.Value.transform;

                            //===============================================================================
                            // get generic module components
                            //===============================================================================
                            genericModuleObject = new GameObject("GenericVehicleModule");
                            genericModuleObject.SetActive(false);
                            genericModuleObject.transform.SetParent(equipment.transform, false);

                            // set module position
                            genericModuleObject.transform.localPosition = topLeftSlot.localPosition;

                            // add background child gameobject and components
                            var genericModuleBackground = new GameObject("Background");
                            genericModuleBackground.transform.SetParent(genericModuleObject.transform, false);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<RectTransform>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), genericModuleBackground);

                            // save these I guess?
                            genericModuleSlotSprite = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            genericModuleSlotMaterial = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // configure slot background image
                            genericModuleObject.EnsureComponent<uGUI_EquipmentSlot>().background = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>();
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.sprite = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.material = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // add iconrect child gameobject
                            genericModuleIconRect = new GameObject("IconRect");
                            genericModuleIconRect.transform.SetParent(genericModuleObject.transform, false);
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(topLeftSlot.Find("IconRect").GetComponent<RectTransform>(), genericModuleIconRect);

                            //===============================================================================
                            // get background image components
                            //===============================================================================
                            modulesBackground = new GameObject("VehicleModuleBackground");
                            modulesBackground.SetActive(false);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<RectTransform>(), modulesBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<CanvasRenderer>(), modulesBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>(), modulesBackground);
                            backgroundSprite = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().sprite;
                            modulesBackground.EnsureComponent<UnityEngine.UI.Image>().material = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().material;
                            // this can remain active, because its parent's Activity is controlled
                            modulesBackground.SetActive(true);
                            break;
                        }
                    case "ExosuitModule4":
                        // get slot location
                        bottomRightSlot = pair.Value.transform;
                        break;
                    case "ExosuitArmLeft":
                        {
                            // get slot location
                            leftArmSlot = pair.Value.transform;
                            armModuleObject = new GameObject("ArmVehicleModule");
                            armModuleObject.SetActive(false);
                            Transform arm = pair.Value.transform;

                            // adjust the module transform
                            armModuleObject.transform.localPosition = arm.localPosition;

                            // add background child gameobject and components
                            var genericModuleBackground = new GameObject("Background");
                            genericModuleBackground.transform.SetParent(armModuleObject.transform, false);

                            // configure background image
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<RectTransform>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), genericModuleBackground);

                            // add iconrect child gameobject
                             var thisModuleIconRect = new GameObject("IconRect");
                            thisModuleIconRect.transform.SetParent(armModuleObject.transform, false);
                            armModuleObject.EnsureComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(topLeftSlot.Find("IconRect").GetComponent<RectTransform>(), thisModuleIconRect);

                            // add 'hints' to show which arm is which (left vs right)
                            leftArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                            genericModuleHint = new GameObject("Hint");
                            genericModuleHint.transform.SetParent(armModuleObject.transform, false);
                            genericModuleHint.transform.localScale = new Vector3(.75f, .75f, .75f);
                            genericModuleHint.transform.localEulerAngles = new Vector3(0, 180, 0);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<RectTransform>(), genericModuleHint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<CanvasRenderer>(), genericModuleHint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<UnityEngine.UI.Image>(), genericModuleHint);
                            rightArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                            break;
                        }
                    default:
                        break;
                }
            }
            // TODO: fix this constant value somehow
            BuildVehicleModuleSlots(12, true);
            main.areModulesReady = true;
            haveSlotsBeenInited = true;
        }
        public void BuildVehicleModuleSlots(int modules, bool arms)
        {
            int numModules = arms ? modules + 2 : modules;

            // build, link, and position modules
            for (int i=0; i<modules; i++)
            {
                GameObject thisModule = GetGenericModuleSlot();
                thisModule.name = "VehicleModule" + i.ToString();
                thisModule.SetActive(false);
                thisModule.transform.SetParent(equipment.transform, false);
                thisModule.transform.localScale = Vector3.one;
                thisModule.GetComponent<uGUI_EquipmentSlot>().slot = "VehicleModule" + i.ToString();
                thisModule.GetComponent<uGUI_EquipmentSlot>().manager = equipment;

                LinkModule(ref thisModule);

                DistributeModule(ref thisModule, i, numModules);

                if (i == 0)
                {
                    AddBackgroundImage(ref thisModule);
                }
            }

            // build, link, and position left arm
            GameObject leftArm = GetLeftArmSlot();
            if (arms)
            {
                leftArm.name = ModuleBuilder.LeftArmSlotName;
                leftArm.SetActive(false);
                leftArm.transform.SetParent(equipment.transform, false);
                leftArm.transform.localScale = new Vector3(-1, 1, 1); // need to flip this hand to look "left"
                leftArm.EnsureComponent<uGUI_EquipmentSlot>().slot = ModuleBuilder.LeftArmSlotName;
                leftArm.EnsureComponent<uGUI_EquipmentSlot>().manager = equipment;
                LinkArm(ref leftArm);
                DistributeModule(ref leftArm, modules, numModules);
            }

            // build, link, and position right arm
            GameObject rightArm = GetRightArmSlot();
            if (arms)
            {
                rightArm.name = ModuleBuilder.RightArmSlotName;
                rightArm.SetActive(false);
                rightArm.transform.SetParent(equipment.transform, false);
                rightArm.transform.localScale = Vector3.one;
                rightArm.EnsureComponent<uGUI_EquipmentSlot>().slot = ModuleBuilder.RightArmSlotName;
                rightArm.EnsureComponent<uGUI_EquipmentSlot>().manager = equipment;
                LinkArm(ref rightArm);
                DistributeModule(ref rightArm, modules + 1, numModules);
            }
        }
        public void LinkModule(ref GameObject thisModule)
        {
            // add background
            GameObject backgroundTop = thisModule.transform.Find("Background").gameObject;
            VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = genericModuleSlotSprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = genericModuleSlotMaterial;
        }
        public void DistributeModule(ref GameObject thisModule, int position, int numModules)
        {
            int row_size = 4;
            int arrayX = position % row_size;
            int arrayY = position / row_size;

            float centerX = (topLeftSlot.localPosition.x + bottomRightSlot.localPosition.x) / 2;
            float centerY = (topLeftSlot.localPosition.y + bottomRightSlot.localPosition.y) / 2;

            float stepX = Mathf.Abs(topLeftSlot.localPosition.x - centerX);
            float stepY = Mathf.Abs(topLeftSlot.localPosition.y - centerY);

            Vector3 arrayOrigin = new Vector3(centerX - 2 * stepX, centerY - 2 * stepY, 0);

            float thisX = arrayOrigin.x + arrayX * stepX;
            float thisY = arrayOrigin.y + arrayY * stepY;

            thisModule.transform.localPosition = new Vector3(thisX, thisY, 0);
        }
        public void AddBackgroundImage(ref GameObject parent)
        {
            GameObject thisBackground = GameObject.Instantiate(modulesBackground);
            thisBackground.transform.SetParent(parent.transform);
            thisBackground.transform.localRotation = Quaternion.identity;
            thisBackground.transform.localPosition = new Vector3(250,250,0);
            thisBackground.transform.localScale = 5 * Vector3.one;
            thisBackground.EnsureComponent<UnityEngine.UI.Image>().sprite = backgroundSprite;
        }
        public void LinkArm(ref GameObject thisModule)
        {
            // add background
            GameObject backgroundTop = thisModule.transform.Find("Background").gameObject;
            VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(genericModuleObject.transform.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = genericModuleSlotSprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = genericModuleSlotMaterial;
        }
        public GameObject GetGenericModuleSlot()
        {
            return GameObject.Instantiate(genericModuleObject);
        }
        public GameObject GetBackgroundModuleSlot(int image)
        {
            GameObject bgmSlot = GameObject.Instantiate(modulesBackground);
            bgmSlot.GetComponent<UnityEngine.UI.Image>().sprite = backgroundSprite;
            return bgmSlot;
        }
        public GameObject GetLeftArmSlot()
        {
            GameObject armSlot = GameObject.Instantiate(armModuleObject);
            armSlot.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite = leftArmModuleSlotSprite;
            return armSlot;
        }
        public GameObject GetRightArmSlot()
        {
            GameObject armSlot = GameObject.Instantiate(armModuleObject);
            armSlot.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite = rightArmModuleSlotSprite;
            return armSlot;
        }
    }
}
