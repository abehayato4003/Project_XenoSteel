using System.Collections.Generic;

using TurnBasedStrategyFramework.Common.AI.BehaviourTrees;
using TurnBasedStrategyFramework.Common.AI.Evaluators;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

using TurnBasedStrategyFramework.Unity.AI.BehaviourTrees;
using XenoSteel.AI;

using UnityEngine;

namespace XenoSteel.AI
{
    /// <summary>
    /// XenoSteel用のBehaviour Tree。
    /// TBSF標準のAttackSequenceNodeを使用せず、
    /// XenoSteel独自のSkill・射程・ダメージ計算によって攻撃する。
    /// </summary>
    public class XenoSteelRegularBehaviourTreeResource
        : BehaviourTreeResource
    {
        [Space]
        [Header("General")]

        [SerializeField]
        private int _actionDelay = 100;

        [SerializeField]
        private int _turnFinishDelay = 100;

        [Space]
        [Header("High Health Actions")]

        [SerializeField]
        private float _highHealthThreshold = 0.5f;

        [Space]
        [Header("High Health - Damage Dealt Position Evaluator")]

        [SerializeField]
        private float _highHealthDamageDealtPositionEvaluatorWeight = 1f;

        [SerializeField]
        private float _highHealthDamageDealtPositionEvaluatorDecay = 0.5f;

        [Space]
        [Header("High Health - Damage Received Position Evaluator")]

        [SerializeField]
        private float _highHealthDamageReceivedPositionEvaluatorWeight = -0.1f;

        [SerializeField]
        private float _highHealthDamageReceivedPositionEvaluatorDecay = 0.5f;

        [Space]
        [Header("High Health - Distance Position Evaluator")]

        [SerializeField]
        private float _highHealthDistancePositionEvaluatorWeight = -0.1f;

        [SerializeField]
        private int _highHealthDistancePositionEvaluatorThreshold = 10;

        [Space]
        [Header("Low Health - Damage Dealt Position Evaluator")]

        [SerializeField]
        private float _lowHealthDamageDealtPositionEvaluatorWeight = 0.9f;

        [SerializeField]
        private float _lowHealthDamageDealtPositionEvaluatorDecay = 0.5f;

        [Space]
        [Header("Low Health - Damage Received Position Evaluator")]

        [SerializeField]
        private float _lowHealthDamageReceivedPositionEvaluatorWeight = -1f;

        [SerializeField]
        private float _lowHealthDamageReceivedPositionEvaluatorDecay = 0.5f;

        [Space]
        [Header("Low Health - Distance Position Evaluator")]

        [SerializeField]
        private float _lowHealthDistancePositionEvaluatorWeight = -0.1f;

        [SerializeField]
        private int _lowHealthDistancePositionEvaluatorThreshold = 10;

        [Space]
        [Header("Attack Sequence")]

        [SerializeField]
        private float _healthTargetEvaluatorWeight = 1f;

        [SerializeField]
        private float _damageGivenTargetEvaluatorWeight = 1f;

        public override void Initialize(
            IUnit unit,
            IGridController gridController)
        {
            BehaviourTree = new SequenceNode(
                new List<ITreeNode>
                {
                    new SuccederNode(
                        new XenoSteelAIMoveActionNode(
                            unit,
                            gridController
                        )
                    ),

                    new RealtimeDelayNode(_actionDelay),

                    // XenoSteel独自の攻撃処理
                    new SuccederNode(
                        new XenoSteelAIAttackActionNode(
                        unit,
                        gridController)
                    ),

                    new RealtimeDelayNode(_turnFinishDelay)
                }
            );
        }
    }
}