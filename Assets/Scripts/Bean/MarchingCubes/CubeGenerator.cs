using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// 큐브의 프리팹
        /// TODO : Address 설정이 되면, 생성자에서 받아오는 것을 주소로 받아오도록 수정하기.
        /// </summary>
        private Cube prefab;

        private Func<Vector3, int> cbSubMeshIndex;

        public CubeGenerator(int axisX, int axisZ, int axisY, Cube prefab, int updateTick = 1, Transform trCubeParent = null)
        {
            this.trCubeParent = trCubeParent;
            this.prefab = prefab;
            AxisXCount = axisX;
            AxisZCount = axisZ;
            AxisYCount = axisY;
            currUpdateTick = 0;
            UpdateTick = updateTick;
            currUpdateTick = 0;
        }

        public void Init(Func<Vector3, int> cbSubMeshIndex = null)
        {
            this.cbSubMeshIndex = cbSubMeshIndex;
            cubes = new Cube[AxisXCount, AxisZCount, AxisYCount];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int z = 0; z < AxisZCount; ++z)
                {
                    for (int y = 0; y < AxisYCount; ++y)
                    {
                        var cube = GameObject.Instantiate(prefab, trCubeParent);
                        cube.Init(CalcSubMeshIndex);
                        cube.transform.position = new Vector3(x * MarchingCubes.CubeSize, y * MarchingCubes.CubeSize, z * MarchingCubes.CubeSize);
                        cubes[x, z, y] = cube;
                    }
                }
            }

            scalarField = new float[AxisXCount + 1, AxisZCount + 1, AxisYCount + 1];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int z = 0; z < AxisZCount; ++z)
                {
                    for (int y = 0; y < AxisYCount; ++y)
                    {
                        scalarField[x, z, y] = 1f;
                    }
                }
            }
        }

        /// <summary>
        /// 스칼라값을 세팅한다.
        /// </summary>
        public void SetScalar(int x, int z, int y, float scalar)
        {
            scalarField[x, z, y] = scalar;
        }

        /// <summary>
        /// 스칼라 값을 반환한다.
        /// </summary>
        public float GetScalar(int x, int z, int y)
        {
            return scalarField[x, z, y];
        }

        public Vector3 GetCubePosition(int x, int z, int y)
        {
            return cubes[x, z, y].transform.position;
        }

        /// <summary>
        /// 스칼라 필드를 기준으로 메시를 계산한다.
        /// </summary>
        public void UpdateMeshes()
        {
            // 현재 UpdateTick이 0일때만 실제로 갱신을 한다.
            currUpdateTick = (currUpdateTick + 1) % UpdateTick;
            if (currUpdateTick > 0) return;
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int z = 0; z < AxisZCount; ++z)
                {
                    for (int y = 0; y < AxisYCount; ++y)
                    {
                        var cube = cubes[x, z, y];

                        cube.ScalarVal[0] = scalarField[x, z + 1, y];
                        cube.ScalarVal[1] = scalarField[x + 1, z + 1, y];
                        cube.ScalarVal[2] = scalarField[x + 1, z, y];
                        cube.ScalarVal[3] = scalarField[x, z, y];

                        cube.ScalarVal[4] = scalarField[x, z + 1, y + 1];
                        cube.ScalarVal[5] = scalarField[x + 1, z + 1, y + 1];
                        cube.ScalarVal[6] = scalarField[x + 1, z, y + 1];
                        cube.ScalarVal[7] = scalarField[x, z, y + 1];

                        cube.CalcIsoSurface();
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