using System;

namespace VehicleFramework.VehicleComponents
{
    /*
    public static class SubsystemExtensions
    {
        public static Subsystem WithMaxHealth(this Subsystem sys, float health)
        {
            sys.SetupLiveMixin(health);
            return sys;
        }
        public static Subsystem WithOnTakeDamage(this Subsystem sys, Action<DamageInfo> OnTakeDamage)
        {
            sys.OnTakeDamage = OnTakeDamage;
            return sys;
        }
        public static Subsystem WithOnHealDamage(this Subsystem sys, Action<float> OnHealDamage)
        {
            sys.liveMixin.onHealDamage.AddHandler(sys.gameObject, new UWE.Event<float>.HandleFunction(OnHealDamage));
            return sys;
        }
        public static Subsystem WithOnFullyRepaired(this Subsystem sys, Action OnFullyRepaired)
        {
            sys.OnFullyRepaired = OnFullyRepaired;
            return sys;
        }
        public static Subsystem WithOnHandHover(this Subsystem sys, Action<GUIHand> OnHandHover)
        {
            sys.OnHandHover = OnHandHover;
            return sys;
        }
        public static Subsystem WithOnHandClick(this Subsystem sys, Action<GUIHand> OnHandClick)
        {
            sys.OnHandClick = OnHandClick;
            return sys;
        }
    }
    public class Subsystem : HandTarget, IHandTarget
    {
        internal LiveMixin liveMixin;
        internal Action<GUIHand> OnHandHover;
        internal Action<GUIHand> OnHandClick;
        internal Action<DamageInfo> OnTakeDamage;
        internal Action OnFullyRepaired;

        void IHandTarget.OnHandClick(GUIHand hand)
        {
            OnHandClick?.Invoke(hand);
        }

        void IHandTarget.OnHandHover(GUIHand hand)
        {
            OnHandHover?.Invoke(hand);
        }

        public void OnRepair()
        {
            OnFullyRepaired?.Invoke();
        }

        public override void Awake()
        {
            // This happens during AddComponent<Subsystem> so it's okay to do WithMaxHealth in the same line, like
            // gameObject.AddComponent<Subsystem>().WithMaxHealth(150);
            if (gameObject.GetComponent<LiveMixin>() != null)
            {
                ErrorMessage.AddError($"The Subsystem {name} was added to a GameObject that already has a LiveMixin! Dying!");
                UnityEngine.Component.DestroyImmediate(this);
            }
            base.Awake();
            SetupLiveMixin(100);
        }

        internal void SetupLiveMixin(float maxHealth)
        {
            liveMixin = gameObject.EnsureComponent<LiveMixin>();
            liveMixin.data = new LiveMixinData
            {
                weldable = true,
                canResurrect = true,
                invincibleInCreative = true,
                knifeable = false,
                maxHealth = maxHealth,
            };
            liveMixin.health = maxHealth;
        }
    }
    */
}
