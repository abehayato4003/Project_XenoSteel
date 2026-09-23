using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace XenoSteel.Core
{
    public class XenoSteelStageSelectItem : MonoBehaviour
    {
        [SerializeField] private StageData _stageData;
        [SerializeField] private Button _stageButton;
        [SerializeField] private TMP_Text _stageText;
        [SerializeField] private TMP_Text _statusText;

        public void Initialize(StageData stageData)
        {
            _stageData = stageData;

            if (_stageButton == null)
            {
                Debug.LogError("Stage Buttonが設定されていません。");
                return;
            }

            _stageButton.onClick.RemoveAllListeners();
            _stageButton.onClick.AddListener(StartStage);

            UpdateStageButton();
        }

        private void UpdateStageButton()
        {
            if (_stageData == null)
            {
                Debug.LogError("StageDataが設定されていません。");
                return;
            }

            bool unlocked =
                XenoSteelStageProgress.IsStageUnlocked(_stageData);

            bool cleared =
                XenoSteelStageProgress.IsStageCleared(_stageData.stageId);

            _stageButton.interactable = unlocked;

            if (_stageText != null)
            {
                _stageText.text = _stageData.stageName;
            }

            if (_statusText != null)
            {
                if (cleared)
                {
                    _statusText.text = "CLEAR";
                }
                else if (unlocked)
                {
                    _statusText.text = "";
                }
                else
                {
                    _statusText.text = "LOCKED";
                }
            }
        }

        private void StartStage()
        {
            if (_stageData == null)
            {
                Debug.LogError("StageDataが設定されていません。");
                return;
            }

            if (!XenoSteelStageProgress.IsStageUnlocked(_stageData))
            {
                return;
            }

            if (string.IsNullOrEmpty(_stageData.battleSceneName))
            {
                Debug.LogError("Battle Scene Nameが設定されていません。");
                return;
            }

            SceneManager.LoadScene(_stageData.battleSceneName);
        }
    }
}