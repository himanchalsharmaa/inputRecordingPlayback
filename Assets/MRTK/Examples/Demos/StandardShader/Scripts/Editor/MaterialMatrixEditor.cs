﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.﻿

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.StandardShader
{
    /// <summary>
    /// Editor to build a matrix of spheres demonstrating a spectrum of material properties.
    /// </summary>
    [CustomEditor(typeof(MaterialMatrix))]
    public class MaterialMatrixEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Build"))
            {
                var materialMatrix = target as MaterialMatrix;
                Debug.Assert(materialMatrix != null);
                materialMatrix.BuildMatrix();

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
