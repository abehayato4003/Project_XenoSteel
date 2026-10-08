using UnityEngine;
using TurnBasedStrategyFramework.Common.Units;

using XenoSteel.Information;

namespace XenoSteel.AI
{
    /// <summary>
    /// AIが判断した行動内容を保持するデータ。
    /// 将来的に通信・傍受システムから参照する。
    /// </summary>
    public class XenoSteelAIDecision
    {
        public string Action { get; }
        public string ThoughtState { get; }
        public Vector2Int? TargetCell { get; }
        public IUnit TargetUnit { get; }
        public XenoSteelInformationSource InformationSource { get; }

        public XenoSteelAIDecision(
            string action,
            string thoughtState,
            Vector2Int? targetCell,
            IUnit targetUnit,
            XenoSteelInformationSource informationSource)
        {
            Action = action;
            ThoughtState = thoughtState;
            TargetCell = targetCell;
            TargetUnit = targetUnit;
            InformationSource = informationSource;
        }
    }
}