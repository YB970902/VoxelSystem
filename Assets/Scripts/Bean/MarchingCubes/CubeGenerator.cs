using System;
using System.Collections;
using System.Collections.Generic;
using Bean.Addressable;
using Define;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// 마칭 큐브를 생성해주는 제너레이터
    /// 외부에서는 큐브에 직접적으로 접근할 순 없고, 이 제너레이터를 통해서만 접근할 수 있다.
    /// 청크로 감싸기, LOD 등의 기능들도 제너레이터가 처리한다.
    /// </summary>
    public class CubeGenerator
    {
        /// <summary> 큐브 인스턴스의 부모 트랜스폼 </summary>
        private Transform trCubeParent;

        /// TODO : 모든 큐브가 동일한 인스턴스를 가지고 있는거보단, 필요한 만큼만 가지고 있는게 나아보인다. 풀에 넣어두고 필요한 시점만 뺴서 써도 될거같다.
        /// <summary> 큐브 인스턴스 배열 </summary>
        private Cube[,,] cubes;

        /// <summary> 청크 배열 </summary>
        private Chunk[,,] chunks;

        /// <summary> 청크를 묶어주는 처리를 하는 바인더. 하나의 인스턴스만 가지고 있다. </summary>
        private ChunkBinder chunkBinder;

        /// <summary> 원본 스칼라 필드 </summary>
        private float[,,] originScalarField;

        /// <summary> 스칼라 필드에 LOD를 적용하기 위해 가공한 데이터 </summary>
        private float[,,] scalarField;

        /// <summary> X축 큐브 개수 </summary>
        public int AxisXCount { get; private set; }
        /// <summary> Y축 큐브 개수 </summary>
        public int AxisYCount { get; private set; }
        /// <summary> Z축 큐브 개수 </summary>
        public int AxisZCount { get; private set; }

        /// <summary> 실제로 업데이트가 이루어지는 타이밍 </summary>
        public int UpdateTick { get; private set; }

        private int currUpdateTick;
        
        private Material[] sharedMaterials;

        /// <summary>
        /// 큐브의 프리팹
        /// </summary>
        private Cube prefabCube;

        /// <summary>
        /// 청크 프리팹
        /// </summary>
        private Chunk prefabChunk;
        
        public int ChunkXCount { get; private set; }
        public int ChunkYCount { get; private set; }
        public int ChunkZCount { get; private set; }

        private Func<Vector3, int> cbSubMeshIndex;

        public CubeGenerator(int axisX, int axisY, int axisZ, int updateTick = 1, Transform trCubeParent = null)
        {
            this.trCubeParent = trCubeParent;
            prefabCube = AddressableManager.Instance.LoadAssetSync<GameObject>("Sources/Prefabs/MountainCube.prefab", string.Empty).GetComponent<Cube>();
            prefabChunk = AddressableManager.Instance.LoadAssetSync<GameObject>("Sources/Prefabs/Chunk.prefab", string.Empty).GetComponent<Chunk>();
            AxisXCount = axisX;
            AxisYCount = axisY;
            AxisZCount = axisZ;
            currUpdateTick = 0;
            UpdateTick = updateTick;
            currUpdateTick = 0;
            sharedMaterials = new List<Material>
            {
                AddressableManager.Instance.LoadAssetSync<Material>("Sources/Materials/matMountain.mat", string.Empty),
                AddressableManager.Instance.LoadAssetSync<Material>("Sources/Materials/matMountainTop.mat", string.Empty)
            }.ToArray();
            
            chunkBinder = new ChunkBinder(sharedMaterials);
            
            ChunkXCount = AxisXCount / Define.MarchingCubes.ChunkSize;
            ChunkYCount = AxisYCount / Define.MarchingCubes.ChunkSize;
            ChunkZCount = AxisZCount / Define.MarchingCubes.ChunkSize;
            
            chunks = new Chunk[ChunkXCount, ChunkYCount, ChunkZCount];
            for (int x = 0, countX = ChunkXCount; x < countX; ++x)
            {
                for (int y = 0, countY = ChunkYCount; y < countY; ++y)
                {
                    for (int z = 0, countZ = ChunkZCount; z < countZ; ++z)
                    {
                        chunks[x,y,z] = GameObject.Instantiate(prefabChunk, trCubeParent).GetComponent<Chunk>();
                        chunks[x,y,z].Init(sharedMaterials, chunkBinder);
                    }
                }
            }
        }

        public void Init(Func<Vector3, int> cbSubMeshIndex = null)
        {
            this.cbSubMeshIndex = cbSubMeshIndex;
            cubes = new Cube[AxisXCount, AxisYCount, AxisZCount];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int y = 0; y < AxisYCount; ++y)
                {
                    for (int z = 0; z < AxisZCount; ++z)
                    {
                        var cube = GameObject.Instantiate(prefabCube, trCubeParent);
                        cube.Init(CalcSubMeshIndex, sharedMaterials);
                        cube.transform.position = new Vector3(x * MarchingCubes.CubeSize, y * MarchingCubes.CubeSize, z * MarchingCubes.CubeSize);
                        cubes[x, y, z] = cube;
                    }
                }
            }
            
            originScalarField = new float[AxisXCount + 1, AxisYCount + 1, AxisZCount + 1];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int y = 0; y < AxisYCount; ++y)
                {
                    for (int z = 0; z < AxisZCount; ++z)
                    {
                        originScalarField[x, y, z] = 1f;
                    }
                }
            }
        }

        /// <summary>
        /// 스칼라값을 세팅한다.
        /// </summary>
        public void SetScalar(int x, int y, int z, float scalar)
        {
            originScalarField[x, y, z] = scalar;
        }

        /// <summary>
        /// 스칼라 값을 반환한다.
        /// </summary>
        public float GetScalar(int x, int y, int z)
        {
            return originScalarField[x, y, z];
        }

        public Vector3 GetCubePosition(int x, int y, int z)
        {
            return cubes[x, y, z].transform.position;
        }

        /// <summary>
        /// 스칼라 필드를 기준으로 메시를 계산한다.
        /// </summary>
        public void UpdateMeshes(bool immediate = false)
        {
            // 현재 UpdateTick이 0일때만 실제로 갱신을 한다.
            currUpdateTick = (currUpdateTick + 1) % UpdateTick;
            if (!immediate && currUpdateTick > 0) return;
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int y = 0; y < AxisYCount; ++y)
                {
                    for (int z = 0; z < AxisZCount; ++z)
                    {
                        var cube = cubes[x, y, z];

                        cube.ScalarVal[0] = originScalarField[x, y, z + 1];
                        cube.ScalarVal[1] = originScalarField[x + 1, y, z + 1];
                        cube.ScalarVal[2] = originScalarField[x + 1, y, z];
                        cube.ScalarVal[3] = originScalarField[x, y, z];

                        cube.ScalarVal[4] = originScalarField[x, y + 1, z + 1];
                        cube.ScalarVal[5] = originScalarField[x + 1, y + 1, z + 1];
                        cube.ScalarVal[6] = originScalarField[x + 1, y + 1, z];
                        cube.ScalarVal[7] = originScalarField[x, y + 1, z];

                        cube.CalcIsoSurface();
                    }
                }
            }
            
            for (int x = 0, countX = ChunkXCount; x < countX; ++x)
            {
                for (int y = 0, countY = ChunkYCount; y < countY; ++y)
                {
                    for (int z = 0, countZ = ChunkZCount; z < countZ; ++z)
                    {
                        chunks[x,y,z].Combine(GetCubeList(x, y, z), CalcSubMeshIndex);
                    }
                }
            }
            
             for (int x = 0; x < AxisXCount; ++x)
             {
                 for (int y = 0; y < AxisYCount; ++y)
                 {
                     for (int z = 0; z < AxisZCount; ++z)
                     {
                         var cube = cubes[x, y, z];
                         cube.gameObject.SetActive(false);
                     }
                 }
             }
        }

        private List<Cube> GetCubeList(int chunkX, int chunkY, int chunkZ)
        {
            List<Cube> result = new List<Cube>(Define.MarchingCubes.ChunkSize * Define.MarchingCubes.ChunkSize * Define.MarchingCubes.ChunkSize);
            
            int cubeX = chunkX * Define.MarchingCubes.ChunkSize;
            int cubeY = chunkY * Define.MarchingCubes.ChunkSize;
            int cubeZ = chunkZ * Define.MarchingCubes.ChunkSize;
            
            for (int x = 0; x < Define.MarchingCubes.ChunkSize; ++x)
            {
                for (int y = 0; y < Define.MarchingCubes.ChunkSize; ++y)
                {
                    for (int z = 0; z < Define.MarchingCubes.ChunkSize; ++z)
                    {
                        result.Add(cubes[cubeX + x, cubeY + y, cubeZ + z]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 서브메시를 계산하는 함수
        /// 콜백함수가 있다면 그 함수를 호출하고, 없다면 0을 반환한다. 
        /// </summary>
        private int CalcSubMeshIndex(Vector3 position)
        {
            if (cbSubMeshIndex == null) return 0;
            
            return cbSubMeshIndex(position);
        }

        #region TEST
        #if UNITY_EDITOR

        public bool IsMeshRendererEnabled { get; private set; } = true;
        public bool IsMeshColliderEnabled { get; private set; } = true;
        
        public void SetEnableMeshRenderer(bool enable)
        {
            IsMeshRendererEnabled = enable;
            foreach (var cube in cubes)
            {
                cube.SetEnableMeshRenderer(enable);
            }
        }
        
        public void SetEnableMeshCollider(bool enable)
        {
            IsMeshColliderEnabled = enable;
            foreach (var cube in cubes)
            {
                cube.SetEnableMeshCollider(enable);
            }
        }
        
        #endif
        #endregion
    }
}