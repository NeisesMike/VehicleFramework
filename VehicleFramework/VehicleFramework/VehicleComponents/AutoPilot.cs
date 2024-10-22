using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VehicleFramework.VehicleTypes;
using VehicleFramework.VehicleComponents;

namespace VehicleFramework
{
    public class AutoPilot : MonoBehaviour, IVehicleStatusListener, IPlayerListener, IPowerListener, ILightsStatusListener, IAutoPilotListener
	{
		public ModVehicle mv;
        public EnergyInterface aiEI;
        public AutoPilotVoice apVoice;
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
        private float PitchDelta => transform.rotation.eulerAngles.x >= 180 ? 360 - transform.rotation.eulerAngles.x : transform.rotation.eulerAngles.x;
        private float RollDelta => transform.rotation.eulerAngles.z >= 180 ? 360 - transform.rotation.eulerAngles.z : transform.rotation.eulerAngles.z;
        private float smoothTime = 0.3f;
        public float autoLevelRate = 11f;
        private bool _autoLeveling = false;
        private bool autoLeveling
        {
            get
            {
                return _autoLeveling;
            }
            set
            {
                if(value)
                {
                    if (!_autoLeveling)
                    {
                        mv.NotifyStatus(AutoPilotStatus.OnAutoLevelBegin);
                    }
                }
                else
                {
                    if (_autoLeveling)
                    {
                        mv.NotifyStatus(AutoPilotStatus.OnAutoLevelEnd);
                    }
                }
                _autoLeveling = value;
            }
        }
        private bool isDead = false;
        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
            mv.voice = apVoice = mv.gameObject.EnsureComponent<AutoPilotVoice>();
            mv.voice.voice = VoiceManager.GetDefaultVoice(mv);
            mv.gameObject.EnsureComponent<AutoPilotNavigator>();
            liveMixin = mv.liveMixin;
            eInterf = mv.energyInterface;
            healthStatus = HealthState.Safe;
            powerStatus = PowerState.Safe;
            depthStatus = DepthState.Safe;
            dangerStatus = DangerState.Safe;
        }
        public void Start()
        {
            if (mv.BackupBatteries != null && mv.BackupBatteries.Count > 0)
            {
                aiEI = mv.BackupBatteries[0].BatterySlot.GetComponent<EnergyInterface>();
            }
            else
            {
                aiEI = mv.energyInterface;
            }
        }

        public void Update()
        {
            UpdateHealthState();
            UpdatePowerState();
            UpdateDepthState();
            if(mv as Drone == null)
            {
                MaybeRefillOxygen();
            }
            if (mv as Submarine != null)
            {
                MaybeAutoLevel(mv as Submarine);
                CheckForDoubleTap(mv as Submarine);
            }
        }
        public void MaybeAutoLevel(Submarine mv)
        {
            Vector2 lookDir = GameInput.GetLookDelta();
            if (autoLeveling && (10f < lookDir.magnitude || !mv.GetIsUnderwater()))
            {
                autoLeveling = false;
                return;
            }
            if ((!isDead || aiEI.hasCharge) && (autoLeveling || !mv.IsPlayerControlling()) && mv.GetIsUnderwater())
            {
                if (RollDelta < 0.4f && PitchDelta < 0.4f && mv.useRigidbody.velocity.magnitude < mv.ExitVelocityLimit)
                {
                    autoLeveling = false;
                    return;
                }
                if (RollDelta > 0.4f || PitchDelta > 0.4f)
                {
                    Quaternion desiredRotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                    // Smoothly move towards target rotation using physics
                    Quaternion smoothedRotation = Quaternion.RotateTowards(
                        mv.useRigidbody.rotation,
                        desiredRotation,
                        smoothTime * Time.deltaTime * autoLevelRate
                    );
                    mv.useRigidbody.MoveRotation(smoothedRotation);
                }
            }
        }
        private void CheckForDoubleTap(Submarine mv)
        {
            if ((!isDead || aiEI.hasCharge) && GameInput.GetButtonDown(GameInput.Button.Exit) && mv.IsPlayerControlling())
            {
                if (Time.time - timeOfLastLevelTap < doubleTapWindow)
                {
                    autoLeveling = true;
                    var smoothTime1 = 5f * PitchDelta / 90f;
                    var smoothTime2 = 5f * RollDelta / 90f;
                    var smoothTime3 = mv.GetComponent<VehicleFramework.Engines.ModVehicleEngine>().GetTimeToStop();
                    smoothTime = Mathf.Max(smoothTime1, smoothTime2, smoothTime3);
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
                    apVoice.EnqueueClip(apVoice.voice.HullFailureImminent);
                }
                healthStatus = HealthState.DoomImminent;
            }
            else if (percentHealth < .25f)
            {
                if (healthStatus < HealthState.Critical)
                {
                    apVoice.EnqueueClip(apVoice.voice.HullIntegrityCritical);
                }
                healthStatus = HealthState.Critical;
            }
            else if (percentHealth < .40f)
            {
                if (healthStatus < HealthState.Low)
                {
                    apVoice.EnqueueClip(apVoice.voice.HullIntegrityLow);
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
            float totalAIPower = eInterf.TotalCanProvide(out _);
            if (totalPower < 0.1 && totalAIPower < 0.1)
            {
                if (powerStatus < PowerState.OxygenOffline)
                {
                    apVoice.EnqueueClip(apVoice.voice.OxygenProductionOffline);
                }
                powerStatus = PowerState.OxygenOffline;
            }
            else if (totalPower < 5)
            {
                if (powerStatus < PowerState.Depleted)
                {
                    apVoice.EnqueueClip(apVoice.voice.BatteriesDepleted);
                }
                powerStatus = PowerState.Depleted;
            }
            else if (totalPower < 100)
            {
                if (powerStatus < PowerState.NearMT)
                {
                    apVoice.EnqueueClip(apVoice.voice.BatteriesNearlyEmpty);
                }
                powerStatus = PowerState.NearMT;
            }
            else if (totalPower < 320)
            {
                if (powerStatus < PowerState.Low)
                {
                    apVoice.EnqueueClip(apVoice.voice.PowerLow);
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
            if (depth < crushDepth)
            {
                if (depthStatus < DepthState.Lethal)
                {
                    apVoice.EnqueueClip(apVoice.voice.MaximumDepthReached);
                }
                depthStatus = DepthState.Lethal;
            }
            else if (depth < perilousDepth)
            {
                if (depthStatus < DepthState.Perilous)
                {
                    apVoice.EnqueueClip(apVoice.voice.PassingSafeDepth);
                }
                depthStatus = DepthState.Perilous;
            }
            else
            {
                depthStatus = DepthState.Safe;
            }
        }
        private void MaybeRefillOxygen()
        {
            float totalPower = mv.energyInterface.TotalCanProvide(out _);
            float totalAIPower = eInterf.TotalCanProvide(out _);
            if (totalPower < 0.1 && totalAIPower >= 0.1 && mv.IsUnderCommand)
            {
                // The main batteries are out, so the AI will take over life support.
                OxygenManager oxygenMgr = Player.main.oxygenMgr;
                float num;
                float num2;
                oxygenMgr.GetTotal(out num, out num2);
                float amount = Mathf.Min(num2 - num, mv.oxygenPerSecond * Time.deltaTime) * mv.oxygenEnergyCost;
                float secondsToAdd = mv.AIEnergyInterface.ConsumeEnergy(amount) / mv.oxygenEnergyCost;
                oxygenMgr.AddOxygen(secondsToAdd);
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
            apVoice.EnqueueClip(apVoice.voice.EnginePoweringUp);
            if (mv.IsUnderCommand)
            {
                IEnumerator ShakeCamera()
                {
                    yield return new WaitForSeconds(4.6f);
                    MainCameraControl.main.ShakeCamera(1f, 0.5f, MainCameraControl.ShakeMode.Linear, 1f);
                }
                StartCoroutine(ShakeCamera());
                MainCameraControl.main.ShakeCamera(0.15f, 4.5f, MainCameraControl.ShakeMode.Linear, 1f);
            }
        }

        void IPowerListener.OnPowerDown()
        {
            Logger.DebugLog("OnPowerDown");
            isDead = true;
            autoLeveling = false;
            apVoice.EnqueueClip(apVoice.voice.EnginePoweringDown);
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
                apVoice.EnqueueClip(apVoice.voice.WelcomeAboardAllSystemsOnline);
            }
            else
            {
                apVoice.EnqueueClip(apVoice.voice.WelcomeAboard);
            }
        }

        void IPlayerListener.OnPlayerExit()
        {
            Logger.DebugLog("OnPlayerExit");
            apVoice.EnqueueClip(apVoice.voice.Goodbye);
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
            apVoice.EnqueueClip(apVoice.voice.Leveling);
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
                    apVoice.EnqueueClip(apVoice.voice.LeviathanDetected);
                }
                else
                {
                    apVoice.EnqueueClip(apVoice.voice.UhOh);
                }
            }
        }
    }
}
