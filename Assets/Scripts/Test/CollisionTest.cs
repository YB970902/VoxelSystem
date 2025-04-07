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

    [SerializeField] private int UpdateTick;
    
    [SerializeField] private float moveSpeed;

    [SerializeField] private int noiseGridScale;
    
    [SerializeField] private float sphereRadius;

    [SerializeField] private float digTime;
    [SerializeField] private float digPower;

    private float elapsedTime;
    private bool isClicked;
    private bool isDig;
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

        elapsedTime = 0f;
        isClicked = false;
        isDig = true;
        isDirty = false;

        int gridXCount = Mathf.CeilToInt(axisXCount * cubeSize) * noiseGridScale;
        int gridZCount = Mathf.CeilToInt(axisZCount * cubeSize) * noiseGridScale;
        
        // noise = new PerlinNoise(gridXCount, gridZCount, 1f);
        // noise.Init();
        
        scalarField = new float[axisXCount + 1, axisYCount + 1, axisZCount + 1];

        // 노이즈로 얻은 값을 스칼라 필드에 넣는다.
        // for (int x = 0; x < axisXCount; ++x)
        // {
        //     for (int z = 0; z < axisZCount; ++z)
        //     {
        //         float height = noise.GetNoise(x * cubeSize, z * cubeSize);
        //         // height가 -1 ~ 1 이므로, 1을 더해서 0 ~ 2 로 만든 뒤에 2로 나눠 0 ~ 1로 만든다.
        //         height = (height + 1f) * 0.5f;
        //         height *= axisYCount;
        //
        //         for (int y = 0; y < axisYCount; ++y)
        //         {
        //             scalarField[x, z, y] = Mathf.Clamp01(height - y);
        //         }
        //     }
        // }
        
        FastNoiseLite noise = new FastNoiseLite();

        // 노이즈 타입 설정 (예: Worley / fBM)
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S); // Worley
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);  // fBM
        noise.SetFrequency(0.05f); // 주파수 설정 (값이 작을수록 큰 패턴)

        generator = new CubeGenerator(axisXCount, axisYCount, axisZCount, UpdateTick, transform);
        generator.Init(CalcSubMeshIndex);

        for (int x = 0; x < axisXCount; ++x)
        {
            for (int y = 0; y < axisYCount; ++y)
            {
                for (int z = 0; z < axisZCount; ++z)
                {
                    float noiseValue = noise.GetNoise(x * cubeSize, y * cubeSize, z * cubeSize);
                    noiseValue = (noiseValue + 1.0f) * 0.5f;
                    scalarField[x, y, z] = noiseValue;
                }
            }
        }

        UpdateScalarField();
        
        generator.UpdateMeshes();
        isDirty = true;
    }

    private void FixedUpdate()
    {
        if (isClicked && elapsedTime >= digTime)
        {
            elapsedTime -= digTime;
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                isDirty = true;
                
                int gridCount = (int)(sphereRadius / cubeSize) + 1;
                int currX = (int)(hitInfo.point.x / cubeSize);
                int currY = (int)(hitInfo.point.y / cubeSize);
                int currZ = (int)(hitInfo.point.z / cubeSize);
                int minX = Mathf.Max(currX - gridCount, 0);
                int maxX = Mathf.Min(currX + gridCount, axisXCount - 1);
                int minY = Mathf.Max(currY - gridCount, 0);
                int maxY = Mathf.Min(currY + gridCount, axisYCount - 1);
                int minZ = Mathf.Max(currZ - gridCount, 0);
                int maxZ = Mathf.Min(currZ + gridCount, axisZCount - 1);

                Vector3 point = hitInfo.point;

                for (int x = minX; x <= maxX; ++x)
                {
                    for (int y = minY; y <= maxY; ++y)
                    {
                        for (int z = minZ; z <= maxZ; ++z)
                        {
                            Vector3 cubePos = generator.GetCubePosition(x, y, z);
                            float dist = (point - cubePos).magnitude;
                            if (dist > sphereRadius) continue;
                            
                            float scalar = scalarField[x, y, z];
                            scalar -= isDig ? digPower : -digPower;

                            scalarField[x, y, z] = scalar;
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
            isClicked = true;
            isDig = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isClicked = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isClicked = true;
            isDig = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isClicked = false;
        }

        if (isClicked)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            elapsedTime += Time.deltaTime;
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
            for (int y = 0; y < axisYCount; ++y)
            {
                for (int z = 0; z < axisZCount; ++z)
                {
                    generator.SetScalar(x, y, z, scalarField[x, y, z]);
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
