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

        private float forwardTopSpeed = 9;
        private float backwardTopSpeed = 3;
        private float strafeTopSpeed = 5;
        private float UpDownTopSpeed = 4;

        // Start is called before the first frame update
        public void Start()
        {
            rb.centerOfMass = Vector3.zero;
        }

        // Update is called once per frame
        public void FixedUpdate()
        {
            if (mv.transform.position.y >= Ocean.main.GetOceanLevel())
            {
                //atramaVehicle.transform.position -= (atramaVehicle.transform.position.y + 3) * Vector3.up;
                rb.AddForce(new Vector3(0, -9.8f * rb.mass, 0), ForceMode.Acceleration);
            }

            if(mv.IsPowered())
            {
                if (mv.IsPlayerPiloting())
                {
                    // Get Input Vector
                    Vector3 moveDirection = GameInput.GetMoveDirection();

                    // Apply Movement based on Input Vector (and modifiers)
                    applyPlayerControls(moveDirection);

                    // Drain power based on Input Vector (and modifiers)
                    DrainPower(moveDirection);
                }
            }
        }
        public enum forceDirection
        {
            forward,
            backward,
            strafe,
            updown
        }
        public void applyPlayerControls(Vector3 moveDirection)
        {
            float getForce(forceDirection dir)
            {
                float thisTopSpeed;
                switch(dir)
                {
                    case forceDirection.forward:
                        thisTopSpeed = forwardTopSpeed;
                        break;
                    case forceDirection.backward:
                        thisTopSpeed = backwardTopSpeed;
                        break;
                    case forceDirection.strafe:
                        thisTopSpeed = strafeTopSpeed;
                        break;
                    case forceDirection.updown:
                        thisTopSpeed = UpDownTopSpeed;
                        break;
                    default:
                        thisTopSpeed = 5f;
                        break;
                }
                return (thisTopSpeed + 2) * 10;
            }

            // Control velocity
            float xMove = moveDirection.x;
            float yMove = moveDirection.y;
            float zMove = moveDirection.z;
            if (zMove >= 0)
            {
                rb.AddForce(mv.transform.forward * zMove * getForce(forceDirection.forward) * Time.deltaTime, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddForce(mv.transform.forward * zMove * getForce(forceDirection.backward) * Time.deltaTime, ForceMode.VelocityChange);
            }
            rb.AddForce(mv.transform.right *   xMove * getForce(forceDirection.strafe) * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddForce(mv.transform.up *      yMove * getForce(forceDirection.updown) * Time.deltaTime, ForceMode.VelocityChange);

            // Control rotation
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            rb.AddTorque(mv.transform.up    * xRot *  5 * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(mv.transform.right * yRot * -5 * Time.deltaTime, ForceMode.VelocityChange);

            return;
        }

        private void DrainPower(Vector3 moveDirection)
        {
            /* Rationale for these values
             * Seamoth spends this on Update
             * base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
             * where vector.magnitude in [0,3];
             * instead of enginePowerConsumption, we have upgradeModifier, but they are similar if not identical
             * so the power consumption is similar to that of a seamoth.
             * it should probably be higher, by maybe 2.5 times
             */
            float scalarFactor = 2.5f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
    }
}
