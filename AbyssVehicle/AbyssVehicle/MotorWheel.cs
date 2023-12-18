using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AbyssVehicle
{
    public enum MotorWheelType
    {
        downup,
        leftright,
        backforth
    }


    public class MotorWheel : MonoBehaviour, IHandTarget
    {
        public static readonly Dictionary<MotorWheelType, string> wheelMessages = new Dictionary<MotorWheelType, string>()
        {
            { MotorWheelType.downup, GameInput.Button.LeftHand.ToString() + ": UP\n" + GameInput.Button.RightHand.ToString() + ": DOWN"},
            { MotorWheelType.leftright, GameInput.Button.LeftHand.ToString() + ": RIGHT\n" + GameInput.Button.RightHand.ToString() + ": LEFT"},
            { MotorWheelType.backforth, GameInput.Button.LeftHand.ToString() + ": FORWARD\n" + GameInput.Button.RightHand.ToString() + ": BACKWARD"}
        };

        public MotorWheelType mwt = 0;
        public VehicleFramework.ModVehicle mv = null;
        private bool isPersonallyDisallowingAutopilotFromControl = false;
        private int m_wheelstate = 0;
        private const int max_wheelstate = 6;
        public int wheelstate
        {
            get
            {
                return m_wheelstate;
            }
            set
            {
                if(max_wheelstate < value)
                {
                    m_wheelstate = max_wheelstate;
                }
                else if(value < -max_wheelstate)
                {
                    m_wheelstate = -max_wheelstate;
                }
                else
                {
                    m_wheelstate = value;
                }
                /*
                if(m_wheelstate == 0)
                {
                    isPersonallyDisallowingAutopilotFromControl = false;
                    //mv.GetComponent<VehicleFramework.AutoPilot>().isDisallowedFromControl--;
                }
                else
                {
                    if(isPersonallyDisallowingAutopilotFromControl == false)
                    {
                        //mv.GetComponent<VehicleFramework.AutoPilot>().isDisallowedFromControl++;
                    }
                    isPersonallyDisallowingAutopilotFromControl = true;
                }
                */
            }
        }

        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
            {
                wheelstate++;
            }
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            //HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, wheelMessages[mwt]);
            if (GameInput.GetButtonDown(GameInput.Button.RightHand))
            {
                wheelstate--;
            }
        }

        private float myZeroAngle = 0f;
        public void Start()
        {
            myZeroAngle = transform.localRotation.eulerAngles.z;
        }

        public void Update()
        {
            float myDesiredAngle = (myZeroAngle + 2f * wheelstate * (360f / (2f * max_wheelstate + 1f)));
            float myActualAngle = transform.localRotation.eulerAngles.z;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(myActualAngle, myDesiredAngle, Time.deltaTime*3f));
            /*
            if(wheelstate == 0)
            {
                return;
            }
            float wState = wheelstate;
            float factor = wState / 10f;
            switch (mwt)
            {
                case MotorWheelType.downup:
                    mv.engine.ApplyPlayerControls(Vector3.up * factor);
                    break;
                case MotorWheelType.leftright:
                    mv.engine.ApplyPlayerControls(Vector3.right * factor);
                    break;
                case MotorWheelType.backforth:
                    mv.engine.ApplyPlayerControls(Vector3.forward * factor);
                    break;
                default:
                    break;
            }
            */
        }
    }
}
