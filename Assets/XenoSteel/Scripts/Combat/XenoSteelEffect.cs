using UnityEngine;

namespace XenoSteel.Combat
{
    public abstract class XenoSteelEffect : ScriptableObject
    {
        public abstract void Execute(
            XenoSteelEffectContext context
        );
    }
}