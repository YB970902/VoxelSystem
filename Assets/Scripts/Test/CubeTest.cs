using System;
using System.Collections;
using System.Collections.Generic;
using Bean.MC;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    [SerializeField] int cubeCount;
    [SerializeField] int cubeHeightCount;
    
    [SerializeField] Cube prefab;
    
    private float[,] arrWaveVal;
    private int[,] arrWaveDir;

    [SerializeField] float minWaveHeight;
    [SerializeField] float maxWaveHeight;
    [SerializeField] float waveSpeed;
    
    private CubeGenerator cubeGenerator;
    
    private void Awake()
    {
        arrWaveVal = new float[cubeCount + 1, cubeCount + 1];
        arrWaveDir = new int[cubeCount + 1, cubeCount + 1];

        float curWave = minWaveHeight;
        int curDir = 1;
        
        for (int x = 0; x < cubeCount + 1; ++x)
        {
            for (int z = 0; z < cubeCount + 1; ++z)
            {
                arrWaveVal[x,z] = curWave;
                arrWaveDir[x,z] = curDir;
            }
            
            curWave += waveSpeed * curDir;
            if (curWave > maxWaveHeight)
            {
                curWave = maxWaveHeight;
                curDir = -1;
            }
            else if (curWave < minWaveHeight)
            {
                curWave = minWaveHeight;
                curDir = 1;
            }
        }
        
        cubeGenerator = new CubeGenerator(cubeCount, cubeCount, cubeHeightCount, prefab);
        cubeGenerator.Init();
    }

    private void Update()
    {
        for (int x = 0; x < cubeCount + 1; ++x)
        {
            for (int z = 0; z < cubeCount + 1; ++z)
            {
                float value = arrWaveVal[x,z];
                int dir = arrWaveDir[x,z];
                value += waveSpeed * dir;
                if (value > maxWaveHeight)
                {
                    value = maxWaveHeight;
                    dir = -1;
                }
                else if (value < minWaveHeight)
                {
                    value = minWaveHeight;
                    dir = 1;
                }
            
                arrWaveVal[x,z] = value;
                arrWaveDir[x,z] = dir;

                SetWaveHeight(x, z);
            }
        }
        
        cubeGenerator.CalcMeshes();
    }

    private void SetWaveHeight(int x, int z)
    {
        for (int y = 0; y < cubeHeightCount; ++y)
        {
            cubeGenerator.SetScalar(x, z, y, Mathf.Clamp01(arrWaveVal[x, z] - y));
        }
    }
}
