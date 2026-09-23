using UnityEngine;

namespace XenoSteel.Information
{
    /// <summary>
    /// プレイヤーが取得した1体の敵情報。
    /// 実際の敵ユニットそのものではなく、
    /// プレイヤー側が保持している情報を表す。
    /// </summary>
    [System.Serializable]
    public class XenoSteelEnemyInformation
    {
        public XenoSteelEnemyInformationState State { get; private set; }

        public XenoSteelInformationSource Source { get; private set; }

        public Vector3 LastKnownPosition { get; private set; }

        public int LastUpdatedRound { get; private set; }

        public int RecognitionPendingRound { get; private set; }

        public int RecognitionPendingTurnIndex { get; private set; }

        public XenoSteelEnemyInformation(
            XenoSteelEnemyInformationState state,
            XenoSteelInformationSource source,
            Vector3 position,
            int round)
        {
            State = state;
            Source = source;
            LastKnownPosition = position;
            LastUpdatedRound = round;
        }

        public void UpdateInformation(
            XenoSteelInformationSource source,
            Vector3 position,
            int round)
        {
            State = XenoSteelEnemyInformationState.Confirmed;
            Source = source;
            LastKnownPosition = position;
            LastUpdatedRound = round;
        }

        public void SetRecognitionPending(
            int round,
            int turnIndex)
        {
            State = XenoSteelEnemyInformationState.RecognitionPending;
            RecognitionPendingRound = round;
            RecognitionPendingTurnIndex = turnIndex;
        }
    }
}