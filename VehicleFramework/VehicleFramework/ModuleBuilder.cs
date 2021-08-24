using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class ModuleBuilder : MonoBehaviour
    {
        public enum ModuleSlotType
        {
            Default,
            LeftArm,
            RightArm
        }

        public GameObject genericModuleObject;
        public Sprite genericModuleSlotSprite;
        public Material genericModuleSlotMaterial;

        public GameObject backgroundModuleObject;
        public Sprite backgroundSprite;

        public GameObject armModuleObject;
        public Sprite leftArmModuleSlotSprite;
        public Sprite rightArmModuleSlotSprite;
        public Material armModuleSlotMaterial;

        public Transform topLeftSlot = null;
        public Transform bottomRightSlot = null;
        public Transform leftArmSlot = null;

        public void grabComponents()
        {
            StartCoroutine(BuildGenericModulesASAP());
        }
        private IEnumerator BuildGenericModulesASAP()
        {
            while (!uGUI_EquipmentPatcher.hasInited)
            {
                yield return new WaitForSeconds(0.1f);
            }

            foreach (KeyValuePair<string, uGUI_EquipmentSlot> pair in uGUI_EquipmentPatcher.vehicleAllSlots)
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

                            // set module position
                            genericModuleObject.transform.position = topLeftSlot.position;
                            genericModuleObject.transform.localPosition = topLeftSlot.localPosition;

                            // add background child gameobject and components
                            GameObject background = GameObject.Instantiate(new GameObject("Background"), genericModuleObject.transform);
                            // add rt to background
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<RectTransform>(), background);
                            // add cr to background
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), background);
                            // add image to background
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), background);

                            // save these I guess?
                            genericModuleSlotSprite = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            genericModuleSlotMaterial = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // configure slot background image
                            genericModuleObject.EnsureComponent<uGUI_EquipmentSlot>().background = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>();
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.sprite = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.material = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // add iconrect child gameobject
                            GameObject iconRect = GameObject.Instantiate(new GameObject("IconRect"), genericModuleObject.transform);

                            // configure iconrect
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(topLeftSlot.Find("IconRect").GetComponent<RectTransform>(), iconRect);

                            //===============================================================================
                            // get background image components
                            //===============================================================================

                            backgroundModuleObject = new GameObject("BackgroundVehicleModule");
                            backgroundModuleObject.SetActive(false);
                            backgroundModuleObject.transform.position = topLeftSlot.Find("Exosuit").transform.position;
                            backgroundModuleObject.transform.localPosition = topLeftSlot.Find("Exosuit").transform.localPosition;
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<RectTransform>(), backgroundModuleObject);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<CanvasRenderer>(), backgroundModuleObject);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>(), backgroundModuleObject);
                            backgroundSprite = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().sprite;
                            backgroundModuleObject.EnsureComponent<UnityEngine.UI.Image>().material = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().material;

                            //===============================================================================
                            //===============================================================================

                            /*
                            // set module position
                            thisModule.transform.position = source.position;
                            thisModule.transform.localPosition = source.localPosition;

                            // add background child gameobject and components
                            GameObject background = GameObject.Instantiate(new GameObject("Background"), thisModule.transform);
                            // add rt to background
                            AtramaBuilder.CopyComponent(source.Find("Background").GetComponent<RectTransform>(), background);
                            // add cr to background
                            AtramaBuilder.CopyComponent(source.Find("Background").GetComponent<CanvasRenderer>(), background);
                            // add image to background
                            AtramaBuilder.CopyComponent(source.Find("Background").GetComponent<UnityEngine.UI.Image>(), background);

                            // configure slot background image
                            thisModule.GetComponent<uGUI_EquipmentSlot>().background = thisModule.transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
                            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = source.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = source.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // add iconrect child gameobject
                            GameObject iconRect = GameObject.Instantiate(new GameObject("IconRect"), thisModule.transform);

                            // configure iconrect
                            thisModule.GetComponent<uGUI_EquipmentSlot>().iconRect = AtramaBuilder.CopyComponent(source.Find("IconRect").GetComponent<RectTransform>(), iconRect);
                            */

                            //===============================================================================
                            //===============================================================================

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
                            armModuleObject.transform.position = arm.position;
                            armModuleObject.transform.localPosition = arm.localPosition;

                            GameObject background = GameObject.Instantiate(new GameObject("Background"), armModuleObject.transform);
                            VehicleBuilder.CopyComponent(arm.Find("Background").GetComponent<RectTransform>(), background);
                            VehicleBuilder.CopyComponent(arm.Find("Background").GetComponent<CanvasRenderer>(), background);
                            armModuleObject.EnsureComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(arm.Find("Background").GetComponent<UnityEngine.UI.Image>(), background);
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().background.sprite = arm.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().background.material = arm.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            GameObject iconRect = GameObject.Instantiate(new GameObject("IconRect"), armModuleObject.transform);
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(arm.Find("IconRect").GetComponent<RectTransform>(), iconRect);
                            GameObject armBackground = GameObject.Instantiate(new GameObject("ArmBackground"), armModuleObject.transform);
                            VehicleBuilder.CopyComponent(arm.Find("ArmBackground").GetComponent<RectTransform>(), armBackground);
                            VehicleBuilder.CopyComponent(arm.Find("ArmBackground").GetComponent<CanvasRenderer>(), armBackground);
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(arm.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>(), armBackground);
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().background.sprite = arm.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>().sprite;
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().background.material = arm.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>().material;

                            GameObject hint = GameObject.Instantiate(new GameObject("Hint"), armModuleObject.transform);
                            hint.transform.localScale = new Vector3(.75f, .75f, .75f);
                            hint.transform.localEulerAngles = new Vector3(0, 180, 0);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<RectTransform>(), hint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<CanvasRenderer>(), hint);
                            var hintImg = VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<UnityEngine.UI.Image>(), hint);
                            leftArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                            armModuleSlotMaterial = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().material;
                            armModuleObject.GetComponent<uGUI_EquipmentSlot>().hint = hint;

                            break;
                        }
                    case "ExosuitArmRight":
                        rightArmModuleSlotSprite = pair.Value.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                        break;
                    default:
                        break;
                }
            }

            // flag as ready to go
            VehicleBuilder.areModulesReady = true;

            genericModuleObject.SetActive(true);
            backgroundModuleObject.SetActive(true);
            armModuleObject.SetActive(true);
        }

        public void CobbleNewModules(Transform slot1, Transform slot4, Transform leftArm)
        {
            /*
            // top module
            AtramaManager.atramaModule5.transform.position = new Vector3((slot1.position.x + slot4.position.x) / 2, 1.5f * slot1.position.y - leftArm.position.y, 0);
            AtramaManager.atramaModule5.transform.localPosition = new Vector3((slot1.localPosition.x + slot4.localPosition.x) / 2, 1.5f * slot1.localPosition.y - leftArm.localPosition.y, 0);

            GameObject backgroundTop = GameObject.Instantiate(new GameObject("Background"), AtramaManager.atramaModule5.transform);
            AtramaBuilder.CopyComponent(slot1.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            AtramaBuilder.CopyComponent(slot1.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background = AtramaBuilder.CopyComponent(slot1.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background.sprite = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background.material = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

            GameObject iconRectTop = GameObject.Instantiate(new GameObject("IconRect"), AtramaManager.atramaModule5.transform);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().iconRect = AtramaBuilder.CopyComponent(slot1.Find("IconRect").GetComponent<RectTransform>(), iconRectTop);
            */
        }

        public GameObject GetGenericModuleSlot()
        {
            return GameObject.Instantiate(genericModuleObject);
        }
        public GameObject GetBackgroundModuleSlot(int image)
        {
            GameObject bgmSlot = GameObject.Instantiate(backgroundModuleObject);
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
