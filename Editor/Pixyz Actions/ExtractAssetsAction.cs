using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PixyzPlugin4Unity.UI;
using System.IO;
using UnityEditor;

public class ExtractAssetsAction : ActionInOut<IList<GameObject>, IList<GameObject>>
{
    [UserParameter]
    public bool extractMeshes = true;

    [UserParameter]
    public bool extractMaterials = true;

    [UserParameter]
    public string targetPath = "PiXYZ";

    [UserParameter]
    public bool replaceReferences = true;

    [HelperMethod]
    public void resetParameters()
    {
        extractMeshes = true;
        extractMaterials = true;
        targetPath = "PiXYZ";
        replaceReferences = true;
    }

    public override int id { get { return 72873680;} }
    public override string menuPathRuleEngine { get { return null;} }
    public override string menuPathToolbox { get { return "Scene Dressing/Extract Assets";} }
    public override string tooltip { get { return "Extract Assets to Project Folder";} }

    public override IList<GameObject> run(IList<GameObject> input)
    {
        Dictionary<Mesh, Mesh> meshes = new Dictionary<Mesh, Mesh>();
        Dictionary<Material, Material> materials = new Dictionary<Material, Material>();

        if (extractMeshes)
        {
            foreach (var gameObject in input)
            {
                if (gameObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
                {
                    if (!meshes.ContainsKey(meshFilter.sharedMesh))
                    {
                        var path = Path.Combine(targetPath, string.Format("{0}/Meshes", gameObject.transform.root.name));
                        meshes.Add(meshFilter.sharedMesh, ExtractMesh(meshFilter.sharedMesh, path, gameObject.name));
                    }

                    if (replaceReferences)
                        AssignMesh(meshFilter, meshes[meshFilter.sharedMesh], true);
                }
            }
        }

        if (extractMaterials)
        {
            foreach (var gameObject in input)
            {
                if (gameObject.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                {
                    for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                    {
                        if (!materials.ContainsKey(meshRenderer.sharedMaterials[i]))
                        {
                            var path = Path.Combine(targetPath, string.Format("{0}/Materials", gameObject.transform.root.name));
                            materials.Add(meshRenderer.sharedMaterials[i], ExtractMaterial(meshRenderer.sharedMaterials[i], path));
                        }

                        if (replaceReferences)
                            AssignMaterial(meshRenderer, i, materials[meshRenderer.sharedMaterials[i]], true);
                    }
                }
            }
        }

        return input;
    }

    Mesh ExtractMesh(Mesh original, string path = "", string name = "")
    {
        if (path == string.Empty)
            path = "Assets/_Meshes";
        else
            path = Path.Combine("Assets", path);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (name == string.Empty)
            name = original.name;

        Mesh newMesh = Object.Instantiate(original);
        string destinationPath = Path.Combine(path, name);
        destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".asset");
        AssetDatabase.CreateAsset(newMesh, destinationPath);

        return AssetDatabase.LoadAssetAtPath<Mesh>(destinationPath);
    }

    Material ExtractMaterial(Material original, string path = "", string name = "")
    {
        if (path == string.Empty)
            path = "Assets/_Materials";
        else
            path = Path.Combine("Assets", path);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (name == string.Empty)
            name = original.name;

        Material material = new Material(original);
        string destinationPath = Path.Combine(path, name);
        destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".mat");
        AssetDatabase.CreateAsset(material, destinationPath);

        return AssetDatabase.LoadAssetAtPath<Material>(destinationPath);
    }

    void AssignMesh(MeshFilter meshFilter, Mesh mesh, bool applyOverridesToMostInnerPrefab = false)
    {
        var undoLvl = Undo.GetCurrentGroup();
        Undo.RecordObject(meshFilter, "Replace Mesh");

        meshFilter.sharedMesh = mesh;

        if (PrefabUtility.IsPartOfPrefabInstance(meshFilter))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
        }

        if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshFilter) && applyOverridesToMostInnerPrefab)
        {
            var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshFilter);
            var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
            //ApplyMeshOverrides(meshFilter, leafPath);
        }

        Undo.SetCurrentGroupName("Replace Mesh");
        Undo.CollapseUndoOperations(undoLvl);
    }

    void AssignMaterial(MeshRenderer meshRenderer, int index, Material material, bool applyOverridesToMostInnerPrefab = false)
    {
        var undoLvl = Undo.GetCurrentGroup();
        Undo.RecordObject(meshRenderer, "Replace Material");

        if (meshRenderer.sharedMaterials.Length == 1)
        {
            meshRenderer.sharedMaterial = material;
        }
        else
        {
            var materials = meshRenderer.sharedMaterials;
            materials[index] = material;
            meshRenderer.sharedMaterials = materials;
        }

        if (PrefabUtility.IsPartOfPrefabInstance(meshRenderer))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshRenderer);
        }

        if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshRenderer) && applyOverridesToMostInnerPrefab)
        {
            var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer);
            var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
            //ApplyMaterialsOverrides(meshRenderer, leafPath);
        }

        Undo.SetCurrentGroupName("Replace Material");
        Undo.CollapseUndoOperations(undoLvl);
    }

    //void ApplyMeshOverrides(MeshFilter meshFilter, string path)
    //{
    //    SerializedObject serializedObject = new SerializedObject(meshFilter);
    //    SerializedProperty serializedProperty = serializedObject.FindProperty("m_Mesh");

    //    if (serializedProperty != null)
    //    {
    //        PrefabUtility.ApplyPropertyOverride(serializedProperty, path, InteractionMode.AutomatedAction);
    //    }
    //}

    //void ApplyMaterialsOverrides(MeshRenderer meshRenderer, string path)
    //{
    //    SerializedObject serializedObject = new SerializedObject(meshRenderer);
    //    SerializedProperty serializedProperty = serializedObject.FindProperty("m_Materials");

    //    if (serializedProperty != null)
    //    {
    //        PrefabUtility.ApplyPropertyOverride(serializedProperty, path, InteractionMode.AutomatedAction);
    //        if (serializedProperty.arraySize > 0)
    //        {
    //            for (var i = 0; i < serializedProperty.arraySize; i++)
    //            {
    //                var element = serializedProperty.GetArrayElementAtIndex(i);
    //                //Debug.LogFormat("<color=cyan>{0} --> {1}</color>", element.propertyPath, element.objectReferenceValue);
    //                PrefabUtility.ApplyPropertyOverride(element, path, InteractionMode.AutomatedAction);
    //            }
    //        }
    //    }
    //}
}