using UnityEngine;

namespace XenoSteel.Core
{
    [CreateAssetMenu(
        fileName = "StageData",
        menuName = "XenoSteel/Stage Data"
    )]
    public class StageData : ScriptableObject
    {
        [Header("Stage Information")]
        public string stageId;
        public string stageName;
        public int stageNumber;

        [Header("Battle Scene")]
        public string battleSceneName;

        [Header("Stage Progression")]
        public string nextStageId;
    }
}