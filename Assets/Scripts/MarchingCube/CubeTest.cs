using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    [SerializeField] int cubeCount;
    [SerializeField] int cubeHeightCount;
    
    [SerializeField] Bean.MC.Cube prefab;
    
    private Bean.MC.Cube[,,] arrCube;

    private float[,] arrWaveVal;
    private int[,] arrWaveDir;

    [SerializeField] float minWaveHeight;
    [SerializeField] float maxWaveHeight;
    [SerializeField] float waveSpeed;
    
    private void Awake()
    {
        arrCube = new Bean.MC.Cube[cubeCount, cubeCount, cubeHeightCount ];
        
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
                    
                    cube.ScalarVal[0] = GetWaveHeight(x, z + 1, y);
                    cube.ScalarVal[1] = GetWaveHeight(x + 1, z + 1, y);
                    cube.ScalarVal[2] = GetWaveHeight(x + 1, z, y);
                    cube.ScalarVal[3] = GetWaveHeight(x, z , y);
                    
                    cube.ScalarVal[4] = GetWaveHeight(x, z + 1, y + 1);
                    cube.ScalarVal[5] = GetWaveHeight(x + 1, z + 1, y + 1);
                    cube.ScalarVal[6] = GetWaveHeight(x + 1, z, y + 1);
                    cube.ScalarVal[7] = GetWaveHeight(x, z, y + 1);
                    cube.CalcIsoSurface();
                }
            }
        }
    }

    private float GetWaveHeight(int x, int z, int y)
    {
        float height = arrWaveVal[x, z] - y;
        return Mathf.Clamp01(height);
    }
}
