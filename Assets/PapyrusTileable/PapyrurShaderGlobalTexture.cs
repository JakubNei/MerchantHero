using UnityEngine;

[ExecuteInEditMode]
public class PapyrurShaderGlobalTexture : MonoBehaviour
{

    public Texture2D configPapyrus;

    void Update()
    {
        Shader.SetGlobalTexture("_PapyrusTex", configPapyrus);
    }
}
