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
    public abstract class Drone : ModVehicle, IDroneInterface
    {
        public static Drone BroadcastingDrone = null;
        public DroneStation pairedStation = null;

        public virtual ModVehicleEngine Engine { get; set; }
        public virtual List<VehicleParts.VehicleArmProxy> Arms => null;
        public abstract Transform CameraLocation { get; }
        public abstract List<GameObject> PairingButtons { get; }
        private VehicleComponents.MVCameraController camControl;

        public override void Awake()
        {
            base.Awake();
            camControl = CameraLocation.gameObject.EnsureComponent<VehicleComponents.MVCameraController>();
            Admin.GameObjectManager<Drone>.Register(this);
            replenishesOxygen = false;
        }
        public override void Start()
        {
            base.Start();
            pairedStation = FindNearestUnpairedStation();

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
        public static Drone mountedDrone = null;
        private Coroutine CheckingPower = null;
        private IEnumerator CheckPower()
        {
            energyInterface.GetValues(out float charge, out _);
            while (charge > 3)
            {
                yield return new WaitForSeconds(1f);
                energyInterface.GetValues(out charge, out _);
            }
            Player.main.ExitLockedMode();
            Logger.Output("Disconnected: Low Power", time:5f, y:150);
            yield break;
        }
        public virtual void BeginControlling()
        {
            guihand = true;
            lastSubRoot = Player.main.GetCurrentSub();
            base.PlayerEntry();
            Player.main.EnterLockedMode(null, false);
            uGUI.main.quickSlots.SetTarget(this);
            SwapToDroneCamera();
            NotifyStatus(PlayerStatus.OnPilotBegin);
            if (IsVehicleDocked)
            {
                VehicleDockingBay thisBay = transform.parent.gameObject.GetComponentInChildren<VehicleDockingBay>();
                UWE.CoroutineHost.StartCoroutine(MaybeToggleCyclopsCollision(thisBay));
                thisBay.vehicle_docked_param = false;
                UWE.CoroutineHost.StartCoroutine(Undock(Player.main, thisBay.transform.position.y));
                SkyEnvironmentChanged.Broadcast(gameObject, (GameObject)null);
                thisBay.dockedVehicle = null;
                OnVehicleUndocked();
            }
            mountedDrone = this;
            CheckingPower = UWE.CoroutineHost.StartCoroutine(CheckPower());
        }
        public virtual void StopControlling()
        {
            base.StopPiloting();
            base.PlayerExit();
            Player.main.SetCurrentSub(lastSubRoot, true);
            guihand = false;
            SwapToPlayerCamera();
            mountedDrone = null;
            UWE.CoroutineHost.StopCoroutine(CheckingPower);
        }
        public void SwapToDroneCamera()
        {
            camControl.MovePlayerCameraToTransform(CameraLocation);
            Logger.Output("Press " + LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " to disconnect.");
        }
        public void SwapToPlayerCamera()
        {
            camControl.MovePlayerCameraToTransform(camControl.PlayerCamPivot);
        }

        public bool IsInPairingMode
        {
            get
            {
                return (this as IDroneInterface).IsInPairingModeAsInitiator() || (this as IDroneInterface).IsInPairingModeAsInitiator();
            }
        }
        DroneStation FindNearestUnpairedStation()
        {
            return Admin.GameObjectManager<DroneStation>.FindNearestSuch(transform.position, x => x.pairedDrone is null);
        }
        void IDroneInterface.InitiatePairingMode()
        {
            Drone.BroadcastingDrone = this;
            isInitiator = true;
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.ForEach(x => (x as IDroneInterface).RespondWithPairingMode());
            Admin.GameObjectManager<Drone>.AllSuchObjects.Where(x => x != this).ForEach(x => (x as IDroneInterface).ExitPairingMode());
        }
        void IDroneInterface.FinalizePairingMode()
        {
            DroneStation.BroadcastingStation = null;
            Drone.BroadcastingDrone = null;
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.ForEach(x => (x as IDroneInterface).ExitPairingMode());
            Admin.GameObjectManager<Drone>.AllSuchObjects.ForEach(x => (x as IDroneInterface).ExitPairingMode());
        }
        void IDroneInterface.RespondWithPairingMode()
        {
            isInitiator = false;
            isResponder = true;
        }
        void IDroneInterface.ExitPairingMode()
        {
            isInitiator = false;
            isResponder = false;
        }
        bool isInitiator = false;
        bool isResponder = false;
        bool IDroneInterface.IsInPairingModeAsInitiator()
        {
            return isInitiator;
        }
        bool IDroneInterface.IsInPairingModeAsResponder()
        {
            return isResponder;
        }

        public override void OnPlayerDocked()
        {
            Player.main.ExitLockedMode();
        }
        public override void OnPlayerUndocked()
        {
            base.OnPlayerUndocked();
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
    }
}
