using BepInEx.Logging;
using Nautilus.Utility;

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
            if (MainPatcher.VFConfig.isDebugLogging)
            {
                MyLog.LogInfo("[VehicleFramework] " + message);
            }
        }
        public static BasicText Output(string msg, float time = 4, int x = 500, int y = 0)
        {
            if(GUIController.main.GetHidePhase() < GUIController.HidePhase.HUD)
            {
                BasicText message = new BasicText(x, y);
                message.ShowMessage(msg, time);
                return message;
            }
            return null;
        }
    }
}
