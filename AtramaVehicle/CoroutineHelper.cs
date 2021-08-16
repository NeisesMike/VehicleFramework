using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle
{
    public class CoroutineHelper : MonoBehaviour
    {
        public void grabComponents()
        {
            StartCoroutine(grabComponentsASAP());
        }
        private IEnumerator grabComponentsASAP()
        {
            while (!uGUI_EquipmentPatcher.hasInited)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Transform slot1 = null;
            Transform slot4 = null;
            Transform leftArmSlot = null;

            foreach (KeyValuePair<string, uGUI_EquipmentSlot> pair in uGUI_EquipmentPatcher.atramaAllSlots)
            {
                switch(pair.Key)
                {
                    case "ExosuitModule1":
                        cobbleModule(pair.Value.transform, AtramaManager.atramaModule1);
                        createAtramaImage(pair.Value.transform);
                        slot1 = pair.Value.transform;
                        break;
                    case "ExosuitModule2":
                        cobbleModule(pair.Value.transform, AtramaManager.atramaModule2);
                        break;
                    case "ExosuitModule3":
                        cobbleModule(pair.Value.transform, AtramaManager.atramaModule3);
                        break;
                    case "ExosuitModule4":
                        cobbleModule(pair.Value.transform, AtramaManager.atramaModule4);
                        slot4 = pair.Value.transform;
                        break;
                    case "ExosuitArmLeft":
                        cobbleLeftArm(pair.Value.transform, AtramaManager.atramaArmLeft);
                        leftArmSlot = pair.Value.transform;
                        break;
                    case "ExosuitArmRight":
                        cobbleRightArm(pair.Value.transform, AtramaManager.atramaArmRight);
                        break;
                    default:
                        break;
                }
            }

            cobbleNewModules(slot1, slot4, leftArmSlot);

            AtramaManager.atramaModule1.SetActive(true);
            AtramaManager.atramaModule2.SetActive(true);
            AtramaManager.atramaModule3.SetActive(true);
            AtramaManager.atramaModule4.SetActive(true);
            AtramaManager.atramaModule5.SetActive(true);
            AtramaManager.atramaModule6.SetActive(true);
            AtramaManager.atramaArmLeft.SetActive(true);
            AtramaManager.atramaArmRight.SetActive(true);

            // flag as ready to go
            uGUI_EquipmentSlotPatcher.hasInited = true;

            // clean up after ourselves
            Destroy(gameObject);
        }

        public void cobbleModule(Transform source, GameObject thisModule)
        {
            // adjust the module transform
            thisModule.transform.position = source.position;
            thisModule.transform.localPosition = source.localPosition;

            GameObject background = GameObject.Instantiate(new GameObject("Background"), thisModule.transform);
            AtramaPreparer.CopyComponent(source.Find("Background").GetComponent<RectTransform>(), background);
            AtramaPreparer.CopyComponent(source.Find("Background").GetComponent<CanvasRenderer>(), background);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = AtramaPreparer.CopyComponent(source.Find("Background").GetComponent<UnityEngine.UI.Image>(), background);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = source.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = source.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

            GameObject iconRect = GameObject.Instantiate(new GameObject("IconRect"), thisModule.transform);
            thisModule.GetComponent<uGUI_EquipmentSlot>().iconRect = AtramaPreparer.CopyComponent(source.Find("IconRect").GetComponent<RectTransform>(), iconRect);
        }

        public void cobbleArm(Transform source, GameObject thisModule)
        {
            cobbleModule(source, thisModule);

            GameObject armBackground = GameObject.Instantiate(new GameObject("ArmBackground"), thisModule.transform);
            AtramaPreparer.CopyComponent(source.Find("ArmBackground").GetComponent<RectTransform>(), armBackground);
            AtramaPreparer.CopyComponent(source.Find("ArmBackground").GetComponent<CanvasRenderer>(), armBackground);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = AtramaPreparer.CopyComponent(source.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>(), armBackground);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = source.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>().sprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = source.Find("ArmBackground").GetComponent<UnityEngine.UI.Image>().material;
        }

        public void cobbleLeftArm(Transform source, GameObject thisModule)
        {
            cobbleArm(source, thisModule);

            GameObject hint = GameObject.Instantiate(new GameObject("Hint"), thisModule.transform);
            hint.transform.localScale = new Vector3(.75f, .75f, .75f);
            hint.transform.localEulerAngles = new Vector3(0, 180, 0);
            AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<RectTransform>(), hint);
            AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<CanvasRenderer>(), hint);
            var hintImg = AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<UnityEngine.UI.Image>(), hint);
            hintImg.sprite = source.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
            hintImg.material = source.Find("Hint").GetComponent<UnityEngine.UI.Image>().material;
            thisModule.GetComponent<uGUI_EquipmentSlot>().hint = hint;
        }
        public void cobbleRightArm(Transform source, GameObject thisModule)
        {
            cobbleArm(source, thisModule);

            GameObject hint = GameObject.Instantiate(new GameObject("Hint"), thisModule.transform);
            hint.transform.localScale = new Vector3(.75f, .75f, .75f);
            AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<RectTransform>(), hint);
            AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<CanvasRenderer>(), hint);
            var hintImg = AtramaPreparer.CopyComponent(source.Find("Hint").GetComponent<UnityEngine.UI.Image>(), hint);
            hintImg.sprite = source.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
            hintImg.material = source.Find("Hint").GetComponent<UnityEngine.UI.Image>().material;
            thisModule.GetComponent<uGUI_EquipmentSlot>().hint = hint;
        }

        public void createAtramaImage(Transform source)
        {                        
            // create the atrama image here, unique to this slot
            GameObject atramaImage = GameObject.Instantiate(new GameObject("Atrama"), AtramaManager.atramaModule1.transform);
            atramaImage.transform.position = source.Find("Exosuit").transform.position;
            atramaImage.transform.localPosition = source.Find("Exosuit").transform.localPosition;

            var exosuitRect = AtramaPreparer.CopyComponent(source.Find("Exosuit").GetComponent<RectTransform>(), atramaImage);
            exosuitRect.anchorMin = source.Find("Exosuit").GetComponent<RectTransform>().anchorMin;
            exosuitRect.anchorMax = source.Find("Exosuit").GetComponent<RectTransform>().anchorMax;
            exosuitRect.offsetMin = source.Find("Exosuit").GetComponent<RectTransform>().offsetMin;
            exosuitRect.offsetMax = source.Find("Exosuit").GetComponent<RectTransform>().offsetMax;
            exosuitRect.sizeDelta = source.Find("Exosuit").GetComponent<RectTransform>().sizeDelta;

            AtramaPreparer.CopyComponent(source.Find("Exosuit").GetComponent<CanvasRenderer>(), atramaImage);

            var exosuitImage = AtramaPreparer.CopyComponent(source.Find("Exosuit").GetComponent<UnityEngine.UI.Image>(), atramaImage);
            exosuitImage.sprite = source.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().sprite;
            exosuitImage.material = source.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().material;
        }

        public void cobbleNewModules(Transform slot1, Transform slot4, Transform leftArm)
        {
            // top module
            // adjust the module transform
            AtramaManager.atramaModule5.transform.position = new Vector3((slot1.position.x + slot4.position.x) / 2, 1.5f * slot1.position.y - leftArm.position.y, 0);
            AtramaManager.atramaModule5.transform.localPosition = new Vector3((slot1.localPosition.x + slot4.localPosition.x) / 2, 1.5f * slot1.localPosition.y - leftArm.localPosition.y, 0);

            GameObject backgroundTop = GameObject.Instantiate(new GameObject("Background"), AtramaManager.atramaModule5.transform);
            AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background = AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background.sprite = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().background.material = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

            GameObject iconRectTop = GameObject.Instantiate(new GameObject("IconRect"), AtramaManager.atramaModule5.transform);
            AtramaManager.atramaModule5.GetComponent<uGUI_EquipmentSlot>().iconRect = AtramaPreparer.CopyComponent(slot1.Find("IconRect").GetComponent<RectTransform>(), iconRectTop);


            // bottom module
            AtramaManager.atramaModule6.transform.position = new Vector3((slot1.position.x + slot4.position.x) / 2, -1.5f * slot4.position.y + leftArm.position.y, 0);
            AtramaManager.atramaModule6.transform.localPosition = new Vector3((slot1.localPosition.x + slot4.localPosition.x) / 2, -1.5f * slot1.localPosition.y + leftArm.localPosition.y, 0);

            GameObject backgroundBottom = GameObject.Instantiate(new GameObject("Background"), AtramaManager.atramaModule6.transform);
            AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<RectTransform>(), backgroundBottom);
            AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<CanvasRenderer>(), backgroundBottom);
            AtramaManager.atramaModule6.GetComponent<uGUI_EquipmentSlot>().background = AtramaPreparer.CopyComponent(slot1.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundBottom);
            AtramaManager.atramaModule6.GetComponent<uGUI_EquipmentSlot>().background.sprite = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
            AtramaManager.atramaModule6.GetComponent<uGUI_EquipmentSlot>().background.material = slot1.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

            GameObject iconRectBottom = GameObject.Instantiate(new GameObject("IconRect"), AtramaManager.atramaModule6.transform);
            AtramaManager.atramaModule6.GetComponent<uGUI_EquipmentSlot>().iconRect = AtramaPreparer.CopyComponent(slot1.Find("IconRect").GetComponent<RectTransform>(), iconRectBottom);

        }
    }
}
