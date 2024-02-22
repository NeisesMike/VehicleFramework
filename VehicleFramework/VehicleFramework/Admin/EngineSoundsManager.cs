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
using VehicleFramework.Engines;

namespace VehicleFramework
{
    public enum KnownEngineSounds
    {
        // Only add new items to the end of this list
        // That way, dependent mods won't get bamboozled
        ShirubaFoxy
    }
    public class EngineSounds
    {
        public AudioClip hum;
        public AudioClip whistle;
    }
    public static class EngineSoundsManager
    {
        public static List<ModVehicleEngine> engines = new List<ModVehicleEngine>();
        // EngineSounds names : EngineSounds
        private static Dictionary<string, EngineSounds> EngineSoundss = new Dictionary<string, EngineSounds>();
        // vehicle names : EngineSounds names
        private static Dictionary<string, string> defaultEngineSounds = new Dictionary<string, string>();
        public static EngineSounds silentVoice = new EngineSounds();
        public static void RegisterVoice(string name, EngineSounds voice)
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
        public static IEnumerator RegisterVoice(string name, string voicepath="")
        {
            yield return LoadEngineSoundClips(name, EngineSounds =>
            {
                // Once the voice is loaded, store it in the dictionary
                RegisterVoice(name, EngineSounds);
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
                defaultEngineSounds.Add(mv.name, voice);
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
        public static void RegisterDefault(ModVehicle mv, KnownEngineSounds voice)
        {
            RegisterDefault(mv, GetKnownVoice(voice));
        }
        public static EngineSounds GetDefaultVoice(ModVehicle mv)
        {
            try
            {
                return EngineSoundss[defaultEngineSounds[mv.name]];
            }
            catch(Exception e)
            {
                Logger.Warn("No default engine sounds for vehicle type: " + mv.name + ". Using Shiruba. " + e.Message);
                return GetVoice(GetKnownVoice(KnownEngineSounds.ShirubaFoxy));
            }
        }
        public static IEnumerator LoadAllVoices()
        {
            GetSilence();
            yield return RegisterVoice(GetKnownVoice(KnownEngineSounds.ShirubaFoxy));
        }
        public static IEnumerator GetSilence()
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
        public static IEnumerator LoadEngineSoundClips(string voice, Action<EngineSounds> onComplete, string voicepath)
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
        public static string GetKnownVoice(KnownEngineSounds name)
        {
            switch (name)
            {
                case KnownEngineSounds.ShirubaFoxy:
                    return "ShirubaFoxy";
                default:
                    break;
            }
            return "The KnownEngineSounds enum is likely outdated";
        }
    }
}
