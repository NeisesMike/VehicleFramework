using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.Engines;
using UnityEngine;

namespace VehicleFramework.VehicleTypes
{
    public abstract class Drone : ModVehicle
    {
        public static Drone mountedDrone = null;
        public DroneStation pairedStation = null;
        public const float baseConnectionDistance = 350;
        public float addedConnectionDistance = 0;
        public abstract Transform CameraLocation { get; }
        private VehicleComponents.MVCameraController camControl;
        private const GameInput.Button AutoHomeButton = GameInput.Button.PDA;
        private bool _IsConnecting = false;
        public bool IsConnecting
        {
            get
            {
                return _IsConnecting;
            }
            private set
            {
                _IsConnecting = value;
                if(value)
                {
                    UWE.CoroutineHost.StartCoroutine(EstablishConnection());
                    GetComponent<ModVehicleEngine>().enabled = false;
                }
            }
        }
        private IEnumerator EstablishConnection()
        {
            yield return new WaitForSeconds(0.1f);
            while (!LargeWorldStreamer.main.isIdle)
            {
                yield return null;
            }
            IsConnecting = false;
            GetComponent<ModVehicleEngine>().enabled = true;
        }
        public override void Awake()
        {
            base.Awake();
            camControl = CameraLocation.gameObject.EnsureComponent<VehicleComponents.MVCameraController>();
            Admin.GameObjectManager<Drone>.Register(this);
            replenishesOxygen = false;
            gameObject.AddComponent<VFXSchoolFishRepulsor>();
        }
        public override void Update()
        {
            base.Update();
            if(IsUnderCommand && GameInput.GetButtonDown(AutoHomeButton) && pairedStation != null)
            {
                Vector3 destination = pairedStation.transform.position;
                Player.main.ExitLockedMode();
                GetComponent<VehicleComponents.AutoPilotNavigator>().NaiveGo(destination);
            }
        }
        public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
        {
            //base.EnterVehicle(player, teleport, playEnterAnimation);
        }
        private GUIHand _guihand = null;
        private bool guihand
        {
            set
            {
                if (_guihand == null)
                {
                    _guihand = gameObject.EnsureComponent<GUIHand>();
                    _guihand.player = Player.main;
                }
                Player.main.GetComponent<GUIHand>().enabled = !value;
                _guihand.enabled = value;
            }
        }
        public SubRoot lastSubRoot = null;
        public Vehicle lastVehicle = null;
        private IEnumerator MaybeToggleCyclopsCollision(VehicleDockingBay bay)
        {
            if (bay.subRoot.name.ToLower().Contains("cyclops"))
            {
                bay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(false);
                yield return new WaitForSeconds(2f);
                bay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(true);
            }
            yield break;
        }
        private Coroutine CheckingPower = null;
        private IEnumerator CheckPower()
        {
            while (energyInterface.hasCharge)
            {
                yield return new WaitForSeconds(1f);
            }
            Player.main.ExitLockedMode();
            Logger.Output("Disconnected: Low Power", time:5f, y:150);
            yield break;
        }
        private GameObject temporaryParent = null;
        private Transform previousParent = null;
        private void SetupTemporaryParent()
        {
            previousParent = Player.main.transform.parent;
            temporaryParent = new GameObject("DroneStationTempParent");
            temporaryParent.transform.SetParent(pairedStation.transform);
            temporaryParent.transform.position = Player.main.transform.position;
            Player.main.transform.SetParent(temporaryParent.transform);
            MainCameraControl.main.LookAt(pairedStation.transform.position);
        }
        private void DestroyTemporaryParent()
        {
            Vector3 worldPosition = Player.main.transform.position;
            Player.main.transform.SetParent(previousParent);
            Player.main.transform.position = worldPosition;
            GameObject.DestroyImmediate(temporaryParent);
            MainCameraControl.main.LookAt(pairedStation.transform.position);
        }
        public virtual void BeginControlling()
        {
            guihand = true;
            lastSubRoot = Player.main.GetCurrentSub();
            lastVehicle = Player.main.GetVehicle();
            Player.main.currentMountedVehicle = null;
            Player.main.SetCurrentSub(null, true);
            IsUnderCommand = true;
            Player.main.SetScubaMaskActive(false);
            SetupTemporaryParent();
            Player.main.EnterLockedMode(temporaryParent.transform, false);
            uGUI.main.quickSlots.SetTarget(this);
            SwapToDroneCamera();
            NotifyStatus(PlayerStatus.OnPilotBegin);
            if (IsVehicleDocked)
            {
                VehicleDockingBay thisBay = transform.parent.gameObject.GetComponentsInChildren<VehicleDockingBay>().Where(x=>x.dockedVehicle == this).First();
                UWE.CoroutineHost.StartCoroutine(MaybeToggleCyclopsCollision(thisBay));
                thisBay.vehicle_docked_param = false;
                UWE.CoroutineHost.StartCoroutine(Undock(Player.main, thisBay.transform.position.y));
                SkyEnvironmentChanged.Broadcast(gameObject, (GameObject)null);
                thisBay.dockedVehicle = null;
                OnVehicleUndocked();
            }
            mountedDrone = this;
            CheckingPower = UWE.CoroutineHost.StartCoroutine(CheckPower());
            IsConnecting = true;
        }
        public virtual void StopControlling()
        {
            base.StopPiloting();
            IsUnderCommand = false;
            Player.main.SetCurrentSub(lastSubRoot, true);
            Player.main.currentMountedVehicle = lastVehicle;
            lastVehicle = null;
            lastSubRoot = null;
            guihand = false;
            SwapToPlayerCamera();
            DestroyTemporaryParent();
            mountedDrone = null;
            pairedStation = null;
            UWE.CoroutineHost.StopCoroutine(CheckingPower);
            GetComponent<ModVehicleEngine>().KillMomentum();
        }
        public void SwapToDroneCamera()
        {
            camControl.MovePlayerCameraToTransform(CameraLocation);
            Logger.Output("Press " + LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " to disconnect.", y: 25);
            string text3 = uGUI.FormatButton(AutoHomeButton, false, " / ", false);
            Logger.Output("Press " + text3 + " to return home.", y: -25);
            Player.main.SetHeadVisible(true);
        }
        public void SwapToPlayerCamera()
        {
            camControl.MovePlayerCameraToTransform(camControl.PlayerCamPivot);
            Player.main.SetHeadVisible(false);
        }
        public override void OnPlayerDocked(Vehicle vehicle, Vector3 exitLocation)
        {
            Player.main.ExitLockedMode();
        }
        public override System.Collections.IEnumerator Undock(Player player, float yUndockedPosition)
        {
            docked = false;
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, true, false);
            Vector3 initialPosition = transform.position;
            Vector3 finalPosition = new Vector3(initialPosition.x, yUndockedPosition, initialPosition.z);
            float duration = (initialPosition.y - finalPosition.y) / 5f;
            float timeInterpolated = 0f;
            if (duration > 0f)
            {
                do
                {
                    transform.position = Vector3.Lerp(initialPosition, finalPosition, timeInterpolated / duration);
                    timeInterpolated += Time.deltaTime;
                    yield return null;
                }
                while (timeInterpolated < duration);
            }
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, false, false);
            useRigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
            yield break;
        }
        public override void ScuttleVehicle()
        {
            base.ScuttleVehicle();
            if (mountedDrone == this)
            {
                Player.main.ExitLockedMode();
            }
        }
    }
}
