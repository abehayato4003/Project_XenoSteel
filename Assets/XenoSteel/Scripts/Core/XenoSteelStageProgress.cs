using UnityEngine;
using System.Collections.Generic;
namespace XenoSteel.Core
{
    public static class XenoSteelStageProgress
    {
        private const string ClearedStageKey = "XenoSteel_ClearedStage_";
        private const string UnlockedStageKey = "XenoSteel_UnlockedStage_";

        public static bool IsStageCleared(string stageId)
        {
            return PlayerPrefs.GetInt(
                ClearedStageKey + stageId,
                0
            ) == 1;
        }

        public static void ClearStage(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }

            PlayerPrefs.SetInt(
                ClearedStageKey + stageData.stageId,
                1
            );

            if (!string.IsNullOrEmpty(stageData.nextStageId))
            {
                PlayerPrefs.SetInt(
                    UnlockedStageKey + stageData.nextStageId,
                    1
                );
            }

            PlayerPrefs.Save();
        }

        public static bool IsStageUnlocked(StageData stageData)
        {
            if (stageData == null)
            {
                return false;
            }

            // 最初のステージは常に解放
            if (stageData.stageNumber == 1)
            {
                return true;
            }

            return PlayerPrefs.GetInt(
                UnlockedStageKey + stageData.stageId,
                0
            ) == 1;
        }

        public static void UpdateUnlockedStages(
            List<StageData> stages
        )
        {
            if (stages == null)
            {
                return;
            }

            foreach (StageData stageData in stages)
            {
                if (stageData == null)
                {
                    continue;
                }

                if (!IsStageCleared(stageData.stageId))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(stageData.nextStageId))
                {
                    continue;
                }

                PlayerPrefs.SetInt(
                    UnlockedStageKey + stageData.nextStageId,
                    1
                );
            }

            PlayerPrefs.Save();
        }

        public static void ResetStageProgress()
        {
            PlayerPrefs.DeleteKey(
                ClearedStageKey + "STAGE_001"
            );

            PlayerPrefs.DeleteKey(
                UnlockedStageKey + "STAGE_002"
            );

            PlayerPrefs.Save();
        }
    }
}