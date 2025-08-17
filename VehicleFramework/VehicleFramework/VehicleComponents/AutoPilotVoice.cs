using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public class AutoPilotVoice : MonoBehaviour, IScuttleListener
    {
        public ModVehicle mv;
        public EnergyInterface aiEI;
        private readonly List<AudioSource> speakers = new();
        private readonly PriorityQueue<AudioClip> speechQueue = new();
        private bool isReadyToSpeak = false; 
        public bool blockVoiceChange = false;
        public VehicleVoice voice = null;
        private float m_balance = 1f;
        public float Balance // used by WraithJet
        {
            get
            {
                return m_balance;
            }
            set
            {
                if(value < 0)
                {
                    m_balance = 0;
                }
                else if(1 < value)
                {
                    m_balance = 1;
                }
                else
                {
                    m_balance = value;
                }
            }
        }

        public void PauseSpeakers(bool pause)
        {
            foreach (var sp in speakers)
            {
                if(sp != null)
                {
                    if (pause)
                    {
                        sp.Pause();
                    }
                    else
                    {
                        sp.UnPause();
                    }
                }
            }
        }
        public void Awake()
        {
            isReadyToSpeak = false;
            mv = GetComponent<ModVehicle>();
            if (mv.BackupBatteries != null && mv.BackupBatteries.Count > 0)
            {
                aiEI = mv.BackupBatteries[0].BatterySlot.GetComponent<EnergyInterface>();
            }
            else
            {
                aiEI = mv.energyInterface;
            }

            // register self with mainpatcher, for on-the-fly voice selection updating
            VoiceManager.voices.Add(this);
            IEnumerator WaitUntilReadyToSpeak()
            {
                yield return new WaitUntil(() => Admin.GameStateWatcher.IsWorldSettled);
                NotifyReadyToSpeak();
                yield break;
            }
            Admin.Utils.StartCoroutine(WaitUntilReadyToSpeak());

        }
        private void SetupSpeakers()
        {
            //speakers.Add(mv.VehicleModel.EnsureComponent<AudioSource>());
            if (mv as Submarine != null)
            {
                speakers.Add((mv as Submarine).PilotSeat.Seat.EnsureComponent<AudioSource>().Register());
                foreach (var ps in (mv as Submarine).Hatches)
                {
                    speakers.Add(ps.Hatch.EnsureComponent<AudioSource>().Register());
                }
                foreach (var ps in (mv as Submarine).TetherSources)
                {
                    speakers.Add(ps.EnsureComponent<AudioSource>().Register());
                }
            }
            if (mv as Submersible != null)
            {
                speakers.Add((mv as Submersible).PilotSeat.Seat.EnsureComponent<AudioSource>().Register());
                foreach (var ps in (mv as Submersible).Hatches)
                {
                    speakers.Add(ps.Hatch.EnsureComponent<AudioSource>().Register());
                }
            }
            foreach (var sp in speakers)
            {
                sp.gameObject.EnsureComponent<AudioLowPassFilter>().cutoffFrequency = 1500;
                sp.priority = 1;
                sp.playOnAwake = false;
                sp.clip = VoiceManager.silence;
                sp.spatialBlend = 0.92f;
                sp.spatialize = true;
                sp.rolloffMode = AudioRolloffMode.Linear;
                sp.minDistance = 0f;
                sp.maxDistance = 100f;
                sp.spread = 180;
            }
        }
        public void Start()
        {
            SetupSpeakers();
        }
        public void SetVoice(VehicleVoice inputVoice)
        {
            if (!blockVoiceChange)
            {
                voice = inputVoice;
            }
        }
        public void Update()
        {
            foreach (var speaker in speakers)
            {
                if (mv.IsUnderCommand)
                {
                    speaker.GetComponent<AudioLowPassFilter>().enabled = false;
                }
                else
                {
                    speaker.GetComponent<AudioLowPassFilter>().enabled = true;
                }
            }
            if (aiEI.hasCharge && isReadyToSpeak)
            {
                if (speechQueue.Count > 0)
                {
                    bool tmp = false;
                    foreach(var but in speakers)
                    {
                        if(but.isPlaying)
                        {
                            tmp = true;
                            break;
                        }
                    }
                    if (!tmp)
                    {
                        TryPlayNextClipInQueue();
                    }
                }
            }
        }
        public void TryPlayNextClipInQueue()
        {
            if (speechQueue.TryDequeue(out AudioClip clip))
            {
                foreach(var speaker in speakers)
                {
                    speaker.volume = Balance * VehicleConfig.GetConfig(mv).AutopilotVolume.Value * SoundSystem.GetVoiceVolume() * SoundSystem.GetMasterVolume();
                    speaker.clip = clip;
                    speaker.Play();
                    if (MainPatcher.NautilusConfig.IsSubtitles && clip != VoiceManager.silence)
                    {
                        CreateSubtitle(clip);
                    }
                }
            }
        }
        public void EnqueueClipWithPriority(AudioClip clip, int priority)
        {
            if (mv && aiEI.hasCharge)
            {
                speechQueue.Enqueue(clip, priority);
            }
        }
        public void EnqueueClip(AudioClip clip)
        {
            if (mv && aiEI.hasCharge && clip && isReadyToSpeak && mv.IsConstructed)
            {
                speechQueue.Enqueue(clip, 0);
            }
        }
        public void NotifyReadyToSpeak()
        {
            isReadyToSpeak = true;
        }
        void IScuttleListener.OnScuttle()
        {
            enabled = false;
        }
        void IScuttleListener.OnUnscuttle()
        {
            enabled = true;
        }
        #region subtitles
        public string BatteriesDepleted = Language.main.Get("VFSubtitleBatteriesDepleted");
        public string BatteriesNearlyEmpty = Language.main.Get("VFSubtitleBatteriesNearlyEmpty");
        public string PowerLow = Language.main.Get("VFSubtitlePowerLow");
        public string EnginePoweringDown = Language.main.Get("VFSubtitleEnginePoweringDown");
        public string EnginePoweringUp = Language.main.Get("VFSubtitleEnginePoweringUp");
        public string Goodbye = Language.main.Get("VFSubtitleGoodbye");
        public string HullFailureImminent = Language.main.Get("VFSubtitleHullFailureImminent");
        public string HullIntegrityCritical = Language.main.Get("VFSubtitleHullIntegrityCritical");
        public string HullIntegrityLow = Language.main.Get("VFSubtitleHullIntegrityLow");
        public string Leveling = Language.main.Get("VFSubtitleLeveling");
        public string WelcomeAboard = Language.main.Get("VFSubtitleWelcomeAboard");
        public string OxygenProductionOffline = Language.main.Get("VFSubtitleOxygenProductionOffline");
        public string WelcomeAboardAllSystemsOnline = Language.main.Get("VFSubtitleWelcomeAboardAllSystemsOnline");
        public string MaximumDepthReached = Language.main.Get("VFSubtitleMaximumDepthReached");
        public string PassingSafeDepth = Language.main.Get("VFSubtitlePassingSafeDepth");
        public string LeviathanDetected = Language.main.Get("VFSubtitleLeviathanDetected");
        public string UhOh = Language.main.Get("VFSubtitleUhOh");
        private void CreateSubtitle(AudioClip clip)
        {
            if(clip == null)
            {
                return;
            }
            if(clip == voice.BatteriesDepleted)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {BatteriesDepleted}");
            }
            else if (clip == voice.BatteriesNearlyEmpty)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {BatteriesNearlyEmpty}");
            }
            else if (clip == voice.PowerLow)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {PowerLow}");
            }
            else if (clip == voice.EnginePoweringDown)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {EnginePoweringDown}");
            }
            else if (clip == voice.EnginePoweringUp)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {EnginePoweringUp}");
            }
            else if (clip == voice.Goodbye)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {Goodbye}");
            }
            else if (clip == voice.HullFailureImminent)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {HullFailureImminent}");
            }
            else if (clip == voice.HullIntegrityCritical)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {HullIntegrityCritical}");
            }
            else if (clip == voice.HullIntegrityLow)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {HullIntegrityLow}");
            }
            else if (clip == voice.Leveling)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {Leveling}");
            }
            else if (clip == voice.WelcomeAboard)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {WelcomeAboard}");
            }
            else if (clip == voice.OxygenProductionOffline)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {OxygenProductionOffline}");
            }
            else if (clip == voice.WelcomeAboardAllSystemsOnline)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {WelcomeAboardAllSystemsOnline}");
            }
            else if (clip == voice.MaximumDepthReached)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {MaximumDepthReached}");
            }
            else if (clip == voice.PassingSafeDepth)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {PassingSafeDepth}");
            }
            else if (clip == voice.LeviathanDetected)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {LeviathanDetected}");
            }
            else if (clip == voice.UhOh)
            {
                Logger.PDANote($"{mv.subName.hullName.text}: {UhOh}");
            }
            else
            {
                Logger.Warn($"Vehicle {mv.subName.hullName.text} with voice {name} did not recognize clip {clip.name}");
            }
        }
        #endregion
    }
}
