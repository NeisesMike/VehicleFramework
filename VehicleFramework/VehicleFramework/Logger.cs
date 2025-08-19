using BepInEx.Logging;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VehicleFramework
{
    public static class Logger
    {
        #region BepInExLog
        internal static ManualLogSource MyLog { get; set; } = null!;
        public static void Log(string message)
        {
            MyLog.LogInfo(message);
        }
        public static void Warn(string message)
        {
            MyLog.LogWarning(message);
        }
        public static void Error(string message)
        {
            MyLog.LogError(message);
        }
        public static void DebugLog(string message)
        {
            if (MainPatcher.NautilusConfig.IsDebugLogging)
            {
                MyLog.LogInfo($"[DebugLog]: {message}");
            }
        }
        public static void WarnException(string message, System.Exception e, bool outputToScreen = false)
        {
            Warn(message);
            Warn(e.Message);
            Warn(e.StackTrace);
            if (outputToScreen)
            {
                ErrorMessage.AddWarning(message);
            }
        }
        public static void LogException(string message, System.Exception e, bool outputToScreen = false)
        {
            Error(message);
            Error(e.Message);
            Error(e.StackTrace);
            if (outputToScreen)
            {
                ErrorMessage.AddError(message);
            }
        }
        #endregion

        #region PdaNotifications
        private static int IDCounter = 65536;
        private static readonly Dictionary<string, int> NoteIDsMemory = new();
        public static int GetFreshID()
        {
            int returnID = IDCounter++;
            while (Subtitles.main.queue.messages.Select(x => x.id).Contains(returnID))
            {
                returnID = IDCounter++;
            }
            return returnID;
        }
        public static void PDANote(string msg, float duration = 1.4f, float delay = 0)
        {
            int id;
            if(NoteIDsMemory.TryGetValue(msg, out int value))
            {
                id = value;
            }
            else
            {
                id = GetFreshID();
                NoteIDsMemory.Add(msg, id);
            }
            if(Subtitles.main.queue.messages.Select(x => x.id).Contains(id))
            {
                // don't replicate the message
            }
            else
            {
                Subtitles.main.queue.Add(id, new StringBuilder(msg), delay, duration);
            }
        }
        #endregion

        #region MainMenuLoopingMessages
        private static readonly List<string> Notifications = new();
        public static void LoopMainMenuError(string message, string prefix = "")
        {
            string result = $"<color=#FF0000>{prefix} Error: </color><color=#FFFF00>{message}</color>";
            Notifications.Add(result);
            Logger.Error(message);
        }
        public static void LoopMainMenuWarning(string message, string prefix = "")
        {
            string result = $"<color=#FF0000>{prefix} Warning: </color><color=#FFFF00>{message}</color>";
            Notifications.Add(result);
            Logger.Error(message);
        }
        internal static IEnumerator MakeAlerts()
        {
            yield return new WaitUntil(() => ErrorMessage.main != null);
            yield return new WaitForSeconds(1);
            yield return new WaitUntil(() => ErrorMessage.main != null);
            ErrorMessage eMain = ErrorMessage.main;
            float messageDuration = eMain.timeFlyIn + eMain.timeDelay + eMain.timeFadeOut + eMain.timeInvisible + 0.1f;
            while (Player.main == null)
            {
                foreach (string notif in Notifications)
                {
                    ErrorMessage.AddMessage(notif);
                    yield return new WaitForSeconds(1);
                }
                yield return new WaitForSeconds(messageDuration);
            }
        }
        #endregion
    }
}
