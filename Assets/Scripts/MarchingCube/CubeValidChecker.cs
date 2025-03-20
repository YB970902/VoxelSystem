using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeValidChecker : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float p1Val;
    [Range(0, 1)]
    [SerializeField] float p2Val;
    [Range(0, 1)]
    [SerializeField] float p3Val;
    [Range(0, 1)]
    [SerializeField] float p4Val;
    [Range(0, 1)]
    [SerializeField] float p5Val;
    [Range(0, 1)]
    [SerializeField] float p6Val;
    [Range(0, 1)]
    [SerializeField] float p7Val;
    [Range(0, 1)]
    [SerializeField] float p8Val;
    
    [SerializeField] MarchingCube marchingCube;
    
    void Update()
    {
        marchingCube.GridVal[0] = p1Val;
        marchingCube.GridVal[1] = p2Val;
        marchingCube.GridVal[2] = p3Val;
        marchingCube.GridVal[3] = p4Val;
        marchingCube.GridVal[4] = p5Val;
        marchingCube.GridVal[5] = p6Val;
        marchingCube.GridVal[6] = p7Val;
        marchingCube.GridVal[7] = p8Val;
        
        marchingCube.CalcIsoSurface();
    }
}
