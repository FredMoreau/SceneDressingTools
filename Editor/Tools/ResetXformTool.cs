using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

internal static class ResetXformTool
{
    [MenuItem("Tools/Reset XForm")]
    static void ResetXSelection()
    {
        int undoLvl = Undo.GetCurrentGroup();

        var root = Selection.activeTransform;

        var children = root.GetComponentsInChildren<Transform>(true).ToList();
        children.Sort((x, y) => y.ParentCount().CompareTo(x.ParentCount()));

        List<Mesh> processedMeshes = new List<Mesh>();
        Dictionary<Transform, Vector3> localDestinations = children.ToDictionary(x => x, x => x.position);

        foreach (var child in children)
        {
            Undo.RecordObject(child, "");

            var inheritedScale = child.InheritedScale(root);

            if (child.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
            {
                if (meshFilter.sharedMesh != null && !processedMeshes.Contains(meshFilter.sharedMesh))
                {
                    Undo.RecordObject(meshFilter.sharedMesh, "");
                    meshFilter.sharedMesh.Scale(Vector3.Scale(child.localScale, inheritedScale));
                    processedMeshes.Add(meshFilter.sharedMesh);
                }
            }

            child.localScale = Vector3.one;
        }

        children.Sort((x, y) => x.ParentCount().CompareTo(y.ParentCount()));
        foreach (var child in children)
        {
            child.position = localDestinations[child];

            if (PrefabUtility.IsPartOfPrefabInstance(child))
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
        }

        Undo.SetCurrentGroupName("Reset XForm");
        Undo.CollapseUndoOperations(undoLvl);
    }

    [MenuItem("Tools/Reset XForm", validate = true)]
    static bool ResetXSelection_Validate()
    {
        return Selection.activeTransform != null;
    }
}

internal static class MeshExtensions
{
    /// <summary>
    /// Applies <paramref name="scale"/> to the <paramref name="mesh"/> vertices.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="scale"></param>
    internal static void Scale(this Mesh mesh, Vector3 scale)
    {
        Vector3[] vertices = mesh.vertices;

        for (var i = 0; i < vertices.Length; i++)
            vertices[i].Scale(scale);

        mesh.vertices = vertices;
        //mesh.SetVertices(vertices);

        mesh.RecalculateBounds();
    }
}

internal static class TransformExtensions
{
    internal static int ParentCount(this Transform transform)
    {
        int count = 0;
        Transform parent = transform.parent;

        while (parent != null)
        {
            count++;
            parent = parent.parent;
        }

        return count;
    }

    internal static Vector3 InheritedScale(this Transform transform, Transform relativeTo)
    {
        Vector3 scale = transform.localScale;

        var parent = transform.parent;
        while (parent != null)
        {
            scale.Scale(parent.localScale);
            parent = parent.parent;
            if (parent == relativeTo)
                break;
        }

        return scale;
    }
}