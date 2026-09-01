using UnityEngine;

namespace XenoSteel.Combat
{
    [CreateAssetMenu(
        fileName = "NewSkillData",
        menuName = "XenoSteel/Skill Data"
    )]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string skillName;

        [Header("攻撃性能")]
        public int range = 1;
        public int power = 100;

        [Header("EN")]
        public int energyCost = 0;

        [Header("攻撃範囲")]
        public int area = 0;

        [Header("範囲攻撃ダメージ")]
        [Range(0f, 1f)]
        public float areaDamageMultiplier = 0.5f;

        [Header("属性")]
        public string attribute;

        [Header("使用条件")]
        public bool requiresTarget = true;
    }
}