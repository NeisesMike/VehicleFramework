using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;

namespace VehicleFramework.VehicleTypes
{
    /*
     * Submersible is the class of non-walkable vehicles
     */
    public abstract class Submersible : ModVehicle
    { 
        public abstract VehicleParts.VehiclePilotSeat PilotSeat { get; } // Need a way to start and stop piloting
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; } // Need a way to get in and out.
        public virtual GameObject SteeringWheelLeftHandTarget { get; }
        public virtual GameObject SteeringWheelRightHandTarget { get; }
        public override PilotingStyle pilotingStyle => PilotingStyle.Seamoth;
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }
        protected IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
            Player.main.playerAnimator.speed = 100f;
            yield return new WaitForSeconds(0.05f);
            Player.main.playerAnimator.speed = 1f;
        }
        protected IEnumerator StandUpFromChair()
        {
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
            Player.main.playerAnimator.speed = 100f;
            yield return new WaitForSeconds(0.05f);
            Player.main.playerAnimator.speed = 1f;
            yield return null;
        }
        public IEnumerator EventuallyStandUp()
        {
            yield return new WaitForSeconds(1f);
            yield return StandUpFromChair();
            uGUI.main.quickSlots.SetTarget(null);
            Player.main.currentMountedVehicle = null;
            Player.main.transform.SetParent(null);
        }
        public override void BeginPiloting()
        {
            base.BeginPiloting();
            Player.main.EnterSittingMode();
            StartCoroutine(SitDownInChair());
            //StartCoroutine(TryStandUpFromChair());
            Player.main.armsController.ikToggleTime = 0;
            Player.main.armsController.SetWorldIKTarget(SteeringWheelLeftHandTarget?.transform, SteeringWheelRightHandTarget?.transform);
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
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            UWE.CoroutineHost.StartCoroutine(StandUpFromChair());
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            uGUI.main.quickSlots.SetTarget(null);
            PlayerExit();
        }
        public override void PlayerEntry()
        {
            base.PlayerEntry();
            if (!isScuttled)
            {
                Logger.DebugLog("start submersible player entry");
                Player.main.currentSub = null;
                Player.main.currentMountedVehicle = this;
                Player.main.transform.SetParent(transform);
                //Player.main.playerController.activeController.SetUnderWater(false);
                //Player.main.isUnderwater.Update(false);
                //Player.main.isUnderwaterForSwimming.Update(false);
                //Player.main.playerController.SetMotorMode(Player.MotorMode.Walk);
                //Player.main.motorMode = Player.MotorMode.Walk;
                Player.main.SetScubaMaskActive(false);
                //Player.main.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);
                BeginPiloting();
            }
        }
        public override void PlayerExit()
        {
            base.PlayerExit();
            Logger.DebugLog("start submersible player exit");
            Player.main.currentSub = null;
            Player.main.currentMountedVehicle = null;
            Logger.DebugLog("end submersible player exit");
            if (!IsVehicleDocked)
            {
                Player.main.transform.SetParent(null);
                Player.main.transform.position = Hatches.First().ExitLocation.position;
            }
            else
            {
                UWE.CoroutineHost.StartCoroutine(EventuallyStandUp());
            }
        }
        public override void OnVehicleDocked(Vehicle vehicle, Vector3 exitLocation)
        {
            base.OnVehicleDocked(vehicle, exitLocation);
            Hatches.ForEach(x => x.Hatch.GetComponent<VehicleHatch>().isLive = false);
            PilotSeat.Seat.GetComponent<PilotingTrigger>().isLive = false;
        }
        public override void OnVehicleUndocked()
        {
            Hatches.ForEach(x => x.Hatch.GetComponent<VehicleHatch>().isLive = true);
            PilotSeat.Seat.GetComponent<PilotingTrigger>().isLive = true;
            base.OnVehicleUndocked();
        }
        public override void ScuttleVehicle()
        {
            base.ScuttleVehicle();
            GetComponentsInChildren<VehicleHatch>().ForEach(x => x.isLive = false);
        }
        public override void UnscuttleVehicle()
        {
            base.UnscuttleVehicle();
            GetComponentsInChildren<VehicleHatch>().ForEach(x => x.isLive = true);
        }
    }
}
