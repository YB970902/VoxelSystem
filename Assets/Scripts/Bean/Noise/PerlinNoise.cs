using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bean.Noise
{
    /// <summary>
    /// 펄린 노이즈
    /// 현재 테스트 단계이므로 구조보다는 구현에 집중하자.
    /// </summary>
    public class PerlinNoise
    {
        /// <summary> 각 격자마다 가지고 있는 normal 벡터 배열 </summary>
        public Vector2[,] gradients;
        
        /// <summary> 격자의 크기. </summary>
        public float GridSize { get; private set; }
        /// <summary> X축 격자의 개수 </summary>
        public int AxisXGridCount { get; private set; }
        /// <summary> Y축 격자의 개수 </summary>
        public int AxisYGridCount { get; private set; }

        public PerlinNoise(int axisX, int axisY, float gridSize)
        {
            GridSize = gridSize;
            AxisXGridCount = axisX + 1;
            AxisYGridCount = axisY + 1;
            
            gradients = new Vector2[AxisXGridCount, AxisYGridCount];
        }
        
        public void Init()
        {
            for (int x = 0; x < AxisXGridCount; ++x)
            {
                for (int y = 0; y < AxisYGridCount; ++y)
                {
                    gradients[x, y] = Random.insideUnitCircle.normalized;
                }
            }
        }

        /// <summary>
        /// x, y 위치의 노이즈 값을 구한다.
        /// </summary>
        public float GetNoise(float x, float y)
        {
            // 1. 입력 좌표가 속하는 격자 셀을 결정 (정수 부분)
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            // 2. 격자 셀 내의 상대적 좌표 (0~1 범위)
            float sx = x - x0;
            float sy = y - y0;

            // 3. 각 격자 코너의 gradient 벡터 가져오기 (의사 랜덤하게 생성)
            Vector2 g00 = gradients[x0, y0];
            Vector2 g10 = gradients[x1, y0];
            Vector2 g01 = gradients[x0, y1];
            Vector2 g11 = gradients[x1, y1];

            // 4. 각 격자 코너와의 상대 위치 (거리 벡터)
            Vector2 d00 = new Vector2(x - x0, y - y0);
            Vector2 d10 = new Vector2(x - x1, y - y0);
            Vector2 d01 = new Vector2(x - x0, y - y1);
            Vector2 d11 = new Vector2(x - x1, y - y1);

            // 5. 각 코너에서의 기여도: gradient와 거리 벡터의 내적
            float dot00 = Vector2.Dot(g00, d00);
            float dot10 = Vector2.Dot(g10, d10);
            float dot01 = Vector2.Dot(g01, d01);
            float dot11 = Vector2.Dot(g11, d11);

            // 6. 부드러운 보간 인자 계산 (fade 함수 사용)
            float u = Fade(sx);
            float v = Fade(sy);

            // 7. x축 방향으로 선형 보간 (lerp) 수행
            float ix0 = Mathf.Lerp(dot00, dot10, u);
            float ix1 = Mathf.Lerp(dot01, dot11, u);

            // 8. y축 방향 보간하여 최종 결과 계산
            float value = Mathf.Lerp(ix0, ix1, v);
            return value;
        }
        
        /// <summary>
        /// 페이드 함수: 부드러운 보간 계수를 계산합니다.
        /// fade(t) = 6t^5 - 15t^4 + 10t^3
        /// </summary>
        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
    }
}