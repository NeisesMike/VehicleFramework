using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class AutoPilot : MonoBehaviour, IVehicleStatusListener
	{
		public ModVehicle mv;
        public EnergyInterface aiEI;

        private float timeOfLastLevelTap = 0f;
        private const float doubleTapWindow = 1f;
        private float rollVelocity = 0.0f;
        private float pitchVelocity = 0.0f;
        private float smoothTime = 0.3f;
        private bool autoLeveling = true;
        private bool isDead = false;
        public void Start()
        {
            aiEI = mv.BackupBatteries[0].BatterySlot.GetComponent<EnergyInterface>();
        }
        public void Update()
        {
            if ((!isDead || aiEI.hasCharge) && GameInput.GetButtonDown(GameInput.Button.Exit))
            {
                if (Time.time - timeOfLastLevelTap < doubleTapWindow)
                {
                    float pitch = transform.rotation.eulerAngles.x;
                    float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;
                    float roll = transform.rotation.eulerAngles.z;
                    float rollDelta = roll >= 180 ? 360 - roll : roll;
                    mv.NotifyStatus(VehicleStatus.OnAutoLevel);
                    autoLeveling = true;
                    var smoothTime1 = 2f * pitchDelta / 90f;
                    var smoothTime2 = 2f * rollDelta / 90f;
                    smoothTime = Mathf.Max(smoothTime1, smoothTime2);
                }
                else
                {
                    timeOfLastLevelTap = Time.time;
                }
            }

            // drain power
            if(autoLeveling)
            {
                //TODO rotation doesn't actually consume energy, so... ?
                // consume energy as if we're firing thrusters along all 3 axes at max magnitude
                //mv.engine.DrainPower(Vector3.one);
            }

        }
        public void FixedUpdate()
        {
            if ((!isDead || aiEI.hasCharge) && (autoLeveling || !mv.IsPlayerInside()) && mv.GetIsUnderwater())
            {
                float x = transform.rotation.eulerAngles.x;
                float y = transform.rotation.eulerAngles.y;
                float z = transform.rotation.eulerAngles.z;
                float pitchDelta = x >= 180 ? 360 - x : x;
                float rollDelta = z >= 180 ? 360 - z : z;
                if (rollDelta < 1 && pitchDelta < 1)
                {
                    autoLeveling = false;
                    return;
                }

                float newPitch;
                float newRoll;
                if (x < 180)
                {
                    newPitch = Mathf.SmoothDamp(x, 0, ref pitchVelocity, smoothTime);
                }
                else
                {
                    newPitch = Mathf.SmoothDamp(x, 360, ref pitchVelocity, smoothTime);
                }
                if(z < 180)
                {
                    newRoll = Mathf.SmoothDamp(z, 0, ref rollVelocity, smoothTime);
                }
                else
                {
                    newRoll = Mathf.SmoothDamp(z, 360, ref rollVelocity, smoothTime);
                }
                transform.rotation = Quaternion.Euler(new Vector3(newPitch, y, newRoll));
            }
        }

        void IVehicleStatusListener.OnAutoLevel()
        {
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPilotBegin()
        {
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
        }

        void IVehicleStatusListener.OnPowerDown()
        {
            isDead = true;
            autoLeveling = false;
        }

        void IVehicleStatusListener.OnPowerUp()
        {
            isDead = false;
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            // if current health total is too low, disable auto pilot
        }

        void IVehicleStatusListener.OnHeadLightsOn()
        {
        }

        void IVehicleStatusListener.OnHeadLightsOff()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
        }

        void IVehicleStatusListener.OnBatteryLow()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryDepletion()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnFloodLightsOn()
        {
        }

        void IVehicleStatusListener.OnFloodLightsOff()
        {
        }

        void IVehicleStatusListener.OnNavLightsOn()
        {
        }

        void IVehicleStatusListener.OnNavLightsOff()
        {
        }
    }
}
