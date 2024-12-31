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
        public override bool CanMoveAboveWater => true;
        public override bool CanRotateAboveWater => true;

        public override void Awake()
        {
            base.Awake();
            GetComponent<WorldForces>().handleGravity = false;
        }
        public override void ControlRotation()
        {
            float yawFactor = 1.4f;
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            RB.AddTorque(MV.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
            // don't accept pitch inputs!
        }
        protected override void MoveWithInput(Vector3 moveDirection)
        {
            UpdateRightMomentum(moveDirection.x);
            UpdateForwardMomentum(moveDirection.z);
            return;
        }
        protected override void DoFixedUpdate()
        {
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
        public virtual bool IsTrackingSurface()
        {
            return true;
        }
    }
}
