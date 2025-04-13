using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bean.MC
{
    /// <summary>
    /// 큐브의 정보를 가지고 실제로 렌더링을 해주는 렌더러 클래스
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class CubeRenderer : MonoBehaviour
    {
        public MeshFilter MeshFilter { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }
        public MeshCollider MeshCollider { get; private set; }

        private void Awake()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshCollider = GetComponent<MeshCollider>();
        }

        public void SetCubeData(Cube cube, Material[] sharedMaterials)
        {
            MeshFilter.sharedMesh = cube.Mesh;
            MeshCollider.sharedMesh = cube.Mesh;
            MeshRenderer.sharedMaterials = sharedMaterials;
        }
    }
}