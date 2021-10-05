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
        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }
        public void Start()
        {
            foreach (var ps in mv.PilotSeats)
            {
                var speaker = ps.Seat.AddComponent<AudioSource>();
                speaker.spatialBlend = 1f;
                speakers.Add(speaker);
            }
            StartCoroutine(GetAudioClip());
        }
        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Backslash))
            {
                EnqueueClip(leveling);
            }
            if(speechQueue.Count > 0)
            {
                TryPlayNextClipInQueue();
            }
        }
        public void TryPlayNextClipInQueue()
        {
            if (speechQueue.TryDequeue(out AudioClip clip))
            {
                foreach(var speaker in speakers)
                {
                    speaker.clip = clip;
                    speaker.Play();
                }
            }
        }
        public void EnqueueClipWithPriority(AudioClip clip, int priority)
        {
            speechQueue.Enqueue(clip, priority);
        }
        public void EnqueueClip(AudioClip clip)
        {
            speechQueue.Enqueue(clip, 0);
        }
        IEnumerator GetAudioClip()
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetPath = Path.Combine(modPath, "AutoPilotSally/Leveling.ogg");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + assetPath, AudioType.OGGVORBIS))
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
        }
    }
}
