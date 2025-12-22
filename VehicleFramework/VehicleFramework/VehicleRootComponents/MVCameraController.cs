using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Interfaces;

namespace VehicleFramework.VehicleRootComponents
{
    public class MVCameraController : MonoBehaviour, IPlayerListener
    {
        public static readonly string playerCameraState = "player";
        private string state = playerCameraState;
        private Transform? playerCamActual = null;
        public static Transform? PlayerCamPivot => Player.main.transform.Find("camPivot");
        public Transform? PlayerCam
        {
            get
            {
                if (playerCamActual == null && PlayerCamPivot != null)
                {
                    playerCamActual = PlayerCamPivot.Find("camRoot");
                }
                return playerCamActual;
            }
        }
        private readonly Dictionary<string, Transform> cameras = new();
        private ModVehicle mv = null!;
        private void Awake()
        {
            mv = GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                throw Admin.SessionManager.Fatal("MVCameraController did not detect a ModVehicle in the parents of this GameObject: " + gameObject.name);
            }
        }
        public void Start()
        {
            if(PlayerCamPivot == null)
            {
                throw Admin.SessionManager.Fatal("MVCameraController could not find the Player camera pivot.");
            }   
            AddCamera(PlayerCamPivot, playerCameraState);
        }
        public void Update()
        {
            if(playerCamActual == null)
            {
                Transform? _ = PlayerCam;
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
            cameras.TryGetValue(name, out Transform ret);
            return ret;
        }
        public bool SetState(string label)
        {
            if (state == label || !cameras.TryGetValue(label, out Transform? value))
            {
                return false;
            }
            state = label;
            MovePlayerCameraToTransform(value);
            return true;
        }
        public void MovePlayerCameraToTransform(Transform destination)
        {
            if (PlayerCam == null)
            {
                throw Admin.SessionManager.Fatal("MVCameraController could not find the PlayerCam.");
            }
            PlayerCam.SetParent(destination);
            PlayerCam.localPosition = Vector3.zero;
            PlayerCam.localRotation = Quaternion.identity;
        }
        private void ScanInput()
        {
            if (DevConsole.instance.state)
            {
                return;
            }
            int currentIndex = cameras.Keys.ToList().IndexOf(state);
            if (GameInput.GetButtonDown(MainPatcher.Instance.NextCameraKey))
            {
                currentIndex++;
            }
            else if (GameInput.GetButtonDown(MainPatcher.Instance.PreviousCameraKey))
            {
                currentIndex--;
            }
            else if (GameInput.GetButtonDown(MainPatcher.Instance.ExitCameraKey))
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
