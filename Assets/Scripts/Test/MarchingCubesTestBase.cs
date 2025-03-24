using System.Collections;
using System.Collections.Generic;
using Bean.MC;
using UnityEngine;

public class MarchingCubesTestBase : MonoBehaviour
{
    [SerializeField] protected int axisXCount;
    [SerializeField] protected int axisYCount;
    [SerializeField] protected int axisZCount;

    [SerializeField] protected Cube prefab;

    [SerializeField] protected float isoLevel;
    [SerializeField] protected float cubeSize;
}
