using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class VehicleEngine : MonoBehaviour
    {
        public ModVehicle mv;
        public Rigidbody rb;

        public bool canControlRotation = true;

        private const float FORWARD_TOP_SPEED = 1500;
        private const float REVERSE_TOP_SPEED = 500;
        private const float STRAFE_MAX_SPEED = 500;
        private const float VERT_MAX_SPEED = 500;

        public const float FORWARD_ACCEL = FORWARD_TOP_SPEED / 10f;
        public const float REVERSE_ACCEL = REVERSE_TOP_SPEED / 10f;
        public const float STRAFE_ACCEL = STRAFE_MAX_SPEED / 10f;
        public const float VERT_ACCEL = VERT_MAX_SPEED / 10f;

        // SOAK describes how low to go before grinding to an abrupt halt.
        // This is useful because otherwise the low-speed light are always blinking
        private const float DEAD_ZONE_SOAK = 50;
        // IMPULSE describes the immediate boost you get from the impulse engines when they fire
        // the impulse engine recharges every second, so manueverability is not especially nimble
        private const float IMPULSE_BOOST = 300;

        /* TODO: RacingEngine : VehicleEngine
        private float _timeOfLastImpulse = 0f;
        private float ImpulseBoost
        {
            get
            {
                if(_timeOfLastImpulse + 1f < Time.time)
                {
                    _timeOfLastImpulse = Time.time;
                    return IMPULSE_BOOST;
                }
                else
                {
                    return 0;
                }
            }
        }
        */

        // a value of 0.25 here indicates that
        // velocity will decay 25% every second
        private readonly float waterDragDecay = 0.25f;
        private readonly float airDragDecay = 0.025f;
        private float DragDecay
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

        private float _forwardMomentum = 0;
        private float ForwardMomentum
        {
            get
            {
                if (Mathf.Abs(_forwardMomentum) < DEAD_ZONE_SOAK)
                {
                    _forwardMomentum = 0;
                }
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
        private void UpdateForwardMomentum(float inputMagnitude)
        {
            if (ForwardMomentum < IMPULSE_BOOST && 0 < inputMagnitude)
            {
                ForwardMomentum = IMPULSE_BOOST;
                return;
            }
            if (-IMPULSE_BOOST < ForwardMomentum && inputMagnitude < 0)
            {
                ForwardMomentum = -IMPULSE_BOOST;
                return;
            }
            if (0 < inputMagnitude)
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * FORWARD_ACCEL * Time.deltaTime;
            }
            else
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * REVERSE_ACCEL * Time.deltaTime;
            }
        }

        private float _rightMomentum = 0;
        private float RightMomentum
        {
            get
            {
                if (Mathf.Abs(_rightMomentum) < DEAD_ZONE_SOAK)
                {
                    _rightMomentum = 0;
                }
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
        private void UpdateRightMomentum(float inputMagnitude)
        {
            if (RightMomentum < IMPULSE_BOOST && 0 < inputMagnitude)
            {
                RightMomentum = IMPULSE_BOOST;
                return;
            }
            if (-IMPULSE_BOOST < RightMomentum && inputMagnitude < 0)
            {
                RightMomentum = -IMPULSE_BOOST;
                return;
            }
            if (inputMagnitude != 0)
            {
                RightMomentum += inputMagnitude * STRAFE_ACCEL * Time.deltaTime;
            }
        }

        private float _upMomentum = 0;
        private float UpMomentum
        {
            get
            {
                if (Mathf.Abs(_upMomentum) < DEAD_ZONE_SOAK)
                {
                    _upMomentum = 0;
                }
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
        private void UpdateUpMomentum(float inputMagnitude)
        {
            if(UpMomentum < IMPULSE_BOOST && 0 < inputMagnitude)
            {
                UpMomentum = IMPULSE_BOOST;
                return;
            }
            if (-IMPULSE_BOOST < UpMomentum && inputMagnitude < 0)
            {
                UpMomentum = -IMPULSE_BOOST;
                return;
            }
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
        private void ApplyDrag(Vector3 move)
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
        public enum forceDirection
        {
            forward,
            backward,
            strafe,
            updown
        }
        public float GetCurrentPercentOfTopSpeed()
        {
            float totalMomentumNow = Mathf.Abs(ForwardMomentum) + Mathf.Abs(RightMomentum) + Mathf.Abs(UpMomentum);
            float topMomentum = FORWARD_TOP_SPEED + STRAFE_MAX_SPEED + VERT_MAX_SPEED;
            return totalMomentumNow / topMomentum;
        }
        public void ApplyPlayerControls(Vector3 moveDirection)
        {
            // Control velocity
            UpdateRightMomentum(moveDirection.x);
            UpdateUpMomentum(moveDirection.y);
            UpdateForwardMomentum(moveDirection.z);
            // Maybe control rotation
            MaybeControlRotation();

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
        public void MaybeControlRotation()
        {
            if (canControlRotation)
            {
                // Control rotation
                float pitchFactor = 1.2f * (1 - GetCurrentPercentOfTopSpeed());
                float yawFactor = 1f * (1 - GetCurrentPercentOfTopSpeed());
                Vector2 mouseDir = GameInput.GetLookDelta();
                float xRot = mouseDir.x;
                float yRot = mouseDir.y;
                rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
                rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
            }
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
