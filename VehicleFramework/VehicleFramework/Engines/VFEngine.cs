using System;
using UnityEngine;
using VehicleFramework.Interfaces;
using VehicleFramework.VehicleRootComponents;

namespace VehicleFramework.Engines
{
    public abstract class VFEngine : MonoBehaviour, IScuttleListener
    {
        internal protected ModVehicle MV
        {
            get
            {
                return GetComponent<ModVehicle>() ?? throw Admin.SessionManager.Fatal($"No ModVehicle component found on {transform.name}. VFEngine requires a ModVehicle component.");
            }
        }
        protected Rigidbody RB => MV.useRigidbody ?? throw Admin.SessionManager.Fatal($"No MV.useRigidbody component found on {transform.name}. VFEngine requires a ModVehicle.useRigidbody component.");
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
        public abstract void ControlRotation(Vector2 lookInput);
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
            if (!Player.main.GetPDA().isOpen)
            {
                ApplyPlayerControls(moveDirection);
                DrainPower(moveDirection);
            }
        }
        protected virtual void DoMovement()
        {
        }
        protected virtual void DoFixedUpdate()
        {
        }
        protected virtual void DrainPower(Vector3 moveDirection)
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
            float upgradeModifier = Mathf.Pow(0.85f, MV.NumEfficiencyModules);
            MV.gameObject.EnsureComponent<PowerManager>().TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.fixedDeltaTime);
        }

        [Obsolete("This method was removed. Use ControlRotation(Vector2) as a drop in replacement.", false)]
        public virtual void ControlRotation() { }
        #endregion

        #region methods
        internal protected void ApplyPlayerControls(Vector3 moveDirection)
        {
            GetComponentsInChildren<VehicleAccelerationModifier>().ForEach(x => x.ModifyAcceleration(ref moveDirection));
            MoveWithInput(moveDirection);
            return;
        }
        internal protected void ApplyRotationControls()
        {
            if (CanRotate())
            {
                Vector2 mouseDir = GameInput.GetLookDelta();
                ControlRotation(mouseDir);

#pragma warning disable CS0618 // 'OldFoo' is obsolete: 'Use NewFoo instead.'
                ControlRotation();
#pragma warning restore CS0618
            }
        }
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
