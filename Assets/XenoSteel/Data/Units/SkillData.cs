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

        [Header("UI")]
        public Sprite icon;

        [Header("攻撃性能")]
        public int range = 1;

        [Header("EN")]
        public int energyCost = 0;

        [Header("攻撃範囲")]
        public int area = 0;

        [Header("攻撃形状")]
        public SkillAttackShape attackShape = SkillAttackShape.Single;

        [Header("攻撃幅")]
        [Min(1)]
        public int attackWidth = 1;

        [Header("使用条件")]
        public bool requiresTarget = true;

        [Header("対象選択")]
        public SkillTargetType targetType = SkillTargetType.Unit;

        [Header("戦闘演出")]
        public XenoSteelCombatPresentationData presentation;

        [Header("Effects")]
        [SerializeReference]
        public XenoSteelEffect[] effects;
    }

    public enum SkillAttackShape
    {
        Single,
        Line,
        Cross
    }

    public enum SkillTargetType
    {
        Unit,
        Cell,
        None
    }
}