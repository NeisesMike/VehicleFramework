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
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public virtual GameObject SteeringWheelLeftHandTarget { get; }
        public virtual GameObject SteeringWheelRightHandTarget { get; }
        protected bool isPlayerInside = false;


        public abstract ModVehicleEngine Engine { get; }
        public virtual List<GameObject> Arms => null;

        public override void FixedUpdate()
        {
            if (worldForces.IsAboveWater() != wasAboveWater)
            {
                PlaySplashSound();
                wasAboveWater = worldForces.IsAboveWater();
            }
            if (stabilizeRoll)
            {
                StabilizeRoll();
            }
            prevVelocity = useRigidbody.velocity;
        }
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }

        public bool IsPlayerInside()
        {
            // this one is correct ?
            return isPlayerInside;
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
            Player.main.playerAnimator.speed = 100f;
            yield return new WaitForSeconds(0.05f);
            Player.main.playerAnimator.speed = 1f;
            yield return null;
        }
        protected IEnumerator TryStandUpFromChair()
        {
            /*
            while (IsPlayerInside())
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(2);
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
            */
            yield return null;
        }
        public override void BeginPiloting()
        {
            base.BeginPiloting();
            Player.main.EnterSittingMode();
            StartCoroutine(SitDownInChair());
            //StartCoroutine(TryStandUpFromChair());
            isPlayerInside = true;
            Player.main.armsController.ikToggleTime = 0;
            Player.main.armsController.SetWorldIKTarget(SteeringWheelLeftHandTarget?.transform, SteeringWheelRightHandTarget?.transform);
        }
        public override void StopPiloting()
        {
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            StartCoroutine(StandUpFromChair());
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            uGUI.main.quickSlots.SetTarget(null);
            PlayerExit();
        }
        public override void PlayerEntry()
        {
            base.PlayerEntry();
            Logger.DebugLog("start submersible player entry");
            Player.main.currentSub = null;
            isPlayerInside = true;
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
        public override void PlayerExit()
        {
            base.PlayerExit();
            Logger.DebugLog("start submersible player exit");

            Player.main.currentSub = null;
            isPlayerInside = false;
            Player.main.currentMountedVehicle = null;
            Player.main.transform.SetParent(null);
            Player.main.transform.position = Hatches.First().ExitLocation.position;
            Logger.DebugLog("end submersible player exit");
        }
    }
}
