using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// 스칼라 필드 값을 설정한다.
    /// 청크의 LODLevel에 맞게 보간하기도 한다.
    /// </summary>
    public class ScalarFieldGenerator
    {
        /// <summary> 원본 스칼라 필드 </summary>
        private float[,,] originScalarField;

        /// <summary> 스칼라 필드에 LOD를 적용하기 위해 가공한 데이터 </summary>
        public  float[,,] ScalarField { get; private set; }

        /// <summary>
        /// cubeGenerator가 관리하는 청크 값
        /// </summary>
        private Chunk[,,] chunks;

        private CubeGenerator cubeGenerator;

        /// <summary>
        /// 스칼라 필드를 세팅하고 기본 값을 넣어준다.
        /// </summary>
        public ScalarFieldGenerator(CubeGenerator cubeGenerator)
        {
            this.cubeGenerator = cubeGenerator;
            
            // 스칼라 필드 세팅
            originScalarField = new float[cubeGenerator.AxisXCount + 1, cubeGenerator.AxisYCount + 1, cubeGenerator.AxisZCount + 1];
            
            for (int x = 0; x < cubeGenerator.AxisXCount; ++x)
            {
                for (int y = 0; y < cubeGenerator.AxisYCount; ++y)
                {
                    for (int z = 0; z < cubeGenerator.AxisZCount; ++z)
                    {
                        originScalarField[x, y, z] = 1f;
                    }
                }
            }

            // 복사해서 사용한다.
            ScalarField = originScalarField.Clone() as float[,,];
        }
        
        /// <summary>
        /// 제너레이터가 실행되기 전에 1회 호출되는 초기화 함수
        /// </summary>
        public void Init(Chunk[,,] chunks)
        {
            this.chunks = chunks;
        }

        public void SetScalar(int x, int y, int z, float scalar)
        {
            originScalarField[x, y, z] = scalar;
            ScalarField[x, y, z] = scalar;
        }

        /// <summary>
        /// 해당 위치의 청크 LODLevel을 설정한다.
        /// LOD 레벨이 높아지면, 원본 스칼라 값에서 낮은 해상도의 값을 가져오고, 그 사이는 보간으로 채운다.
        /// </summary>
        public void SetChunkLODLevel(int chunkX, int chunkY, int chunkZ, MarchingCubes.ChunkLODLevel level)
        {
            Chunk chunk = chunks[chunkX, chunkY, chunkZ];
            if (chunk.LODLevel == level) return;
            chunk.Refresh();

            // 청크 해상도. 값이 클수록 해상도가 낮다.
            int chunkResolution = MarchingCubes.ChunkSize[(int)level] / MarchingCubes.ChunkSize[0];
            
            // 해상도만큼 루프를 돈다.
            for (int x = chunkX, countX = chunkX + MarchingCubes.ChunkSize[0]; x < countX; x += chunkResolution)
            {
                for (int y = chunkY, countY = chunkY + MarchingCubes.ChunkSize[0]; y < countY; y += chunkResolution)
                {
                    for (int z = chunkZ, countZ = chunkZ + MarchingCubes.ChunkSize[0]; z < countZ; z += chunkResolution)
                    {
                        // 원본 스칼라 필드의 값을 해상도 값 만큼 간격을 두고 스칼라 필드 값에 넣어준다. 
                        ScalarField[x, y, z] = originScalarField[x, y, z];

                        if (x - chunkResolution < chunkX) continue;
                        if (y - chunkResolution < chunkY) continue;
                        if (z - chunkResolution < chunkZ) continue;
                        
                        // 해상도 사이사이 값을 보간하며 채워준다.
                    }
                }
            }
        }

        private void LerpScalarField(int x, int y, int z, int resolution)
        {
            for (int countX = x + resolution; x < countX; ++x)
            {
                for (int countY = y + resolution; y < countY; ++y)
                {
                    for (int countZ = z + resolution; z < countZ; ++z)
                    {
                        
                    }
                }
            }
        }
    }
}