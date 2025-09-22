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
    public class AutoPilot : MonoBehaviour, IPowerListener
    {
        public Submarine Sub => GetComponent<Submarine>();
        private bool HasSomePower => !isPoweredDown || Sub.energyInterface != null && Sub.energyInterface.hasCharge;

        private float timeOfLastLevelTap = 0f;
        private const float doubleTapWindow = 1f;
        private float PitchDelta => transform.rotation.eulerAngles.x >= 180 ? 360 - transform.rotation.eulerAngles.x : transform.rotation.eulerAngles.x;
        private float RollDelta => transform.rotation.eulerAngles.z >= 180 ? 360 - transform.rotation.eulerAngles.z : transform.rotation.eulerAngles.z;
        private float smoothTime = 0.3f;
        public float autoLevelRate = 11f;
        private bool _autoLeveling = false;
        public bool AutoLeveling
        {
            get
            {
                return _autoLeveling;
            }
            private set
            {
                if(value)
                {
                    if (!_autoLeveling)
                    {
                        GetComponent<AutoPilotSignals>()?.NotifyStatus(AutoPilotStatus.OnAutoLevelBegin);
                    }
                }
                else
                {
                    if (_autoLeveling)
                    {
                        GetComponent<AutoPilotSignals>()?.NotifyStatus(AutoPilotStatus.OnAutoLevelEnd);
                    }
                }
                _autoLeveling = value;
            }
        }
        private bool isPoweredDown = false;
        public void Awake()
        {
            if (Sub == null)
            {
                throw Admin.SessionManager.Fatal("AutoPilotVoice is not attached to a Submarine!");
            }
        }

        public void Update()
        {
            if (Sub != null && Sub.DoesAutolevel && Sub.VFEngine is Engines.ModVehicleEngine)
            {
                MaybeAutoLevel(Sub);
                CheckForDoubleTap(Sub);
            }
        }
        public void MaybeAutoLevel(Submarine mv)
        {
            Vector2 lookDir = GameInput.GetLookDelta();
            if (AutoLeveling && (10f < lookDir.magnitude || !mv.GetIsUnderwater()))
            {
                AutoLeveling = false;
                return;
            }
            if (HasSomePower && (AutoLeveling || !mv.IsPlayerControlling()) && mv.GetIsUnderwater())
            {
                if (RollDelta < 0.4f && PitchDelta < 0.4f && mv.useRigidbody.velocity.magnitude < mv.ExitVelocityLimit)
                {
                    AutoLeveling = false;
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
            if (HasSomePower && GameInput.GetButtonDown(GameInput.Button.Exit) && mv.IsPlayerControlling())
            {
                if (Time.time - timeOfLastLevelTap < doubleTapWindow)
                {
                    AutoLeveling = true;
                    var smoothTime1 = 5f * PitchDelta / 90f;
                    var smoothTime2 = 5f * RollDelta / 90f;
                    var smoothTime3 = mv.GetComponent<Engines.ModVehicleEngine>().GetTimeToStop();
                    smoothTime = Mathf.Max(smoothTime1, smoothTime2, smoothTime3);
                }
                else
                {
                    timeOfLastLevelTap = Time.time;
                }
            }
        }


        void IPowerListener.OnPowerUp()
        {
            isPoweredDown = false;
        }
        void IPowerListener.OnPowerDown()
        {
            isPoweredDown = true;
            AutoLeveling = false;
        }
        void IPowerListener.OnBatterySafe()
        {
        }
        void IPowerListener.OnBatteryLow()
        {
        }
        void IPowerListener.OnBatteryNearlyEmpty()
        {
        }
        void IPowerListener.OnBatteryDepleted()
        {
        }
        void IPowerListener.OnBatteryDead()
        {
        }
        void IPowerListener.OnBatteryRevive()
        {
        }

    }
}
