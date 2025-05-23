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

        /// <summary> 청크 배열 </summary>
        private Chunk[,,] chunks;

        /// <summary> 청크를 묶어주는 처리를 하는 바인더. 하나의 인스턴스만 가지고 있다. </summary>
        private ChunkBinder chunkBinder;

        private ScalarFieldGenerator scalarGenerator;

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
            
            ChunkXCount = AxisXCount / MarchingCubes.ChunkSize[0];
            ChunkYCount = AxisYCount / MarchingCubes.ChunkSize[0];
            ChunkZCount = AxisZCount / MarchingCubes.ChunkSize[0];
        }

        public void Init(Func<Vector3, int> cbSubMeshIndex = null)
        {
            this.cbSubMeshIndex = cbSubMeshIndex;

            scalarGenerator = new ScalarFieldGenerator(this);
            
            chunks = new Chunk[ChunkXCount, ChunkYCount, ChunkZCount];
            for (int x = 0, countX = ChunkXCount; x < countX; ++x)
            {
                for (int y = 0, countY = ChunkYCount; y < countY; ++y)
                {
                    for (int z = 0, countZ = ChunkZCount; z < countZ; ++z)
                    {
                        chunks[x,y,z] = GameObject.Instantiate(prefabChunk, trCubeParent).GetComponent<Chunk>();
                        chunks[x,y,z].Init(new Vector3Int(x, y, z), sharedMaterials, chunkBinder, CalcSubMeshIndex, scalarGenerator.ScalarField);
                        chunks[x,y,z].Refresh(true);
                    }
                }
            }
            
            scalarGenerator.Init(chunks);
        }

        /// <summary>
        /// 스칼라값을 세팅한다.
        /// </summary>
        public void SetScalar(int x, int y, int z, float scalar)
        {
            scalarGenerator.SetScalar(x, y, z, scalar);
        }

        /// <summary>
        /// 스칼라 필드를 기준으로 메시를 계산한다.
        /// </summary>
        public void UpdateMeshes(bool immediate = false)
        {
            // 현재 UpdateTick이 0일때만 실제로 갱신을 한다.
            currUpdateTick = (currUpdateTick + 1) % UpdateTick;
            if (!immediate && currUpdateTick > 0) return;
            
            for (int x = 0, countX = ChunkXCount; x < countX; ++x)
            {
                for (int y = 0, countY = ChunkYCount; y < countY; ++y)
                {
                    for (int z = 0, countZ = ChunkZCount; z < countZ; ++z)
                    {
                        chunks[x,y,z].Refresh(immediate);
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
    }
}