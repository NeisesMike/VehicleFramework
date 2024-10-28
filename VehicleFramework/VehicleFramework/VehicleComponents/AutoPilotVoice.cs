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
    public class AutoPilotVoice : MonoBehaviour
    {
        public ModVehicle mv;
        public EnergyInterface aiEI;
        private List<AudioSource> speakers = new List<AudioSource>();
        private PriorityQueue<AudioClip> speechQueue = new PriorityQueue<AudioClip>();
        private bool isReadyToSpeak = false; 
        public bool blockVoiceChange = false;
        public VehicleVoice voice = null;
        private float m_balance = 1f;
        public float balance
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
                while (!Admin.GameStateWatcher.IsWorldSettled)
                {
                    yield return null;
                }
                NotifyReadyToSpeak();
                yield break;
            }
            UWE.CoroutineHost.StartCoroutine(WaitUntilReadyToSpeak());

        }
        private void SetupSpeakers()
        {
            //speakers.Add(mv.VehicleModel.EnsureComponent<AudioSource>());
            if (mv as Submarine != null)
            {
                foreach (var ps in (mv as Submarine).PilotSeats)
                {
                    speakers.Add(ps.Seat.EnsureComponent<AudioSource>());
                }
                foreach (var ps in (mv as Submarine).Hatches)
                {
                    speakers.Add(ps.Hatch.EnsureComponent<AudioSource>());
                }
                foreach (var ps in (mv as Submarine).TetherSources)
                {
                    speakers.Add(ps.EnsureComponent<AudioSource>());
                }
            }
            if (mv as Submersible != null)
            {
                speakers.Add((mv as Submersible).PilotSeat.Seat.EnsureComponent<AudioSource>());
                foreach (var ps in (mv as Submersible).Hatches)
                {
                    speakers.Add(ps.Hatch.EnsureComponent<AudioSource>());
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
        public void SetVoiceEnum(KnownVoices voiceName)
        {
            if (!blockVoiceChange)
            {
                voice = VoiceManager.GetVoice(VoiceManager.GetKnownVoice(voiceName));
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
                //speakers.ForEach(x => x.PlayOneShot(clip, MainPatcher.VFConfig.aiVoiceVolume / 100f));
                foreach(var speaker in speakers)
                {
                    speaker.volume = balance * MainPatcher.VFConfig.aiVoiceVolume / 100f;
                    speaker.clip = clip;
                    speaker.Play();
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
            if (mv && aiEI.hasCharge && clip && isReadyToSpeak && mv.GetComponent<PingInstance>().enabled)
            {
                speechQueue.Enqueue(clip, 0);
            }
        }
        public void NotifyReadyToSpeak()
        {
            isReadyToSpeak = true;
        }
    }
}
