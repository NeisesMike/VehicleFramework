using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;
using VehicleFramework.Interfaces;

namespace VehicleFramework.VehicleTypes
{
    /*
     * Submersible is the class of non-walkable vehicles
     */
    public abstract class Submersible : ModVehicle
    { 
        public abstract VehicleBuilding.VehiclePilotSeat PilotSeat { get; } // Need a way to start and stop piloting
        public abstract List<VehicleBuilding.VehicleHatchStruct> Hatches { get; } // Need a way to get in and out.
        public override PilotingStyleEnum PilotingStyle => PilotingStyleEnum.Seamoth;
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }
        protected static IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
            Player.main.playerAnimator.speed = 100f;
            yield return new WaitForSeconds(0.05f);
            Player.main.playerAnimator.speed = 1f;
        }
        protected static IEnumerator StandUpFromChair()
        {
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
            Player.main.playerAnimator.speed = 100f;
            yield return new WaitForSeconds(0.05f);
            Player.main.playerAnimator.speed = 1f;
            yield return null;
        }
        public static IEnumerator EventuallyStandUp()
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
            Admin.SessionManager.StartCoroutine(SitDownInChair());
            //StartCoroutine(TryStandUpFromChair());
            Player.main.armsController.ikToggleTime = 0;
            if(this is ISteeringWheel wheel)
            {
                Player.main.armsController.SetWorldIKTarget(wheel.GetSteeringWheelLeftHandTarget()?.transform, wheel.GetSteeringWheelRightHandTarget()?.transform);

            }
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
            Admin.SessionManager.StartCoroutine(StandUpFromChair());
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            uGUI.main.quickSlots.SetTarget(null);
            PlayerExit();
        }
        public override void PlayerEntry()
        {
            base.PlayerEntry();
            if (!IsScuttled)
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
            Logger.DebugLog("end submersible player exit");
            if (!IsDocked)
            {
                Player.main.transform.SetParent(null);
                Player.main.transform.position = Hatches.First().ExitLocation.position;
            }
            else
            {
                Admin.SessionManager.StartCoroutine(EventuallyStandUp());
            }
        }
    }
}
