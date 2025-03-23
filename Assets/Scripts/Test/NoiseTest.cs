using System;
using System.Collections;
using System.Collections.Generic;
using Bean.MC;
using Bean.Noise;
using Define;
using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    private PerlinNoise noise;
    private CubeGenerator generator;

    [SerializeField] private int axisXCount;
    [SerializeField] private int axisYCount;
    [SerializeField] private int axisZCount;

    [SerializeField] private Cube prefab;

    [SerializeField] private float isoLevel;
    [SerializeField] private float cubeSize;
    private void Awake()
    {
        MarchingCubes.IsoLevel = isoLevel;
        MarchingCubes.CubeSize = cubeSize;

        int gridXCount = Mathf.CeilToInt(axisXCount * cubeSize);
        int gridZCount = Mathf.CeilToInt(axisZCount * cubeSize);

        noise = new PerlinNoise(gridXCount, gridZCount, 1f);
        noise.Init();
        generator = new CubeGenerator(axisXCount, axisZCount, axisYCount, prefab);
        generator.Init();

        for (int x = 0; x < axisXCount; ++x)
        {
            for (int z = 0; z < axisZCount; ++z)
            {
                SetHeight(x, z);
            }
        }
        
        generator.CalcMeshes();
    }
    
    private void SetHeight(int x, int z)
    {
        float height = noise.GetNoise(x * cubeSize, z * cubeSize);
        // height가 -1 ~ 1 이므로, 1을 더해서 0 ~ 2 로 만든 뒤에 2로 나눠 0 ~ 1로 만든다.
        height = (height + 1f) * 0.5f;
        height *= axisYCount;
        
        for (int y = 0; y < axisYCount; ++y)
        {
            generator.SetScalar(x, z, y, Mathf.Clamp01(height - y));
        }
    }
}
