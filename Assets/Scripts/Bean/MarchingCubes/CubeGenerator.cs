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
    /// </summary>
    public class CubeGenerator
    {
        /// <summary> 큐브 인스턴스의 부모 트랜스폼 </summary>
        private Transform trCubeParent;

        /// <summary> 큐브 인스턴스 배열 </summary>
        private Cube[,,] cubes;

        /// <summary> 테스트용 청크 </summary>
        private Chunk testChunk;

        /// <summary> 청크를 묶어주는 바인다. 하나의 인스턴스만 가지고 있다. </summary>
        private ChunkBinder chunkBinder;

        /// <summary> 꼭짓점의 스칼라값이 있는 필드 </summary>
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
        private Cube prefab;

        private Func<Vector3, int> cbSubMeshIndex;

        public CubeGenerator(int axisX, int axisY, int axisZ, int updateTick = 1, Transform trCubeParent = null)
        {
            this.trCubeParent = trCubeParent;
            prefab = AddressableManager.Instance.LoadAssetSync<GameObject>("Sources/Prefabs/MountainCube.prefab", string.Empty).GetComponent<Cube>();
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
            testChunk = new GameObject("TestChunk").AddComponent<Chunk>();
            testChunk.Init(sharedMaterials, chunkBinder);
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
                        var cube = GameObject.Instantiate(prefab, trCubeParent);
                        cube.hideFlags = HideFlags.HideInInspector;
                        cube.Init(CalcSubMeshIndex, sharedMaterials);
                        cube.transform.position = new Vector3(x * MarchingCubes.CubeSize, y * MarchingCubes.CubeSize, z * MarchingCubes.CubeSize);
                        cubes[x, y, z] = cube;
                    }
                }
            }
            
            scalarField = new float[AxisXCount + 1, AxisYCount + 1, AxisZCount + 1];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int y = 0; y < AxisYCount; ++y)
                {
                    for (int z = 0; z < AxisZCount; ++z)
                    {
                        scalarField[x, y, z] = 1f;
                    }
                }
            }
        }

        /// <summary>
        /// 스칼라값을 세팅한다.
        /// </summary>
        public void SetScalar(int x, int y, int z, float scalar)
        {
            scalarField[x, y, z] = scalar;
        }

        /// <summary>
        /// 스칼라 값을 반환한다.
        /// </summary>
        public float GetScalar(int x, int y, int z)
        {
            return scalarField[x, y, z];
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

                        cube.ScalarVal[0] = scalarField[x, y, z + 1];
                        cube.ScalarVal[1] = scalarField[x + 1, y, z + 1];
                        cube.ScalarVal[2] = scalarField[x + 1, y, z];
                        cube.ScalarVal[3] = scalarField[x, y, z];

                        cube.ScalarVal[4] = scalarField[x, y + 1, z + 1];
                        cube.ScalarVal[5] = scalarField[x + 1, y + 1, z + 1];
                        cube.ScalarVal[6] = scalarField[x + 1, y + 1, z];
                        cube.ScalarVal[7] = scalarField[x, y + 1, z];

                        cube.CalcIsoSurface();
                    }
                }
            }
            
            testChunk.Combine(cubes, CalcSubMeshIndex);
            
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