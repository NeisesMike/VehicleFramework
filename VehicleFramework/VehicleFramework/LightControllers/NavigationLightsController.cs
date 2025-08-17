using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace VehicleFramework
{
    public class NavigationLightsController : BaseLightController
    {
        VehicleTypes.Submarine MV => GetComponent<VehicleTypes.Submarine>();
        protected override void HandleLighting(bool active)
        {
            if(active)
            {
                EnableLightClass(LightClass.Positions);
                EnableLightClass(LightClass.Ports);
                EnableLightClass(LightClass.Starboards);
            }
            else
            {
                foreach (LightClass lc in Enum.GetValues(typeof(LightClass)).Cast<LightClass>())
                {
                    DisableLightClass(lc);
                }
            }
            foreach (var component in GetComponentsInChildren<ILightsStatusListener>())
            {
                if (active)
                {
                    component.OnNavLightsOn();
                }
                else
                {
                    component.OnNavLightsOff();
                }
            }
        }

        protected override void HandleSound(bool playSound)
        {
            return;
        }

        protected virtual void Awake()
        {
            bool noPort = MV.NavigationPortLights == null || MV.NavigationPortLights.Count < 1;
            bool noStar = MV.NavigationStarboardLights == null || MV.NavigationStarboardLights.Count < 1;
            bool noPosi = MV.NavigationPositionLights == null || MV.NavigationPositionLights.Count < 1;
            bool noReds = MV.NavigationRedStrobeLights == null || MV.NavigationRedStrobeLights.Count < 1;
            bool noWhit = MV.NavigationWhiteStrobeLights == null || MV.NavigationWhiteStrobeLights.Count < 1;
            if (noPort && noStar && noPosi && noReds && noWhit)
            {
                Component.DestroyImmediate(this);
            }
        }
        protected void Start()
        {
            rb = GetComponent<Rigidbody>();
            if(rb is null)
            {
                throw Admin.SessionManager.Fatal(MV.GetName() + " has no Rigidbody! Please add a Rigidbody to the GameObject before starting the NavigationLightsController.");
            }
            if (MV.NavigationPositionLights != null)
            {
                foreach (GameObject lightObj in MV.NavigationPositionLights)
                {
                    positionMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
                BlinkOn(positionMats, Color.white);
            }
            if (MV.NavigationRedStrobeLights != null)
            {
                foreach (GameObject lightObj in MV.NavigationRedStrobeLights)
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
            if (MV.NavigationWhiteStrobeLights != null)
            {
                foreach (GameObject lightObj in MV.NavigationWhiteStrobeLights)
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
            if (MV.NavigationPortLights != null)
            {
                foreach (GameObject lightObj in MV.NavigationPortLights)
                {
                    portMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
            }
            if (MV.NavigationStarboardLights != null)
            {
                foreach (GameObject lightObj in MV.NavigationStarboardLights)
                {
                    starboardMats.Add(lightObj.GetComponent<MeshRenderer>().material);
                }
            }
            Admin.SessionManager.StartCoroutine(ControlLights());
        }

        Rigidbody rb = null!;
        bool position = false;
        Coroutine? white = null;
        Coroutine? red = null;
        Coroutine? port = null;
        Coroutine? starboard = null;
        public const float lightBrightness = 1f;
        public const float strobeBrightness = 30f;
        private readonly List<Material> positionMats = new();
        private readonly List<Material> portMats = new();
        private readonly List<Material> starboardMats = new();
        private readonly List<Material> whiteStrobeMats = new();
        private readonly List<Material> redStrobeMats = new();
        private readonly List<Light> whiteStrobeLights = new();
        private readonly List<Light> redStrobeLights = new();

        private enum LightClass
        {
            WhiteStrobes,
            RedStrobes,
            Positions,
            Ports,
            Starboards
        }

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
                    if (MainPatcher.NautilusConfig.IsFlashingLights && white == null)
                    {
                        white = Admin.SessionManager.StartCoroutine(Strobe(LightClass.WhiteStrobes));
                    }
                    break;
                case LightClass.RedStrobes:
                    if (MainPatcher.NautilusConfig.IsFlashingLights && red == null)
                    {
                        red = Admin.SessionManager.StartCoroutine(Strobe(LightClass.RedStrobes));
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
                    port ??= Admin.SessionManager.StartCoroutine(BlinkNarySequence(2, true));
                    break;
                case LightClass.Starboards:
                    starboard ??= Admin.SessionManager.StartCoroutine(BlinkNarySequence(2, false));
                    break;
            }
        }
        private void BlinkThisLightOn(Material mat, Color col)
        {
            mat.EnableKeyword(Admin.Utils.emissionKeyword);
            mat.EnableKeyword(Admin.Utils.specmapKeyword);
            mat.SetFloat(Admin.Utils.glowField, lightBrightness);
            mat.SetFloat(Admin.Utils.glowNightField, lightBrightness);
            mat.SetColor(Admin.Utils.colorField, col);
            mat.SetColor(Admin.Utils.glowColorField, col);
        }
        private void BlinkThisStrobeOn(Material mat, Color col)
        {
            mat.EnableKeyword(Admin.Utils.emissionKeyword);
            mat.EnableKeyword(Admin.Utils.specmapKeyword);
            mat.SetFloat(Admin.Utils.glowField, strobeBrightness);
            mat.SetFloat(Admin.Utils.glowNightField, strobeBrightness);
            mat.SetColor(Admin.Utils.colorField, col);
            mat.SetColor(Admin.Utils.glowColorField, col);
        }
        private void BlinkThisLightOff(Material mat)
        {
            mat.DisableKeyword(Admin.Utils.emissionKeyword);
        }
        private void BlinkOn(List<Material> mats, Color col)
        {
            foreach (Material mat in mats)
            {
                BlinkThisLightOn(mat, col);
            }
        }
        private void BlinkOff(List<Material> mats)
        {
            foreach (Material mat in mats)
            {
                BlinkThisLightOff(mat);
            }
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
        private void BlinkOnStrobe(LightClass lc)
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
        private void BlinkOffStrobe(LightClass lc)
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
        private IEnumerator Strobe(LightClass lc)
        {
            while (true)
            {
                BlinkOnStrobe(lc);
                yield return new WaitForSeconds(0.01f);
                BlinkOffStrobe(lc);
                yield return new WaitForSeconds(2.99f);
            }
        }
        private IEnumerator BlinkNarySequence(int n, bool isPortSide)
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
                for (int i = 0; i < sequenceLength - n; i++)
                {
                    if (0 <= i && i < m)
                    {
                        if (isPortSide)
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
        private IEnumerator ControlLights()
        {
            while (true)
            {
                if (IsLightsOn)
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
    }
}
