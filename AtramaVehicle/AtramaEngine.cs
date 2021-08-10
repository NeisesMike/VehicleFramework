using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle
{
    public class AtramaEngine : MonoBehaviour
    {
        public GameObject atramaVehicle;
        public Rigidbody atramaRB;

        private float forwardTopSpeed = 9;
        private float backwardTopSpeed = 3;
        private float strafeTopSpeed = 5;
        private float UpDownTopSpeed = 4;

        // Start is called before the first frame update
        public void Start()
        {
            Logger.Log("Engine Starting!");
            atramaVehicle = this.gameObject;
            atramaRB = atramaVehicle.GetComponent<Rigidbody>();
            atramaRB.centerOfMass = Vector3.zero;
        }

        // Update is called once per frame
        public void FixedUpdate()
        {
            if (atramaVehicle.transform.position.y >= Ocean.main.GetOceanLevel())
            {
                //atramaVehicle.transform.position -= (atramaVehicle.transform.position.y + 3) * Vector3.up;
                atramaVehicle.GetComponent<Rigidbody>().AddForce(new Vector3(0, -100, 0), ForceMode.Acceleration);
            }

            Atrama atrama = GetComponentInParent<Atrama>();
            if(atrama.vehicle.IsPowered())
            {
                if (atrama.isPlayerPiloting)
                {
                    applyPlayerControls();
                }
                else
                {
                    atrama.autoLeveling = true;
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
        public void applyPlayerControls()
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
            Vector3 moveDirection = GameInput.GetMoveDirection();
            float xMove = moveDirection.x;
            float yMove = moveDirection.y;
            float zMove = moveDirection.z;
            if (zMove >= 0)
            {
                atramaRB.AddForce(atramaVehicle.transform.forward * zMove * getForce(forceDirection.forward) * Time.deltaTime, ForceMode.VelocityChange);
            }
            else
            {
                atramaRB.AddForce(atramaVehicle.transform.forward * zMove * getForce(forceDirection.backward) * Time.deltaTime, ForceMode.VelocityChange);
            }
            atramaRB.AddForce(atramaVehicle.transform.right *   xMove * getForce(forceDirection.strafe) * Time.deltaTime, ForceMode.VelocityChange);
            atramaRB.AddForce(atramaVehicle.transform.up *      yMove * getForce(forceDirection.updown) * Time.deltaTime, ForceMode.VelocityChange);

            // Control rotation
            Vector2 mouseDir = GameInput.GetLookDelta();
            float xRot = mouseDir.x;
            float yRot = mouseDir.y;
            atramaRB.AddTorque(atramaVehicle.transform.up    * xRot *  5 * Time.deltaTime, ForceMode.VelocityChange);
            atramaRB.AddTorque(atramaVehicle.transform.right * yRot * -5 * Time.deltaTime, ForceMode.VelocityChange);

            return;
        }


    }
}
