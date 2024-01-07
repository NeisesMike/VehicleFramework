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
        protected override float FORWARD_TOP_SPEED => 1200;
        protected override float REVERSE_TOP_SPEED => 400;
        protected override float STRAFE_MAX_SPEED => 400;
        protected override float VERT_MAX_SPEED => 400;

        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED * 6;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED * 6;
        protected override float STRAFE_ACCEL => STRAFE_MAX_SPEED * 6;
        protected override float VERT_ACCEL => VERT_MAX_SPEED * 6;

        protected override float waterDragDecay => 2.5f;

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
    }
}
