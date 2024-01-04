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
            { MotorWheelType.downup, "Vertical Power: "},
            { MotorWheelType.leftright,"Lateral Power: "},
            { MotorWheelType.backforth, "Forward Power: "}
        };

        public static string InstructionString = 
            uGUI.FormatButton(GameInput.Button.LeftHand, true) + " increases\n" +
            uGUI.FormatButton(GameInput.Button.RightHand) + " decreases";

        public MotorWheelType mwt = 0;
        public VehicleFramework.ModVehicle mv = null;
        private int m_wheelstate = 20;
        private const int max_wheelstate = 30;
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
                else if(value < 0)
                {
                    m_wheelstate = 0;
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
        public string GetWheelStateString()
        {
            string percent = (wheelstate * 5).ToString();
            return (percent + "%");
        }
        public string GetWheelPowerConsumptionString()
        {
            return Mathf.RoundToInt(GetWheelPowerConsumption() * 100f).ToString() + "%";
        }
        public float GetWheelPowerConsumption()
        {
            if (wheelstate <= 20)
            {
                return Mathf.Pow(GetWheelPowerOutput(), 3.4f);
            }
            else
            {
                return Mathf.Pow(GetWheelPowerOutput(), 1.7f);
            }
        }
        public string GetWheelPowerString()
        {
            string percent = (wheelstate * 5).ToString();
            return (percent + "%");
        }
        public float GetWheelPowerOutput()
        {
            // return 1.00 for 100% power which is normal operation
            return ((wheelstate * 5.0f)/100.0f);
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
            string displayString = wheelMessages[mwt] + GetWheelStateString() + "\nPower Consumption: " + GetWheelPowerConsumptionString() + "\n" + InstructionString;
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, displayString);
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
            float myDesiredAngle = (myZeroAngle + 5f * wheelstate * (360f / (2f * max_wheelstate + 1f)));
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
