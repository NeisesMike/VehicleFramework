using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VehicleFramework.VehicleTypes
{
    public abstract class Submarine : ModVehicle
    {
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public abstract List<VehicleParts.VehicleFloodLight> FloodLights { get; }
        public abstract List<GameObject> NavigationPortLights { get; }
        public abstract List<GameObject> NavigationStarboardLights { get; }
        public abstract List<GameObject> NavigationPositionLights { get; }
        public abstract List<GameObject> NavigationWhiteStrobeLights { get; }
        public abstract List<GameObject> NavigationRedStrobeLights { get; }
        public abstract List<GameObject> TetherSources { get; }
        public abstract GameObject ControlPanel { get; }
        public virtual GameObject Fabricator { get; }
        public virtual GameObject ColorPicker { get; }
        public virtual GameObject SteeringWheelLeftHandTarget { get; }
        public virtual GameObject SteeringWheelRightHandTarget { get; }
        public virtual ModVehicleEngine Engine { get; set; }
        public virtual List<Light> InteriorLights { get; }


        public ControlPanel controlPanelLogic;
        public bool isPilotSeated = false;
        public bool isPlayerInside = false;

        public Transform thisStopPilotingLocation;

        public FloodLightsController floodlights;
        public InteriorLightsController interiorlights;
        public NavigationLightsController navlights;
        public GameObject fabricator = null; //fabricator


        public virtual List<VehicleParts.VehicleArmProxy> Arms => null;

        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }


        private void TryRemoveDuplicateFabricator()
        {
            bool foundOne = false;
            foreach (Transform tran in transform)
            {
                if (tran.gameObject.name == "Fabricator(Clone)")
                {
                    if (foundOne)
                    {
                        UnityEngine.Object.Destroy(tran.gameObject);
                        continue;
                    }
                    foundOne = true;
                }
            }
        }

        public override void Awake()
        {
            base.Awake();

            if(FloodLights != null)
            {
                floodlights = gameObject.EnsureComponent<FloodLightsController>();
            }
            if (InteriorLights != null)
            {
                interiorlights = gameObject.EnsureComponent<InteriorLightsController>();
            }
            if (NavigationPortLights != null || NavigationStarboardLights != null || NavigationRedStrobeLights != null || NavigationWhiteStrobeLights != null || NavigationPositionLights != null)
            {
                navlights = gameObject.EnsureComponent<NavigationLightsController>();
            }
            if (TetherSources != null)
            {
                TetherSources.ForEach(x => x.EnsureComponent<TetherSource>().mv = this);
            }
            controlPanelLogic?.Init();
        }
        public override void Start()
        {
            base.Start();

            // now that we're in-game, load the color picker
            // we can't do this before we're in-game because not all assets are ready before the game is started
            if (!(ColorPicker is null))
            {
                if (ColorPicker.transform.Find("EditScreen") is null)
                {
                    StartCoroutine(SetupColorPicker());
                }
                else
                {
                    ActualEditScreen = ColorPicker.transform.Find("EditScreen").gameObject;
                }
            }
            // Ensure our name is still good
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;

        }
        public bool IsPlayerInside()
        {
            // this one is correct ?
            return isPlayerInside;
        }
        public bool IsPlayerPiloting()
        {
            return isPilotSeated;
        }
        protected IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
        }
        protected IEnumerator StandUpFromChair()
        {
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
        }
        protected IEnumerator TryStandUpFromChair()
        {
            while (IsPlayerPiloting())
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(2);
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
        }
        public override void BeginPiloting()
        {
            base.BeginPiloting();
            Player.main.EnterSittingMode();
            UWE.CoroutineHost.StartCoroutine(SitDownInChair());
            UWE.CoroutineHost.StartCoroutine(TryStandUpFromChair());
            isPilotSeated = true;
            Player.main.armsController.ikToggleTime = 0;
            Player.main.armsController.SetWorldIKTarget(SteeringWheelLeftHandTarget?.transform, SteeringWheelRightHandTarget?.transform);
            Player.main.SetCurrentSub(GetComponent<SubRoot>());
        }
        public override void StopPiloting()
        {
            if (Player.main.currentSub != null && Player.main.currentSub.name.ToLower().Contains("cyclops"))
            {
                //Unfortunately, this method shares a name with some Cyclops components.
                // PilotingChair.ReleaseBy broadcasts a message for "StopPiloting"
                // So because a docked vehicle is part of the Cyclops heirarchy,
                // it tries to respond, which causes a game crash.
                // So we'll return if the player is within a Cyclops.
                return;
            }
            base.StopPiloting();
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            //StartCoroutine(StandUpFromChair());
            isPilotSeated = false;
            Player.main.SetScubaMaskActive(false);
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            if (!IsVehicleDocked)
            {
                Player.main.transform.SetParent(transform);
                if (thisStopPilotingLocation == null)
                {
                    Logger.Warn("Warning: pilot exit location was null. Defaulting to first tether.");
                    Player.main.transform.position = TetherSources[0].transform.position;
                }
                else
                {
                    Player.main.transform.position = thisStopPilotingLocation.position;
                }
            }
            if(isScuttled)
            {
                UWE.CoroutineHost.StartCoroutine(GrantPlayerInvincibility(3f));
            }
            Player.main.SetCurrentSub(GetComponent<SubRoot>());
        }
        public static IEnumerator GrantPlayerInvincibility(float time)
        {
            Player.main.liveMixin.invincible = true;
            yield return new WaitForSeconds(time);
            Player.main.liveMixin.invincible = false;
        }
        // These two functions control the transition from in the water to the dry interior
        public override bool PlayerEntry()
        {
            if (base.PlayerEntry())
            {
                isPlayerInside = true;
                if (!isScuttled)
                {
                    Player.main.currentMountedVehicle = this;
                    TryRemoveDuplicateFabricator();
                    if (IsVehicleDocked)
                    {

                    }
                    else
                    {
                        Player.main.transform.SetParent(transform);
                        Player.main.playerController.activeController.SetUnderWater(false);
                        Player.main.isUnderwater.Update(false);
                        Player.main.isUnderwaterForSwimming.Update(false);
                        Player.main.playerController.SetMotorMode(Player.MotorMode.Walk);
                        Player.main.motorMode = Player.MotorMode.Walk;
                        Player.main.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);
                    }
                    return true;
                }
            }
            return false;
        }
        public override void PlayerExit()
        {
            base.PlayerExit();
            //Player.main.currentSub = null;
            isPlayerInside = false;
            Player.main.currentMountedVehicle = null;
            if (!IsVehicleDocked)
            {
                Player.main.transform.SetParent(null);
            }
        }
        public override void SubConstructionBeginning()
        {
            base.SubConstructionBeginning();
            PaintVehicleDefaultStyle(OGVehicleName);
        }
        public override void SubConstructionComplete()
        {
            base.SubConstructionComplete();
            PaintNameDefaultStyle(OGVehicleName);
            // Setup the color picker with the odyssey's name
            var active = transform.Find("ColorPicker/EditScreen/Active");
            if (active)
            {
                active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = NowVehicleName;
                active.transform.Find("InputField/Text").GetComponent<TMPro.TextMeshProUGUI>().text = NowVehicleName;
            }

            IEnumerator TrySpawnFabricator()
            {
                Transform fabLoc = Fabricator.transform;
                if (fabLoc is null)
                {
                    fabLoc = transform.Find("Fabricator-Location");
                    if (fabLoc is null)
                    {
                        Logger.Warn("Warning: " + name + " does not have a Fabricator-Location.");
                        yield break;
                    }
                }

                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return StartCoroutine(CraftData.InstantiateFromPrefabAsync(TechType.Fabricator, result, false));
                fabricator = result.Get();
                fabricator.GetComponent<SkyApplier>().enabled = true;
                fabricator.transform.SetParent(transform);
                fabricator.transform.localPosition = fabLoc.localPosition;
                fabricator.transform.localRotation = fabLoc.localRotation;
                fabricator.transform.localScale = fabLoc.transform.localScale;
                if (fabLoc.transform.localScale.x == 0 || fabLoc.transform.localScale.y == 0 || fabLoc.transform.localScale.z == 0)
                {
                    fabricator.transform.localScale = Vector3.one;
                }
                yield break;
            }
            if (Fabricator != null)
            {
                StartCoroutine(TrySpawnFabricator());
            }
        }
        public virtual void PaintNameDefaultStyle(string name)
        {
            OnNameChangeMaybe(name);
        }
        public virtual void PaintVehicleDefaultStyle(string name)
        {
            ExteriorMainColor = Color.white;
            OldExteriorMainColor = Color.white;
            ExteriorPrimaryAccent = Color.blue;
            OldExteriorPrimaryAccent = Color.blue;
            ExteriorSecondaryAccent = Color.grey;
            OldExteriorSecondaryAccent = Color.grey;
            PaintNameDefaultStyle(name);
        }
        public enum TextureDefinition : int
        {
            twice = 4096,
            full = 2048,
            half = 1024
        }
        public virtual void PaintVehicleSection(string materialName, Color col)
        {
        }
        public virtual void PaintVehicleName(string name, Color nameColor, Color hullColor)
        {
            OnNameChangeMaybe(name);
        }





        public Color ExteriorMainColor;
        public Color ExteriorPrimaryAccent;
        public Color ExteriorSecondaryAccent;
        public Color ExteriorNameLabel;
        protected Color OldExteriorMainColor;
        protected Color OldExteriorPrimaryAccent;
        protected Color OldExteriorSecondaryAccent;
        protected Color OldExteriorNameLabel;
        protected string OGVehicleName;
        public string NowVehicleName;
        protected string OldVehicleName;
        public bool IsDefaultTexture = true;
        public virtual void SetColorPickerUIColor(string name, Color col)
        {
            ActualEditScreen.transform.Find("Active/" + name + "/SelectedColor").GetComponent<Image>().color = col;
        }
        public virtual void OnColorChange(ColorChangeEventData eventData)
        {
            // determine which tab is selected
            // call the desired function
            List<string> tabnames = new List<string>() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
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
                case "MainExterior":
                    IsDefaultTexture = false;
                    OldExteriorMainColor = ExteriorMainColor;
                    ExteriorMainColor = eventData.color;
                    break;
                case "PrimaryAccent":
                    IsDefaultTexture = false;
                    OldExteriorPrimaryAccent = ExteriorPrimaryAccent;
                    ExteriorPrimaryAccent = eventData.color;
                    break;
                case "SecondaryAccent":
                    IsDefaultTexture = false;
                    OldExteriorSecondaryAccent = ExteriorSecondaryAccent;
                    ExteriorSecondaryAccent = eventData.color;
                    break;
                case "NameLabel":
                    //IsDefaultTexture = false;
                    OldExteriorNameLabel = ExteriorNameLabel;
                    ExteriorNameLabel = eventData.color;
                    break;
                default:
                    break;
            }
            ActualEditScreen.transform.Find("Active/MainExterior/SelectedColor").GetComponent<Image>().color = ExteriorMainColor;
        }
        public virtual void OnNameChangeMaybe(string e)
        {
            if (NowVehicleName != e)
            {
                OldVehicleName = NowVehicleName;
                NowVehicleName = e;
                vehicleName = e;
            }
        }
        public virtual void OnNameChange(string e)
        {
            OldVehicleName = NowVehicleName;
            NowVehicleName = e;
            vehicleName = e;
        }
        public virtual void OnColorSubmit()
        {
            if (ExteriorMainColor != OldExteriorMainColor)
            {
                PaintVehicleSection("ExteriorMainColor", ExteriorMainColor);
            }
            if (ExteriorPrimaryAccent != OldExteriorPrimaryAccent)
            {
                PaintVehicleSection("ExteriorPrimaryAccent", ExteriorPrimaryAccent);
            }
            if (ExteriorSecondaryAccent != OldExteriorSecondaryAccent)
            {
                PaintVehicleSection("ExteriorSecondaryAccent", ExteriorSecondaryAccent);
            }
            if (IsDefaultTexture)
            {
                PaintVehicleDefaultStyle(NowVehicleName);
            }
            else
            {
                PaintVehicleName(NowVehicleName, ExteriorNameLabel, ExteriorMainColor);
            }
            return;
        }

        public GameObject ActualEditScreen = null;

        public IEnumerator SetupColorPicker()
        {
            UnityAction CreateAction(string name)
            {
                void Action()
                {
                    List<string> tabnames = new List<string>() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
                    foreach (string tab in tabnames.FindAll(x => x != name))
                    {
                        ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.SetActive(false);
                    }
                    ActualEditScreen.transform.Find("Active/" + name + "/Background").gameObject.SetActive(true);
                }
                return Action;
            }

            GameObject console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>()?.ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;

            if (console is null)
            {
                yield return StartCoroutine(Builder.BeginAsync(TechType.BaseUpgradeConsole));
                Builder.ghostModel.GetComponentInChildren<BaseGhost>().OnPlace();
                console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>().ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;
                Builder.End();
            }
            ActualEditScreen = GameObject.Instantiate(console.transform.Find("EditScreen").gameObject);
            ActualEditScreen.GetComponentInChildren<SubNameInput>().enabled = false;
            ActualEditScreen.name = "EditScreen";
            ActualEditScreen.SetActive(true);
            ActualEditScreen.transform.Find("Inactive").gameObject.SetActive(false);


            GameObject frame = ColorPicker;
            ActualEditScreen.transform.SetParent(frame.transform);
            ActualEditScreen.transform.localPosition = new Vector3(.15f, .28f, 0.01f);
            ActualEditScreen.transform.localEulerAngles = new Vector3(0, 180, 0);

            var but = ActualEditScreen.transform.Find("Active/BaseTab");
            but.name = "MainExterior";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.MainExterior);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("MainExterior"));

            but = ActualEditScreen.transform.Find("Active/NameTab");
            but.name = "PrimaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.PrimaryAccent);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("PrimaryAccent"));

            but = ActualEditScreen.transform.Find("Active/InteriorTab");
            but.name = "SecondaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.SecondaryAccent);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("SecondaryAccent"));

            but = ActualEditScreen.transform.Find("Active/Stripe1Tab");
            but.name = "NameLabel";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.NameLabel);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("NameLabel"));

            GameObject colorPicker = ActualEditScreen.transform.Find("Active/ColorPicker").gameObject;
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.RemoveAllListeners();
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.AddListener(new UnityAction<ColorChangeEventData>(OnColorChange));
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.AddListener(new UnityAction(OnColorSubmit));
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.AddListener(new UnityAction<string>(OnNameChange));
            yield break;
        }

        public override void OnAIBatteryReload()
        {
        }

        // this function returns the number of seconds to wait before opening the PDF,
        // to show off the cool animations~
        public override float OnStorageOpen(string name, bool open)
        {
            return 0;
        }

        public override void ModVehicleReset()
        {
        }
        public void EnableFabricator(bool enabled)
        {
            foreach (Transform tran in transform)
            {
                if (tran.gameObject.name == "Fabricator(Clone)")
                {
                    fabricator = tran.gameObject;
                    fabricator.GetComponentInChildren<Fabricator>().enabled = enabled;
                    fabricator.GetComponentInChildren<Collider>().enabled = enabled;
                    //fabricator.SetActive(enabled);
                }
            }
        }
        public override void OnVehicleDocked()
        {
            base.OnVehicleDocked();
            Hatches.ForEach(x => x.Hatch.GetComponent<VehicleHatch>().isLive = false);
            PilotSeats.ForEach(x => x.Seat.GetComponent<PilotingTrigger>().isLive = false);
            TetherSources.ForEach(x => x.GetComponent<TetherSource>().isLive = false);
            EnableFabricator(false);
        }
        public override void OnVehicleUndocked()
        {
            Hatches.ForEach(x => x.Hatch.GetComponent<VehicleHatch>().isLive = true);
            PilotSeats.ForEach(x => x.Seat.GetComponent<PilotingTrigger>().isLive = true);
            TetherSources.ForEach(x => x.GetComponent<TetherSource>().isLive = true);
            EnableFabricator(true);
            base.OnVehicleUndocked();
        }
        public override void OnPlayerDocked()
        {
            StopPiloting();
            base.OnPlayerDocked();
            //UWE.CoroutineHost.StartCoroutine(TryStandUpFromChair());
        }
        public override void OnPlayerUndocked()
        {
            base.OnPlayerUndocked();
            BeginPiloting();
        }
        public override void ScuttleVehicle()
        {
            base.ScuttleVehicle();
            EnableFabricator(false);
        }
        public override void UnscuttleVehicle()
        {
            base.UnscuttleVehicle();
            EnableFabricator(true);
        }
    }
}
