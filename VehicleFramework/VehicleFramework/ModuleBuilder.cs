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

        uGUI_Equipment equipment;

        public GameObject genericModuleObject;
        public GameObject genericModuleBackground;
        public GameObject genericModuleIconRect;
        public GameObject genericModuleHint;
        public GameObject modulesBackground;
        public GameObject armModuleObject;

        public Sprite backgroundSprite;
        public Sprite genericModuleSlotSprite;
        public Sprite leftArmModuleSlotSprite;
        public Sprite rightArmModuleSlotSprite;

        public Material genericModuleSlotMaterial;
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
                            genericModuleBackground = GameObject.Instantiate(new GameObject("Background"), genericModuleObject.transform);
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
                            GameObject iconRect = GameObject.Instantiate(new GameObject("IconRect"), genericModuleObject.transform);
                            genericModuleObject.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(topLeftSlot.Find("IconRect").GetComponent<RectTransform>(), iconRect);

                            //===============================================================================
                            // get background image components
                            //===============================================================================

                            modulesBackground = new GameObject("VehicleModuleBackground");
                            modulesBackground.SetActive(false);
                            modulesBackground.transform.position = topLeftSlot.Find("Exosuit").transform.position;
                            modulesBackground.transform.localPosition = topLeftSlot.Find("Exosuit").transform.localPosition;
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<RectTransform>(), modulesBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<CanvasRenderer>(), modulesBackground);
                            VehicleBuilder.CopyComponent(topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>(), modulesBackground);
                            backgroundSprite = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().sprite;
                            modulesBackground.EnsureComponent<UnityEngine.UI.Image>().material = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().material;

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

                            leftArmModuleSlotSprite = arm.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            armModuleSlotMaterial = arm.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            genericModuleHint = GameObject.Instantiate(new GameObject("Hint"), armModuleObject.transform);
                            genericModuleHint.transform.localScale = new Vector3(.75f, .75f, .75f);
                            genericModuleHint.transform.localEulerAngles = new Vector3(0, 180, 0);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<RectTransform>(), genericModuleHint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<CanvasRenderer>(), genericModuleHint);
                            var hintImg = VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<UnityEngine.UI.Image>(), genericModuleHint);
                            rightArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;

                            break;
                        }
                    case "ExosuitArmRight":
                        rightArmModuleSlotSprite = pair.Value.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                        break;
                    default:
                        break;
                }
            }

            BuildAtramaModules(6, true);

            // flag as ready to go
            VehicleBuilder.areModulesReady = true;
        }

        public void BuildAtramaModules(int modules, bool arms)
        {
            equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();

            // build, link, and position modules
            for (int i=0; i<modules; i++)
            {
                GameObject thisModule = new GameObject("VehicleModule" + i.ToString());
                thisModule.SetActive(false);
                thisModule.transform.parent = equipment.transform;
                thisModule.transform.localScale = Vector3.one;

                LinkModule(ref thisModule);

                DistributeModule(ref thisModule, i, modules + 2);
            }
            // build, link, and position left arm

            // build, link, and position right arm

        }

        public void DistributeModule(ref GameObject thisModule, int position, int numModules)
        {
            int row_size = 4;
            int arrayX = position % row_size;
            int arrayY = position / row_size + 1;

            float centerX = (topLeftSlot.position.x + bottomRightSlot.position.x) / 2;
            float centerY = (topLeftSlot.position.y + bottomRightSlot.position.y) / 2;

            float stepX = Mathf.Abs(topLeftSlot.position.x - centerX);
            float stepY = Mathf.Abs(topLeftSlot.position.y - centerY);

            Vector3 arrayOrigin = new Vector3(centerX - stepX, centerY + stepY, 0);

            float thisX = arrayOrigin.x + arrayX * stepX;
            float thisY = arrayOrigin.y + arrayY * stepY;

            thisModule.transform.position = new Vector3(thisX, thisY, 0);
            thisModule.transform.localPosition = new Vector3((topLeftSlot.localPosition.x + bottomRightSlot.localPosition.x) / 2, 1.5f * topLeftSlot.localPosition.y - leftArmSlot.localPosition.y, 0);





            GameObject backgroundTop = GameObject.Instantiate(new GameObject("Background"), thisModule.transform);
            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = topLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

            GameObject iconRectTop = GameObject.Instantiate(new GameObject("IconRect"), thisModule.transform);
            thisModule.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(topLeftSlot.Find("IconRect").GetComponent<RectTransform>(), iconRectTop);
        }
        public void LinkModule(ref GameObject thisModule)
        {
            // add background

            // add hint

            // add iconrect

        }
        public void AddBackgroundImage(ref GameObject thisModule)
        {

        }
        public void LinkArm(ref GameObject thisModule)
        {

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
