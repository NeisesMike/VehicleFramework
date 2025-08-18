using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using VehicleFramework.Engines;
using static uGUI_ResourceTracker;

namespace VehicleFramework
{
    public class EngineSounds
    {
        public required AudioClip hum;
        public required AudioClip whistle;
    }
    public static class EngineSoundsManager
    {
        internal static List<ModVehicleEngine> engines = new();
        // EngineSounds names : EngineSounds
        internal static Dictionary<string, EngineSounds> EngineSoundss = new();
        // vehicle names : EngineSounds names
        internal static Dictionary<TechType, string> defaultEngineSounds = new();
        public static void RegisterEngineSounds(string name, EngineSounds voice)
        {
            try
            {
                EngineSoundss.Add(name, voice);
            }
            catch (ArgumentException e)
            {
                Logger.WarnException($"Tried to register an engine-sounds using a name that already exists: {name}.", e);
                return;
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to register a engine-sounds: {name}.", e);
                return;
            }
            Logger.Log($"Successfully registered engine-sounds: {name}.");
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
                Logger.WarnException($"That engine-sounds not found: {name}.", e);
            }
            catch(ArgumentNullException e)
            {
                Logger.WarnException($"That engine-sounds was null: {name}. ", e);
            }
            catch(Exception e)
            {
                Logger.LogException($"GetVoice engine-sounds failed: {name}.", e);
            }
            return new EngineSounds { hum = VoiceManager.silence, whistle = VoiceManager.silence };
        }
        public static void RegisterDefault(ModVehicle mv, string voice)
        {
            if(mv == null)
            {
                Logger.Error($"Cannot register default engine sounds for null ModVehicle: {voice}.");
                return;
            }
            try
            {
                defaultEngineSounds.Add(mv.TechType, voice);
            }
            catch (ArgumentException e)
            {
                Logger.WarnException($"Tried to register a default engine-sounds for a vehicle {mv.GetName()} that already had a default engine-sounds {voice}.", e);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to register a default engine-sounds: {voice} for vehicle {mv.GetName()}.", e);
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
                Logger.Warn($"No default engine sounds for vehicle type: {mv.GetName()}. Using Shiruba.");
                Logger.LogException($"Exception Caught: ", e);
                return EngineSoundss.First().Value;
            }
        }
        internal static IEnumerator LoadAllVoices()
        {
            WaitForSilence();
            yield return RegisterEngineSounds("ShirubaFoxy");
            MainPatcher.Instance.GetEngineSounds = null;
        }
        private static IEnumerator WaitForSilence()
        {
            yield return new WaitUntil(() => VoiceManager.silence != null);
        }
        // Method signature with a callback to return the EngineSounds instance
        private static IEnumerator LoadEngineSoundClips(string voice, Action<EngineSounds> onComplete, string voicepath)
        {
            
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

            AudioClip? myWhistle = null;
            string path1 = "file://" + engineSoundPath + "whistle" + ".ogg";
            yield return LoadAudioClip(path1, clip =>
            {
                clip.name = "whistle";
                myWhistle = clip;
            },
            () =>
            {
                Logger.Warn($"WARNING: {"whistle"} could not be loaded. Assigning Silence.");
            });

            AudioClip? myHum = null;
            string path2 = "file://" + engineSoundPath + "hum" + ".ogg";
            yield return LoadAudioClip(path2, clip =>
            {
                clip.name = "hum";
                myHum = clip;
            },
            () =>
            {
                Logger.Warn($"WARNING: {"hum"} could not be loaded. Assigning Silence.");
            });


            EngineSounds returnVoice = new() { hum = myHum ?? VoiceManager.silence, whistle = myWhistle ?? VoiceManager.silence };

            onComplete?.Invoke(returnVoice);
        }
        private static IEnumerator LoadAudioClip(string filePath, Action<AudioClip> onSuccess, Action onError)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS);
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
