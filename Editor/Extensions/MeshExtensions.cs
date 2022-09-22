using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    internal static class MeshExtensions
    {
        internal static bool GetInstancedRenderers(this Mesh mesh, out MeshRenderer[] meshRenderers)
        {
            meshRenderers = (from item in Object.FindObjectsOfType<MeshFilter>()
                             where item.sharedMesh == mesh
                             select item.GetComponent<MeshRenderer>()).ToArray();

            return meshRenderers.Length > 0;
        }
    }
}