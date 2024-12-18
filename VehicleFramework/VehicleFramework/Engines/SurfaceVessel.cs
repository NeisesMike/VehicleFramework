using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
    public class SurfaceVessel : ModVehicleEngine
    {
        public virtual float WaterLine => 0f;
        public virtual float Buoyancy => 5f;
        public virtual float ForeAftStability => 10f;
        public virtual float PortStarboardStability => 10f;
        public virtual bool IsTrackingSurface()
        {
            return true;
        }

        public override bool CanMoveAboveWater => true;
        public override bool CanRotateAboveWater => true;

        public override void ApplyPlayerControls(Vector3 moveDirection)
        {
            if (Player.main.GetPDA().isOpen)
            {
                return;
            }

            // Thank you to MrPurple6411 for this snip regarding VehicleAccelerationModifier
            var modifiers = base.gameObject.GetComponentsInChildren<VehicleAccelerationModifier>();
            foreach (var modifier in modifiers)
            {
                modifier.ModifyAcceleration(ref moveDirection);
            }

            // Control velocity
            UpdateRightMomentum(moveDirection.x);
            UpdateForwardMomentum(moveDirection.z);
            // don't take any up-down inputs!
            return;
        }

        public override void ControlRotation()
        {
            float yawFactor = 1.4f;
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            // don't accept pitch inputs!
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsTrackingSurface())
            {
                Vector3 targetPosition = new Vector3(transform.position.x, WaterLine, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * Buoyancy);

                Quaternion targetForeAftRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetForeAftRotation, Time.fixedDeltaTime * ForeAftStability);

                Quaternion targetPortStarboardRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetPortStarboardRotation, Time.fixedDeltaTime * PortStarboardStability);
            }
        }

        public override void Awake()
        {
            base.Awake();
            GetComponent<WorldForces>().handleGravity = false;
        }
    }
}
