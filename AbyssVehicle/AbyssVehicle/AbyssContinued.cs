using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Engines;

namespace AbyssVehicle
{
    public partial class Abyss : Submarine
    {
        public static GameObject cameraGUI = null;
        private CameraState currentCameraState = CameraState.player;
        private Transform playerCamActual = null;
        Dictionary<CameraState, GameObject> guiCanvases = new Dictionary<CameraState, GameObject>();

        private Transform bottomCam => transform.Find("RoundCamera1/Inner_corpuse/Lens_camera/Camera");
        private Transform wideCam => transform.Find("RoundCamera2/Inner_corpuse/Lens_camera/Camera");
        private Transform rearCam => transform.Find("RoundCamera3/Inner_corpuse/Lens_camera/Camera");
        private Transform portCam => transform.Find("RoundCamera4/Inner_corpuse/Lens_camera/Camera");
        private Transform starboardCam => transform.Find("RoundCamera5/Inner_corpuse/Lens_camera/Camera");
        private Transform forwardCam => transform.Find("RoundCamera6/Inner_corpuse/Lens_camera/Camera");
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
        public enum CameraState
        {
            bottomHatch,
            port,
            player,
            forward,
            starboard,
            rear,
            widelens
        }

        public override void Awake()
        {
            OGVehicleName = "ABY-" + Mathf.RoundToInt(UnityEngine.Random.value * 10000).ToString();
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;
            base.Awake();

            List<string> cameraNames = new List<string>();
            cameraNames.Add("RoundCamera1");
            cameraNames.Add("RoundCamera2");
            cameraNames.Add("RoundCamera3");
            cameraNames.Add("RoundCamera4");
            cameraNames.Add("RoundCamera5");
            cameraNames.Add("RoundCamera6");
            foreach (string str in cameraNames)
            {
                Transform cameraObject = transform.Find(str + "/Inner_corpuse/Lens_camera/Camera");
                cameraObject.GetComponent<Camera>().enabled = false;
            }

        }
        public override void Start()
        {
            base.Start();
            SetupMotorWheels();
            SetupCameraGUI();
        }
        public override void Update()
        {
            base.Update();
            MaybeControlCameras();
            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localScale = Vector3.one * MainPatcher.config.guiSize);
            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localPosition = new Vector3(MainPatcher.config.guiXPosition, MainPatcher.config.guiYPosition, x.Value.transform.Find("RawImage").localPosition.z));
        }
        public override void StopPiloting()
        {
            base.StopPiloting();
            ForceResetPlayerCameraToHead();
            currentCameraState = CameraState.player;
        }
        private void MaybeControlCameras()
        {
            if (IsPlayerPiloting())
            {
                CameraState lastCameraState = currentCameraState;
                ControlCameraState();
                ControlCameras(lastCameraState);
            }
            else
            {
                guiCanvases.ForEach(x => x.Value.SetActive(false));
            }
        }
        private void ControlCameraState()
        {
            if (Input.GetKeyDown(MainPatcher.config.nextCamera))
            {
                currentCameraState++;
                if (currentCameraState > CameraState.widelens)
                {
                    currentCameraState = 0;
                }
            }
            else if (Input.GetKeyDown(MainPatcher.config.previousCamera))
            {
                currentCameraState--;
                if (currentCameraState < 0)
                {
                    currentCameraState = CameraState.widelens;
                }
            }
            else if (Input.GetKeyDown(MainPatcher.config.exitCamera))
            {
                currentCameraState = CameraState.player;
            }
        }
        private void ControlCameras(CameraState lastState)
        {
            switch (currentCameraState)
            {
                case CameraState.bottomHatch:
                    MovePlayerCameraToTransform(bottomCam, lastState);
                    break;
                case CameraState.port:
                    MovePlayerCameraToTransform(portCam, lastState);
                    break;
                case CameraState.player:
                    ResetPlayerCameraToHead(lastState);
                    break;
                case CameraState.forward:
                    MovePlayerCameraToTransform(forwardCam, lastState);
                    break;
                case CameraState.starboard:
                    MovePlayerCameraToTransform(starboardCam, lastState);
                    break;
                case CameraState.rear:
                    MovePlayerCameraToTransform(rearCam, lastState);
                    break;
                case CameraState.widelens:
                    MovePlayerCameraToTransform(wideCam, lastState);
                    break;
                default:
                    ResetPlayerCameraToHead(lastState);
                    break;
            }
            guiCanvases.Where(x => x.Key == currentCameraState).ForEach(x => x.Value.SetActive(true));
            guiCanvases.Where(x => x.Key != currentCameraState).ForEach(x => x.Value.SetActive(false));
        }
        public void MovePlayerCameraToTransform(Transform destination, CameraState lastState)
        {
            if (lastState != currentCameraState)
            {
                PlayerCam.SetParent(destination);
                PlayerCam.localPosition = Vector3.zero;
                PlayerCam.localRotation = Quaternion.identity;
            }
        }
        public void ResetPlayerCameraToHead(CameraState lastState)
        {
            if (lastState != currentCameraState)
            {
                PlayerCam.SetParent(PlayerCamPivot);
                PlayerCam.localPosition = Vector3.zero;
                PlayerCam.localRotation = Quaternion.identity;
            }
        }
        public void ForceResetPlayerCameraToHead()
        {
            PlayerCam.SetParent(PlayerCamPivot);
            PlayerCam.localPosition = Vector3.zero;
            PlayerCam.localRotation = Quaternion.identity;
        }
        private void SetupCameraGUI()
        {
            GameObject thisCameraGUI = Instantiate(cameraGUI);
            thisCameraGUI.transform.SetParent(transform);
            guiCanvases.Add(CameraState.player, thisCameraGUI.transform.Find("CanvasNone").gameObject);
            guiCanvases.Add(CameraState.forward, thisCameraGUI.transform.Find("CanvasForward").gameObject);
            guiCanvases.Add(CameraState.port, thisCameraGUI.transform.Find("CanvasPort").gameObject);
            guiCanvases.Add(CameraState.starboard, thisCameraGUI.transform.Find("CanvasStarboard").gameObject);
            guiCanvases.Add(CameraState.rear, thisCameraGUI.transform.Find("CanvasAstern").gameObject);
            guiCanvases.Add(CameraState.widelens, thisCameraGUI.transform.Find("CanvasDescent").gameObject);
            guiCanvases.Add(CameraState.bottomHatch, thisCameraGUI.transform.Find("CanvasHatch").gameObject);
        }
        public void SetupMotorWheels()
        {
            var wheelone = transform.Find("model/WheelOne").gameObject.EnsureComponent<MotorWheel>();
            var wheeltwo = transform.Find("model/WheelTwo").gameObject.EnsureComponent<MotorWheel>();
            var wheelthree = transform.Find("model/WheelThree").gameObject.EnsureComponent<MotorWheel>();
            wheelone.mwt = MotorWheelType.backforth;
            wheeltwo.mwt = MotorWheelType.leftright;
            wheelthree.mwt = MotorWheelType.downup;
            wheelone.mv = this;
            wheeltwo.mv = this;
            wheelthree.mv = this;
            gameObject.GetComponent<AbyssEngine>().backforth = wheelone;
            gameObject.GetComponent<AbyssEngine>().leftright = wheeltwo;
            gameObject.GetComponent<AbyssEngine>().downup = wheelthree;
        }
    }
}
