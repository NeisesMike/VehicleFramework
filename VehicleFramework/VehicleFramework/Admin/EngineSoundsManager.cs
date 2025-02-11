using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;
using VehicleFramework.Engines;

namespace VehicleFramework
{
    public class EngineSounds
    {
        public AudioClip hum;
        public AudioClip whistle;
    }
    public static class EngineSoundsManager
    {
        internal static List<ModVehicleEngine> engines = new List<ModVehicleEngine>();
        // EngineSounds names : EngineSounds
        internal static Dictionary<string, EngineSounds> EngineSoundss = new Dictionary<string, EngineSounds>();
        // vehicle names : EngineSounds names
        internal static Dictionary<TechType, string> defaultEngineSounds = new Dictionary<TechType, string>();
        public static EngineSounds silentVoice = new EngineSounds();
        public static void RegisterEngineSounds(string name, EngineSounds voice)
        {
            try
            {
                EngineSoundss.Add(name, voice);
            }
            catch (ArgumentException e)
            {
                Logger.Warn("Tried to register an engine-sounds using a name that already exists: " + name + ". " + e.Message);
                return;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to register a engine-sounds: " + e.Message);
                return;
            }
            Logger.Log("Successfully registered engine-sounds: " + name);
        }
        public static IEnumerator RegisterEngineSounds(string name, string voicepath="")
        {
            yield return LoadEngineSoundClips(name, EngineSounds =>
            {
                // Once the voice is loaded, store it in the dictionary
                RegisterEngineSounds(name, EngineSounds);
            }, voicepath);
        }
        public static EngineSounds GetVoice(string name)
        {
            try
            {
                return EngineSoundss[name];
            }
            catch(KeyNotFoundException e)
            {
                Logger.Warn("That engine-sounds not found: " + name + ". " + e.Message);
            }
            catch(ArgumentNullException e)
            {
                Logger.Warn("That engine-sounds was null: " + e.Message);
            }
            catch(Exception e)
            {
                Logger.Error("GetVoice engine-sounds failed: " + e.Message);
            }
            return silentVoice;
        }
        public static void RegisterDefault(ModVehicle mv, string voice)
        {
            try
            {
                defaultEngineSounds.Add(mv.TechType, voice);
            }
            catch (ArgumentException e)
            {
                Logger.Warn("Tried to register a default engine-sounds for a vehicle that already had a default engine-sounds." + e.Message);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to register a default engine-sounds: " + e.Message);
            }
        }
        internal static void UpdateDefaultVoice(ModVehicle mv, string voice)
        {
            if (defaultEngineSounds.ContainsKey(mv.TechType))
            {
                defaultEngineSounds[mv.TechType] = voice;
            }
            else
            {
                defaultEngineSounds.Add(mv.TechType, voice);
            }
        }
        public static EngineSounds GetDefaultVoice(ModVehicle mv)
        {
            try
            {
                return EngineSoundss[defaultEngineSounds[mv.TechType]];
            }
            catch(Exception e)
            {
                Logger.Warn("No default engine sounds for vehicle type: " + mv.name + ". Using Shiruba. " + e.Message);
                return EngineSoundss.First().Value;
            }
        }
        internal static IEnumerator LoadAllVoices()
        {
            GetSilence();
            yield return RegisterEngineSounds("ShirubaFoxy");
        }
        private static IEnumerator GetSilence()
        {
            while(VoiceManager.silence == null)
            {
                yield return null;
            }
            silentVoice = new EngineSounds
            {
                hum = VoiceManager.silence,
                whistle = VoiceManager.silence,
            };
            yield break;
        }
        // Method signature with a callback to return the EngineSounds instance
        private static IEnumerator LoadEngineSoundClips(string voice, Action<EngineSounds> onComplete, string voicepath)
        {
            EngineSounds returnVoice = new EngineSounds();
            
            string modPath = "";
            if(voicepath == "")
            {
                modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            }
            else
            {
                modPath = voicepath;
            }
            string engineSoundsFolder = Path.Combine(modPath, "EngineSounds");
            string engineSoundPath = Path.Combine(engineSoundsFolder, voice) + "/";

            // List of clip names to load, corresponding to their fields in EngineSounds
            string[] clipNames = {
            "whistle",
            "hum"
        };

            foreach (string clipName in clipNames)
            {
                string path = "file://" + engineSoundPath + clipName + ".ogg";
                yield return LoadAudioClip(path, clip =>
                {
                    // Use reflection to set the clip dynamically based on its name
                    clip.name = clipName;
                    typeof(EngineSounds).GetField(clipName).SetValue(returnVoice, clip);
                },
                () =>
                {
                    // Handle error, potentially logging and assigning Silence
                    Logger.Warn($"WARNING: {clipName} could not be loaded. Assigning Silence.");
                    typeof(EngineSounds).GetField(clipName).SetValue(returnVoice, VoiceManager.silence);
                });
            }

            onComplete?.Invoke(returnVoice);
        }
        private static IEnumerator LoadAudioClip(string filePath, Action<AudioClip> onSuccess, Action onError)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    onError?.Invoke();
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip == null)
                    {
                        Logger.Error("Failed to retrieve AudioClip from file: " + filePath);
                    }
                    else
                    {
                        onSuccess?.Invoke(clip);
                    }
                }
            }
        }
    }
}
