using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class NavigationLightsController : MonoBehaviour, IPowerListener
    {
        private bool isNavLightsEnabled = true;

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
                    if (MainPatcher.VFConfig.isFlashingLightsEnabled && white == null)
                    {
                        white = StartCoroutine(Strobe(LightClass.WhiteStrobes));
                    }
                    break;
                case LightClass.RedStrobes:
                    if (MainPatcher.VFConfig.isFlashingLightsEnabled && red == null)
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
            if (mv.NavigationPositionLights != null)
            {
                foreach (GameObject lightObj in mv.NavigationPositionLights)
                {
                    positionMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
                BlinkOn(positionMats, Color.white);
            }
            if (mv.NavigationRedStrobeLights != null)
            {
                foreach (GameObject lightObj in mv.NavigationRedStrobeLights)
                {
                    redStrobeMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                    Light light = lightObj.EnsureComponent<Light>();
                    light.enabled = false;
                    light.color = Color.red;
                    light.type = LightType.Point;
                    light.intensity = 1f;
                    light.range = 80f;
                    light.shadows = LightShadows.Hard;
                    redStrobeLights.Add(light);
                }
            }
            if (mv.NavigationWhiteStrobeLights != null)
            {
                foreach (GameObject lightObj in mv.NavigationWhiteStrobeLights)
                {
                    whiteStrobeMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                    Light light = lightObj.EnsureComponent<Light>();
                    light.enabled = false;
                    light.color = Color.white;
                    light.type = LightType.Point;
                    light.intensity = 0.5f;
                    light.range = 80f;
                    light.shadows = LightShadows.Hard;
                    whiteStrobeLights.Add(light);
                }
            }
            if (mv.NavigationPortLights != null)
            {
                foreach (GameObject lightObj in mv.NavigationPortLights)
                {
                    portMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
            }
            if (mv.NavigationStarboardLights != null)
            {
                foreach (GameObject lightObj in mv.NavigationStarboardLights)
                {
                    starboardMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
            }
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
            if (isNavLightsEnabled)
            {
                foreach (LightClass lc in Enum.GetValues(typeof(LightClass)).Cast<LightClass>())
                {
                    DisableLightClass(lc);
                }
                isNavLightsEnabled = false;
                mv.NotifyStatus(LightsStatus.OnNavLightsOff);
            }
        }
        public void EnableNavLights()
        {
            if (!isNavLightsEnabled)
            {
                EnableLightClass(LightClass.Positions);
                EnableLightClass(LightClass.Ports);
                EnableLightClass(LightClass.Starboards);
                isNavLightsEnabled = true;
                mv.NotifyStatus(LightsStatus.OnNavLightsOn);
            }
        }
        public void ToggleNavLights()
        {
            if (mv.IsPowered())
            {
                if (isNavLightsEnabled)
                {
                    DisableNavLights();
                }
                else
                {
                    EnableNavLights();
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
            mat.SetFloat("_GlowStrength", lightBrightness);
            mat.SetFloat("_GlowStrengthNight", lightBrightness);
            mat.SetColor("_Color", col);
            mat.SetColor("_GlowColor", col);
        }
        private void BlinkThisStrobeOn(Material mat, Color col)
        {
            mat.EnableKeyword("MARMO_EMISSION");
            mat.SetFloat("_GlowStrength", strobeBrightness);
            mat.SetFloat("_GlowStrengthNight", strobeBrightness);
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
                    Logger.Warn("Warning: passed bad arg to KillStrobe");
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
                    Logger.Warn("Warning: passed bad arg to Strobe");
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
                    Logger.Warn("Warning: passed bad arg to BlinkOnStrobe");
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
                    Logger.Warn("Warning: passed bad arg to BlinkOffStrobe");
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
            if (m == 0)
            {
                yield break;
            }
            int sequenceLength = m == 0 ? 0 : m + 2 * n;
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

        void IPowerListener.OnPowerUp()
        {
            EnableNavLights();
        }

        void IPowerListener.OnPowerDown()
        {
            DisableNavLights();
        }

        void IPowerListener.OnBatterySafe()
        {
            EnableNavLights();
        }

        void IPowerListener.OnBatteryLow()
        {
            EnableNavLights();
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
            DisableNavLights();
        }

        void IPowerListener.OnBatteryDepleted()
        {
            DisableNavLights();
        }

        void IPowerListener.OnBatteryDead()
        {
            DisableNavLights();
        }

        void IPowerListener.OnBatteryRevive()
        {
            EnableNavLights();
        }
    }
}
