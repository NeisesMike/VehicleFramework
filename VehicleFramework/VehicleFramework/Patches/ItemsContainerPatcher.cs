using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
	[HarmonyPatch(typeof(ItemsContainer))]
	class ItemsContainerPatcher
	{
	}
}
