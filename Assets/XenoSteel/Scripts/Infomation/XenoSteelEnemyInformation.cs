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

        public Vector2Int LastKnownCell { get; private set; }

        public int LastUpdatedRound { get; private set; }

        public int InformationPrecision { get; private set; }

        public XenoSteelEnemyInformation(
            XenoSteelEnemyInformationState state,
            XenoSteelInformationSource source,
            Vector2Int cell,
            int round)
        {
            State = state;
            Source = source;
            LastKnownCell = cell;
            LastUpdatedRound = round;
        }

        public void UpdateInformation(
            XenoSteelInformationSource source,
            Vector2Int cell,
            int round)
        {
            State = XenoSteelEnemyInformationState.Confirmed;
            Source = source;
            LastKnownCell = cell;
            LastUpdatedRound = round;
        }

        public void SetRadarInformation(
            Vector2Int cell,
            int round,
            int informationPrecision)
        {
            State = XenoSteelEnemyInformationState.Confirmed;
            Source = XenoSteelInformationSource.Radar;
            LastKnownCell = cell;
            LastUpdatedRound = round;
            InformationPrecision = informationPrecision;
        }

        public void SetLastKnown()
        {
            if (Source != XenoSteelInformationSource.Visual)
                return;

            State = XenoSteelEnemyInformationState.LastKnown;
        }
    }
}