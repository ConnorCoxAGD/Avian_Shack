using UnityEngine;

public class DrawMapBehaviour : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3 (texture.width, 1, texture.height);
    }

    public void DrawMeshShape(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh ();
        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapPreview>().meshSettings.meshScale;
    }
}
