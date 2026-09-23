using UnityEngine;
using UnityEngine.SceneManagement;

using TurnBasedStrategyFramework.Common.Controllers.GameResolvers;
using TurnBasedStrategyFramework.Unity.Controllers;

namespace XenoSteel.Core
{
    public class XenoSteelGameResultHandler : MonoBehaviour
    {
        [SerializeField] private UnityGridController _gridController;
        [SerializeField] private StageData _stageData;

        private void OnEnable()
        {
            Debug.Log("XenoSteelGameResultHandler: OnEnable");

            if (_gridController != null)
            {
                _gridController.GameEnded += HandleGameEnded;
                Debug.Log("XenoSteelGameResultHandler: GameEndedを購読しました。");
            }
            else
            {
                Debug.LogError("XenoSteelGameResultHandler: GridControllerが設定されていません。");
            }
        }

        private void OnDisable()
        {
            if (_gridController != null)
            {
                _gridController.GameEnded -= HandleGameEnded;
            }
        }

        private void HandleGameEnded(GameResult gameResult)
        {
            Debug.Log("XenoSteelGameResultHandler: HandleGameEndedが呼ばれました。");


            
            if (_stageData == null)
            {
                Debug.LogError("StageDataが設定されていません。");
                return;
            }

            foreach (var winner in gameResult.Winners)
            {
                if (winner.PlayerNumber == 0)
                {
                    XenoSteelStageProgress.ClearStage(_stageData);

                    Debug.Log(
                        $"Stage Clear: {_stageData.stageId}"
                    );

                    ReturnToStageSelect();
                    return;
                }
            }

            Debug.Log(
                $"Stage Defeat: {_stageData.stageId}"
            );

            ReturnToStageSelect();
        }

        private void ReturnToStageSelect()
        {
            Debug.Log("ReturnToStageSelect: StageSelectへ移動します。");
            SceneManager.LoadScene("StageSelect");
        }
    }
}