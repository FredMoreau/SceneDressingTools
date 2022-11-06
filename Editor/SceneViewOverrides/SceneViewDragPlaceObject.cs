using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Unity.SceneDressingTools.Editor
{
    public static class SceneViewDragPlaceObject
    {
        //static readonly Plane s_GroundPlane = new Plane(Vector3.up, Vector3.up);

        //[InitializeOnLoadMethod]
        //static void InitPlaceObjectHandler()
        //{
        //    HandleUtility.placeObjectCustomPasses += PlaneRaycast;
        //}

        //// In this example, we register a plane at the scene origin to test for object placement.
        //static bool PlaneRaycast(Vector2 mousePosition, out Vector3 position, out Vector3 normal)
        //{
        //    Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
        //    float distance;

        //    if (s_GroundPlane.Raycast(worldRay, out distance))
        //    {
        //        position = worldRay.GetPoint(distance);
        //        normal = s_GroundPlane.normal;
        //        return true;
        //    }

        //    position = Vector3.zero;
        //    normal = Vector3.up;
        //    return false;
        //}

        [Serializable]
        internal class TransformWorldPlacement
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformWorldPlacement(Vector3 position = default, Quaternion rotation = default, Vector3 scale = default)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        [MenuItem("CONTEXT/MeshRenderer/Copy Bounds Center World Placement")]
        public static void CopyBoundsCenterWorldPlacement(MenuCommand command)
        {
            MeshRenderer renderer = (MeshRenderer)command.context;
            var p = renderer.bounds.center;
            var t = new TransformWorldPlacement(p, Quaternion.identity, Vector3.one);
            var s = "UnityEditor.TransformWorldPlacementJSON:" + EditorJsonUtility.ToJson(t);
            EditorGUIUtility.systemCopyBuffer = s;
        }

        [MenuItem("CONTEXT/MeshRenderer/Copy Bounds Center Position")]
        public static void CopyBoundsCenterPosition(MenuCommand command)
        {
            MeshRenderer renderer = (MeshRenderer)command.context;
            var p = renderer.bounds.center;
            var s = "Vector3" + p.ToString();
            EditorGUIUtility.systemCopyBuffer = s;
        }
    }
}