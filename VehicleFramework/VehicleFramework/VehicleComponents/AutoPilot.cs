using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace VehicleFramework
{
    public class AutoPilot : MonoBehaviour, IVehicleStatusListener, IPlayerListener, IPowerListener, ILightsStatusListener, IAutoPilotListener
	{
		public ModVehicle mv;
        public EnergyInterface aiEI;
        public AutoPilotVoice voice;

        private float timeOfLastLevelTap = 0f;
        private const float doubleTapWindow = 1f;
        private float rollVelocity = 0.0f;
        private float pitchVelocity = 0.0f;
        private float smoothTime = 0.3f;
        private bool _autoLeveling = false;
        private bool autoLeveling
        {
            get
            {
                return _autoLeveling;
            }
            set
            {
                if (value && !_autoLeveling) 
                {
                    mv.NotifyStatus(AutoPilotStatus.OnAutoLevelBegin);
                }
                if (!value && _autoLeveling)
                {
                    mv.NotifyStatus(AutoPilotStatus.OnAutoLevelEnd);
                }
                _autoLeveling = value;
            }
        }
        private bool isDead = false;
        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
            voice = GetComponent<AutoPilotVoice>();
        }
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
                    autoLeveling = true;
                    var smoothTime1 = 5f * pitchDelta / 90f;
                    var smoothTime2 = 5f * rollDelta / 90f;
                    smoothTime = Mathf.Max(smoothTime1, smoothTime2);
                }
                else
                {
                    timeOfLastLevelTap = Time.time;
                }
            }
        }
        public void FixedUpdate()
        {
            Vector2 lookDir = GameInput.GetLookDelta();
            if (30f < lookDir.magnitude)
            {
                autoLeveling = false;
                return;
            }
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

        void ILightsStatusListener.OnHeadLightsOn()
        {
            Logger.Log("OnHeadLightsOn");
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
            Logger.Log("OnHeadLightsOff");
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
            Logger.Log("OnInteriorLightsOn");
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
            Logger.Log("OnInteriorLightsOff");
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
            Logger.Log("OnNavLightsOn");
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
            Logger.Log("OnNavLightsOff");
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
            Logger.Log("OnFloodLightsOn");
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
            Logger.Log("OnFloodLightsOff");
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            Logger.Log("OnTakeDamage");
        }

        void IPowerListener.OnPowerUp()
        {
            Logger.Log("OnPowerUp");
            isDead = false;
        }

        void IPowerListener.OnPowerDown()
        {
            Logger.Log("OnPowerDown");
            isDead = true;
            autoLeveling = false;
        }

        void IPowerListener.OnBatterySafe()
        {
            Logger.Log("OnBatterySafe");
        }

        void IPowerListener.OnBatteryLow()
        {
            Logger.Log("OnBatteryLow");
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
            Logger.Log("OnBatteryNearlyEmpty");
        }

        void IPowerListener.OnBatteryDepleted()
        {
            Logger.Log("OnBatteryDepleted");
        }

        void IPlayerListener.OnPlayerEntry()
        {
            Logger.Log("OnPlayerEntry");
        }

        void IPlayerListener.OnPlayerExit()
        {
            Logger.Log("OnPlayerExit");
        }

        void IPlayerListener.OnPilotBegin()
        {
            Logger.Log("OnPilotBegin");
        }

        void IPlayerListener.OnPilotEnd()
        {
            Logger.Log("OnPilotEnd");
        }

        void IPowerListener.OnBatteryDead()
        {
            Logger.Log("OnBatteryDead");
        }

        void IPowerListener.OnBatteryRevive()
        {
            Logger.Log("OnBatteryRevive");
        }

        void IAutoPilotListener.OnAutoLevelBegin()
        {
            Logger.Log("OnAutoLevelBegin");
            voice.EnqueueClip(voice.leveling);
        }

        void IAutoPilotListener.OnAutoLevelEnd()
        {
            Logger.Log("OnAutoLevelEnd");
        }

        void IAutoPilotListener.OnAutoPilotBegin()
        {
            Logger.Log("OnAutoPilotBegin");
        }

        void IAutoPilotListener.OnAutoPilotEnd()
        {
            Logger.Log("OnAutoPilotEnd");
        }
    }
}
