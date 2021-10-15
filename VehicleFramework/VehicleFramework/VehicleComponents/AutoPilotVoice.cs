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

namespace VehicleFramework
{
    public class AutoPilotVoice : MonoBehaviour
    {
        public ModVehicle mv;
        private List<AudioSource> speakers = new List<AudioSource>();
        private PriorityQueue<AudioClip> speechQueue = new PriorityQueue<AudioClip>();

        public AudioClip leveling;
        public AudioClip welcomeAboardCASO;
        public AudioClip poweringUp;


        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }
        public void Start()
        {
            AudioSource speakerPtr;
            foreach (var ps in mv.PilotSeats)
            {
                speakerPtr = ps.Seat.AddComponent<AudioSource>();
                speakerPtr.spatialBlend = 1f;
                speakers.Add(speakerPtr);
            }
            foreach (var ps in mv.Hatches)
            {
                speakerPtr = ps.Hatch.AddComponent<AudioSource>();
                speakerPtr.spatialBlend = 1f;
                speakers.Add(speakerPtr);
            }
            foreach (var ps in mv.TetherSources)
            {
                speakerPtr = ps.AddComponent<AudioSource>();
                speakerPtr.spatialBlend = 1f;
                speakers.Add(speakerPtr);
            }
            StartCoroutine(GetAudioClip());
        }
        public void Update()
        {
            if (aiEI.hasCharge)
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
                var nearestSpeaker = speakers.First();
                float nearestSpeakerDist = Vector3.Distance(Player.main.transform.position, nearestSpeaker.transform.position);
                foreach(var speaker in speakers)
                {
                    var thisDist = Vector3.Distance(Player.main.transform.position, speaker.transform.position);
                    if (thisDist < nearestSpeakerDist)
                    {
                        nearestSpeaker = speaker;
                        nearestSpeakerDist = thisDist;
                    }
                }
                nearestSpeaker.volume = MainPatcher.Config.aiVoiceVolume / 100f;
                nearestSpeaker.clip = clip;
                nearestSpeaker.Play();
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
            if (mv && aiEI.hasCharge && clip)
            {
                speechQueue.Enqueue(clip, 0);
            }
        }
        IEnumerator GetAudioClip()
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string autoPilotSallyPath = Path.Combine(modPath, "AutoPilotSally/");
            string levelPath = Path.Combine(autoPilotSallyPath, "Leveling.ogg");
            string welcoPath = Path.Combine(autoPilotSallyPath, "WelcomeAboard.ogg");
            string powerPath = Path.Combine(autoPilotSallyPath, "PoweringUp.ogg");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + levelPath, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    leveling = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + welcoPath, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    welcomeAboardCASO = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + powerPath, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    poweringUp = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }
    }
}
