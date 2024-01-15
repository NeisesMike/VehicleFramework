using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace VehicleFramework.Admin
{
    public static class GameStateWatcher
    {
        public static bool IsPlayerAwaked;
        public static bool IsPlayerStarted;

        public static List<IGameObjectManager> GOManagers = new List<IGameObjectManager>();
        public static void OnResetScene(Scene scene)
        {
            VehicleManager.VehiclesInPlay.Clear();
            GOManagers.ForEach(x => x.ClearList());
            IsPlayerAwaked = false;
            IsPlayerStarted = false;
        }
    }
}
