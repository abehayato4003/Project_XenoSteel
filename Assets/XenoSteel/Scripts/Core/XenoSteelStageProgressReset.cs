using UnityEngine;

namespace XenoSteel.Core
{
    public class XenoSteelStageProgressReset : MonoBehaviour
    {
        private void Awake()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("PlayerPrefsをすべて削除しました。");

            Destroy(gameObject);
        }
    }
}