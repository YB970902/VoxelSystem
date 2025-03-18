using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마칭 큐브 구현을 위한 클래스
/// 보여지는것을 구현해야 하기 때문에 구조보단 구현에 집중하자.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingCube : MonoBehaviour
{
    private Mesh mesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1)
        };

        int[] triangles = new int[]
        {
            0, 2, 1,
            0, 3, 2,
            3, 6, 2,
            3, 7, 6,
            7, 5, 6,
            7, 4, 5,
            4, 1, 5,
            4, 0, 1,
            4, 3, 0,
            7, 3, 4,
            1, 2, 5,
            2, 6, 5
        };
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
