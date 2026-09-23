using UnityEngine;

[ExecuteInEditMode]
public class SRpgCellTextureController : MonoBehaviour
{
    private static readonly int RandomOffsetID = Shader.PropertyToID("_RandomOffset");
    private static readonly int AtlasRectID = Shader.PropertyToID("_AtlasRect");
    private static readonly int TextureSizeID = Shader.PropertyToID("_TextureSize");

    [Header("保存されたランダムオフセット (配置時に自動固定)")]
    [SerializeField] private Vector2 savedOffset = Vector2.zero;

    [Header("アトラス設定（マテリアル側と同じ値を入れる）")]
    [SerializeField] private Vector4 atlasRectPixels = Vector4.zero; 
    [SerializeField] private float textureSize = 2048f;

    [SerializeField] private bool isInitialized = false;

    private Renderer _renderer;

    private void Awake()
    {
        ApplySavedOffset();
    }

    private void OnValidate()
    {
        ApplySavedOffset();
    }

    private void Update()
    {
        if (!isInitialized && !Application.isPlaying)
        {
            InitializeRandomOffset();
        }
    }

    private void InitializeRandomOffset()
    {
        // 素材のUVサイズを計算
        float widthUV  = atlasRectPixels.z / textureSize;
        float heightUV = atlasRectPixels.w / textureSize;

        // ランダムオフセットの最大値（素材サイズに応じて自動調整）
        float maxOffsetX = widthUV  * 0.25f;
        float maxOffsetY = heightUV * 0.25f;

        savedOffset = new Vector2(
            Random.Range(-maxOffsetX, maxOffsetX),
            Random.Range(-maxOffsetY, maxOffsetY)
        );

        isInitialized = true;
        ApplySavedOffset();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    private void ApplySavedOffset()
    {
        if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
        if (_renderer == null) return;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(propBlock);

        propBlock.SetVector(RandomOffsetID, savedOffset);
        propBlock.SetVector(AtlasRectID, atlasRectPixels);
        propBlock.SetFloat(TextureSizeID, textureSize);

        _renderer.SetPropertyBlock(propBlock);
    }

    [ContextMenu("Force Re-Randomize Offset")]
    public void ForceReRandomize()
    {
        isInitialized = false;
        InitializeRandomOffset();
    }
}
