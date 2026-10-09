using System;

namespace XenoSteel.Combat
{
    [Serializable]
    public abstract class XenoSteelEffect
    {
        public abstract void Execute(
            XenoSteelEffectContext context
        );
    }
}