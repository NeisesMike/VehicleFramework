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
            if ((!isDead || aiEI.hasCharge) && GameInput.GetButtonDown(GameInput.Button.Exit) && mv.IsPlayerPiloting())
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
            if ((!isDead || aiEI.hasCharge) && (autoLeveling || !mv.IsPlayerPiloting()) && mv.GetIsUnderwater())
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
            Logger.DebugLog("OnHeadLightsOn");
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
            Logger.DebugLog("OnHeadLightsOff");
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
            Logger.DebugLog("OnInteriorLightsOn");
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
            Logger.DebugLog("OnInteriorLightsOff");
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
            Logger.DebugLog("OnNavLightsOn");
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
            Logger.DebugLog("OnNavLightsOff");
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
            Logger.DebugLog("OnFloodLightsOn");
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
            Logger.DebugLog("OnFloodLightsOff");
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            Logger.DebugLog("OnTakeDamage");
        }

        void IPowerListener.OnPowerUp()
        {
            Logger.DebugLog("OnPowerUp");
            isDead = false;
            voice.EnqueueClip(voice.poweringUp);
        }

        void IPowerListener.OnPowerDown()
        {
            Logger.DebugLog("OnPowerDown");
            isDead = true;
            autoLeveling = false;
        }

        void IPowerListener.OnBatterySafe()
        {
            Logger.DebugLog("OnBatterySafe");
        }

        void IPowerListener.OnBatteryLow()
        {
            Logger.DebugLog("OnBatteryLow");
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
            Logger.DebugLog("OnBatteryNearlyEmpty");
        }

        void IPowerListener.OnBatteryDepleted()
        {
            Logger.DebugLog("OnBatteryDepleted");
        }

        void IPlayerListener.OnPlayerEntry()
        {
            Logger.DebugLog("OnPlayerEntry");
            // TODO: conditional welcome aboard lines
            voice.EnqueueClip(voice.welcomeAboardCASO);
        }

        void IPlayerListener.OnPlayerExit()
        {
            Logger.DebugLog("OnPlayerExit");
        }

        void IPlayerListener.OnPilotBegin()
        {
            Logger.DebugLog("OnPilotBegin");
        }

        void IPlayerListener.OnPilotEnd()
        {
            Logger.DebugLog("OnPilotEnd");
        }

        void IPowerListener.OnBatteryDead()
        {
            Logger.DebugLog("OnBatteryDead");
        }

        void IPowerListener.OnBatteryRevive()
        {
            Logger.DebugLog("OnBatteryRevive");
        }

        void IAutoPilotListener.OnAutoLevelBegin()
        {
            Logger.DebugLog("OnAutoLevelBegin");
            voice.EnqueueClip(voice.leveling);
        }

        void IAutoPilotListener.OnAutoLevelEnd()
        {
            Logger.DebugLog("OnAutoLevelEnd");
        }

        void IAutoPilotListener.OnAutoPilotBegin()
        {
            Logger.DebugLog("OnAutoPilotBegin");
        }

        void IAutoPilotListener.OnAutoPilotEnd()
        {
            Logger.DebugLog("OnAutoPilotEnd");
        }
    }
}
