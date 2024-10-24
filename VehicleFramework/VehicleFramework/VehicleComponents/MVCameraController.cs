using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.VehicleComponents
{
    public class MVCameraController : MonoBehaviour, IPlayerListener
    {
        public static readonly string playerCameraState = "player";
        private string state = playerCameraState;
        private Transform playerCamActual = null;
        public Transform PlayerCamPivot => Player.main.transform.Find("camPivot");
        public Transform PlayerCam
        {
            get
            {
                if (playerCamActual == null)
                {
                    playerCamActual = PlayerCamPivot.Find("camRoot");
                }
                return playerCamActual;
            }
        }
        private Dictionary<string, Transform> cameras = new Dictionary<string, Transform>();
        private ModVehicle mv = null;
        public void Start()
        {
            mv = GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                Logger.Error("MVCameraController did not detect a ModVehicle in the parents of this GameObject: " + gameObject.name);
                enabled = false;
            }
            AddCamera(PlayerCamPivot, playerCameraState);
        }
        public void Update()
        {
            if(playerCamActual == null)
            {
                Transform _ = PlayerCam;
            }
            if(mv.IsPlayerControlling())
            {
                ScanInput();
            }
        }
        public bool AddCamera(Transform tran, string label)
        {
            if (cameras.ContainsKey(label))
            {
                Logger.Error("That camera label already exists in this list of cameras.");
                return false;
            }
            if (cameras.ContainsValue(tran))
            {
                Logger.Error("That camera transform already exists in this list of cameras.");
                return false;
            }
            cameras.Add(label, tran);
            return true;
        }
        public string GetState()
        {
            return state;
        }
        public Transform GetCamera(string name)
        {
            Transform ret = null;
            cameras.TryGetValue(name, out ret);
            return ret;
        }
        public bool SetState(string label)
        {
            if (state == label || !cameras.ContainsKey(label))
            {
                return false;
            }
            state = label;
            MovePlayerCameraToTransform(cameras[label]);
            return true;
        }
        public void MovePlayerCameraToTransform(Transform destination)
        {
            PlayerCam.SetParent(destination);
            PlayerCam.localPosition = Vector3.zero;
            PlayerCam.localRotation = Quaternion.identity;
        }
        private void ScanInput()
        {
            if(DevConsole.instance.state)
            {
                return;
            }
            int currentIndex = cameras.Keys.ToList().IndexOf(state);
            if (Input.GetKeyDown(MainPatcher.VFConfig.nextCamera))
            {
                currentIndex++;
            }
            else if (Input.GetKeyDown(MainPatcher.VFConfig.previousCamera))
            {
                currentIndex--;
            }
            else if (Input.GetKeyDown(MainPatcher.VFConfig.exitCamera))
            {
                SetState(playerCameraState);
                return;
            }
            if (currentIndex >= cameras.Count)
            {
                currentIndex = 0;
            }
            if (currentIndex < 0)
            {
                currentIndex = cameras.Keys.Count - 1;
            }
            SetState(cameras.Keys.ToList()[currentIndex]);
        }
        public void ResetCamera()
        {
            state = playerCameraState;
            MovePlayerCameraToTransform(cameras[playerCameraState]);
        }
        void IPlayerListener.OnPilotEnd()
        {
            SetState(playerCameraState);
        }
        void IPlayerListener.OnPlayerEntry()
        {
        }
        void IPlayerListener.OnPlayerExit()
        {
        }
        void IPlayerListener.OnPilotBegin()
        {
        }
    }
}
