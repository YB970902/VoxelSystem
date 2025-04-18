namespace Define
{
    public static class MarchingCubes
    {
        /// <summary>
        /// 등치면을 그리는 레벨
        /// </summary>
        public static float IsoLevel = 0.5f;

        public static float CubeSize = 1f;
        public static float CubeHalfSize => CubeSize * 0.5f;
        
        /// <summary>
        /// 청크 한 변에 위치한 큐브의 개수
        /// </summary>
        public const int ChunkSize = 16;
    }
}
