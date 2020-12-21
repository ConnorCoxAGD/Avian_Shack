using UnityEngine;

public class TerrainChunk {
	
	const float colliderGenerationDistanceThreshold = 5;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	 
	GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	LODInfo[] detailLevels;
	LODMesh[] lodMeshes;
	int colliderLODIndex;

	NoiseMap _noiseMap, _moistureMap;
	bool heightMapReceived, moistureMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider;
	float maxViewDst;

	private NoiseMapSettings _noiseMapSettings, _oceanMapSettings;
	MeshSettings meshSettings;
	Transform player;

	public TerrainChunk(Vector2 coord, NoiseMapSettings noiseMapSettings, NoiseMapSettings oceanMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform player, Material material) {
		this.coord = coord;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		_noiseMapSettings = noiseMapSettings;
		_oceanMapSettings = oceanMapSettings;
		this.meshSettings = meshSettings;
		this.player = player;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );


		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			lodMeshes[i] = new LODMesh(detailLevels[i].lod);
			lodMeshes[i].updateCallback += UpdateTerrainChunk;
			if (i == colliderLODIndex) {
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
			}
		}

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;

	}

	public void Load()
	{
		ThreadedDataRequester.RequestData(
			() => NoiseMapGenerator.GenerateNoiseMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine,
				_noiseMapSettings, sampleCentre), OnHeightMapReceived);
		ThreadedDataRequester.RequestData(() => NoiseMapGenerator.GenerateNoiseMap (meshSettings.numVertsPerLine, 
			meshSettings.numVertsPerLine, _oceanMapSettings, sampleCentre), OnMoistureMapReceived);
	}

	void OnMoistureMapReceived(object moistureMapObject)
	{
		_moistureMap = (NoiseMap) moistureMapObject;
		moistureMapReceived = true;
	}

	void OnHeightMapReceived(object heightMapObject) 
	{
		_noiseMap = (NoiseMap)heightMapObject;
		heightMapReceived = true;

		UpdateTerrainChunk ();
	}

	Vector2 playerPosition {
		get {
			return new Vector2 (player.position.x, player.position.z);
		}
	}


	public void UpdateTerrainChunk() {
		if (heightMapReceived && moistureMapReceived) 
		{
			float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (playerPosition));

			bool wasVisible = IsVisible ();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			if (visible) {
				int lodIndex = 0;

				for (int i = 0; i < detailLevels.Length - 1; i++) {
					if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
						lodIndex = i + 1;
					} else {
						break;
					}
				}

				if (lodIndex != previousLODIndex) {
					LODMesh lodMesh = lodMeshes [lodIndex];
					if (lodMesh.hasMesh) {
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					} else if (!lodMesh.hasRequestedMesh) {
						lodMesh.RequestMesh (_noiseMap, _moistureMap, meshSettings);
					}
				}


			}

			if (wasVisible != visible) {
				
				SetVisible (visible);
				if (onVisibilityChanged != null) {
					onVisibilityChanged (this, visible);
				}
			}
		}
	}

	public void UpdateCollisionMesh() {
		if (!hasSetCollider) {
			float sqrDstFromViewerToEdge = bounds.SqrDistance (playerPosition);

			if (sqrDstFromViewerToEdge < detailLevels [colliderLODIndex].sqrVisibleDstThreshold) {
				if (!lodMeshes [colliderLODIndex].hasRequestedMesh) {
					lodMeshes [colliderLODIndex].RequestMesh (_noiseMap, _moistureMap, meshSettings);
				}
			}

			if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
				if (lodMeshes [colliderLODIndex].hasMesh) {
					meshCollider.sharedMesh = lodMeshes [colliderLODIndex].mesh;
					hasSetCollider = true;
				}
			}
		}
	}

	public void SetVisible(bool visible) {
		meshObject.SetActive (visible);
	}

	public bool IsVisible() {
		return meshObject.activeSelf;
	}

}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	readonly int lod;
	public event System.Action updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
		hasMesh = true;

		updateCallback ();
	}

	public void RequestMesh(NoiseMap noiseMap, NoiseMap moistureMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData (() => MeshGenerator.GenerateTerrainMesh (noiseMap.values, moistureMap.values,  meshSettings, lod), OnMeshDataReceived);
	}

}