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
        public LiveMixin liveMixin;
        public EnergyInterface eInterf;

        public enum HealthState
        {
            Safe,
            Low,
            Critical,
            DoomImminent
        }
        public HealthState healthStatus;
        public enum PowerState
        {
            Safe,
            Low,
            NearMT,
            Depleted,
            OxygenOffline
        }
        public PowerState powerStatus;
        public enum DepthState
        {
            Safe,
            Perilous,
            Lethal
        }
        public DepthState depthStatus;
        public enum DangerState
        {
            Safe,
            LeviathanNearby,
        }
        public DangerState dangerStatus;

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
            liveMixin = mv.liveMixin;
            eInterf = mv.energyInterface;
            healthStatus = HealthState.Safe;
            powerStatus = PowerState.Safe;
            depthStatus = DepthState.Safe;
            dangerStatus = DangerState.Safe;
        }
        public void Start()
        {
            aiEI = mv.AutopilotBattery.BatterySlot.GetComponent<EnergyInterface>();
        }

        public void Update()
        {
            CheckForDoubleTap();
            UpdateHealthState();
            UpdatePowerState();
            UpdateDepthState();
        }
        public void FixedUpdate()
        {
            Vector2 lookDir = GameInput.GetLookDelta();
            if (10f < lookDir.magnitude)
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

        private void CheckForDoubleTap()
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
        private void UpdateHealthState()
        {
            float percentHealth = (liveMixin.health / liveMixin.maxHealth);
            if (percentHealth < .05f)
            {
                if (healthStatus < HealthState.DoomImminent)
                {
                    voice.EnqueueClip(voice.HullFailureImminent);
                }
                healthStatus = HealthState.DoomImminent;
            }
            else if (percentHealth < .25f)
            {
                if (healthStatus < HealthState.Critical)
                {
                    voice.EnqueueClip(voice.HullIntegrityCritical);
                }
                healthStatus = HealthState.Critical;
            }
            else if (percentHealth < .40f)
            {
                if (healthStatus < HealthState.Low)
                {
                    voice.EnqueueClip(voice.HullIntegrityLow);
                }
                healthStatus = HealthState.Low;
            }
            else
            {
                healthStatus = HealthState.Safe;
            }
        }
        private void UpdatePowerState()
        {
            float totalPower = eInterf.TotalCanProvide(out _);
            if (totalPower < 0.1)
            {
                if (powerStatus < PowerState.OxygenOffline)
                {
                    voice.EnqueueClip(voice.OxygenProductionOffline);
                }
                powerStatus = PowerState.OxygenOffline;
            }
            else if (totalPower < 5)
            {
                if (powerStatus < PowerState.Depleted)
                {
                    voice.EnqueueClip(voice.BatteriesDepleted);
                }
                powerStatus = PowerState.Depleted;
            }
            else if (totalPower < 100)
            {
                if (powerStatus < PowerState.NearMT)
                {
                    voice.EnqueueClip(voice.BatteriesNearlyEmpty);
                }
                powerStatus = PowerState.NearMT;
            }
            else if (totalPower < 320)
            {
                if (powerStatus < PowerState.Low)
                {
                    voice.EnqueueClip(voice.PowerLow);
                }
                powerStatus = PowerState.Low;
            }
            else
            {
                powerStatus = PowerState.Safe;
            }
        }
        private void UpdateDepthState()
        {
            float crushDepth = GetComponent<CrushDamage>().crushDepth * -1;
            float perilousDepth = crushDepth + 100;
            float depth = transform.position.y;
            if (depth < crushDepth)
            {
                if (depthStatus < DepthState.Lethal)
                {
                    voice.EnqueueClip(voice.MaximumDepthReached);
                }
                depthStatus = DepthState.Lethal;
            }
            else if (depth < perilousDepth)
            {
                if (depthStatus < DepthState.Perilous)
                {
                    voice.EnqueueClip(voice.PassingSafeDepth);
                }
                depthStatus = DepthState.Perilous;
            }
            else
            {
                depthStatus = DepthState.Safe;
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
            voice.EnqueueClip(voice.EnginePoweringUp);
        }

        void IPowerListener.OnPowerDown()
        {
            Logger.DebugLog("OnPowerDown");
            isDead = true;
            autoLeveling = false;
            voice.EnqueueClip(voice.EnginePoweringDown);
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
            if (powerStatus < PowerState.NearMT)
            {
                voice.EnqueueClip(voice.WelcomeAboardAllSystemsOnline);
            }
            else
            {
                voice.EnqueueClip(voice.WelcomeAboard);
            }
        }

        void IPlayerListener.OnPlayerExit()
        {
            Logger.DebugLog("OnPlayerExit");
            voice.EnqueueClip(voice.Goodbye);
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
            voice.EnqueueClip(voice.Leveling);
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

        readonly float MAX_TIME_TO_WAIT = 3f;
        float timeWeStartedWaiting = 0f;
        void IVehicleStatusListener.OnNearbyLeviathan()
        {
            Logger.DebugLog("OnNearbyLeviathan");
            IEnumerator ResetDangerStatusEventually()
            {
                while (Mathf.Abs(Time.time - timeWeStartedWaiting) < MAX_TIME_TO_WAIT)
                {
                    yield return null;
                }
                dangerStatus = DangerState.Safe;
            }
            StopAllCoroutines();
            timeWeStartedWaiting = Time.time;
            StartCoroutine(ResetDangerStatusEventually());
            if (dangerStatus == DangerState.Safe)
            {
                dangerStatus = DangerState.LeviathanNearby;
                if ((new System.Random()).NextDouble() < 0.5)
                {
                    voice.EnqueueClip(voice.LeviathanDetected);
                }
                else
                {
                    voice.EnqueueClip(voice.UhOh);
                }
            }
        }
    }
}
