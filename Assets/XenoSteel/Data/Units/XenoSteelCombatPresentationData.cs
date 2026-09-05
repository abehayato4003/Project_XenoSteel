using UnityEngine;

namespace XenoSteel.Combat
{
    [CreateAssetMenu(
        fileName = "NewCombatPresentationData",
        menuName = "XenoSteel/Combat Presentation Data"
    )]
    public class XenoSteelCombatPresentationData : ScriptableObject
    {
        [Header("攻撃アニメーション")]
        public AnimationClip attackAnimation;

        [Header("攻撃エフェクト")]
        public GameObject attackEffectPrefab;
    }
}