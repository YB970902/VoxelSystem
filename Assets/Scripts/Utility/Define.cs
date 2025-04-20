namespace Define
{
    public static class MarchingCubes
    {
        /// <summary>
        /// 등치면을 그리는 레벨
        /// </summary>
        public static float IsoLevel = 0.5f;

        /// <summary> 청크의 LOD 레벨 별 큐브의 크기 </summary>
        public static readonly float[] CubeSize = new float[4] {0.5f, 0.5f, 1.0f, 2.0f};
        /// <summary> 청크의 LOD 레벨 별 큐브의 절반 크기 </summary>
        public static readonly float[] CubeHalfSize = new float[4] {CubeSize[0] * 0.5f, CubeSize[1] * 0.5f, CubeSize[2] * 0.5f, CubeSize[3] * 0.5f };

        /// <summary>
        /// 청크 한 변에 위치한 큐브의 개수
        /// </summary>
        public static readonly int[] ChunkSize = new int[4] { 16, 16, 8, 4 };

        /// <summary>
        /// 청크의 LOD 레벨. 레벨이 높을수록 간소화한다.
        /// </summary>
        public enum ChunkLODLevel
        {
            Level0, // 청크에 있는 큐브가 변경 가능한 상태. 이때는 큐브가 큐브 렌더러를 가지고 있다.
            Level1, // 청크 한 변에 있는 큐브의 수가 16개 
            Level2, // 청크 한 변에 있는 큐브의 수가 8개
            Level3, // 청크 한 변에 있는 큐브의 수가 4개
        }
    }
}
