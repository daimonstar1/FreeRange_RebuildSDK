using UnityEngine;

[AddComponentMenu("FX/Scrolling Texture")]
public class ScrollingTexture : MonoBehaviour {

    string textureName = "_MainTex";
    public float uRate = 0f;
    public float vRate = 0f;

    Renderer rend;
    Vector2 uv = Vector2.zero;

    void Awake() {

        rend = GetComponent<Renderer>();
        if(rend == null) {
            Debug.LogWarning("ScrollingTexture Awake abort because no Renderer found.");
            return;
        }
        //block = new MaterialPropertyBlock();
        //rend.GetPropertyBlock(block);
        uv = rend.sharedMaterial.GetTextureOffset(textureName);
    }


    void Update() {
        if(rend == null) return;

        uv.x += uRate * Time.deltaTime;
        uv.y += vRate * Time.deltaTime;
        rend.material.SetTextureOffset(textureName, uv);

        //rend.GetPropertyBlock(block);
        //block.SetTexture(textureID, uv);
        //block.SetFloat(propertyID, uv.x);
        //block.SetFloat(propertyID, uv.y);
        //rend.SetPropertyBlock(block);
    }
}