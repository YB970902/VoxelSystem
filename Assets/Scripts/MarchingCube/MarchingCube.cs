using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마칭 큐브 구현을 위한 클래스
/// 보여지는것을 구현해야 하기 때문에 구조보단 구현에 집중하자.
/// </summary>
public class MarchingCube : MonoBehaviour
{
    /// <summary>
    /// 한 변에 있는 큐브의 개수
    /// </summary>
    public const int CubeCount = 10;
    /// <summary>
    /// 꼭짓점마다 있는 스칼라 필드
    /// 인덱스는 x, y, z로 구한다.
    /// </summary>
    private List<float> scalarField;

    private void Awake()
    {
        int capacity = CubeCount * CubeCount * CubeCount;
        scalarField = new List<float>(capacity);

        for (int i = 0; i < capacity; ++i)
        {
            scalarField.Add(1f);
        }
    }
    
    /// <summary>
    /// 스칼라 필드 값 반환
    /// </summary>
    private float GetScalarFieldValue(int x, int y, int z)
    {
        const int YCoef = CubeCount;
        const int ZCoef = CubeCount * CubeCount;
        
        return scalarField[x + y * YCoef + z * ZCoef];
    }
}
