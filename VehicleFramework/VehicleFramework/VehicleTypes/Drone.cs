using System;
using System.Collections.Generic;
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
        }
        public override void Start()
        {
            base.Start();
            pairedStation = FindNearestUnpairedStation();

        }
        public override void Update()
        {
            base.Update();
            if(IsPlayerDry)
            {
                if (GameInput.GetButtonHeld(GameInput.Button.Exit))
                {
                    StopControlling();
                }
            }
        }
        public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
        {
            //base.EnterVehicle(player, teleport, playEnterAnimation);
        }
        private SubRoot memory = null;
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
        public virtual void BeginControlling()
        {
            guihand = true;
            memory = Player.main.GetCurrentSub();
            base.PlayerEntry();
            //base.EnterVehicle(Player.main, true); //Don't actually want to do this. Just do the relevant things instead:
            //player.SetCurrentSub(null, false);
            Player.main.EnterLockedMode(null, false);
            // the noraml BeginPiloting stuff
            uGUI.main.quickSlots.SetTarget(this);
            SwapToDroneCamera();
            NotifyStatus(PlayerStatus.OnPilotBegin);
        }
        public virtual void StopControlling()
        {
            base.PlayerExit();
            base.StopPiloting();
            Player.main.ExitLockedMode();
            Player.main.SetCurrentSub(memory, true);
            guihand = false;
            SwapToPlayerCamera();
        }
        public void SwapToDroneCamera()
        {
            //uGUI.main.screenCanvas.transform.Find("Pings").GetComponent<uGUI_Pings>().enabled = false;
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
        /*
        public new void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Cancel Pairing");
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Confirm Pairing");
            }
            else
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Enter Pairing Mode");
            }
        }
        public new void OnHandClick(GUIHand hand)
        {
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                (this as IDroneInterface).FinalizePairingMode();
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                DroneStation.FastenConnection(DroneStation.BroadcastingStation, this);
                (this as IDroneInterface).FinalizePairingMode();
            }
            else
            {
                (this as IDroneInterface).InitiatePairingMode();
            }
        }
        */
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
    }
}
