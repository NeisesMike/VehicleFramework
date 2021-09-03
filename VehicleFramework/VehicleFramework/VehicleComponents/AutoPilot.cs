using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class AutoPilot : MonoBehaviour, VehicleComponent
	{
		public ModVehicle mv;

        private float timeOfLastLevelTap = 0f;
        private const float doubleTapWindow = 1f;
        private float rollVelocity = 0.0f;
        private float pitchVelocity = 0.0f;
        private float smoothTime = 0.3f;
        private bool autoLeveling = true;
        private bool isDead = false;

        public void Update()
        {
            if (!isDead && GameInput.GetButtonDown(GameInput.Button.Exit))
            {
                if (Time.time - timeOfLastLevelTap < doubleTapWindow)
                {
                    float pitch = transform.rotation.eulerAngles.x;
                    float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;
                    BroadcastMessage("OnAutoLevel");
                    autoLeveling = true;
                    smoothTime = 2f * pitchDelta / 90f;
                }
                else
                {
                    timeOfLastLevelTap = Time.time;
                }
            }
        }
        public void FixedUpdate()
        {
            if (!isDead && (autoLeveling || !mv.IsPlayerInside()))
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
                if (x >= 180)
                {
                    newPitch = Mathf.SmoothDamp(x, 360, ref pitchVelocity, smoothTime);
                }
                else
                {
                    newPitch = Mathf.SmoothDamp(x, 0, ref pitchVelocity, smoothTime);
                }
                float newRoll = Mathf.SmoothDamp(z, 0, ref rollVelocity, smoothTime);
                transform.rotation = Quaternion.Euler(new Vector3(newPitch, y, newRoll));
            }
        }


        void VehicleComponent.OnAutoLevel()
        {
        }

        void VehicleComponent.OnAutoPilotBegin()
        {
        }

        void VehicleComponent.OnAutoPilotEnd()
        {
        }

        void VehicleComponent.OnLightsOff()
        {
        }

        void VehicleComponent.OnLightsOn()
        {
        }

        void VehicleComponent.OnPilotBegin()
        {
        }

        void VehicleComponent.OnPilotEnd()
        {
        }

        void VehicleComponent.OnPlayerEntry()
        {
        }

        void VehicleComponent.OnPlayerExit()
        {
        }

        void VehicleComponent.OnPowerDown()
        {
            isDead = true;
            autoLeveling = false;
        }

        void VehicleComponent.OnPowerUp()
        {
            isDead = false;
        }

        void VehicleComponent.OnTakeDamage()
        {
            // if current health total is too low, disable auto pilot
        }
    }
}
