using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace VehicleFramework.Admin
{
    public static class GameStateWatcher
    {
        public static bool IsPlayerAwaked;
        public static bool IsPlayerStarted;
        public static bool IsWorldSettled => LargeWorldStreamer.main != null && Player.main != null && LargeWorldStreamer.main.IsRangeActiveAndBuilt(new Bounds(Player.main.transform.position, new Vector3(5f, 5f, 5f)));

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
