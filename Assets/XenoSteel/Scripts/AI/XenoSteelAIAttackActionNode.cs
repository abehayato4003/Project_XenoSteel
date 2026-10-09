using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TurnBasedStrategyFramework.Common.AI.BehaviourTrees;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

using XenoSteel.Combat;
using XenoSteel.Core;
using XenoSteel.Units;

using UnityEngine;

namespace XenoSteel.AI
{
    public class XenoSteelAIAttackActionNode : ITreeNode
    {
        private readonly IUnit _unit;
        private readonly IGridController _gridController;

        private class AttackCandidate
        {
            public SkillData Skill;
            public IUnit Target;
            public int Damage;
        }

        public XenoSteelAIAttackActionNode(
            IUnit unit,
            IGridController gridController)
        {
            _unit = unit;
            _gridController = gridController;
        }

        public async Task<bool> Execute(bool debugMode)
        {
            if (_unit.ActionPoints <= 0)
            {
                return false;
            }

            var unit =
                _unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unit == null)
            {
                return false;
            }

            var initiative =
                unit.GetComponent<XenoSteelInitiative>();

            if (initiative == null ||
                initiative.UnitData == null ||
                initiative.UnitData.skills == null ||
                initiative.UnitData.skills.Length == 0 ||
                initiative.Stats == null)
            {
                return false;
            }

            var attackAbility =
                unit.GetComponent<XenoSteelAttackAbility>();

            if (attackAbility == null)
            {
                return false;
            }

            var enemyUnits =
                _gridController.UnitManager.GetEnemyUnits(
                    _unit.PlayerNumber
                );

            var candidates =
                new List<AttackCandidate>();

            // ----------------------------------------
            // Skill × Target の候補を作る
            // ----------------------------------------

            foreach (var skill in initiative.UnitData.skills)
            {
                if (skill == null)
                {
                    continue;
                }

                // Unit対象以外はこの攻撃AIでは扱わない
                if (skill.targetType != SkillTargetType.Unit)
                {
                    continue;
                }

                // EN不足
                if (!initiative.Stats.CanConsumeEN(
                        skill.energyCost))
                {
                    continue;
                }

                // DamageEffectを取得
                var damageEffect =
                    skill.effects?
                        .OfType<XenoSteelDamageEffect>()
                        .FirstOrDefault();

                // ダメージを与えないSkillは
                // このAttackActionNodeでは使用しない
                if (damageEffect == null)
                {
                    continue;
                }

                // 実際に攻撃可能な対象を取得
                var attackableTargets =
                    XenoSteelAttackTargeting
                        .GetAttackableTargets(
                            _unit,
                            skill,
                            _gridController
                        );

                foreach (var target in attackableTargets)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    if (!enemyUnits.Contains(target))
                    {
                        continue;
                    }

                    var targetUnit =
                        target as TurnBasedStrategyFramework
                            .Unity.Units.Unit;

                    if (targetUnit == null)
                    {
                        continue;
                    }

                    var targetInitiative =
                        targetUnit.GetComponent<XenoSteelInitiative>();

                    if (targetInitiative == null ||
                        targetInitiative.Stats == null)
                    {
                        continue;
                    }

                    int damage =
                        XenoSteelDamageCalculator.CalculateDamage(
                            initiative.Stats,
                            damageEffect.power,
                            targetInitiative.Stats
                        );

                    candidates.Add(
                        new AttackCandidate
                        {
                            Skill = skill,
                            Target = target,
                            Damage = damage
                        }
                    );
                }
            }

            // 攻撃候補がなければ攻撃しない
            if (candidates.Count == 0)
            {
                return false;
            }

            // ----------------------------------------
            // ダメージ順に並べる
            // ----------------------------------------

            candidates = candidates
                .OrderByDescending(candidate => candidate.Damage)
                .ToList();

            // ----------------------------------------
            // 上位候補からランダム選択
            //
            // 1候補 : 100%
            // 2候補 : 60 / 40
            // 3候補以上 : 60 / 30 / 10
            // ----------------------------------------

            int candidateCount =
                Mathf.Min(3, candidates.Count);

            AttackCandidate selectedCandidate;

            if (candidateCount == 1)
            {
                selectedCandidate = candidates[0];
            }
            else
            {
                float[] weights;

                if (candidateCount == 2)
                {
                    weights = new float[]
                    {
                        0.60f,
                        0.40f
                    };
                }
                else
                {
                    weights = new float[]
                    {
                        0.60f,
                        0.30f,
                        0.10f
                    };
                }

                float random =
                    Random.Range(0f, 1f);

                float cumulative = 0f;

                selectedCandidate = candidates[0];

                for (int i = 0;
                     i < candidateCount;
                     i++)
                {
                    cumulative += weights[i];

                    if (random <= cumulative)
                    {
                        selectedCandidate =
                            candidates[i];

                        break;
                    }
                }
            }

            // ----------------------------------------
            // 選択された攻撃を実行
            // ----------------------------------------

            Debug.Log(
                $"AI Selected Skill: " +
                $"{selectedCandidate.Skill.skillName}, " +
                $"Target={selectedCandidate.Target}, " +
                $"PredictedDamage={selectedCandidate.Damage}"
            );

            return await attackAbility.ExecuteAISkill(
                selectedCandidate.Skill,
                selectedCandidate.Target,
                _gridController
            );
        }
    }
}