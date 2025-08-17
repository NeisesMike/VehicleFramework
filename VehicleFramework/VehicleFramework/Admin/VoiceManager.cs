using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;

namespace VehicleFramework
{
    public class VehicleVoice
    {
        public AudioClip BatteriesDepleted;
        public AudioClip BatteriesNearlyEmpty;
        public AudioClip PowerLow;
        public AudioClip EnginePoweringDown;
        public AudioClip EnginePoweringUp;
        public AudioClip Goodbye;
        public AudioClip HullFailureImminent;
        public AudioClip HullIntegrityCritical;
        public AudioClip HullIntegrityLow;
        public AudioClip Leveling;
        public AudioClip WelcomeAboard;
        public AudioClip OxygenProductionOffline;
        public AudioClip WelcomeAboardAllSystemsOnline;
        public AudioClip MaximumDepthReached;
        public AudioClip PassingSafeDepth;
        public AudioClip LeviathanDetected;
        public AudioClip UhOh;
    }
    public static class VoiceManager
    {
        #region public_api
        // voice that are in-play
        public static List<AutoPilotVoice> voices = new();
        public static AudioClip silence;
        public static VehicleVoice silentVoice = new();
        public const string DefaultVoicePath = "AutoPilotVoices";

        public static VehicleVoice GetVoice(string name)
        {
            try
            {
                return vehicleVoices[name];
            }
            catch(KeyNotFoundException e)
            {
                Logger.WarnException($"That voice '{name}' not found.", e);
            }
            catch(ArgumentNullException e)
            {
                Logger.WarnException($"That voice '{name}' was null: ", e);
            }
            catch(Exception e)
            {
                Logger.LogException($"GetVoice failed on {name}.", e);
            }
            return silentVoice;
        }
        public static void RegisterDefaultVoice(this ModVehicle mv, string voice)
        {
            try
            {
                defaultVoices.Add(mv.TechType, voice);
            }
            catch (ArgumentException e)
            {
                Logger.WarnException($"Tried to register a default voice {voice} for a vehicle {mv.GetName()} that already had a default voice.", e);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to register a default voice {voice} for the vehicle {mv.GetName()}.", e);
            }
        }
        public static IEnumerator RegisterVoice(string name, string conventionalPath = "")
        {
            yield return RegisterVoice(name, false, conventionalPath);
        }
        public static IEnumerator RegisterVoice(string name, bool verbose, string conventionalPath = "")
        {
            string modPath;
            if (conventionalPath == "")
            {
                modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                modPath = conventionalPath;
            }
            string folderWithVoiceFiles = Path.Combine(modPath, DefaultVoicePath, name);
            if(!CheckForVoiceClips(folderWithVoiceFiles, verbose))
            {
                Logger.Error(
                    "Voice Registration Error: " +
                    "Couldn't find voice files at this path: " + folderWithVoiceFiles + "\n"
                    + "This method takes the absolute path to the folder containing the AutoPilotVoices folder.\n"
                    + "This AutoPilotVoices folder should contain a folder named " + name + " that contains the voice files.\n"
                    + "You can use RegisterVoiceWithRelativePath or RegisterVoiceAbsolute to avoid this old naming convention."
                    );
                yield break;
            }

            yield return LoadVoiceClips(vehicleVoice =>
            {
                // Once the voice is loaded, store it in the dictionary
                RegisterVoice(name, vehicleVoice);
            }, folderWithVoiceFiles);
        }
        public static void RegisterVoiceWithRelativePath(string name, string relativePathToFolderWithVoiceFiles)
        {
            RegisterVoiceWithRelativePath(name, relativePathToFolderWithVoiceFiles, false);
        }
        public static void RegisterVoiceWithRelativePath(string name, string relativePathToFolderWithVoiceFiles, bool verbose)
        {
            string folderWithVoiceFiles = Path.Combine(
                Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),
                relativePathToFolderWithVoiceFiles);
            MainPatcher.Instance.StartCoroutine(RegisterVoiceAbsolute(name, folderWithVoiceFiles, verbose));
        }
        private static IEnumerator RegisterVoiceAbsolute(string name, string absolutePath, bool verbose)
        {
            if (!CheckForVoiceClips(absolutePath, verbose))
            {
                Logger.Error(
                    "Voice Registration Error: " +
                    "Couldn't find voice files at this path: " + absolutePath + "\n"
                    + "This method takes the relative path from your mod to the folder containing the voice sound files."
                    );
                yield break;
            }

            yield return LoadVoiceClips(vehicleVoice =>
            {
                // Once the voice is loaded, store it in the dictionary
                RegisterVoice(name, vehicleVoice);
            }, absolutePath);
        }
        public static void LogAllAvailableVoices()
        {
            Logger.Log("Voices available:");
            vehicleVoices.Select(x => x.Key).ForEach(x => Logger.Log(x));
        }
        #endregion

        #region internal
        private static readonly string[] clipNames = {
            "BatteriesDepleted",
            "BatteriesNearlyEmpty",
            "PowerLow",
            "EnginePoweringDown",
            "EnginePoweringUp",
            "Goodbye",
            "HullFailureImminent",
            "HullIntegrityCritical",
            "HullIntegrityLow",
            "Leveling",
            "WelcomeAboard",
            "OxygenProductionOffline",
            "WelcomeAboardAllSystemsOnline",
            "MaximumDepthReached",
            "PassingSafeDepth",
            "LeviathanDetected",
            "UhOh"
        };
        // voice names : voices
        internal static Dictionary<string, VehicleVoice> vehicleVoices = new();
        // vehicle names : voice names
        private static readonly Dictionary<TechType, string> defaultVoices = new();
        private static void RegisterVoice(string name, VehicleVoice voice)
        {
            RegisterVoice(name, voice, false);
        }
        private static void RegisterVoice(string name, VehicleVoice voice, bool verbose)
        {
            try
            {
                vehicleVoices.Add(name, voice);
            }
            catch (ArgumentException e)
            {
                Logger.WarnException($"Tried to register a voice using a name that already exists: {name}.", e);
                return;
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to register a voice: {name} " , e);
                return;
            }
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, $"Successfully registered voice: {name}.");
        }
        internal static VehicleVoice GetDefaultVoice(ModVehicle mv)
        {
            if(mv == null)
            {
                Logger.Error("Cannot get default voice for null ModVehicle!");
                return default;
            }
            string defaultOption;
            try
            {
                defaultOption = defaultVoices[mv.TechType];
            }
            catch (KeyNotFoundException e)
            {
                Logger.Warn($"Default voice option not found for vehicle: {mv.GetName()}.");
                Logger.LogException($"KeyNotFound: ", e);
                goto exit;
            }
            catch (ArgumentNullException e)
            {
                Logger.LogException($"That mv.name was null: {mv.GetName()}.", e);
                goto exit;
            }
            catch (Exception e)
            {
                Logger.LogException($"GetDefaultVoice option failed: {mv.GetName()}.", e);
                goto exit;
            }

            try
            {
                return vehicleVoices[defaultOption];
            }
            catch (KeyNotFoundException e)
            {
                Logger.Warn($"Default voice not found for vehicle: {mv.GetName()}.");
                Logger.LogException($"KeyNotFound: ", e);
            }
            catch (ArgumentNullException e)
            {
                Logger.LogException($"That default voice index was null: {mv.GetName()}.", e);
            }
            catch (Exception e)
            {
                Logger.LogException($"GetDefaultVoice failed: {mv.GetName()}.", e);
            }

        exit:
            return GetVoice(VehicleConfig.GetConfig(mv).AutopilotVoice.Value);
        }
        internal static void UpdateDefaultVoice(ModVehicle mv, string voice)
        {
            if (defaultVoices.ContainsKey(mv.TechType))
            {
                defaultVoices[mv.TechType] = voice;
            }
            else
            {
                defaultVoices.Add(mv.TechType, voice);
            }
        }
        internal static IEnumerator LoadAllVoices()
        {
            GetSilence();
            yield return RegisterVoice("ShirubaFoxy");
            yield return RegisterVoice("Airon");
            yield return RegisterVoice("Chels-E");
            yield return RegisterVoice("Mikjaw");
            yield return RegisterVoice("Turtle");
            yield return RegisterVoice("Salli");
            MainPatcher.Instance.GetVoices = null;
        }
        internal static IEnumerator GetSilence()
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string autoPilotVoicesFolder = Path.Combine(modPath, DefaultVoicePath);
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + autoPilotVoicesFolder + "/Silence.ogg", AudioType.OGGVORBIS);
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Logger.Error("ERROR: Silence.ogg not found. Directory error.");
                yield break;
            }
            silence = DownloadHandlerAudioClip.GetContent(www);

            silentVoice = new VehicleVoice
            {
                BatteriesDepleted = silence,
                BatteriesNearlyEmpty = silence,
                PowerLow = silence,
                EnginePoweringDown = silence,
                EnginePoweringUp = silence,
                Goodbye = silence,
                HullFailureImminent = silence,
                HullIntegrityCritical = silence,
                HullIntegrityLow = silence,
                Leveling = silence,
                WelcomeAboard = silence,
                OxygenProductionOffline = silence,
                WelcomeAboardAllSystemsOnline = silence,
                MaximumDepthReached = silence,
                PassingSafeDepth = silence,
                LeviathanDetected = silence,
                UhOh = silence
            };

            yield break;
        }
        private static IEnumerator LoadVoiceClips(Action<VehicleVoice> onComplete, string autoPilotVoicesFolder)
        {
            string autoPilotVoicePath = autoPilotVoicesFolder + "/";
            yield return LoadVoiceClips(onComplete, autoPilotVoicePath, false);
        }
        // Method signature with a callback to return the VehicleVoice instance
        private static IEnumerator LoadVoiceClips(Action<VehicleVoice> onComplete, string inputPath, bool verbose)
        {
            VehicleVoice returnVoice = new();
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "AutoPilot Voice Path is : " + inputPath);
            foreach (string clipName in clipNames)
            {
                string path = "file://" + inputPath + clipName + ".ogg";
                yield return LoadAudioClip(path, clip =>
                {
                    // Use reflection to set the clip dynamically based on its name
                    clip.name = clipName;
                    typeof(VehicleVoice).GetField(clipName).SetValue(returnVoice, clip);
                },
                () =>
                {
                    // Handle error, potentially logging and assigning Silence
                    VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Warn, verbose, $"WARNING: {clipName} could not be loaded. Assigning Silence.");
                    typeof(VehicleVoice).GetField(clipName).SetValue(returnVoice, silence);
                });
            }
            onComplete?.Invoke(returnVoice);
        }
        private static bool CheckForVoiceClips(string voicePath, bool verbose)
        {
            bool hasAtLeastOneClip = false;
            foreach (string clipName in clipNames)
            {
                string path = Path.Combine(voicePath, clipName) + ".ogg";
                bool thisClipExists = File.Exists(path);
                hasAtLeastOneClip |= thisClipExists;
                if (!thisClipExists)
                {
                    VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Warn, verbose, "Voice Registration Error: clip not found: " + path);
                }
            }
            return hasAtLeastOneClip;
        }
        private static IEnumerator LoadAudioClip(string filePath, Action<AudioClip> onSuccess, Action onError)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS);
            yield return www.SendWebRequest();
            if (www.isHttpError)
            {
                onError?.Invoke();
            }
            else if (www.isNetworkError)
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
        #endregion
    }
}
