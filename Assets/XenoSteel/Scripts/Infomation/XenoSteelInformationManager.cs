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

        public void SetRecognitionPending(
            object enemy,
            Vector3 position,
            int round,
            int turnIndex)
        {
            if (_enemyInformations.TryGetValue(enemy, out var information))
            {
                information.SetRecognitionPending(
                    round,
                    turnIndex
                );
                return;
            }

            _enemyInformations.Add(
                enemy,
                new XenoSteelEnemyInformation(
                    XenoSteelEnemyInformationState.RecognitionPending,
                    XenoSteelInformationSource.None,
                    position,
                    round));

            _enemyInformations[enemy].SetRecognitionPending(
                round,
                turnIndex
            );
        }

        public void ConfirmEnemy(
            object enemy,
            XenoSteelInformationSource source,
            Vector3 position,
            int round)
        {
            if (_enemyInformations.TryGetValue(enemy, out var information))
            {
                information.UpdateInformation(source, position, round);
                return;
            }

            _enemyInformations.Add(
                enemy,
                new XenoSteelEnemyInformation(
                    XenoSteelEnemyInformationState.Confirmed,
                    source,
                    position,
                    round));
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

        public void ConfirmPendingEnemy(
            object enemy,
            Vector3 position,
            int round)
        {
            if (!_enemyInformations.TryGetValue(enemy, out var information))
                return;

            if (information.State != XenoSteelEnemyInformationState.RecognitionPending)
                return;

            information.UpdateInformation(
                XenoSteelInformationSource.Visual,
                position,
                round
            );
        }

        public void ConfirmRadarEnemy(
            IUnit enemy,
            Vector3 position,
            int round,
            int informationPrecision)
        {
            if (enemy == null)
                return;

            if (!_enemyInformations.TryGetValue(enemy, out var information))
            {
                information = new XenoSteelEnemyInformation(
                XenoSteelEnemyInformationState.Confirmed,
                XenoSteelInformationSource.Radar,
                position,
                round
            );
                _enemyInformations.Add(enemy, information);
            }

            information.SetRadarInformation(
                position,
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