using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VehicleFramework.Interfaces;

namespace VehicleFramework.VehicleComponents
{
    /*
     * ColorPicker is a component that provides a color picker UI for changing vehicle colors.
     */
    public class ColorPicker : MonoBehaviour, IPlayerListener
    {
        private ModVehicle MV = null!;
        private const string EditScreenName = "EditScreen";
        private const string MainExteriorName = "MainExterior";
        private const string PrimaryAccentName = "PrimaryAccent";
        private const string SecondaryAccentName = "SecondaryAccent";
        private const string NameLabelName = "NameLabel";
        private readonly List<string> tabnames = new() { MainExteriorName, PrimaryAccentName, SecondaryAccentName, NameLabelName };
        private GameObject? ActualEditScreen = null;

        // this type pair is (isSet, color)
        private (bool, Color) tempMainExterior = (false, Color.white);
        private (bool, Color) tempPrimaryAccent = (false, Color.white);
        private (bool, Color) tempSecondaryAccent = (false, Color.white);
        private (bool, Color) tempNameLabel = (false, Color.white);


        private void Awake()
        {
            Admin.SessionManager.StartCoroutine(SetupColorPicker());
        }
        internal void Init(ModVehicle mv)
        {
            MV = mv;
            if(MV == null)
            {
                throw Admin.SessionManager.Fatal("MV == null in ColorPicker.Init!");
            }
        }
        private IEnumerator SetupColorPicker()
        {
            yield return new WaitUntil(() => MV != null);

            UnityAction CreateAction(string name)
            {
                void Action()
                {
                    foreach (string tab in tabnames.FindAll(x => x != name))
                    {
                        ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.SetActive(false);
                    }
                    ActualEditScreen.transform.Find("Active/" + name + "/Background").gameObject.SetActive(true);
                }
                return Action;
            }

            GameObject console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>().ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;

            if (console == null)
            {
                yield return Admin.SessionManager.StartCoroutine(Builder.BeginAsync(TechType.BaseUpgradeConsole));
                Builder.ghostModel.GetComponentInChildren<BaseGhost>().OnPlace();
                console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>().ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;
                Builder.End();
            }
            ActualEditScreen = GameObject.Instantiate(console.transform.Find(EditScreenName).gameObject);
            ActualEditScreen.GetComponentInChildren<SubNameInput>().enabled = false;
            ActualEditScreen.name = EditScreenName;
            ActualEditScreen.SetActive(true);
            ActualEditScreen.transform.Find("Inactive").gameObject.SetActive(false);
            Vector3 originalLocalScale = ActualEditScreen.transform.localScale;


            ActualEditScreen.transform.SetParent(transform);
            ActualEditScreen.transform.localPosition = new(.15f, .28f, 0.01f);
            ActualEditScreen.transform.localEulerAngles = new(0, 180, 0);
            ActualEditScreen.transform.localScale = originalLocalScale;

            var tempString = ActualEditScreen.transform.Find("Active/BaseTab");
            tempString.name = tabnames[0];
            tempString.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFMainExterior");
            tempString.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction(tabnames[0]));

            tempString = ActualEditScreen.transform.Find("Active/NameTab");
            tempString.name = tabnames[1];
            tempString.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFPrimaryAccent");
            tempString.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction(tabnames[1]));

            tempString = ActualEditScreen.transform.Find("Active/InteriorTab");
            tempString.name = tabnames[2];
            tempString.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFSecondaryAccent");
            tempString.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction(tabnames[2]));

            tempString = ActualEditScreen.transform.Find("Active/Stripe1Tab");
            tempString.name = tabnames[3];
            tempString.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFNameLabel");
            tempString.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction(tabnames[3]));

            GameObject colorPicker = ActualEditScreen.transform.Find("Active/ColorPicker").gameObject;
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.RemoveAllListeners();
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.AddListener(new(OnColorChange));
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.AddListener(new UnityAction(OnColorSubmit));
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.AddListener(new(OnNameChange));

            EnsureColorPickerEnabled();
            yield break;
        }
        private void EnsureColorPickerEnabled()
        {
            if(transform.Find(EditScreenName) == null)
            {
                return;
            }
            ActualEditScreen = transform.Find(EditScreenName).gameObject;
            if(ActualEditScreen == null)
            {
                return;
            }
            // why is canvas sometimes disabled, and Active is sometimes inactive?
            // Don't know!
            ActualEditScreen.GetComponent<Canvas>().enabled = true;
            ActualEditScreen.transform.Find("Active").gameObject.SetActive(true);
        }
        private void OnColorChange(ColorChangeEventData eventData)
        {
            // determine which tab is selected
            // call the desired function
            if (ActualEditScreen == null)
            {
                return;
            }
            string selectedTab = "";
            foreach (string tab in tabnames)
            {
                if (ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.activeSelf)
                {
                    selectedTab = tab;
                    break;
                }
            }
            SetColorPickerUIColor(selectedTab, eventData.color);
            switch (selectedTab)
            {
                case MainExteriorName:
                    tempMainExterior = (true, eventData.color);
                    break;
                case PrimaryAccentName:
                    tempPrimaryAccent = (true, eventData.color);
                    break;
                case SecondaryAccentName:
                    tempSecondaryAccent = (true, eventData.color);
                    break;
                case NameLabelName:
                    tempNameLabel = (true, eventData.color);
                    break;
            }
        }
        private void OnNameChange(string e) // why is this independent from OnNameChange?
        {
            if (!string.Equals(MV.vehicleName, e))
            {
                MV.SetName(e);
            }
        }
        private void OnColorSubmit() // called by color picker submit button
        {
            if(tempMainExterior.Item1)
            {
                MV.SetBaseColor(tempMainExterior.Item2);
                tempMainExterior = (false, Color.white);
            }
            if (tempPrimaryAccent.Item1)
            {
                MV.SetInteriorColor(tempPrimaryAccent.Item2);
                tempPrimaryAccent = (false, Color.white);
            }
            if (tempSecondaryAccent.Item1)
            {
                MV.SetStripeColor(tempSecondaryAccent.Item2);
                tempSecondaryAccent = (false, Color.white);
            }
            if (tempNameLabel.Item1)
            {
                MV.SetName(MV.GetName(), tempNameLabel.Item2);
                tempNameLabel = (false, Color.white);
            }
        }
        private void SetColorPickerUIColor(string name, Color col)
        {
            if (ActualEditScreen == null)
            {
                return;
            }
            ActualEditScreen.transform.Find("Active/" + name + "/SelectedColor").GetComponent<Image>().color = col;
        }
        internal void BumpNameDecals()
        {
            var active = transform.Find("EditScreen/Active");
            if (active)
            {
                active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = MV.GetName();
                active.transform.Find("InputField/Text").GetComponent<TMPro.TextMeshProUGUI>().text = MV.GetName();
            }
        }

        void IPlayerListener.OnPlayerEntry()
        {
            EnsureColorPickerEnabled();
        }

        void IPlayerListener.OnPlayerExit()
        {
        }

        void IPlayerListener.OnPilotBegin()
        {
        }

        void IPlayerListener.OnPilotEnd()
        {
        }
    }
}
