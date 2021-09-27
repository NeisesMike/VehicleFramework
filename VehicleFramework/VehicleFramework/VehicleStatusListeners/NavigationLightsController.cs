using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class NavigationLightsController : MonoBehaviour, IVehicleStatusListener
    {
        private bool isNavLightsEnabled = true;
        private bool isFlashingLightsEnabled = true;

        private ModVehicle mv;
        private List<Material> positionMats = new List<Material>();
        private List<Material> portMats = new List<Material>();
        private List<Material> starboardMats = new List<Material>();

        private List<Material> whiteStrobeMats = new List<Material>();
        private List<Material> redStrobeMats = new List<Material>();
        private List<Light> whiteStrobeLights = new List<Light>();
        private List<Light> redStrobeLights = new List<Light>();

        public const float lightBrightness = 1f;
        public const float strobeBrightness = 30f;


        Rigidbody rb = null;
        bool position = false;
        Coroutine white = null;
        Coroutine red = null;
        Coroutine port = null;
        Coroutine starboard = null;

        private void DisableLightClass(LightClass lc)
        {
            switch (lc)
            {
                case LightClass.WhiteStrobes:
                    if (white != null)
                    {
                        StopCoroutine(white);
                        white = null;
                    }
                    KillStrobes(LightClass.WhiteStrobes);
                    break;
                case LightClass.RedStrobes:
                    if (red != null)
                    {
                        StopCoroutine(red);
                        red = null;
                    }
                    KillStrobes(LightClass.RedStrobes);
                    break;
                case LightClass.Positions:
                    if (position)
                    {
                        BlinkOff(positionMats);
                        position = false;
                    }
                    break;
                case LightClass.Ports:
                    if (port != null)
                    {
                        StopCoroutine(port);
                        port = null;
                    }
                    BlinkOff(portMats);
                    break;
                case LightClass.Starboards:
                    if (starboard != null)
                    {
                        StopCoroutine(starboard);
                        starboard = null;
                    }
                    BlinkOff(starboardMats);
                    break;
            }
        }
        private void EnableLightClass(LightClass lc)
        {
            switch (lc)
            {
                case LightClass.WhiteStrobes:
                    if (white == null)
                    {
                        white = StartCoroutine(Strobe(LightClass.WhiteStrobes));
                    }
                    break;
                case LightClass.RedStrobes:
                    if (red == null)
                    {
                        red = StartCoroutine(Strobe(LightClass.RedStrobes));
                    }
                    break;
                case LightClass.Positions:
                    if (!position)
                    {
                        BlinkOn(positionMats, Color.white);
                        position = true;
                    }
                    break;
                case LightClass.Ports:
                    if (port == null)
                    {
                        port = StartCoroutine(BlinkNarySequence(2, true));
                    }
                    break;
                case LightClass.Starboards:
                    if (starboard == null)
                    {
                        starboard = StartCoroutine(BlinkNarySequence(2, false));
                    }
                    break;
            }
        }


        public bool GetNavLightsEnabled()
        {
            return isNavLightsEnabled;
        }


        public void Start()
        {
            rb = GetComponent<Rigidbody>();
            mv = GetComponent<ModVehicle>();
            foreach (GameObject lightObj in mv.NavigationPositionLights)
            {
                positionMats.Add(lightObj.GetComponent<MeshRenderer>().material);
            }
            foreach (GameObject lightObj in mv.NavigationRedStrobeLights)
            {
                redStrobeMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                Light light = lightObj.EnsureComponent<Light>();
                light.enabled = false;
                light.color = Color.red;
                light.type = LightType.Point;
                light.intensity = 1f;
                light.range = 120f;
                light.shadows = LightShadows.Hard;
                redStrobeLights.Add(light);
            }
            foreach (GameObject lightObj in mv.NavigationWhiteStrobeLights)
            {
                whiteStrobeMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                Light light = lightObj.EnsureComponent<Light>();
                light.enabled = false;
                light.color = Color.white;
                light.type = LightType.Point;
                light.intensity = 0.5f;
                light.range = 120f;
                light.shadows = LightShadows.Hard;
                whiteStrobeLights.Add(light);
            }
            foreach (GameObject lightObj in mv.NavigationPortLights)
            {
                portMats.Add(lightObj.GetComponent<MeshRenderer>().material);
            }
            foreach (GameObject lightObj in mv.NavigationStarboardLights)
            {
                starboardMats.Add(lightObj.GetComponent<MeshRenderer>().material);
            }
            BlinkOn(positionMats, Color.white);
            StartCoroutine(ControlLights());
        }

        public enum LightClass
        {
            WhiteStrobes,
            RedStrobes,
            Positions,
            Ports,
            Starboards
        }
        public void DisableNavLights()
        {
            foreach (LightClass lc in Enum.GetValues(typeof(LightClass)).Cast<LightClass>())
            {
                Logger.Log("disable: " + lc.ToString());
                DisableLightClass(lc);
            }
        }
        public void EnableNavLights()
        {
            foreach (LightClass lc in Enum.GetValues(typeof(LightClass)).Cast<LightClass>())
            {
                Logger.Log("enable: " + lc.ToString());
                EnableLightClass(lc);
            }
            if(isFlashingLightsEnabled)
            {
                // TODO
            }
        }
        public void ToggleNavLights()
        {
            if (mv.IsPowered())
            {
                isNavLightsEnabled = !isNavLightsEnabled;
                if (isNavLightsEnabled)
                {
                    EnableNavLights();
                    mv.NotifyStatus(VehicleStatus.OnNavLightsOn);
                }
                else
                {
                    DisableNavLights();
                    mv.NotifyStatus(VehicleStatus.OnNavLightsOff);
                }
            }
        }
        private IEnumerator ControlLights()
        {
            while(true)
            {
                if(isNavLightsEnabled && mv.IsPowered())
                {
                    EnableLightClass(LightClass.Positions);
                    EnableLightClass(LightClass.Ports);
                    EnableLightClass(LightClass.Starboards);
                    if (white == null && 10f <= rb.velocity.magnitude)
                    {
                        EnableLightClass(LightClass.WhiteStrobes);
                    }
                    else if (3f < rb.velocity.magnitude && rb.velocity.magnitude < 10f)
                    {
                        DisableLightClass(LightClass.WhiteStrobes);
                        DisableLightClass(LightClass.RedStrobes);
                    }
                    else if (red == null && 0.001f < rb.velocity.magnitude && rb.velocity.magnitude <= 3f)
                    {
                        EnableLightClass(LightClass.RedStrobes);
                    }
                    else
                    {
                        DisableLightClass(LightClass.WhiteStrobes);
                        DisableLightClass(LightClass.RedStrobes);
                    }
                }
                else
                {
                    DisableLightClass(LightClass.Ports);
                    DisableLightClass(LightClass.Starboards);
                    DisableLightClass(LightClass.WhiteStrobes);
                    DisableLightClass(LightClass.RedStrobes);
                    DisableLightClass(LightClass.WhiteStrobes);
                    DisableLightClass(LightClass.Positions);
                }    
                yield return new WaitForSeconds(1f);
            }
        }

        private void BlinkThisLightOn(Material mat, Color col)
        {
            mat.EnableKeyword("MARMO_EMISSION");
            mat.SetFloat("_EmissionLM", lightBrightness);
            mat.SetFloat("_EmissionLMNight", lightBrightness);
            mat.SetColor("_Color", col);
            mat.SetColor("_GlowColor", col);
        }
        private void BlinkThisStrobeOn(Material mat, Color col)
        {
            mat.EnableKeyword("MARMO_EMISSION");
            mat.SetFloat("_EmissionLM", strobeBrightness);
            mat.SetFloat("_EmissionLMNight", strobeBrightness);
            mat.SetColor("_Color", col);
            mat.SetColor("_GlowColor", col);
        }
        private void BlinkThisLightOff(Material mat)
        {
            mat.DisableKeyword("MARMO_EMISSION");
        }
        public void BlinkOn(List<Material> mats, Color col)
        {
            foreach (Material mat in mats)
            {
                BlinkThisLightOn(mat, col);
            }
        }
        public void BlinkOff(List<Material> mats)
        {
            foreach (Material mat in mats)
            {
                BlinkThisLightOff(mat);
            }
        }
        public void BlinkAllLightsOn(Color col)
        {
            BlinkOn(positionMats, Color.white);
            BlinkOn(whiteStrobeMats, Color.white);
            BlinkOn(redStrobeMats, Color.red);
            BlinkOn(portMats, Color.red);
            BlinkOn(starboardMats, Color.green);
        }
        public void BlinkAllLightsOff()
        {
            BlinkOff(positionMats);
            BlinkOff(redStrobeMats);
            BlinkOff(whiteStrobeMats);
            BlinkOff(portMats);
            BlinkOff(starboardMats);
        }
        private void KillStrobes(LightClass lc)
        {
            switch (lc)
            {
                case LightClass.RedStrobes:
                    foreach (var tmp in redStrobeLights)
                    {
                        tmp.enabled = false;
                    }
                    break;
                case LightClass.WhiteStrobes:
                    foreach (var tmp in whiteStrobeLights)
                    {
                        tmp.enabled = false;
                    }
                    break;
                default:
                    Logger.Log("Warning: passed bad arg to KillStrobe");
                    break;
            }
        }
        private void PowerStrobes(LightClass lc)
        {
            switch (lc)
            {
                case LightClass.RedStrobes:
                    foreach (var tmp in redStrobeLights)
                    {
                        tmp.enabled = true;
                    }
                    break;
                case LightClass.WhiteStrobes:
                    foreach (var tmp in whiteStrobeLights)
                    {
                        tmp.enabled = true;
                    }
                    break;
                default:
                    Logger.Log("Warning: passed bad arg to Strobe");
                    break;
            }
        }
        public void BlinkOnStrobe(LightClass lc)
        {
            switch (lc)
            {
                case LightClass.RedStrobes:
                    foreach (Material mat in redStrobeMats)
                    {
                        BlinkThisStrobeOn(mat, Color.red);
                    }
                    break;
                case LightClass.WhiteStrobes:
                    foreach (Material mat in whiteStrobeMats)
                    {
                        BlinkThisStrobeOn(mat, Color.white);
                    }
                    break;
                default:
                    Logger.Log("Warning: passed bad arg to BlinkOnStrobe");
                    break;
            }
            PowerStrobes(lc);
        }
        public void BlinkOffStrobe(LightClass lc)
        {
            KillStrobes(lc);
            switch (lc)
            {
                case LightClass.RedStrobes:
                    BlinkOff(redStrobeMats);
                    break;
                case LightClass.WhiteStrobes:
                    BlinkOff(whiteStrobeMats);
                    break;
                default:
                    Logger.Log("Warning: passed bad arg to BlinkOffStrobe");
                    break;
            }
        }
        public IEnumerator Strobe(LightClass lc)
        {
            while (true)
            {
                BlinkOnStrobe(lc);
                yield return new WaitForSeconds(0.01f);
                BlinkOffStrobe(lc);
                yield return new WaitForSeconds(2.99f);
            }
        }
        public IEnumerator BlinkSingleSequence()
        {
            while (true)
            {
                for (int i = 0; i < portMats.Count + 1; i++)
                {
                    if (i < portMats.Count)
                    {
                        BlinkThisLightOn(portMats[i], Color.red);
                        BlinkThisLightOn(starboardMats[i], Color.green);
                    }
                    if (0 <= i - 1)
                    {
                        BlinkThisLightOff(portMats[i - 1]);
                        BlinkThisLightOff(starboardMats[i - 1]);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(1.0f - 0.1f * portMats.Count);
            }
        }
        public IEnumerator BlinkDoubleSequence()
        {
            while (true)
            {
                for (int i = 0; i < portMats.Count + 2; i++)
                {
                    if (0 <= i - 2)
                    {
                        BlinkThisLightOff(portMats[(i - 2) % portMats.Count]);
                        BlinkThisLightOff(starboardMats[(i - 2) % portMats.Count]);
                    }
                    if (0 <= i - 1 && i - 1 < portMats.Count)
                    {
                        BlinkThisLightOn(portMats[i - 1], Color.red);
                        BlinkThisLightOn(starboardMats[i - 1], Color.green);
                    }
                    if (i < portMats.Count)
                    {
                        BlinkThisLightOn(portMats[i], Color.red);
                        BlinkThisLightOn(starboardMats[i], Color.green);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(1.0f - 0.1f * portMats.Count);
            }
        }
        public IEnumerator BlinkNarySequence(int n, bool isPortSide)
        {
            int m;
            if (isPortSide)
            {
                m = portMats.Count;
            }
            else
            {
                m = starboardMats.Count;
            }
            int sequenceLength = m + 2*n;
            while (true)
            {
                for (int i = 0; i < sequenceLength-n; i++)
                {
                    if (0 <= i && i < m)
                    {
                        if(isPortSide)
                        {
                            BlinkThisLightOn(portMats[i], Color.red);
                        }
                        else
                        {
                            BlinkThisLightOn(starboardMats[i], Color.green);
                        }
                    }
                    if (0 <= i - n && i - n < m)
                    {
                        if (isPortSide)
                        {
                            BlinkThisLightOff(portMats[i - n]);
                        }
                        else
                        {
                            BlinkThisLightOff(starboardMats[i - n]);
                        }
                    }
                    yield return new WaitForSeconds(0.25f / (sequenceLength - n));
                }
                if (isPortSide)
                {
                    BlinkThisLightOff(portMats[m - 1]);
                }
                else
                {
                    BlinkThisLightOff(starboardMats[m - 1]);
                }
                yield return new WaitForSeconds(0.75f);
            }
        }
        void IVehicleStatusListener.OnAutoLevel()
        {
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
        }

        void IVehicleStatusListener.OnHeadLightsOff()
        {
        }

        void IVehicleStatusListener.OnHeadLightsOn()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
        }

        void IVehicleStatusListener.OnPilotBegin()
        {
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
        }

        void IVehicleStatusListener.OnPowerDown()
        {
        }

        void IVehicleStatusListener.OnPowerUp()
        {
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }

        void IVehicleStatusListener.OnBatteryLow()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryDepletion()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnFloodLightsOn()
        {
        }

        void IVehicleStatusListener.OnFloodLightsOff()
        {
        }

        void IVehicleStatusListener.OnNavLightsOn()
        {
        }

        void IVehicleStatusListener.OnNavLightsOff()
        {
        }
    }
}
