using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class Scuttler : MonoBehaviour
    {
        private Vector3 scuttlePosition = Vector3.zero;
        private Vector3 initialLandRayCast = Vector3.zero;
        private Coroutine? ScuttleCor;
        private Coroutine? Establish;
        private Coroutine? Check;
        public void Scuttle()
        {
            ScuttleCor = Admin.SessionManager.StartCoroutine(DoScuttle());
        }
        public void Unscuttle()
        {
            Admin.SessionManager.StopCoroutine(ScuttleCor);
            Admin.SessionManager.StopCoroutine(Establish);
            Admin.SessionManager.StopCoroutine(Check);
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        public IEnumerator DoScuttle()
        {
            Establish = Admin.SessionManager.StartCoroutine(EstablishScuttlePosition());
            yield return Establish;
            Check = Admin.SessionManager.StartCoroutine(CheckScuttlePosition());
            yield return Check;
        }
        public IEnumerator EstablishScuttlePosition()
        {
            scuttlePosition = Vector3.zero;
            while (scuttlePosition == Vector3.zero)
            {
                yield return new WaitForSeconds(1f);
                scuttlePosition = RaycastDownToLand(transform);
            }
            initialLandRayCast = scuttlePosition;
            while (true)
            {
                if (transform.position.y < (initialLandRayCast.y - 40)) // why 40?
                {
                    transform.position = initialLandRayCast + Vector3.up * 25f;
                }
                Vector3 oldScuttle = RaycastDownToLand(transform);
                Vector3 oldPosition = transform.position;
                yield return new WaitForSeconds(1f);
                scuttlePosition = RaycastDownToLand(transform);
                if(scuttlePosition == oldScuttle && Vector3.Distance(oldPosition, transform.position) < 0.1)
                {
                    // If we got here,
                    // then we had the same scuttlePosition for one second,
                    // and we were stationary for that same second.
                    if (Vector3.Distance(scuttlePosition, transform.position) < 5f) // why 5?
                    {
                        // If we got here,
                        // we are nearby the scuttle position.
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        yield break;
                    }
                }
            }
        }
        public IEnumerator CheckScuttlePosition()
        {
            while (true)
            {
                if(transform == null)
                {
                    break;
                }
                if (transform.position.y < (scuttlePosition.y - 20)) // why 20?
                {
                    Logger.Log("Moving wreck to " + scuttlePosition.ToString());
                    transform.position = scuttlePosition + Vector3.up * 3f;
                }
                yield return new WaitForSeconds(5f);
            }
        }
        public static Vector3 RaycastDownToLand(Transform root)
        {
            RaycastHit[] allHits;
            allHits = Physics.RaycastAll(root.position, Vector3.down, 1000f);
            foreach(var hit in allHits)
            {
                if (hit.transform.GetComponent<TerrainChunkPieceCollider>() != null)
                {
                    return hit.point;
                }
            }
            // we did not hit terrain
            return Vector3.zero;
        }
        public static bool IsDescendant(Transform child, Transform ancestor)
        {
            if(child == ancestor)
            {
                return true;
            }
            Transform currentParent = child;
            while (currentParent != null)
            {
                if (currentParent == ancestor)
                {
                    return true; // The child is a descendant of ancestor
                }
                currentParent = currentParent.parent; // Move up in the hierarchy
            }
            return false; // No ancestor found in the hierarchy
        }

    }
}
