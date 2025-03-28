using System;
using System.Collections;
using System.Collections.Generic;
using Bean.MC;
using Bean.Noise;
using Define;
using UnityEngine;

public class CollisionTest : MarchingCubesTestBase
{
    private PerlinNoise noise;
    private CubeGenerator generator;

    [SerializeField] private int UpdateTick = 1;
    
    [SerializeField] private float moveSpeed;

    [SerializeField] private int noiseGridScale;
    
    [SerializeField] private float sphereRadius;

    private bool isCircleClicked;
    private bool isSphereClicked;
    private Ray ray;

    /// <summary>
    /// 맵의 정보인 스칼라 필드
    /// </summary>
    private float[,,] scalarField;

    private bool isDirty;
    
    private void Awake()
    {
        MarchingCubes.IsoLevel = isoLevel;
        MarchingCubes.CubeSize = cubeSize;

        isCircleClicked = false;
        isSphereClicked = false;
        isDirty = false;

        int gridXCount = Mathf.CeilToInt(axisXCount * cubeSize) * noiseGridScale;
        int gridZCount = Mathf.CeilToInt(axisZCount * cubeSize) * noiseGridScale;
        
        noise = new PerlinNoise(gridXCount, gridZCount, 1f);
        noise.Init();
        
        scalarField = new float[axisXCount + 1, axisZCount + 1, axisYCount + 1];

        // 노이즈로 얻은 값을 스칼라 필드에 넣는다.
        for (int x = 0; x < axisXCount; ++x)
        {
            for (int z = 0; z < axisZCount; ++z)
            {
                float height = noise.GetNoise(x * cubeSize, z * cubeSize);
                // height가 -1 ~ 1 이므로, 1을 더해서 0 ~ 2 로 만든 뒤에 2로 나눠 0 ~ 1로 만든다.
                height = (height + 1f) * 0.5f;
                height *= axisYCount;
        
                for (int y = 0; y < axisYCount; ++y)
                {
                    scalarField[x, z, y] = Mathf.Clamp01(height - y);
                }
            }
        }
        
        generator = new CubeGenerator(axisXCount, axisZCount, axisYCount, prefab, 1, transform);
        generator.Init(CalcSubMeshIndex);

        UpdateScalarField();
        
        generator.UpdateMeshes();
        isDirty = true;
    }

    private void FixedUpdate()
    {
        if (isSphereClicked)
        {
            isSphereClicked = false;
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                isDirty = true;
                
                int gridCount = (int)(sphereRadius / cubeSize) + 1;
                int currX = (int)(hitInfo.point.x / cubeSize);
                int currZ = (int)(hitInfo.point.z / cubeSize);
                int currY = (int)(hitInfo.point.y / cubeSize);
                int minX = Mathf.Max(currX - gridCount, 0);
                int maxX = Mathf.Min(currX + gridCount, axisXCount);
                int minZ = Mathf.Max(currZ - gridCount, 0);
                int maxZ = Mathf.Min(currZ + gridCount, axisZCount);
                int minY = Mathf.Max(currY - gridCount, 0);
                int maxY = Mathf.Min(currY + gridCount, axisYCount);

                Vector3 point = hitInfo.point;

                for (int x = minX; x <= maxX; ++x)
                {
                    for (int z = minZ; z <= maxZ; ++z)
                    {
                        for (int y = minY; y <= maxY; ++y)
                        {
                            float scalar = scalarField[x, z, y];
                            if (scalar <= Single.Epsilon) continue;
                            Vector3 cubePos = generator.GetCubePosition(x, z, y);
                            float dist = (point - cubePos).magnitude;
                            if (dist > sphereRadius) continue;

                            scalar -= sphereRadius - dist;

                            scalarField[x, z, y] = scalar;
                        }
                    }
                }
            }
        }

        if (isDirty == false) return;
        
        UpdateScalarField();
        
        generator.UpdateMeshes();
        isDirty = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCircleClicked = true;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            isSphereClicked = true;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        #if UNITY_EDITOR
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            generator.SetEnableMeshRenderer(!generator.IsMeshRendererEnabled);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            generator.SetEnableMeshCollider(!generator.IsMeshColliderEnabled);
        }
        
        #endif
    }

    /// <summary>
    /// 스칼라 필드에 있는 값을 제너레이터에 넣는다.
    /// </summary>
    private void UpdateScalarField()
    {
        for (int x = 0; x < axisXCount; ++x)
        {
            for (int z = 0; z < axisZCount; ++z)
            {
                for (int y = 0; y < axisYCount; ++y)
                {
                    generator.SetScalar(x, z, y, scalarField[x, z, y]);
                }
            }
        }
    }
    
    private int CalcSubMeshIndex(Vector3 position)
    {
        float height = axisYCount * cubeSize * 0.55f;
        if (position.y >= height) return 1;
        return 0;
    }
}
