using UnityEngine;

namespace XenoSteel.Combat
{
    public enum PresentationType
    {
        None,
        SimpleMotion,
        NormalMotion,
        Special
    }

    [CreateAssetMenu(
        fileName = "NewCombatPresentationData",
        menuName = "XenoSteel/Combat Presentation Data"
    )]
    public class XenoSteelCombatPresentationData : ScriptableObject
    {
        [Header("演出方式")]
        public PresentationType presentationType =
            PresentationType.None;

        
        [Header("Animator")]
        public string animatorTriggerName;

        public AnimationClip attackAnimation;

        [Header("前後運動")]
        public float attackDistance = 0.5f;

        public float attackDuration = 0.15f;

        [Header("攻撃エフェクト")]
        public GameObject attackEffectPrefab;
    }
}