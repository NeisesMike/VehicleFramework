using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using VehicleFramework.Engines;
using VehicleFramework.VehicleComponents;
using VehicleFramework.Assets;
using VehicleFramework.Admin;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.LightControllers;
using VehicleFramework.StorageComponents;
using VehicleFramework.Interfaces;
using VehicleFramework.Extensions;
using VehicleFramework.VehicleTypes;
using VehicleFramework.AutoPilot;

namespace VehicleFramework
{
    /*
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        #region virtual_methods
        public virtual void PaintBaseColor(Vector3 hsb, Color color)
        {
            baseColor = color;
        }
        public virtual void PaintInteriorColor(Vector3 hsb, Color color)
        {
            interiorColor = color;
        }
        public virtual void PaintStripeColor(Vector3 hsb, Color color)
        {
            stripeColor = color;
        }
        #endregion

        #region private_fields
        private Color baseColor = Color.white;
        private Color interiorColor = Color.white;
        private Color stripeColor = Color.white;
        private Color nameColor = Color.black;
        private bool IsDefaultStyle = false;
        #endregion


        #region public_methods
        public void SetBaseColor(Color col)
        {
            subName.SetColor(0, Vector3.zero, col);
        }
        public void SetInteriorColor(Color col)
        {
            subName.SetColor(2, Vector3.zero, col);
        }
        public void SetStripeColor(Color col)
        {
            subName.SetColor(3, Vector3.zero, col);
        }
        public void SetName(string name)
        {
            vehicleName = name;
            subName.SetName(name);
        }
        public void SetName(string name, Color col)
        {
            SetName(name);
            subName.SetColor(1, Vector3.zero, col); // see SubNamePatcher.cs for more details
        }
        internal void SetVehicleDefaultStyle()
        {
            IsDefaultStyle = true;
            PaintVehicleDefaultStyle();
        }
        internal void SetVehicleDefaultStyle(string name)
        {
            SetVehicleDefaultStyle();
            SetName(name);
        }
        public virtual void PaintVehicleDefaultStyle()
        {
        }
        #endregion

    }
}
