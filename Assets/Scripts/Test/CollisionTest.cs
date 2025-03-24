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
    
    private Vector2 offset;
    private Vector2 maxOffset;
    
    private void Awake()
    {
        MarchingCubes.IsoLevel = isoLevel;
        MarchingCubes.CubeSize = cubeSize;

        int gridXCount = Mathf.CeilToInt(axisXCount * cubeSize) * noiseGridScale;
        int gridZCount = Mathf.CeilToInt(axisZCount * cubeSize) * noiseGridScale;

        offset = Vector2.zero;
        maxOffset = new Vector2(axisXCount * cubeSize * (noiseGridScale - 1), axisZCount * cubeSize * (noiseGridScale - 1));
        
        noise = new PerlinNoise(gridXCount, gridZCount, 1f);
        noise.Init();
        generator = new CubeGenerator(axisXCount, axisZCount, axisYCount, prefab, UpdateTick, transform);
        generator.Init(CalcSubMeshIndex);

        for (int x = 0; x < axisXCount; ++x)
        {
            for (int z = 0; z < axisZCount; ++z)
            {
                SetHeight(x, z);
            }
        }
        
        generator.UpdateMeshes();
    }

    private void FixedUpdate()
    {
        for (int x = 0; x < axisXCount; ++x)
        {
            for (int z = 0; z < axisZCount; ++z)
            {
                SetHeight(x, z);
            }
        }
        
        generator.UpdateMeshes();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        offset.x += h * (moveSpeed * Time.deltaTime);
        offset.y += v * (moveSpeed * Time.deltaTime);
        
        offset.x = Mathf.Clamp(offset.x, 0f, maxOffset.x);
        offset.y = Mathf.Clamp(offset.y, 0f, maxOffset.y);

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

    private void SetHeight(int x, int z)
    {
        float height = noise.GetNoise(x * cubeSize + offset.x, z * cubeSize + offset.y);
        // height가 -1 ~ 1 이므로, 1을 더해서 0 ~ 2 로 만든 뒤에 2로 나눠 0 ~ 1로 만든다.
        height = (height + 1f) * 0.5f;
        height *= axisYCount;
        
        for (int y = 0; y < axisYCount; ++y)
        {
            generator.SetScalar(x, z, y, Mathf.Clamp01(height - y));
        }
    }
    
    private int CalcSubMeshIndex(Vector3 position)
    {
        float height = axisYCount * cubeSize * 0.55f;
        if (position.y >= height) return 1;
        return 0;
    }
}
