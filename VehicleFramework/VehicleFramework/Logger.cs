using BepInEx.Logging;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace VehicleFramework
{
    public static class Logger
    {
        internal static ManualLogSource MyLog { get; set; }
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
            if (MainPatcher.VFConfig.IsDebugLogging.Value)
            {
                MyLog.LogInfo($"[DebugLog]: {message}");
            }
        }
        private static int IDCounter = 65536; 
        public static int GetFreshID()
        {
            int returnID = IDCounter++;
            while (Subtitles.main.queue.messages.Select(x => x.id).Contains(returnID))
            {
                returnID = IDCounter++;
            }
            return returnID;
        }
        private static readonly Dictionary<string, int> NoteIDsMemory = new Dictionary<string, int>();
        public static void PDANote(string msg, float duration = 1.4f, float delay = 0)
        {
            int id;
            if(NoteIDsMemory.ContainsKey(msg))
            {
                id = NoteIDsMemory[msg];
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
        public static void Output(string msg, float time = 4f, int x = 500, int y = 0)
        {
            ErrorMessage.AddWarning(msg);
        }
    }
}
