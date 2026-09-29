using System.Collections.Generic;
using UnityEngine;

using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    /// <summary>
    /// プレイヤーが取得した敵情報を管理する。
    /// 視認・レーダーなど、情報取得方法そのものとは分離して管理する。
    /// </summary>
    public class XenoSteelInformationManager : MonoBehaviour
    {
        private readonly Dictionary<object, XenoSteelEnemyInformation> _enemyInformations
            = new Dictionary<object, XenoSteelEnemyInformation>();

        public void ConfirmEnemy(
            object enemy,
            XenoSteelInformationSource source,
            Vector2Int cell,
            int round)
        {
            if (_enemyInformations.TryGetValue(enemy, out var information))
            {
                information.UpdateInformation(source, cell, round);
            }
            else
            {
                _enemyInformations.Add(
                    enemy,
                    new XenoSteelEnemyInformation(
                        XenoSteelEnemyInformationState.Confirmed,
                        source,
                        cell,
                        round));
            }

            Debug.Log(
                $"Visual Confirm: " +
                $"Enemy={enemy}, " +
                $"Source={source}, " +
                $"Cell={cell}, " +
                $"Round={round}"
            );
        }

        public bool TryGetInformation(
            object enemy,
            out XenoSteelEnemyInformation information)
        {
            return _enemyInformations.TryGetValue(enemy, out information);
        }

        public IReadOnlyDictionary<object, XenoSteelEnemyInformation> GetAllInformations()
        {
            return _enemyInformations;
        }

        public void ConfirmRadarEnemy(
            IUnit enemy,
            Vector2Int cell,
            int round,
            int informationPrecision)
        {
            if (enemy == null)
                return;

            if (!_enemyInformations.TryGetValue(
                enemy,
                out var information))
            {
                information = new XenoSteelEnemyInformation(
                    XenoSteelEnemyInformationState.Confirmed,
                    XenoSteelInformationSource.Radar,
                    cell,
                    round
                );

                _enemyInformations.Add(enemy, information);
            }
            else
            {
                // Visualで確認済みなら、
                // Radar情報で上書きしない。
                if (information.Source ==
                    XenoSteelInformationSource.Visual &&
                    information.State !=
                    XenoSteelEnemyInformationState.LastKnown)
                {
                    return;
                }

                // Radar同士の場合は、
                // より高い精度の情報を維持する。
                if (information.InformationPrecision >
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
                $"Radar Detection: " +
                $"Enemy={enemy.UnitID}, " +
                $"Precision={informationPrecision}, " +
                $"Round={round}"
            );
        }
    }
}