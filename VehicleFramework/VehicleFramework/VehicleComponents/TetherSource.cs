using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    /*
     * Tether Sources are meant to be placed throughout the Submarine.
     * The Submarine has one TetherSource component to manage them.
     * They should be strictly inside the ship.
     * A player will "leash" to them when close enough,
     * which ensures the player's entry is recognized no matter what (warp in).
     */
    public class TetherSource : MonoBehaviour, IScuttleListener, IDockListener
    {
        public Submarine mv = null;
        private bool isLive = true;
        public bool isSimple;
        public Bounds bounds
        {
            get
            {
                if (mv == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                BoxCollider collider = mv.BoundingBoxCollider;
                if(collider == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                collider.gameObject.SetActive(true);
                collider.enabled = true;
                Bounds result = collider.bounds;
                collider.enabled = false;
                if (result.size == Vector3.zero)
                {
                    if(!collider.gameObject.activeInHierarchy)
                    {
                        if(!collider.gameObject.activeSelf)
                        {
                            Logger.Warn("TetherSource Error: BoundingBoxCollider was not active. Setting it active.");
                            collider.gameObject.SetActive(true);
                        }
                        if (!collider.gameObject.activeInHierarchy)
                        {
                            Logger.Warn("TetherSource Error: BoundingBoxCollider was not active in its hierarchy. One of its parents must be inactive. Trying to set them active...");
                            Transform iterator = collider.transform;
                            while (iterator != mv.transform)
                            {
                                if (!iterator.gameObject.activeSelf)
                                {
                                    iterator.gameObject.SetActive(true);
                                    Logger.Warn("Set " + iterator.name + " active.");
                                }
                                iterator = iterator.parent;
                            }
                        }
                        collider.enabled = true;
                        result = collider.bounds;
                        collider.enabled = false;
                        return result;
                    }
                    else
                    {
                        Logger.Warn("TetherSource Error: BoundingBox Bounds had zero volume (size was zero).");
                    }
                    return new Bounds(Vector3.one, Vector3.zero);
                }
                return result;
            }
        }

        public void Start()
        {
            if (mv.BoundingBoxCollider == null || mv.TetherSources.Count() == 0)
            {
                isSimple = true;
            }
            else
            {
                isSimple = false;
                mv.BoundingBoxCollider.gameObject.SetActive(true);
                mv.BoundingBoxCollider.enabled = false;
                mv.TetherSources.ForEach(x => x.SetActive(false));
            }
            Player.main.StartCoroutine(ManageTether());
        }

        public void TryToDropLeash()
        {
            if (isSimple)
            {
                if (Vector3.Distance(Player.main.transform.position, transform.position) > 10)
                {
                    mv.PlayerExit();
                }
            }
            else
            {
                if (!bounds.Contains(Player.main.transform.position))
                {
                    mv.PlayerExit();
                }
            }
        }

        public void TryToEstablishLeash()
        {
            bool PlayerWithinLeash(GameObject tetherSrc)
            {
                float radius = 0.75f;
                if (tetherSrc.GetComponentInChildren<SphereCollider>(true) != null)
                {
                    radius = tetherSrc.GetComponentInChildren<SphereCollider>(true).radius;
                }
                return Vector3.Distance(Player.main.transform.position, tetherSrc.transform.position) < radius;
            }
            if (Player.main.GetVehicle() == null)
            {
                if (isSimple)
                {
                    if (Vector3.Distance(Player.main.transform.position, transform.position) < 1f)
                    {
                        mv.PlayerEntry();
                    }
                    else if (Vector3.Distance(Player.main.transform.position, mv.PilotSeats.First().Seat.transform.position) < 1f)
                    {
                        mv.PlayerEntry();
                    }
                }
                else
                {
                    if (mv.TetherSources.Where(x => PlayerWithinLeash(x)).Count() > 0)
                    {
                        mv.PlayerEntry();
                    }
                }
            }

        }

        public IEnumerator ManageTether()
        {
            yield return new WaitForSeconds(3f);
            while (true)
            {
                if(mv == null)
                {
                    yield break;
                }
                if(isLive)
                {
                    if (mv.IsPlayerInside())
                    {
                        TryToDropLeash();
                    }
                    else
                    {
                        TryToEstablishLeash();
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        void IScuttleListener.OnScuttle()
        {
            isLive = false;
        }

        void IScuttleListener.OnUnscuttle()
        {
            isLive = true;
        }

        void IDockListener.OnDock()
        {
            isLive = false;
        }

        void IDockListener.OnUndock()
        {
            isLive = true;
        }
    }
}
