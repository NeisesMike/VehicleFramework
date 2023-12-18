using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AbyssVehicle
{
    public class AbyssEngine : VehicleFramework.Engines.ModVehicleEngine
    {
        public MotorWheel downup;
        public MotorWheel leftright;
        public MotorWheel backforth;
        protected override float FORWARD_TOP_SPEED => 1000;
        protected override float REVERSE_TOP_SPEED => 1000;
        protected override float STRAFE_MAX_SPEED => 1000;
        protected override float VERT_MAX_SPEED => 1000;

        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED / 3.33f;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED / 3.33f;
        protected override float STRAFE_ACCEL => STRAFE_MAX_SPEED / 3.33f;
        protected override float VERT_ACCEL => VERT_MAX_SPEED / 3.33f;

        // SOAK describes how low to go before grinding to an abrupt halt.
        // This is useful because otherwise the low-speed light are always blinking
        private const float DEAD_ZONE_SOAK = 5;
        // IMPULSE describes the immediate boost you get from the impulse engines when they fire
        // TODO:
        // I've turned this to (basically) zero because it makes handling a bit awkward.
        // It works as intended, but I'm not sure what's the right way to trigger it.
        // Perhaps I can add an Impulse Upgrade Module later on.
        // NOT TRUE: the impulse engine recharges every second, so manueverability is not especially nimble

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
        public float GetCurrentPercentOfTopSpeed()
        {
            float totalMomentumNow = Mathf.Abs(ForwardMomentum) + Mathf.Abs(RightMomentum) + Mathf.Abs(UpMomentum);
            float topMomentum = FORWARD_TOP_SPEED + STRAFE_MAX_SPEED + VERT_MAX_SPEED;
            return totalMomentumNow / topMomentum;
        }
        public override void ControlRotation()
        {
            // Control rotation
            float pitchFactor = 1.5f * (1.5f - GetCurrentPercentOfTopSpeed());
            float yawFactor = 1.5f * (1.5f - GetCurrentPercentOfTopSpeed());
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
        }
        public override void DrainPower(Vector3 moveDirection)
        {
            float scalarFactor = 0.36f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.GetComponent<VehicleFramework.PowerManager>().TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
        /*
        public override void FixedUpdate()
        {
            Vector3 moveDirection = Vector3.zero;
            if (!mv.GetIsUnderwater()) //above water
            {
                UpdateEngineWhir(-3f);
            }
            else if (mv.CanPilot() && mv.IsPlayerPiloting()) //player piloting
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
            else if(backforth.wheelstate != 0 || leftright.wheelstate != 0 || downup.wheelstate != 0) // valve control
            {
                moveDirection = new Vector3(leftright.wheelstate / 10f, downup.wheelstate / 10f, backforth.wheelstate / 10f);
            }
            if (moveDirection == Vector3.zero)
            {
                UpdateEngineWhir(-3);
            }
            else
            {
                UpdateEngineWhir(moveDirection.magnitude);
            }
            PlayEngineWhir();
            PlayEngineWhistle(moveDirection);

            // Execute a state-based physics move
            ExecutePhysicsMove();
            ApplyDrag(moveDirection);
        }
        */
    }
}
