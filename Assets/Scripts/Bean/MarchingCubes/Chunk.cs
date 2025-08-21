using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// 여러개의 큐브를 묶어서 하나의 덩어리로 보이게 하는 클래스.
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

        private Material[] sharedMaterials;

        private ChunkBinder chunkBinder;

        private Func<Vector3, int> cbCalcIndex;
        
        /// <summary> 청크의 LOD 레벨이 1일때의 큐브 리스트 </summary>
        private List<Cube> lod1Cubes = new List<Cube>();
        /// <summary> 청크의 LOD 레벨이 2일때의 큐브 리스트 </summary>
        private List<Cube> lod2Cubes = new List<Cube>();
        /// <summary> 청크의 LOD 레벨이 3일때의 큐브 리스트 </summary>
        private List<Cube> lod3Cubes = new List<Cube>();
        
        /// <summary>
        /// 현재 LOD레벨의 cube 사이즈
        /// </summary>
        private List<Cube> cubes
        {
            get
            {
                switch (LODLevel)
                {
                    case MarchingCubes.ChunkLODLevel.Level0:
                    case MarchingCubes.ChunkLODLevel.Level1:
                        return lod1Cubes;
                    case MarchingCubes.ChunkLODLevel.Level2:
                        return lod2Cubes;
                    case MarchingCubes.ChunkLODLevel.Level3:
                        return lod3Cubes;
                }

                return null;
            }
        }
        
        /// <summary> 청크의 LOD 레벨 </summary>
        public MarchingCubes.ChunkLODLevel LODLevel { get; private set; }

        /// <summary>
        /// 현재 청크의 한 변에 있는 큐브의 수
        /// </summary>
        public int ChunkSize => Define.MarchingCubes.ChunkSize[(int)LODLevel];
        
        /// <summary> 현재 청크가 갱신이 필요한지 여부 </summary>
        public bool IsDirty { get; private set; }
        
        private void Awake()
        {
            LODLevel = MarchingCubes.ChunkLODLevel.Level0;
            
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 큰 메시도 지원
            combineInstances = new CombineInstance[ChunkSize * ChunkSize * ChunkSize];
            for (int x = 0; x < ChunkSize; ++x)
            {
                for (int y = 0; y < ChunkSize; ++y)
                {
                    for (int z = 0; z < ChunkSize; ++z)
                    {
                        int index = GetCombinedIndex(x, y, z);
                        combineInstances[index] = new CombineInstance();
                    }
                }
            }
        }
        
        public void Init(Vector3Int chunkIndex, Material[] sharedMaterials, ChunkBinder chunkBinder, Func<Vector3, int> calcSubMeshIndex, float[,,] scalarField)
        {
            this.chunkBinder = chunkBinder;
            this.sharedMaterials = sharedMaterials;
            cbCalcIndex = calcSubMeshIndex;
            meshFilter.sharedMesh = combinedMesh;

            lod1Cubes = new List<Cube>();
            lod2Cubes = new List<Cube>();
            lod3Cubes = new List<Cube>();

            for (int lodLevel = (int)MarchingCubes.ChunkLODLevel.Level1, end = (int)MarchingCubes.ChunkLODLevel.Level3; lodLevel <= end; ++lodLevel)
            {
                int chunkSize = MarchingCubes.ChunkSize[lodLevel];
                float cubeSize = MarchingCubes.CubeSize[lodLevel];
                List<Cube> cubes = null;
                switch ((MarchingCubes.ChunkLODLevel)lodLevel)
                {
                    case MarchingCubes.ChunkLODLevel.Level1:
                        cubes = lod1Cubes;
                        break;
                    case MarchingCubes.ChunkLODLevel.Level2:
                        cubes = lod2Cubes;
                        break;
                    case MarchingCubes.ChunkLODLevel.Level3:
                        cubes = lod3Cubes;
                        break;
                    default: break;
                }
                
                for (int x = chunkIndex.x * chunkSize, countX = x + chunkSize; x < countX; ++x)
                {
                    for (int y = chunkIndex.y * chunkSize, countY = y + chunkSize; y < countY; ++y)
                    {
                        for (int z = chunkIndex.z * chunkSize, countZ = z + chunkSize; z < countZ; ++z)
                        {
                            Vector3 position = new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);
                            Cube cube = new Cube(sharedMaterials, position, calcSubMeshIndex, scalarField, new Vector3Int(x, y, z));
                            cubes.Add(cube);
                        }
                    }
                }
            }
        }

        public void SetLODLevel(MarchingCubes.ChunkLODLevel level)
        {
            if (level == LODLevel) return;
            
            LODLevel = level;
            IsDirty = true;
        }
        
        private int GetCombinedIndex(int x, int y, int z)
        {
            return x + y * ChunkSize + z * ChunkSize * ChunkSize;
        }

        private void Combine()
        {
            chunkBinder.Bind(cubes, cbCalcIndex, ref combinedMesh, out Material[] materials);
            meshRenderer.sharedMaterials = materials;
        }

        /// <summary>
        /// 청크를 갱신한다.
        /// 만약 변한 것이 없다면 무시하지만, 강제로 리프레시를 해야 하거나, 변한 것이 있다면 그 순간에만 갱신한다.
        /// </summary>
        public void Refresh(bool isForcedRefresh = false)
        {
            if(!IsDirty && !isForcedRefresh) return;

            foreach (var cube in cubes)
            {
                cube.CalcIsoSurface(LODLevel);
            }
            
            Combine();
        }
    }
}