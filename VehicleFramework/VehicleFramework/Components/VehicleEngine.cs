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

        private readonly float forwardTopSpeed = 1500;
        private readonly float backwardTopSpeed = 300;
        private readonly float strafeTopSpeed = 500;
        private readonly float upDownTopSpeed = 400;

        private readonly float deadZoneSize = 300;

        private float _upMomentum = 0;
        private float UpMomentum
        {
            get
            {
                return _upMomentum;
            }
            set
            {
                if(Mathf.Abs(value) < deadZoneSize)
                {
                    if(value < _upMomentum)
                    {
                        _upMomentum = -deadZoneSize;
                    }
                    else if(_upMomentum < value)
                    {
                        _upMomentum = deadZoneSize;
                    }
                    return;
                }

                if (value < -upDownTopSpeed)
                {
                    _upMomentum = -upDownTopSpeed;
                }
                else if(upDownTopSpeed < value)
                {
                    _upMomentum = upDownTopSpeed;
                }
                else
                {
                    _upMomentum = value;
                }
            }
        }

        private float _rightMomentum = 0;
        private float RightMomentum
        {
            get
            {
                return _rightMomentum;
            }
            set
            {
                if (Mathf.Abs(value) < deadZoneSize)
                {
                    if (value < _rightMomentum)
                    {
                        _rightMomentum = -deadZoneSize;
                    }
                    else if (_rightMomentum < value)
                    {
                        _rightMomentum = deadZoneSize;
                    }
                    return;
                }

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

        private float _forwardMomentum = 0;
        private float ForwardMomentum
        {
            get
            {
                return _forwardMomentum;
            }
            set
            {
                if (Mathf.Abs(value) < deadZoneSize)
                {
                    if (value < _forwardMomentum)
                    {
                        _forwardMomentum = -deadZoneSize;
                    }
                    else if (_forwardMomentum < value)
                    {
                        _forwardMomentum = deadZoneSize;
                    }
                    return;
                }

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


        // Start is called before the first frame update
        public void Start()
        {
            rb.centerOfMass = Vector3.zero;
        }
        // Update is called once per frame
        public void FixedUpdate()
        {
            // TODO: justify a more reasonable constant here
            if (mv.CanPilot() && mv.transform.position.y < 0.6f)
            {
                if (mv.IsPlayerPiloting())
                {
                    // Get Input Vector
                    Vector3 moveDirection = GameInput.GetMoveDirection();

                    // Apply controls to the vehicle state
                    ApplyPlayerControls(moveDirection);

                    // Execute a state-based physics move
                    ExecutePhysicsMove();

                    // Drain power based on Input Vector (and modifiers)
                    DrainPower(moveDirection);
                }
            }
        }
        public void ExecutePhysicsMove()
        {
            rb.AddForce(mv.transform.forward * ForwardMomentum/100f * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddForce(mv.transform.right * RightMomentum/100f * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddForce(mv.transform.up * UpMomentum/100f * Time.deltaTime, ForceMode.VelocityChange);
        }
        public enum forceDirection
        {
            forward,
            backward,
            strafe,
            updown
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
            if (0 < zMove)
            {
                ForwardMomentum += zMove * getForce(forceDirection.forward) * Time.deltaTime;
            }
            else if(zMove < 0)
            {
                ForwardMomentum += zMove * getForce(forceDirection.backward) * Time.deltaTime;
            }
            else
            {
                ForwardMomentum *= 0.995f;
            }

            if (xMove != 0)
            {
                RightMomentum += xMove * getForce(forceDirection.strafe) * Time.deltaTime;
            }
            else
            {
                RightMomentum *= 0.995f;
            }

            if (yMove != 0)
            {
                UpMomentum += yMove * getForce(forceDirection.updown) * Time.deltaTime;
            }
            else
            {
                UpMomentum *= 0.995f;
            }

            // Some rotation already happens in Vehicle.Update
            // This is for adjusting that, if necessary
            // Control rotation
            float pitchFactor = 0.4f;
            float yawFactor = 0.25f;
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            rb.AddTorque(mv.transform.up    * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);


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
