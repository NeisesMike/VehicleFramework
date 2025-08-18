using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
    public class AtramaEngine : ModVehicleEngine
    {
        protected override float FORWARD_TOP_SPEED => 1500;
        protected override float REVERSE_TOP_SPEED => 500;
        protected override float STRAFE_MAX_SPEED => 500;
        protected override float VERT_MAX_SPEED => 500;

        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED / 5f;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED / 10f;
        protected override float STRAFE_ACCEL => STRAFE_MAX_SPEED / 10f;
        protected override float VERT_ACCEL => VERT_MAX_SPEED / 10f;
        protected override float DragThresholdSpeed => 1;


        // SOAK describes how low to go before grinding to an abrupt halt.
        // This is useful because otherwise the low-speed light are always blinking
        private const float DEAD_ZONE_SOAK = 50;
        // IMPULSE describes the immediate boost you get from the impulse engines when they fire
        // TODO:
        // I've turned this to (basically) zero because it makes handling a bit awkward.
        // It works as intended, but I'm not sure what's the right way to trigger it.
        // Perhaps I can add an Impulse Upgrade Module later on.
        // NOT TRUE: the impulse engine recharges every second, so manueverability is not especially nimble
        private const float IMPULSE_BOOST = DEAD_ZONE_SOAK+1;

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
        protected override float ForwardMomentum
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
        protected override void UpdateForwardMomentum(float inputMagnitude)
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
                ForwardMomentum += inputMagnitude * FORWARD_ACCEL * Time.deltaTime;
            }
            else if (inputMagnitude < 0)
            {
                ForwardMomentum += inputMagnitude * REVERSE_ACCEL * Time.deltaTime;
            }
        }
        protected override float RightMomentum
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
        protected override void UpdateRightMomentum(float inputMagnitude)
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
        protected override float UpMomentum
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
        protected override void UpdateUpMomentum(float inputMagnitude)
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
            if (inputMagnitude != 0)
            {
                UpMomentum += inputMagnitude * VERT_ACCEL * Time.deltaTime;
            }
        }
        public float GetCurrentPercentOfTopSpeed()
        {
            float totalMomentumNow = Mathf.Abs(ForwardMomentum) + Mathf.Abs(RightMomentum) + Mathf.Abs(UpMomentum);
            float topMomentum = FORWARD_TOP_SPEED + STRAFE_MAX_SPEED + VERT_MAX_SPEED;
            return totalMomentumNow / topMomentum;
        }
        public override void ControlRotation()
        {
            // Control rotation
            float pitchFactor = 1.2f * (1.5f - GetCurrentPercentOfTopSpeed());
            float yawFactor = 1f * (1.5f - GetCurrentPercentOfTopSpeed());
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            RB.AddTorque(MV.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            RB.AddTorque(MV.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
        }

        public override void DrainPower(Vector3 moveDirection)
        {
            float scalarFactor = 0.28f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, MV.numEfficiencyModules);
            MV.powerMan.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.fixedDeltaTime);
        }
    }
}
