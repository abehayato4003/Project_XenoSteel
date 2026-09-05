using System.Linq;
using System.Threading.Tasks;

using TurnBasedStrategyFramework.Common.AI.BehaviourTrees;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

using XenoSteel.Combat;
using XenoSteel.Core;

namespace XenoSteel.AI
{
    /// <summary>
    /// XenoSteel用のAI攻撃処理。
    /// スキルの射程内にいる敵を確認し、
    /// ENを考慮して使用可能なSkillで攻撃する。
    /// </summary>
    public class XenoSteelAIAttackActionNode : ITreeNode
    {
        private readonly IUnit _unit;
        private readonly IGridController _gridController;

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
                return Task.FromResult(false);
            }

            var unit =
                _unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unit == null)
            {
                return Task.FromResult(false);
            }

            var initiative =
                unit.GetComponent<XenoSteelInitiative>();

            if (initiative == null ||
                initiative.UnitData == null ||
                initiative.UnitData.skills == null ||
                initiative.UnitData.skills.Length == 0 ||
                initiative.Stats == null)
            {
                return Task.FromResult(false);
            }

            var attackAbility =
                unit.GetComponent<XenoSteelAttackAbility>();

            if (attackAbility == null)
            {
                return Task.FromResult(false);
            }

            var enemyUnits = _gridController.UnitManager
                .GetEnemyUnits(_unit.PlayerNumber);

            SkillData bestSkill = null;
            IUnit bestTarget = null;
            XenoSteelUnitStats bestTargetStats = null;
            int bestDamage = -1;

            foreach (var skill in initiative.UnitData.skills)
            {
                if (skill == null)
                {
                    continue;
                }

                // EN不足のSkillは候補から除外
                if (!initiative.Stats.CanConsumeEN(skill.energyCost))
                {
                    UnityEngine.Debug.Log(
                        $"AI Skill skipped: {skill.skillName}, " +
                        $"EN不足 (必要={skill.energyCost}, " +
                        $"現在={initiative.Stats.EN})"
                    );
                    continue;
                }

                // このSkillの射程内にいる敵を探す
                var target = enemyUnits.FirstOrDefault(enemy =>
                    enemy.CurrentCell != null &&
                    _unit.CurrentCell != null &&
                    enemy.CurrentCell.GetDistance(_unit.CurrentCell) <= skill.range
                );

                if (target == null)
                {
                    continue;
                }

                var targetUnit =
                    target as TurnBasedStrategyFramework.Unity.Units.Unit;

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

                // このSkillで与えられるダメージを計算
                var damage =
                    XenoSteelDamageCalculator.CalculateDamage(
                        initiative.Stats,
                        skill,
                        targetInitiative.Stats
                    );

                // より高いダメージを出せるSkillを採用
                if (damage > bestDamage)
                {
                    bestDamage = damage;
                    bestSkill = skill;
                    bestTarget = target;
                    bestTargetStats = targetInitiative.Stats;
                }
            }

            // 使用可能なSkillがなければ攻撃しない
            if (bestSkill == null || bestTarget == null)
            {
                return Task.FromResult(false);
            }

            // 選択したSkillを設定
            attackAbility.SetCurrentSkill(bestSkill);

            UnityEngine.Debug.Log(
                $"AI Selected Skill: {bestSkill.skillName}, " +
                $"Power={bestSkill.power}, " +
                $"EnergyCost={bestSkill.energyCost}, " +
                $"Damage={bestDamage}"
            );

            var tcs = new TaskCompletionSource<bool>();

            _unit.AIExecuteAbility(
                new AttackCommand(bestTarget, bestDamage),
                _gridController,
                tcs
            );

            // 攻撃完了後にENを消費
            return ConsumeENAfterAttack(
                tcs.Task,
                initiative.Stats,
                bestSkill.energyCost
            );
        }

        private async Task<bool> ConsumeENAfterAttack(
            Task<bool> attackTask,
            XenoSteelUnitStats stats,
            int energyCost)
        {
            bool result = await attackTask;

            if (result)
            {
                if (stats.ConsumeEN(energyCost))
                {
                    UnityEngine.Debug.Log(
                        $"AI EN consumed: {energyCost}, " +
                        $"EN after attack: {stats.EN}"
                    );
                }
            }

            return result;
        }
    }
}