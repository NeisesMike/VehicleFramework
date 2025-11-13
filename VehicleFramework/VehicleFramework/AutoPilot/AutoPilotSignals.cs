using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;

namespace VehicleFramework.AutoPilot
{
    public class AutoPilotSignals : MonoBehaviour, IVehicleStatusListener, IPlayerListener, IPowerListener, ILightsStatusListener, IAutoPilotListener, IScuttleListener
    {
        public AutoPilotVoice? AutoPilotVoice;
        public ModVehicle MV => GetComponent<ModVehicle>();
        public LiveMixin LiveMixin => MV.liveMixin;

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

        public void Awake()
        {
            if (MV == null)
            {
                throw Admin.SessionManager.Fatal("AutoPilotVoice is not attached to a ModVehicle!");
            }
            healthStatus = HealthState.Safe;
            powerStatus = PowerState.Safe;
            depthStatus = DepthState.Safe;
            dangerStatus = DangerState.Safe;
        }

        public void Update()
        {
            UpdateHealthState();
            UpdatePowerState();
            UpdateDepthState();
        }
        private void UpdateHealthState()
        {
            float percentHealth = LiveMixin.health / LiveMixin.maxHealth;
            if (percentHealth < .05f)
            {
                if (healthStatus < HealthState.DoomImminent)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.HullFailureImminent);
                }
                healthStatus = HealthState.DoomImminent;
            }
            else if (percentHealth < .25f)
            {
                if (healthStatus < HealthState.Critical)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.HullIntegrityCritical);
                }
                healthStatus = HealthState.Critical;
            }
            else if (percentHealth < .40f)
            {
                if (healthStatus < HealthState.Low)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.HullIntegrityLow);
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
            MV.GetEnergyValues(out float totalPower, out float totalCapacity);
            if (totalPower < 0.1)
            {
                if (powerStatus < PowerState.OxygenOffline)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.OxygenProductionOffline);
                }
                powerStatus = PowerState.OxygenOffline;
            }
            else if (totalPower < 5)
            {
                if (powerStatus < PowerState.Depleted)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.BatteriesDepleted);
                }
                powerStatus = PowerState.Depleted;
            }
            else if (totalPower < 0.1f * totalCapacity)
            {
                if (powerStatus < PowerState.NearMT)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.BatteriesNearlyEmpty);
                }
                powerStatus = PowerState.NearMT;
            }
            else if (totalPower < 0.3f * totalCapacity)
            {
                if (powerStatus < PowerState.Low)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.PowerLow);
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
            float perilousDepth = crushDepth * 0.9f;
            float depth = transform.position.y;

            DepthState newDepthState = DepthState.Safe;
            if (depth < perilousDepth)
            {
                if(depth < crushDepth)
                {
                    newDepthState = DepthState.Lethal;
                }
                else
                {
                    newDepthState = DepthState.Perilous;
                }
            }

            if(depthStatus != newDepthState)
            {
                depthStatus = newDepthState;
                switch (depthStatus)
                {
                    case DepthState.Perilous:
                        AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.PassingSafeDepth);
                        break;
                    case DepthState.Lethal:
                        AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.MaximumDepthReached);
                        break;
                    default:
                        break;
                }
            }
        }

        public void NotifyStatus(AutoPilotStatus vs)
        {
            foreach (var component in GetComponentsInChildren<IAutoPilotListener>())
            {
                switch (vs)
                {
                    case AutoPilotStatus.OnAutoLevelBegin:
                        component.OnAutoLevelBegin();
                        break;
                    case AutoPilotStatus.OnAutoLevelEnd:
                        component.OnAutoLevelEnd();
                        break;
                    case AutoPilotStatus.OnAutoPilotBegin:
                        component.OnAutoPilotBegin();
                        break;
                    case AutoPilotStatus.OnAutoPilotEnd:
                        component.OnAutoPilotEnd();
                        break;
                    default:
                        Logger.Error("Error: tried to notify using an invalid status");
                        break;
                }
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
            AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.EnginePoweringUp);
            if (MV.IsUnderCommand)
            {
                static IEnumerator ShakeCamera()
                {
                    yield return new WaitForSeconds(4.6f);
                    if (MainCameraControl.main == null) yield break;
                    MainCameraControl.main.ShakeCamera(1f, 0.5f, MainCameraControl.ShakeMode.Linear, 1f);
                }
                Admin.SessionManager.StartCoroutine(ShakeCamera());
                MainCameraControl.main.ShakeCamera(0.15f, 4.5f, MainCameraControl.ShakeMode.Linear, 1f);
            }
        }

        void IPowerListener.OnPowerDown()
        {
            Logger.DebugLog("OnPowerDown");
            AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.EnginePoweringDown);
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
            if (powerStatus < PowerState.NearMT && UnityEngine.Random.value < 0.5f)
            {
                AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.WelcomeAboardAllSystemsOnline);
            }
            else
            {
                AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.WelcomeAboard);
            }
        }

        void IPlayerListener.OnPlayerExit()
        {
            Logger.DebugLog("OnPlayerExit");
            AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.Goodbye);
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
            AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.Leveling);
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
                yield return new WaitUntil(() => Mathf.Abs(Time.time - timeWeStartedWaiting) >= MAX_TIME_TO_WAIT);
                dangerStatus = DangerState.Safe;
            }
            StopAllCoroutines();
            timeWeStartedWaiting = Time.time;
            Admin.SessionManager.StartCoroutine(ResetDangerStatusEventually());
            if (dangerStatus == DangerState.Safe)
            {
                dangerStatus = DangerState.LeviathanNearby;
                if (new System.Random().NextDouble() < 0.5)
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.LeviathanDetected);
                }
                else
                {
                    AutoPilotVoice?.EnqueueClip(AutoPilotVoice.voice?.UhOh);
                }
            }
        }

        void IScuttleListener.OnScuttle()
        {
            enabled = false;
        }

        void IScuttleListener.OnUnscuttle()
        {
            enabled = true;
        }
    }
}
