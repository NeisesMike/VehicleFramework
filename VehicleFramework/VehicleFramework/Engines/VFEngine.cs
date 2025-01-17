using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Patches;

namespace VehicleFramework.Engines
{
    public abstract class VFEngine : MonoBehaviour, IScuttleListener
    {
        protected ModVehicle MV => GetComponent<ModVehicle>();
        protected Rigidbody RB => MV.useRigidbody;
        protected Vector3 CenterOfMass { get; set; } = Vector3.zero;
        protected float AngularDrag { get; set; } = 5f;

        #region public_fields
        public float DamageModifier { get; set; } = 1f;
        #endregion

        #region unity_signals
        public virtual void Start()
        {
            RB.centerOfMass = CenterOfMass;
            RB.angularDrag = AngularDrag;
        }
        public virtual void FixedUpdate()
        {
            if(CanMove())
            {
                if (CanTakeInputs())
                {
                    DoMovementInputs();
                }
                DoMovement();
            }
            DoFixedUpdate();
        }
        #endregion

        #region abstract_members
        protected abstract void MoveWithInput(Vector3 moveInput);
        public abstract void ControlRotation();
        public abstract void KillMomentum();
        #endregion

        #region virtual_methods
        protected virtual bool CanTakeInputs()
        {
            var fcc = MainCameraControl.main.GetComponent<FreecamController>();
            bool isFreecam = false;
            if (fcc.mode || fcc.ghostMode)
            {
                isFreecam = true;
            }
            return MV.CanPilot() && MV.IsPlayerControlling() && !isFreecam;
        }
        protected virtual bool CanMove()
        {
            return true;
        }
        protected virtual bool CanRotate()
        {
            return true;
        }
        protected virtual void DoMovementInputs()
        {
            Vector3 moveDirection = GameInput.GetMoveDirection();
            ApplyPlayerControls(moveDirection);
            DrainPower(moveDirection);
        }
        protected virtual void DoMovement()
        {
        }
        protected virtual void DoFixedUpdate()
        {
        }
        public virtual void DrainPower(Vector3 moveDirection)
        {
            /* Rationale for these values
             * Seamoth spends this on Update
             * base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
             * where vector.magnitude in [0,3];
             * instead of enginePowerConsumption, we have upgradeModifier, but they are similar if not identical
             * so the power consumption is similar to that of a seamoth.
             */
            float scalarFactor = 1.0f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, MV.numEfficiencyModules);
            MV.powerMan.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.fixedDeltaTime);
        } // public for historical reasons
        #endregion

        #region methods
        public void ApplyPlayerControls(Vector3 moveDirection)
        {
            if (Player.main.GetPDA().isOpen)
            {
                return;
            }
            var modifiers = GetComponentsInChildren<VehicleAccelerationModifier>();
            foreach (var modifier in modifiers)
            {
                modifier.ModifyAcceleration(ref moveDirection);
            }
            MoveWithInput(moveDirection);
            return;
        } // public for historical reasons
        #endregion
        void IScuttleListener.OnScuttle()
        {
            enabled = false;
        }

        void IScuttleListener.OnUnscuttle()
        {
            enabled = true;
        }
    }
}
