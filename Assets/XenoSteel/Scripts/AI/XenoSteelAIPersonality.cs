using UnityEngine;

namespace XenoSteel.AI
{
    /// <summary>
    /// AIの行動方針を定義する性格データ。
    /// パイロットごとではなく、性格ごとに共通の設定として使用する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "AIPersonality",
        menuName = "XenoSteel/AI/Personality"
    )]
    public class XenoSteelAIPersonality : ScriptableObject
    {
        [Header("攻撃性/突撃度")]
        [Range(0f, 1f)]
        public float aggression = 1.0f;

        [Header("慎重性/逃避度")]
        [Range(0f, 1f)]
        public float cowardice = 0.0f;

        [Header("遮蔽執着度/ヒット&ラン")]

        [Range(0f, 1f)]
        public float coverAttachment = 0.0f;

        [Header("規律性/命令遵守度")]
        [Range(0f, 1f)]
        public float obedience = 1.0f;

        [Header("索敵重視度/警戒度")]
        [Range(0f, 1f)]
        public float reconImportance = 0.5f;


        [Header("パニック耐性")]
        [Range(0f, 1f)]
        public float panicThreshold = 0.5f;
    }
}