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
using VehicleFramework.VehicleComponents;

namespace AbyssVehicle
{
    public partial class Abyss : Submarine
    {
        public static GameObject cameraGUI = null;
        Dictionary<string, GameObject> guiCanvases = new Dictionary<string, GameObject>();
        public MVCameraController cams = null;
        private Transform bottomCam => transform.Find("RoundCamera1/Inner_corpuse/Lens_camera/Camera");
        private Transform wideCam => transform.Find("RoundCamera2/Inner_corpuse/Lens_camera/Camera");
        private Transform rearCam => transform.Find("RoundCamera3/Inner_corpuse/Lens_camera/Camera");
        private Transform portCam => transform.Find("RoundCamera4/Inner_corpuse/Lens_camera/Camera");
        private Transform starboardCam => transform.Find("RoundCamera5/Inner_corpuse/Lens_camera/Camera");
        private Transform forwardCam => transform.Find("RoundCamera6/Inner_corpuse/Lens_camera/Camera");
        public Transform PlayerCamPivot => Player.main.transform.Find("camPivot");
        public override void Awake()
        {
            OGVehicleName = "ABY-" + Mathf.RoundToInt(UnityEngine.Random.value * 10000).ToString();
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;
            base.Awake();
        }
        public override void Start()
        {
            base.Start();
            SetupMotorWheels();
            SetupCameraGUI();
            cams = gameObject.AddComponent<MVCameraController>();
            cams.AddCamera(forwardCam, "forward");
            cams.AddCamera(starboardCam, "starboard");
            cams.AddCamera(rearCam, "rear");
            cams.AddCamera(wideCam, "wide");
            cams.AddCamera(bottomCam, "bottom");
            cams.AddCamera(portCam, "port");
        }
        public override void Update()
        {
            base.Update();
            UpdateCameraGUI();
        }
        private void UpdateCameraGUI()
        {
            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localScale = Vector3.one * MainPatcher.config.guiSize);
            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localPosition = new Vector3(MainPatcher.config.guiXPosition, MainPatcher.config.guiYPosition, x.Value.transform.Find("RawImage").localPosition.z));
            if (IsPlayerPiloting())
            {
                guiCanvases.Where(x => x.Key == cams.GetState()).ForEach(x => x.Value.SetActive(true));
                guiCanvases.Where(x => x.Key != cams.GetState()).ForEach(x => x.Value.SetActive(false));
            }
            else
            {
                guiCanvases.ForEach(x => x.Value.SetActive(false));
            }
        }
        private void SetupCameraGUI()
        {
            GameObject thisCameraGUI = Instantiate(cameraGUI);
            thisCameraGUI.transform.SetParent(transform);
            guiCanvases.Add("player", thisCameraGUI.transform.Find("CanvasNone").gameObject);
            guiCanvases.Add("forward", thisCameraGUI.transform.Find("CanvasForward").gameObject);
            guiCanvases.Add("port", thisCameraGUI.transform.Find("CanvasPort").gameObject);
            guiCanvases.Add("starboard", thisCameraGUI.transform.Find("CanvasStarboard").gameObject);
            guiCanvases.Add("rear", thisCameraGUI.transform.Find("CanvasAstern").gameObject);
            guiCanvases.Add("wide", thisCameraGUI.transform.Find("CanvasDescent").gameObject);
            guiCanvases.Add("bottom", thisCameraGUI.transform.Find("CanvasHatch").gameObject);
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
