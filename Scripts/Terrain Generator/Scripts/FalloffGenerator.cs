using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public enum IslandMode
    {
        Diagonal,
        Euclidean,
        Manhattan,
    };

    public static IslandMode islandMode;
    
    public static float[,] GenerateFalloffMap(int size, IslandMode islandMode)
    {
        float[,] falloffMap = new float[size,size];
        
        switch (islandMode)
        {
            case IslandMode.Diagonal:
                for (int i = 0; i < size; i++){
                    for (int j = 0; j < size; j++)
                    {
                        float x = (i / (float) size * 2 - 1);
                        float y = (j / (float) size * 2 - 1);

                        //find out which is closest to edge
                        var value = 2* Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                        falloffMap[i, j] = Evaluate(value);
                    }
                }
                break;
            
            case IslandMode.Euclidean:
                for (int i = 0; i < size; i++){
                    for (int j = 0; j < size; j++)
                    {
                        float x = (i / (float) size * 2 - 1);
                        float y = (j / (float) size * 2 - 1);

                        //find out which is closest to edge
                        var value = Mathf.Sqrt(x * x + y * y) / Mathf.Sqrt(0.5f);
                
                        falloffMap[i, j] = Evaluate(value);
                    }
                }
                break;
            
            case IslandMode.Manhattan:
                for (int i = 0; i < size; i++){
                    for (int j = 0; j < size; j++)
                    {
                        float x = (i / (float) size * 2 - 1);
                        float y = (j / (float) size * 2 - 1);

                        //find out which is closest to edge
                        var value = Mathf.Abs(x) + Mathf.Abs(y);
                
                        falloffMap[i, j] = Evaluate(value);
                    }
                }
                break;
        }

        return falloffMap;
    }
    
    

    static float Evaluate(float value)
    {
        var a = 7f;
        var b = 2f;

        return Mathf.Pow(value, a)/ (Mathf.Pow(value, b));
    }
}
