using UnityEngine;

[CreateAssetMenu]
public class NoiseMapSettings : UpdatableData {

    public NoiseSettings noiseSettings;

    public bool isIsland;
    public FalloffGenerator.IslandMode islandMode;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight {
        get {
            return heightMultiplier * heightCurve.Evaluate (0);
        }
    }

    public float maxHeight {
        get {
            return heightMultiplier * heightCurve.Evaluate (1);
        }
    }

#if UNITY_EDITOR

    protected override void OnValidate() {
        noiseSettings.ValidateValues ();
        base.OnValidate ();
    }
#endif

}