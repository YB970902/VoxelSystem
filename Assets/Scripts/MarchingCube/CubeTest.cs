using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    [SerializeField] int cubeCount;
    [SerializeField] int cubeHeightCount;
    
    [SerializeField] MarchingCube prefab;
    
    private MarchingCube[,,] arrCube;

    private float[,] arrWaveVal;
    private int[,] arrWaveDir;

    [SerializeField] float minWaveHeight;
    [SerializeField] float maxWaveHeight;
    [SerializeField] float waveSpeed;
    
    private void Awake()
    {
        arrCube = new MarchingCube[cubeCount, cubeCount, cubeHeightCount];
        
        for (int x = 0; x < cubeCount; ++x)
        {
            for (int z = 0; z < cubeCount; ++z)
            {
                for (int y = 0; y < cubeHeightCount; ++y)
                {
                    var cube = Instantiate(prefab);
                    cube.transform.position = new Vector3(x, y, z);
                    arrCube[x,z,y] = cube;
                }
            }
        }

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
            }
        }
        
        for (int x = 0; x < cubeCount; ++x)
        {
            for (int z = 0; z < cubeCount; ++z)
            {
                for (int y = 0; y < cubeHeightCount; ++y)
                {
                    var cube = arrCube[x, z, y];
                    
                    cube.GridVal[0] = GetLow(x, z, y);
                    cube.GridVal[1] = GetLow(x + 1, z, y);
                    cube.GridVal[2] = GetLow(x + 1, z + 1, y);
                    cube.GridVal[3] = GetLow(x, z + 1, y);
                    
                    cube.GridVal[4] = GetHigh(x, z, y + 1);
                    cube.GridVal[5] = GetHigh(x + 1, z, y + 1);
                    cube.GridVal[6] = GetHigh(x + 1, z + 1, y + 1);
                    cube.GridVal[7] = GetHigh(x, z + 1, y + 1);
                    cube.CalcIsoSurface();
                }
            }
        }
    }

    private float GetLow(int x, int z, int y)
    {
        float height = Mathf.Clamp(arrWaveVal[x, z], y, y + 1) - y;
        return Mathf.Clamp(height, 0f, 0.5f) * 2f;
    }
    
    private float GetHigh(int x, int z, int y)
    {
        float height = Mathf.Clamp(arrWaveVal[x, z], y, y + 1) - y;
        return Mathf.Clamp(height - 0.5f, 0f, 0.5f) * 2f;
    }
}
