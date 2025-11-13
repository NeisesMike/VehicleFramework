using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;

namespace VehicleFramework.AutoPilot
{
    public class AutoPilotNavigator : MonoBehaviour
    {
        public void Update()
        {
            if (GetComponent<ModVehicle>() != null && GetComponent<ModVehicle>().GetPilotingMode())
            {
                StopAllCoroutines();
                return;
            }
        }
        public void NaiveGo(Vector3 destination)
        {
            Admin.SessionManager.StartCoroutine(GoStraightToDestination(destination));
        }

        public void FaceDestinationFrame(Vector3 dest)
        {
            Vector3 direction = (dest - transform.position).normalized;
            Quaternion goal = Quaternion.LookRotation(direction);
            float rotationSpeed = 1f;
            while(0.01f < Quaternion.Angle(transform.rotation, goal))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, goal, rotationSpeed * Time.deltaTime);
            }
        }
        public IEnumerator GoStraightToDestination(Vector3 dest)
        {
            ModVehicleEngine engine = GetComponent<ModVehicleEngine>();
            if(engine == null)
            {
                yield break;
            }
            IEnumerator ForwardLoop(float power)
            {
                float now = Time.time;
                while(Time.time < now + 0.1f)
                {
                    if (engine == null) yield break;
                    engine.ApplyPlayerControls(Vector3.forward * power);
                    engine.ExecutePhysicsMove();
                    yield return new WaitForFixedUpdate();
                }
            }
            IEnumerator BreakLoop()
            {
                float now = Time.time;
                while (Time.time < now + 0.1f && 4 < gameObject.GetComponent<Rigidbody>().velocity.magnitude)
                {
                    if (engine == null) yield break;
                    engine.ApplyPlayerControls(-Vector3.forward);
                    engine.ExecutePhysicsMove();
                    yield return new WaitForFixedUpdate();
                }
            }
            bool RaycastForward(float distance)
            {
                RaycastHit[] allHits = Physics.RaycastAll(transform.position, transform.forward, distance);
                var myHits = allHits
                    .Where(hit => hit.transform.GetComponent<Creature>() == null) // ignore creatures
                    .Where(hit => hit.transform.GetComponent<Player>() == null) // ignore player
                    ;
                return myHits.Any();
            }
            bool CheckClose(Vector3 destin, float magnit)
            {
                return magnit < Vector3.Distance(transform.position, destin) && RaycastForward(magnit);
            }

            while(20 < Vector3.Distance(transform.position, dest))
            {
                yield return new WaitForFixedUpdate();
                if (transform == null) yield break;
                FaceDestinationFrame(dest);
                if (CheckClose(dest, 5f))
                {
                    Logger.PDANote(Language.main.Get("VFAutopilotHint1"));
                    break;
                }
                if (CheckClose(dest, 25f))
                {
                    yield return Admin.SessionManager.StartCoroutine(BreakLoop());
                    if(gameObject?.GetComponent<Rigidbody>() == null) yield break;
                    if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < 0.1f)
                    {
                        Logger.PDANote(Language.main.Get("VFAutopilotHint1"));
                        yield break;
                    }
                }
                else if (CheckClose(dest, 50f))
                {
                    yield return Admin.SessionManager.StartCoroutine(ForwardLoop(0.25f));
                }
                else if (CheckClose(dest, 75f))
                {
                    yield return Admin.SessionManager.StartCoroutine(ForwardLoop(0.5f));
                }
                else
                {
                    yield return Admin.SessionManager.StartCoroutine(ForwardLoop(1f));
                }
            }
        }
    }
}
