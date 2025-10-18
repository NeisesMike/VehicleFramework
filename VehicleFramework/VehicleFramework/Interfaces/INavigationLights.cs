using System.Collections.Generic;
using UnityEngine;

namespace VehicleFramework.Interfaces
{
    public interface INavigationLights
    {
        public List<GameObject>? NavigationPortLights();
        public List<GameObject>? NavigationStarboardLights();
        public List<GameObject>? NavigationPositionLights();
        public List<GameObject>? NavigationWhiteStrobeLights();
        public List<GameObject>? NavigationRedStrobeLights();
    }
}
