using System.Collections.Generic;

using UnityEngine;

using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    /// <summary>
    /// 敵軍が取得したプレイヤー情報を管理する。
    /// 視認・レーダーなど、情報取得方法そのものとは分離して管理する。
    /// 敵軍全体で情報を共有する。
    /// </summary>
    public class XenoSteelEnemyInformationManager : MonoBehaviour
    {
        private readonly Dictionary<object, XenoSteelEnemyInformation> _playerInformations
            = new Dictionary<object, XenoSteelEnemyInformation>();

        public void ConfirmPlayer(
            object player,
            XenoSteelInformationSource source,
            Vector2Int cell,
            int round)
        {
            if (_playerInformations.TryGetValue(
                player,
                out var information))
            {
                information.UpdateInformation(
                    source,
                    cell,
                    round);
            }
            else
            {
                _playerInformations.Add(
                    player,
                    new XenoSteelEnemyInformation(
                        XenoSteelEnemyInformationState.Confirmed,
                        source,
                        cell,
                        round));
            }

            Debug.Log(
                $"Enemy Visual Confirm: " +
                $"Player={player}, " +
                $"Source={source}, " +
                $"Cell={cell}, " +
                $"Round={round}"
            );
        }

        public bool TryGetInformation(
            object player,
            out XenoSteelEnemyInformation information)
        {
            return _playerInformations.TryGetValue(
                player,
                out information);
        }

        public IReadOnlyDictionary<object, XenoSteelEnemyInformation>
            GetAllInformations()
        {
            return _playerInformations;
        }

        public void ConfirmRadarPlayer(
            IUnit player,
            Vector2Int cell,
            int round,
            int informationPrecision)
        {
            if (player == null)
                return;

            if (!_playerInformations.TryGetValue(
                player,
                out var information))
            {
                information = new XenoSteelEnemyInformation(
                    XenoSteelEnemyInformationState.Confirmed,
                    XenoSteelInformationSource.Radar,
                    cell,
                    round
                );

                _playerInformations.Add(
                    player,
                    information);
            }
            else
            {
                // Visualで現在確認できているPlayerは、
                // Radar情報で上書きしない。
                if (information.Source ==
                    XenoSteelInformationSource.Visual &&
                    information.State !=
                    XenoSteelEnemyInformationState.LastKnown)
                {
                    return;
                }

                // LastKnownになったPlayerはRadarで再検出できる。
                // この場合は精度比較を行わず、
                // Radar情報で更新する。
                // Radar同士の場合だけ、
                // より高い精度の情報を維持する。
                if (information.State ==
                    XenoSteelEnemyInformationState.Confirmed &&
                    information.Source ==
                    XenoSteelInformationSource.Radar &&
                    information.InformationPrecision >
                    informationPrecision)
                {
                    return;
                }
            }

            information.SetRadarInformation(
                cell,
                round,
                informationPrecision
            );

            Debug.Log(
                $"Enemy Radar Detection: " +
                $"Player={player.UnitID}, " +
                $"Precision={informationPrecision}"
            );
        }

        public void RemoveExpiredInformations(int currentRound)
        {
            List<object> expiredPlayers =
                new List<object>();

            foreach (
                KeyValuePair<object, XenoSteelEnemyInformation> pair
                in _playerInformations)
            {
                XenoSteelEnemyInformation information =
                    pair.Value;

                if (information == null)
                    continue;

                // 最終更新から3ラウンド経過した情報を削除
                if (currentRound - information.LastUpdatedRound >= 3)
                {
                    expiredPlayers.Add(pair.Key);
                }
            }

            foreach (object player in expiredPlayers)
            {
                _playerInformations.Remove(player);

                Debug.Log(
                    $"Player Information Expired: Player={player}"
                );
            }
        }

        private readonly HashSet<Vector2Int> _observedCells =
            new HashSet<Vector2Int>();

        public void RegisterObservedCells(
            IEnumerable<Vector2Int> cells)
        {
            foreach (Vector2Int cell in cells)
            {
                _observedCells.Add(cell);
            }
        }

        public bool IsObserved(Vector2Int cell)
        {
            return _observedCells.Contains(cell);
        }

        public void RemovePlayerInformation(IUnit player)
        {
            if (player == null)
                return;

            _playerInformations.Remove(player);
        }
    }
}