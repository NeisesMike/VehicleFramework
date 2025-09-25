using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Assets;
using VehicleFramework.AutoPilot;
using VehicleFramework.Engines;
using VehicleFramework.Extensions;
using VehicleFramework.Interfaces;
using VehicleFramework.LightControllers;
using VehicleFramework.StorageComponents;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.VehicleComponents;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    /*
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     * This file contains the recoloring functionality for ModVehicle.
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        #region private_fields
        private Color baseColor = Color.white;
        private Color interiorColor = Color.white;
        private Color stripeColor = Color.white;
        private Color nameColor = Color.black;
        private bool IsDefaultStyle = false;
        #endregion

        #region virtual_methods
        // These methods can be overridden by the vehicle class to customize painting behavior.
        protected internal virtual void PaintBaseColor(Vector3 hsb, Color color)
        {
        }
        protected internal virtual void PaintInteriorColor(Vector3 hsb, Color color)
        {
        }
        protected internal virtual void PaintStripeColor(Vector3 hsb, Color color)
        {
        }
        protected internal virtual void PaintVehicleDefaultStyle()
        {
        }
        #endregion

        #region public_methods
        // These are the methods that should be called to effect a color change.
        public void SetBaseColor(Color col)
        {
            subName.SetColor(0, Vector3.zero, col);
            baseColor = col;
        }
        public void SetInteriorColor(Color col)
        {
            subName.SetColor(2, Vector3.zero, col);
            interiorColor = col;
        }
        public void SetStripeColor(Color col)
        {
            subName.SetColor(3, Vector3.zero, col);
            stripeColor = col;
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
        public void SetVehicleDefaultStyle()
        {
            IsDefaultStyle = true;
            PaintVehicleDefaultStyle();
        }
        public void SetVehicleDefaultStyle(string name)
        {
            SetVehicleDefaultStyle();
            SetName(name);
        }
        #endregion

    }
}
