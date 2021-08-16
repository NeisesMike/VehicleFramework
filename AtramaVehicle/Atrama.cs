using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle
{
    /*
     * The root model is an Atrama
     * The Atrama has and AtramaEngine
     * The root model has a chair model child.
     * The chair model is an AtramaVehicle.
     * The root model has a panel model.
     * The panel model is a VehicleUpgradeConsoleInput
     */
    public class Atrama : MonoBehaviour
    {
        public bool isPlayerInside = false;
        public bool isPlayerPiloting = false;
        public bool wasPowered = false;
        public bool isLightsOn = true;

        // Set all of these in AtramaPreparer
        public GameObject modularStorage;
        public GameObject leftStorage = null;
        public GameObject rightStorage = null;
        public AtramaVehicle vehicle = null;
        public AtramaHatch hatch = null;
        public EnergyInterface energyInterface = null;
        public List<Renderer> interiorRenderers = new List<Renderer>();
        public List<GameObject> volumetricLights = new List<GameObject>();
        public List<GameObject> lights = new List<GameObject>();
        public ChildObjectIdentifier storageRoot = null;
        public PingInstance pingInstance = null;

        private float timeOfLastLevelTap = 0f;
        private const float doubleTapWindow = 1f;
        public bool autoLeveling = true;


        private float rollVelocity = 0.0f;
        private float pitchVelocity = 0.0f;
        private float smoothTime = 0.3f;

        public FMOD_StudioEventEmitter lightsOnSound = null;
        public FMOD_StudioEventEmitter lightsOffSound = null;




        public void Awake()
        {
            //Logger.Log("Atrama waking...");
        }

        public void Start()
        {
            foreach (GameObject light in volumetricLights)
            {
                light.transform.localEulerAngles = Vector3.zero;
            }
        }
        public void Update()
        {
            stabilizeRoll();
            checkAutoLevel();

            if (isPlayerPiloting && Player.main.GetRightHandDown() && !Player.main.GetPDA().isInUse)
            {
                toggleLights();
            }
        }

        public void toggleLights()
        {
            isLightsOn = !isLightsOn;
            if (isLightsOn)
            {
                setFloodLampsActive(true);
                if (vehicle.IsPowered())
                {
                    Utils.PlayEnvSound(lightsOnSound, lightsOnSound.gameObject.transform.position, 20f);
                }
            }
            else
            {
                setFloodLampsActive(false);
                if (vehicle.IsPowered())
                {
                    Utils.PlayEnvSound(lightsOffSound, lightsOffSound.gameObject.transform.position, 20f);
                }
            }


        }

        public void checkAutoLevel()
        {
            float roll = transform.rotation.eulerAngles.z;
            float rollDelta = roll >= 180 ? 360 - roll : roll;
            float pitch = transform.rotation.eulerAngles.x;
            float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;
            if (Input.GetKeyDown(AtramaVehiclePatcher.Config.levelButton))
            {
                if (Time.time - timeOfLastLevelTap < doubleTapWindow)
                {
                    Logger.output("Automatically leveling...");
                    autoLeveling = true;
                    smoothTime = 2f * pitchDelta / 90f;
                }
                else
                {
                    timeOfLastLevelTap = Time.time;
                }
            }

            if (rollDelta < 4 && pitchDelta < 4)
            {
                autoLeveling = false;
            }
        }




        public void FixedUpdate()
        {
            stabilizeRoll();

            if(autoLeveling)
            {
                float newPitch;
                if(transform.rotation.eulerAngles.x >= 180)
                {
                    newPitch = Mathf.SmoothDamp(transform.rotation.eulerAngles.x, 360, ref pitchVelocity, smoothTime);
                }
                else
                {
                    newPitch = Mathf.SmoothDamp(transform.rotation.eulerAngles.x, 0, ref pitchVelocity, smoothTime);
                }
                float newRoll = Mathf.SmoothDamp(transform.rotation.eulerAngles.z, 0, ref rollVelocity, smoothTime);
                transform.rotation = Quaternion.Euler(new Vector3(newPitch, transform.rotation.eulerAngles.y, newRoll));
            }

            checkPower();
        }

        public void stabilizeRoll()
        {
            Vector3 ogRot = transform.rotation.eulerAngles;
            Vector3 newRot = ogRot;
            newRot.z = 0;
            transform.rotation = Quaternion.Euler(newRot);
        }

        public void enter()
        {
            //tp player
            Player.main.transform.position = hatch.transform.position - hatch.transform.up;

            isPlayerInside = true;
            isPlayerPiloting = false;

            setVolumetricLightsActive(false);
        }

        public void exit()
        {

            // tp out, directly above the hatch
            Player.main.transform.position = hatch.transform.position + hatch.transform.up;

            isPlayerInside = false;
            isPlayerPiloting = false;

            setVolumetricLightsActive(true);
        }

        public void checkPower()
        {
            if(vehicle.IsPowered())
            {
                // if newly powered
                if(!wasPowered)
                {
                    setFloodLampsActive(true);
                    enableInteriorLighting();
                }
                wasPowered = true;
            }
            else
            {
                // if newly unpowered
                if(wasPowered)
                {
                    setFloodLampsActive(false);
                    disableInteriorLighting();
                }
                wasPowered = false;
            }
        }

        public void enableInteriorLighting()
        {
            foreach (var renderer in interiorRenderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name != "Main-Body"
                        )
                    {
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }

                }
            }
        }

        public void disableInteriorLighting()
        {
            foreach (var renderer in interiorRenderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name != "Main-Body"
                        )
                    {
                        mat.DisableKeyword("MARMO_EMISSION");
                    }

                }
            }
        }

        public void setVolumetricLightsActive(bool enabled)
        {
            foreach (GameObject light in volumetricLights)
            {
                light.SetActive(!isPlayerInside && enabled && vehicle.IsPowered());
            }
        }
        public void setFloodLampsActive(bool enabled)
        {
            foreach (GameObject light in lights)
            {
                light.SetActive(enabled && vehicle.IsPowered());
            }
            setVolumetricLightsActive(enabled);
        }



    }
}
