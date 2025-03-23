using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// 큐브의 프리팹
        /// TODO : Address 설정이 되면, 생성자에서 받아오는 것을 주소로 받아오도록 수정하기.
        /// </summary>
        private Cube prefab;

        public CubeGenerator(int axisX, int axisZ, int axisY, Cube prefab)
        {
            this.prefab = prefab;
            AxisXCount = axisX;
            AxisZCount = axisZ;
            AxisYCount = axisY;
        }

        public void Init()
        {
            cubes = new Cube[AxisXCount, AxisZCount, AxisYCount];
            
            for (int x = 0; x < AxisXCount; ++x)
            {
                for (int z = 0; z < AxisZCount; ++z)
                {
                    for (int y = 0; y < AxisYCount; ++y)
                    {
                        var cube = GameObject.Instantiate(prefab);
                        cube.transform.position = new Vector3(x, y, z);
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

        /// <summary>
        /// 스칼라 필드를 기준으로 메시를 계산한다.
        /// </summary>
        public void CalcMeshes()
        {
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
    }
}