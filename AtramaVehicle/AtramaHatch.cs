using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle
{
    public class AtramaHatch : HandTarget, IHandTarget
	{
		public Atrama atrama;

		public void Start()
        {
			atrama = transform.parent.gameObject.GetComponent<Atrama>();
        }

		public void OnHandHover(GUIHand hand)
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			if (atrama.isPlayerInside)
			{
				HandReticle.main.SetInteractText("Exit Atrama");
			}
			else
			{
				HandReticle.main.SetInteractText("Enter Atrama");
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			if (atrama.isPlayerInside)
			{
				atrama.exit();
			}
			else
			{
				atrama.enter();
			}
		}
	}
}
