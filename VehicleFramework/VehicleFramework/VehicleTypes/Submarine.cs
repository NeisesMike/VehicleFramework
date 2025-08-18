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
//using VehicleFramework.Localization;

namespace VehicleFramework.VehicleTypes
{
    /*
     * Submarine is the class of self-leveling, walkable submarines
     */
    public abstract class Submarine : ModVehicle
    {
        public abstract VehicleParts.VehiclePilotSeat PilotSeat { get; } // Need a way to start and stop piloting
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; } // Need a way to get in and out.
        public virtual List<VehicleParts.VehicleFloodLight> FloodLights => null;
        public virtual List<GameObject> TetherSources => null;
        public virtual GameObject ControlPanel => null;
        public virtual GameObject Fabricator => null;
        public virtual GameObject ColorPicker => null;
        public virtual GameObject SteeringWheelLeftHandTarget => null;
        public virtual GameObject SteeringWheelRightHandTarget => null;
        public virtual List<Light> InteriorLights => null;
        public virtual List<GameObject> NavigationPortLights => null;
        public virtual List<GameObject> NavigationStarboardLights => null;
        public virtual List<GameObject> NavigationPositionLights => null;
        public virtual List<GameObject> NavigationWhiteStrobeLights => null;
        public virtual List<GameObject> NavigationRedStrobeLights => null;
        public virtual float ExitPitchLimit => 4f;
        public virtual float ExitRollLimit => 4f;
        public virtual float ExitVelocityLimit => 0.5f;
        public virtual GameObject RespawnPoint => null;
        public virtual bool DoesAutolevel => true;


        public ControlPanel controlPanelLogic;
        private bool isPilotSeated = false;
        private bool isPlayerInside = false; // You can be inside a scuttled submarine yet not dry.

        public Transform? thisStopPilotingLocation;

        public FloodLightsController floodlights;
        public InteriorLightsController interiorlights;
        public NavigationLightsController navlights;
        public GameObject fabricator = null; //fabricator

        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }

        public override void Awake()
        {
            base.Awake();
            floodlights = gameObject.AddComponent<FloodLightsController>();
            interiorlights = gameObject.AddComponent<InteriorLightsController>();
            navlights = gameObject.AddComponent<NavigationLightsController>();
            gameObject.EnsureComponent<TetherSource>();
            controlPanelLogic?.Init();
        }
        public override void Start()
        {
            base.Start();

            // now that we're in-game, load the color picker
            // we can't do this before we're in-game because not all assets are ready before the game is started
            if (ColorPicker != null)
            {
                if (ColorPicker.transform.Find("EditScreen") == null)
                {
                    Admin.SessionManager.StartCoroutine(SetupColorPicker());
                }
                else
                {
                    EnsureColorPickerEnabled();
                }
            }
        }
        private void EnsureColorPickerEnabled()
        {
            ActualEditScreen = ColorPicker?.transform.Find("EditScreen")?.gameObject;
            if(ActualEditScreen == null)
            {
                return;
            }
            // why is canvas sometimes disabled, and Active is sometimes inactive?
            // Don't know!
            ActualEditScreen.GetComponent<Canvas>().enabled = true;
            ActualEditScreen.transform.Find("Active").gameObject.SetActive(true);
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
            yield return new WaitUntil(() => !IsPlayerControlling());
            yield return new WaitForSeconds(2);
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
        }
        public override void BeginPiloting()
        {
            base.BeginPiloting();
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
            isPilotSeated = false;
            Player.main.SetScubaMaskActive(false);
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            if (!IsVehicleDocked && IsPlayerControlling())
            {
                Player.main.transform.SetParent(transform);
                if (thisStopPilotingLocation == null)
                {
                    if(TetherSources.First() != null)
                    {
                        Logger.Warn("Warning: pilot exit location was null. Defaulting to first tether.");
                        Player.main.transform.position = TetherSources[0].transform.position;
                    }
                }
                else
                {
                    Player.main.transform.position = thisStopPilotingLocation.position;
                }
            }
            if(isScuttled)
            {
                Admin.SessionManager.StartCoroutine(GrantPlayerInvincibility(3f));
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
        public override void PlayerEntry()
        {
            isPlayerInside = true;
            base.PlayerEntry();
            if (!isScuttled)
            {
                Player.main.currentMountedVehicle = this;
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
            }
            EnsureColorPickerEnabled();
        }
        public override void PlayerExit()
        {
            isPlayerInside = false;
            base.PlayerExit();
            //Player.main.currentSub = null;
            if (!IsVehicleDocked)
            {
                Player.main.transform.SetParent(null);
            }
        }
        public override void SubConstructionBeginning()
        {
            base.SubConstructionBeginning();
            PaintVehicleDefaultStyle(GetName());
        }
        public override void SubConstructionComplete()
        {
            if (!pingInstance.enabled)
            {
                // Setup the color picker with the submarine's name
                var active = transform.Find("ColorPicker/EditScreen/Active");
                if (active)
                {
                    active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = GetName();
                    active.transform.Find("InputField/Text").GetComponent<TMPro.TextMeshProUGUI>().text = GetName();
                }
                Admin.SessionManager.StartCoroutine(TrySpawnFabricator());
            }
            base.SubConstructionComplete();
            PaintNameDefaultStyle(GetName());
        }

        public override void OnKill()
        {
            bool isplayerinthissub = IsPlayerInside();
            base.OnKill();
            if (isplayerinthissub)
            {
                PlayerEntry();
            }
        }

        IEnumerator TrySpawnFabricator()
        {
            if(Fabricator == null)
            {
                yield break;
            }
            foreach (var fab in GetComponentsInChildren<Fabricator>())
            {
                if (fab.gameObject.transform.localPosition == Fabricator.transform.localPosition)
                {
                    // This fabricator blueprint has already been fulfilled.
                    yield break;
                }
            }
            yield return SpawnFabricator(Fabricator.transform);
        }

        IEnumerator SpawnFabricator(Transform location)
        {
            TaskResult<GameObject> result = new();
            yield return Admin.SessionManager.StartCoroutine(CraftData.InstantiateFromPrefabAsync(TechType.Fabricator, result, false));
            fabricator = result.Get();
            fabricator.GetComponent<SkyApplier>().enabled = true;
            fabricator.transform.SetParent(transform);
            fabricator.transform.localPosition = location.localPosition;
            fabricator.transform.localRotation = location.localRotation;
            fabricator.transform.localScale = location.localScale;
            if (location.localScale.x == 0 || location.localScale.y == 0 || location.localScale.z == 0)
            {
                fabricator.transform.localScale = Vector3.one;
            }
            yield break;
        }
        public virtual void PaintNameDefaultStyle(string name)
        {
            OnNameChange(name);
        }
        public virtual void PaintVehicleDefaultStyle(string name)
        {
            IsDefaultTexture = true;
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
            OnNameChange(name);
        }

        public bool IsDefaultTexture = true;

        public override void SetBaseColor(Vector3 hsb, Color color)
        {
            base.SetBaseColor(hsb, color);
            PaintVehicleSection("ExteriorMainColor", baseColor);
        }
        public override void SetInteriorColor(Vector3 hsb, Color color)
        {
            base.SetInteriorColor(hsb, color);
            PaintVehicleSection("ExteriorPrimaryAccent", interiorColor);
        }
        public override void SetStripeColor(Vector3 hsb, Color color)
        {
            base.SetStripeColor(hsb, color);
            PaintVehicleSection("ExteriorSecondaryAccent", stripeColor);
        }

        public virtual void SetColorPickerUIColor(string name, Color col)
        {
            ActualEditScreen.transform.Find("Active/" + name + "/SelectedColor").GetComponent<Image>().color = col;
        }
        public virtual void OnColorChange(ColorChangeEventData eventData)
        {
            // determine which tab is selected
            // call the desired function
            List<string> tabnames = new() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
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
                    baseColor = eventData.color;
                    break;
                case "PrimaryAccent":
                    IsDefaultTexture = false;
                    interiorColor = eventData.color;
                    break;
                case "SecondaryAccent":
                    IsDefaultTexture = false;
                    stripeColor = eventData.color;
                    break;
                case "NameLabel":
                    nameColor = eventData.color;
                    break;
                default:
                    break;
            }
            ActualEditScreen.transform.Find("Active/MainExterior/SelectedColor").GetComponent<Image>().color = baseColor;
        }
        public virtual void OnNameChange(string e) // why is this independent from OnNameChange?
        {
            if (vehicleName != e)
            {
                SetName(e);
            }
        }
        public virtual void OnColorSubmit() // called by color picker submit button
        {
            SetBaseColor(Vector3.zero, baseColor);
            SetInteriorColor(Vector3.zero, interiorColor);
            SetStripeColor(Vector3.zero, stripeColor);
            if (IsDefaultTexture)
            {
                PaintVehicleDefaultStyle(GetName());
            }
            else
            {
                PaintVehicleName(GetName(), nameColor, baseColor);
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
                    List<string> tabnames = new() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
                    foreach (string tab in tabnames.FindAll(x => x != name))
                    {
                        ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.SetActive(false);
                    }
                    ActualEditScreen.transform.Find("Active/" + name + "/Background").gameObject.SetActive(true);
                }
                return Action;
            }

            GameObject console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>()?.ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;

            if (console == null)
            {
                yield return Admin.SessionManager.StartCoroutine(Builder.BeginAsync(TechType.BaseUpgradeConsole));
                Builder.ghostModel.GetComponentInChildren<BaseGhost>().OnPlace();
                console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>().ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;
                Builder.End();
            }
            ActualEditScreen = GameObject.Instantiate(console.transform.Find("EditScreen").gameObject);
            ActualEditScreen.GetComponentInChildren<SubNameInput>().enabled = false;
            ActualEditScreen.name = "EditScreen";
            ActualEditScreen.SetActive(true);
            ActualEditScreen.transform.Find("Inactive").gameObject.SetActive(false);
            Vector3 originalLocalScale = ActualEditScreen.transform.localScale;


            GameObject frame = ColorPicker;
            ActualEditScreen.transform.SetParent(frame.transform);
            ActualEditScreen.transform.localPosition = new(.15f, .28f, 0.01f);
            ActualEditScreen.transform.localEulerAngles = new(0, 180, 0);
            ActualEditScreen.transform.localScale = originalLocalScale;

            var but = ActualEditScreen.transform.Find("Active/BaseTab");
            but.name = "MainExterior";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFMainExterior");
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("MainExterior"));

            but = ActualEditScreen.transform.Find("Active/NameTab");
            but.name = "PrimaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFPrimaryAccent");
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("PrimaryAccent"));

            but = ActualEditScreen.transform.Find("Active/InteriorTab");
            but.name = "SecondaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFSecondaryAccent");
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("SecondaryAccent"));

            but = ActualEditScreen.transform.Find("Active/Stripe1Tab");
            but.name = "NameLabel";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Language.main.Get("VFNameLabel");
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("NameLabel"));

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

        public override void OnAIBatteryReload()
        {
        }

        // this function returns the number of seconds to wait before opening the PDF,
        // to show off the cool animations~
        public override float OnStorageOpen(string name, bool open)
        {
            return 0;
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
        public override void OnVehicleDocked(Vector3 exitLocation)
        {
            base.OnVehicleDocked(exitLocation);
            EnableFabricator(false);
        }
        public override void OnVehicleUndocked()
        {
            base.OnVehicleUndocked();
            EnableFabricator(true);
        }
        public override void OnPlayerDocked(Vector3 exitLocation)
        {
            StopPiloting();
            base.OnPlayerDocked(exitLocation);
            //Admin.SessionManager.StartCoroutine(TryStandUpFromChair());
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
