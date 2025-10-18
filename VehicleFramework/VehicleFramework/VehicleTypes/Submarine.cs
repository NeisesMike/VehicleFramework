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
using VehicleFramework.LightControllers;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.Interfaces;
using VehicleFramework.VehicleChildComponents;

namespace VehicleFramework.VehicleTypes
{
    /*
     * Submarine is the class of self-leveling, walkable submarines
     */
    public abstract class Submarine : ModVehicle
    {
        public abstract VehiclePilotSeat PilotSeat { get; } // Need a way to start and stop piloting
        public abstract List<VehicleHatchStruct> Hatches { get; } // Need a way to get in and out.
        public virtual List<VehicleFloodLight>? FloodLights => null;
        public virtual List<GameObject>? TetherSources => null;
        public virtual GameObject? ControlPanel => null;
        public virtual Transform? ControlPanelLocation => null;
        public virtual GameObject? Fabricator => null;
        public virtual GameObject? ColorPicker => null;
        public virtual List<Light>? InteriorLights => null;
        public virtual float ExitPitchLimit => 4f;
        public virtual float ExitRollLimit => 4f;
        public virtual float ExitVelocityLimit => 0.5f;
        public virtual GameObject? RespawnPoint => null;
        public virtual bool DoesAutolevel => true;
        internal ControlPanel.ControlPanel? controlPanelLogic;
        private bool isPilotSeated = false;
        private bool isPlayerInside = false; // You can be inside a scuttled submarine yet not dry.
        internal Transform? thisStopPilotingLocation;
        private GameObject? fabricator = null; //fabricator
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }
        public override void Awake()
        {
            base.Awake();
            gameObject.AddComponent<FloodLightsController>();
            gameObject.AddComponent<InteriorLightsController>();
            if (this is Interfaces.INavigationLights)
            {
                gameObject.AddComponent<NavigationLightsController>();
            }
            gameObject.EnsureComponent<VehicleComponents.TetherSource>();

            ControlPanel?.EnsureComponent<ControlPanel.ControlPanel>();

            //controlPanelLogic?.Init();
        }
        public override void Start()
        {
            base.Start();

            // now that we're in-game, load the color picker
            // we can't do this before we're in-game because not all assets are ready before the game is started
            InitColorPicker();
        }
        private void InitColorPicker()
        {
            if (ColorPicker == null)
            {
                return;
            }
            ColorPicker.EnsureComponent<ColorPicker>().Init(this);
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
        protected static IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
        }
        protected static IEnumerator StandUpFromChair()
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
            if(this is ISteeringWheel wheel)
            {
                Player.main.armsController.SetWorldIKTarget(wheel.GetSteeringWheelLeftHandTarget()?.transform, wheel.GetSteeringWheelRightHandTarget()?.transform);
            }
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
            if (!IsDocked && IsPlayerControlling())
            {
                Player.main.transform.SetParent(transform);
                if (thisStopPilotingLocation == null)
                {
                    if(TetherSources != null && TetherSources.Count != 0)
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
            if(IsScuttled)
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
            if (!IsScuttled)
            {
                Player.main.currentMountedVehicle = this;
                if (IsDocked)
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
        }
        public override void PlayerExit()
        {
            isPlayerInside = false;
            base.PlayerExit();
            //Player.main.currentSub = null;
            if (!IsDocked)
            {
                Player.main.transform.SetParent(null);
            }
        }
        public override void SubConstructionBeginning()
        {
            base.SubConstructionBeginning();
            SetVehicleDefaultStyle(GetName());
        }
        public override void SubConstructionComplete() // deal with this reference to color picker
        {
            if (GetComponent<PingInstance>() != null && !GetComponent<PingInstance>().enabled)
            {
                ColorPicker?.GetComponent<ColorPicker>()?.BumpNameDecals();
                Admin.SessionManager.StartCoroutine(TrySpawnFabricator());
            }
            base.SubConstructionComplete();
            SetName(GetName());
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
