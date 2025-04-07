using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// 여러개의 큐브를 묶어서 하나의 덩어리로 보이게 하느 클래스.
    /// CombineMeshes 컴포넌트로 여러개의 메시를 하나의 메시로 처리하여 드로우콜을 낮춘다.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        private CombineInstance[] combineInstances;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private Mesh combinedMesh;
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            combinedMesh = new Mesh();
            combineInstances = new CombineInstance[Define.MarchingCubes.ChunkSize * Define.MarchingCubes.ChunkSize * Define.MarchingCubes.ChunkSize];
            for (int x = 0; x < Define.MarchingCubes.ChunkSize; ++x)
            {
                for (int y = 0; y < Define.MarchingCubes.ChunkSize; ++y)
                {
                    for (int z = 0; z < Define.MarchingCubes.ChunkSize; ++z)
                    {
                        int index = GetCombinedIndex(x, y, z);
                        combineInstances[index] = new CombineInstance();
                    }
                }
            }
        }

        public void Combine(Cube[,,] cubes, Func<Vector3, int> cbCalcIndex)
        {
            List<CombineInstance> instances = new List<CombineInstance>();
            for (int x = 0; x < Define.MarchingCubes.ChunkSize; ++x)
            {
                for (int y = 0; y < Define.MarchingCubes.ChunkSize; ++y)
                {
                    for (int z = 0; z < Define.MarchingCubes.ChunkSize; ++z)
                    {
                        Cube cube = cubes[x, y, z];
                        if (cube.HasMesh == false) continue;
                        CombineInstance instance = new CombineInstance();
                        instance.mesh = cube.Mesh;
                        instance.transform = cube.transform.localToWorldMatrix;
                        instances.Add(instance);
                        instance.subMeshIndex = cbCalcIndex(cube.transform.position);
                        //int index = GetCombinedIndex(x, y, z);
                        // CombineInstance ci = combineInstances[index];
                        // ci.mesh = cube.Mesh;
                        // ci.transform = cube.transform.localToWorldMatrix;
                    }
                }
            }
            
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 큰 메시도 지원
            combinedMesh.CombineMeshes(instances.ToArray(), true);

            meshFilter.mesh = combinedMesh;
            meshRenderer.materials = cubes[0,0,0].MeshRenderer.sharedMaterials;
        }

        private int GetCombinedIndex(int x, int y, int z)
        {
            const int chunkSize2 = Define.MarchingCubes.ChunkSize * Define.MarchingCubes.ChunkSize;
            return x + y * Define.MarchingCubes.ChunkSize + z * chunkSize2;
        }
    }
}