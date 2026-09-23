using System.Collections.Generic;
using UnityEngine;

namespace XenoSteel.Core
{
    public class XenoSteelStageSelect : MonoBehaviour
    {
        [SerializeField] private List<StageData> _stages = new List<StageData>();
        [SerializeField] private XenoSteelStageSelectItem _stageItemPrefab;
        [SerializeField] private Transform _stageList;

        private void Start()
        {
            XenoSteelStageProgress.UpdateUnlockedStages(_stages);
            CreateStageItems();
        }

        private void CreateStageItems()
        {
            if (_stageItemPrefab == null)
            {
                Debug.LogError("Stage Item Prefabが設定されていません。");
                return;
            }

            if (_stageList == null)
            {
                Debug.LogError("Stage Listが設定されていません。");
                return;
            }

            foreach (StageData stageData in _stages)
            {
                if (stageData == null)
                {
                    continue;
                }

                XenoSteelStageSelectItem item =
                    Instantiate(_stageItemPrefab, _stageList);

                item.Initialize(stageData);
            }
        }
    }
}