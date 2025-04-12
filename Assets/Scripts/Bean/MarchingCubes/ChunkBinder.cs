using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// Chunk에서 여러개의 Cube를 묶는 부분을 처리해주는 클래스
    /// 하나만 인스턴스 되며, 모든 청크가 사용한다.
    /// </summary>
    public class ChunkBinder
    {
        /// <summary>
        /// 마칭큐브에서 공통으로 사용되는 머터리얼 배열
        /// </summary>
        private Material[] sharedMaterials;
        
        /// <summary>
        /// Cube를 묶기위한 컴바인 인스턴스 리스트.
        /// 첫 번째 인덱싱으로는 머터리얼의 인덱스가 들어간다.
        /// </summary>
        private List<List<CombineInstance>> combineInstanceList;
        
        /// <summary>
        /// 바인딩 결과에 사용될 머터리얼 리스트.
        /// </summary>
        private List<Material> resultMaterials;

        /// <summary>
        /// 바인딩 결과에 사용될 컴바인 인스턴스 리스트.
        /// </summary>
        private List<CombineInstance> resultCombineInstances;

        /// <summary>
        /// 바인딩 결과에 사용될 메시 풀. 미리 만들어두고 필요할때마다 꺼내쓴다.
        /// </summary>
        private List<Mesh> resultMeshPool;
        /// <summary>
        /// 현재 풀에 있는 메시의 개수. 리스트에서 직접 값을 빼지 않고 인덱싱으로 가져오기 위해 사용한다.
        /// </summary>
        private int currMeshCount;

        public ChunkBinder(Material[] materials)
        {
            sharedMaterials = materials;
            
            combineInstanceList = new List<List<CombineInstance>>(materials.Length);
            for (int i = 0; i < materials.Length; ++i)
            {
                combineInstanceList.Add(new List<CombineInstance>());
            }

            resultMaterials = new List<Material>(materials.Length);
            resultCombineInstances = new List<CombineInstance>();
            
            resultMeshPool = new List<Mesh>();
            for (int x = 0; x < Define.MarchingCubes.ChunkSize; ++x)
            {
                for (int y = 0; y < Define.MarchingCubes.ChunkSize; ++y)
                {
                    for (int z = 0; z < Define.MarchingCubes.ChunkSize; ++z)
                    {
                        resultMeshPool.Add(new Mesh());
                    }
                }
            }

            Reset();
        }

        /// <summary>
        /// 모든 값을 초기화한다.
        /// </summary>
        private void Reset()
        {
            for (int i = 0, count = combineInstanceList.Count; i < count; ++i)
            {
                combineInstanceList[i].Clear();
            }
            
            resultMaterials.Clear();
            resultCombineInstances.Clear();
            currMeshCount = resultMeshPool.Count;
        }
        
        /// <summary>
        /// Cubes를 하나로 묶은 메시를 반환한다.
        /// </summary>
        public void Bind(List<Cube> cubes, Func<Vector3, int> cbCalcIndex, ref Mesh mesh, out Material[] materials)
        {
            Reset();

            // 모든 Cube를 subMeshIndex에 맞게 각각 리스트에 넣는다.
            for (int i = 0, count = cubes.Count; i < count; ++i)
            {
                Cube cube = cubes[i];
                if (cube.HasMesh == false) continue;
                CombineInstance instance = new CombineInstance();
                instance.mesh = cube.Mesh;
                instance.transform = cube.transform.localToWorldMatrix;
                int subMeshIndex = cbCalcIndex(cube.transform.position);
                instance.subMeshIndex = subMeshIndex;
                combineInstanceList[subMeshIndex].Add(instance);
            }
            
            // 리스트에 들어간 메시들을 하나씩 묶는다.
            for (int i = 0; i < sharedMaterials.Length; ++i)
            {
                if (combineInstanceList[i].Count == 0) continue;
                
                resultMaterials.Add(sharedMaterials[i]);
                CombineInstance ci = new CombineInstance();
                ci.mesh = resultMeshPool[--currMeshCount];
                ci.mesh.CombineMeshes(combineInstanceList[i].ToArray(), true);
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;
                resultCombineInstances.Add(ci);
            }
            
            // 이미 묶인 메시들을 또 다시 묶는다.
            mesh.CombineMeshes(resultCombineInstances.ToArray(), false);
            materials = resultMaterials.ToArray();
        }
    }
}