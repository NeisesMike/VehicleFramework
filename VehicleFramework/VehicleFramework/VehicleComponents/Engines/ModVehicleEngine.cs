using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
    public abstract class ModVehicleEngine : MonoBehaviour
    {
        public ModVehicle mv;
        public Rigidbody rb;

        protected virtual float FORWARD_TOP_SPEED => 1000;
        protected virtual float REVERSE_TOP_SPEED => 1000;
        protected virtual float STRAFE_MAX_SPEED => 1000;
        protected virtual float VERT_MAX_SPEED => 1000;
        protected virtual float FORWARD_ACCEL => FORWARD_TOP_SPEED / 10f;
        protected virtual float REVERSE_ACCEL => REVERSE_TOP_SPEED / 10f;
        protected virtual float STRAFE_ACCEL => STRAFE_MAX_SPEED / 10f;
        protected virtual float VERT_ACCEL => VERT_MAX_SPEED / 10f;

        // a value of 0.25 here indicates that
        // velocity will decay 25% every second
        protected virtual float waterDragDecay => 0.25f;
        protected virtual float airDragDecay => 0.025f;
        protected virtual float DragDecay
        {
            get
            {
                if(mv.GetIsUnderwater())
                {
                    return waterDragDecay;
                }
                else
                {
                    return airDragDecay;
                }
            }
        }

        protected float _forwardMomentum = 0;
        protected virtual float ForwardMomentum
        {
            get
            {
                return _forwardMomentum;
            }
            set
            {
                if (value < -REVERSE_TOP_SPEED)
                {
                    _forwardMomentum = -REVERSE_TOP_SPEED;
                }
                else if (FORWARD_TOP_SPEED < value)
                {
                    _forwardMomentum = FORWARD_TOP_SPEED;
                }
                else
                {
                    _forwardMomentum = value;
                }
            }
        }
        protected virtual void UpdateForwardMomentum(float inputMagnitude)
        {
            if (0 < inputMagnitude)
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * FORWARD_ACCEL * Time.deltaTime;
            }
            else
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * REVERSE_ACCEL * Time.deltaTime;
            }
        }

        protected float _rightMomentum = 0;
        protected virtual float RightMomentum
        {
            get
            {
                return _rightMomentum;
            }
            set
            {
                if (value < -STRAFE_MAX_SPEED)
                {
                    _rightMomentum = -STRAFE_MAX_SPEED;
                }
                else if (STRAFE_MAX_SPEED < value)
                {
                    _rightMomentum = STRAFE_MAX_SPEED;
                }
                else
                {
                    _rightMomentum = value;
                }
            }
        }
        protected virtual void UpdateRightMomentum(float inputMagnitude)
        {
            if (inputMagnitude != 0)
            {
                RightMomentum += inputMagnitude * STRAFE_ACCEL * Time.deltaTime;
            }
        }

        protected float _upMomentum = 0;
        protected virtual float UpMomentum
        {
            get
            {
                return _upMomentum;
            }
            set
            {
                if (value < -VERT_MAX_SPEED)
                {
                    _upMomentum = -VERT_MAX_SPEED;
                }
                else if (VERT_MAX_SPEED < value)
                {
                    _upMomentum = VERT_MAX_SPEED;
                }
                else
                {
                    _upMomentum = value;
                }
            }
        }
        protected virtual void UpdateUpMomentum(float inputMagnitude)
        {
            UpMomentum += inputMagnitude * VERT_ACCEL * Time.deltaTime;
        }

        // Start is called before the first frame update
        public void Start()
        {
            rb.centerOfMass = Vector3.zero;
        }
        // Update is called once per frame
        public void FixedUpdate()
        {
            Vector3 moveDirection = Vector3.zero;
            if (mv.GetIsUnderwater())
            {
                if (mv.CanPilot() && mv.IsPlayerPiloting())
                {
                    // Get Input Vector
                    moveDirection = GameInput.GetMoveDirection();
                    // Apply controls to the vehicle state
                    ApplyPlayerControls(moveDirection);
                    // Drain power based on Input Vector (and modifiers)
                    // TODO: DrainPower with ApplyPlayerControls...
                    // or would it be better with ExecutePhysicsMove...?
                    DrainPower(moveDirection);
                }
                // Execute a state-based physics move
                ExecutePhysicsMove();
            }
            ApplyDrag(moveDirection);
        }
        protected void ApplyDrag(Vector3 move)
        {
            // Only apply drag if we aren't applying movement in that direction.
            // That is, if we aren't holding forward, our forward momentum should decay.
            if (move.z == 0)
            {
                if (1 < Mathf.Abs(ForwardMomentum))
                {
                    ForwardMomentum -= DragDecay * ForwardMomentum * Time.deltaTime;
                }
            }
            if (move.x == 0)
            {
                if (1 < Mathf.Abs(RightMomentum))
                {
                    RightMomentum -= DragDecay * RightMomentum * Time.deltaTime;
                }
            }
            if (move.y == 0)
            {
                if (1 < Mathf.Abs(UpMomentum))
                {
                    UpMomentum -= DragDecay * UpMomentum * Time.deltaTime;
                }
            }
        }
        public void ExecutePhysicsMove()
        {
            rb.AddForce(mv.transform.forward * ForwardMomentum / 100f * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddForce(mv.transform.right * RightMomentum / 100f * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddForce(mv.transform.up * UpMomentum / 100f * Time.deltaTime, ForceMode.VelocityChange);
        }
        public enum ForceDirection
        {
            forward,
            backward,
            strafe,
            updown
        }
        public void ApplyPlayerControls(Vector3 moveDirection)
        {
            // Control velocity
            UpdateRightMomentum(moveDirection.x);
            UpdateUpMomentum(moveDirection.y);
            UpdateForwardMomentum(moveDirection.z);

            /* TODO steering wheel animation stuff
            base.steeringWheelYaw = Mathf.Lerp(base.steeringWheelYaw, 0f, Time.deltaTime);
            base.steeringWheelPitch = Mathf.Lerp(base.steeringWheelPitch, 0f, Time.deltaTime);
            if (base.mainAnimator)
            {
                base.mainAnimator.SetFloat("view_yaw", base.steeringWheelYaw * 70f);
                base.mainAnimator.SetFloat("view_pitch", base.steeringWheelPitch * 45f);
            }
            */

            return;
        }
        public virtual void ControlRotation()
        {
            // Control rotation
            float pitchFactor = 1.4f;
            float yawFactor = 1.4f;
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
        }
        public void DrainPower(Vector3 moveDirection)
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
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.GetComponent<PowerManager>().TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
    }
}
