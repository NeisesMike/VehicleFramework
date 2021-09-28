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

        private readonly float forwardTopSpeed = 1500;
        private readonly float backwardTopSpeed = 300;
        private readonly float strafeTopSpeed = 500;
        private readonly float upDownTopSpeed = 400;

        private readonly float deadZoneSize = 300;

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
        private void UpdateForwardMomentum(float move, float speed)
        {
            float target = ForwardMomentum + move * speed * Time.deltaTime;
            if(0 < target && target < deadZoneSize && 0 < move)
            {
                ForwardMomentum = deadZoneSize;
                return;
            }
            if (-deadZoneSize < target && target < 0 && move < 0)
            {
                ForwardMomentum = -deadZoneSize;
                return;
            }
            if(Mathf.Abs(target) < deadZoneSize)
            {
                ForwardMomentum = 0;
                return;
            }
            ForwardMomentum = target;
        }
        private float ForwardMomentum
        {
            get
            {
                return _forwardMomentum;
            }
            set
            {
                if (value < -backwardTopSpeed)
                {
                    _forwardMomentum = -backwardTopSpeed;
                }
                else if (forwardTopSpeed < value)
                {
                    _forwardMomentum = forwardTopSpeed;
                }
                else
                {
                    _forwardMomentum = value;
                }
            }
        }

        private float _rightMomentum = 0;
        private void UpdateRightMomentum(float move, float speed)
        {
            float target = RightMomentum + move * speed * Time.deltaTime;
            if (0 < target && target < deadZoneSize && 0 < move)
            {
                RightMomentum = deadZoneSize;
                return;
            }
            if (-deadZoneSize < target && target < 0 && move < 0)
            {
                RightMomentum = -deadZoneSize;
                return;
            }
            if (Mathf.Abs(target) < deadZoneSize)
            {
                RightMomentum = 0;
                return;
            }
            RightMomentum = target;
        }
        private float RightMomentum
        {
            get
            {
                return _rightMomentum;
            }
            set
            {
                if (value < -strafeTopSpeed)
                {
                    _rightMomentum = -strafeTopSpeed;
                }
                else if (strafeTopSpeed < value)
                {
                    _rightMomentum = strafeTopSpeed;
                }
                else
                {
                    _rightMomentum = value;
                }
            }
        }

        private float _upMomentum = 0;
        private void UpdateUpMomentum(float move, float speed)
        {
            float target = UpMomentum + move * speed * Time.deltaTime;
            if (0 < target && target < deadZoneSize && 0 < move)
            {
                UpMomentum = deadZoneSize;
                return;
            }
            if (-deadZoneSize < target && target < 0 && move < 0)
            {
                UpMomentum = -deadZoneSize;
                return;
            }
            if (Mathf.Abs(target) < deadZoneSize)
            {
                UpMomentum = 0;
                return;
            }
            UpMomentum = target;
        }
        private float UpMomentum
        {
            get
            {
                return _upMomentum;
            }
            set
            {
                if (value < -upDownTopSpeed)
                {
                    _upMomentum = -upDownTopSpeed;
                }
                else if (upDownTopSpeed < value)
                {
                    _upMomentum = upDownTopSpeed;
                }
                else
                {
                    _upMomentum = value;
                }
            }
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
            if (move.z == 0)
            {
                if (ForwardMomentum < 0)
                {
                    ForwardMomentum += DragDecay * ForwardMomentum * Time.deltaTime;
                }
                else
                { 
                    ForwardMomentum -= DragDecay * ForwardMomentum * Time.deltaTime;
                }
            }
            if (move.x == 0)
            {
                if (RightMomentum < 0)
                {
                    RightMomentum += DragDecay * RightMomentum * Time.deltaTime;
                }
                else
                {
                    RightMomentum -= DragDecay * RightMomentum * Time.deltaTime;
                }
            }
            if (move.y == 0)
            {
                if (UpMomentum < 0)
                {
                    UpMomentum += DragDecay * UpMomentum * Time.deltaTime;
                }
                else
                {
                    UpMomentum -= DragDecay * UpMomentum * Time.deltaTime;
                }
            }
            if (Mathf.Abs(UpMomentum) < deadZoneSize)
            {
                UpMomentum = 0;
            }
            if (Mathf.Abs(RightMomentum) < deadZoneSize)
            {
                RightMomentum = 0;
            }
            if (Mathf.Abs(ForwardMomentum) < deadZoneSize)
            {
                ForwardMomentum = 0;
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
            float topMomentum = forwardTopSpeed + strafeTopSpeed + upDownTopSpeed;
            return totalMomentumNow / topMomentum;
        }
        public void ApplyPlayerControls(Vector3 moveDirection)
        {
            float getForce(forceDirection dir)
            {
                float thisTopSpeed;
                switch(dir)
                {
                    case forceDirection.forward:
                        thisTopSpeed = forwardTopSpeed / 10f;
                        break;
                    case forceDirection.backward:
                        thisTopSpeed = backwardTopSpeed / 10f;
                        break;
                    case forceDirection.strafe:
                        thisTopSpeed = strafeTopSpeed / 10f;
                        break;
                    case forceDirection.updown:
                        thisTopSpeed = upDownTopSpeed / 10f;
                        break;
                    default:
                        thisTopSpeed = 5f / 10f;
                        break;
                }
                return thisTopSpeed;
            }

            // Control velocity
            float xMove = moveDirection.x;
            float yMove = moveDirection.y;
            float zMove = moveDirection.z;
            if (0.01f < zMove)
            {
                UpdateForwardMomentum(zMove, getForce(forceDirection.forward));
            }
            else if(zMove < -0.01f)
            {
                UpdateForwardMomentum(zMove, getForce(forceDirection.backward));
            }
            if (0.01f < xMove)
            {
                UpdateRightMomentum(xMove, getForce(forceDirection.strafe));
            }
            if (0.01f < yMove)
            {
                UpdateUpMomentum(yMove, getForce(forceDirection.updown));
            }

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
            mv.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
    }
}
