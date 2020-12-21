using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;


	public enum DrawMode {HeightMap, OceanMap, Mesh, FalloffMap};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public NoiseMapSettings noiseMapSettings, oceanMapSettings;


	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;




	public void DrawMapInEditor()
	{
		var noiseMap = NoiseMapGenerator.GenerateNoiseMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, noiseMapSettings, Vector2.zero);
		var moistureMap = NoiseMapGenerator.GenerateNoiseMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, oceanMapSettings, Vector2.zero);

		switch (drawMode)
		{
			case DrawMode.HeightMap:
				DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
				break;
			case DrawMode.OceanMap:
				DrawTexture (TextureGenerator.TextureFromHeightMap (moistureMap));
				break;
			case DrawMode.Mesh:
				DrawMesh (MeshGenerator.GenerateTerrainMesh (noiseMap.values, moistureMap.values, meshSettings, editorPreviewLOD));
				break;
			case DrawMode.FalloffMap:
				DrawTexture(TextureGenerator.TextureFromHeightMap(new NoiseMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine, noiseMapSettings.islandMode),0,1)));
				break;
		}
	}

	public void DrawTexture(Texture2D texture) 
	{
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

		textureRender.gameObject.SetActive (true);
		meshFilter.gameObject.SetActive (false);
	}

	public void DrawMesh(MeshData meshData) 
	{
		meshFilter.sharedMesh = meshData.CreateMesh ();

		textureRender.gameObject.SetActive (false);
		meshFilter.gameObject.SetActive (true);
	}

	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor ();
		}
	}	

	void OnValidate() {

		if (meshSettings != null) {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (noiseMapSettings != null) {
			noiseMapSettings.OnValuesUpdated -= OnValuesUpdated;
			noiseMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (oceanMapSettings != null) {
			oceanMapSettings.OnValuesUpdated -= OnValuesUpdated;
			oceanMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
	}
}