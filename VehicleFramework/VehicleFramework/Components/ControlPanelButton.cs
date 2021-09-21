using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public class ControlPanelButton : HandTarget, IHandTarget
    {
        private System.Func<bool> myClickFunction;
        private System.Func<bool> myHoverFunction;
        public void Init(System.Func<bool> clickFunc, System.Func<bool> hoverFunc)
        {
            myClickFunction = clickFunc;
            myHoverFunction = hoverFunc;
        }

        void IHandTarget.OnHandClick(GUIHand hand)
        {
            myClickFunction();
        }

        void IHandTarget.OnHandHover(GUIHand hand)
        {
            myHoverFunction();
        }
    }
}
