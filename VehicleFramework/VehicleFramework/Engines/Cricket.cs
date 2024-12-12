using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
    public class CricketEngine : ModVehicleEngine
    {
        protected override float FORWARD_TOP_SPEED => 1300;
        protected override float REVERSE_TOP_SPEED => 450;
        protected override float STRAFE_MAX_SPEED => 450;
        protected override float VERT_MAX_SPEED => 450;

        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED * 7;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED * 6;
        protected override float STRAFE_ACCEL => STRAFE_MAX_SPEED * 6;
        protected override float VERT_ACCEL => VERT_MAX_SPEED * 6;

        protected override float waterDragDecay => 6.5f;
        protected override float DragThresholdSpeed => 1;


        public override void ControlRotation()
        {
            // Control rotation
            float pitchFactor = 4.5f;
            float yawFactor = 4.5f;
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
        }
        public override void DrainPower(Vector3 moveDirection)
        {
            /* Rationale for these values
             * Seamoth spends this on Update
             * base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
             * where vector.magnitude in [0,3];
             * instead of enginePowerConsumption, we have upgradeModifier, but they are similar if not identical
             * so the power consumption is similar to that of a seamoth.
             */
            float scalarFactor = 0.08f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.powerMan.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.fixedDeltaTime);
        }
    }
}
