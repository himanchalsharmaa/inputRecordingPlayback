﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.﻿

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.Editor;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Utilities.Editor
{
    [CustomEditor(typeof(BaseMousePointer))]
    public class BaseMousePointerInspector : BaseControllerPointerInspector
    {
        private SerializedProperty hideCursorWhenInactive;
        private SerializedProperty hideTimeout;
        private SerializedProperty movementThresholdToUnHide;
        private bool mousePointerFoldout = true;

        protected override void OnEnable()
        {
            DrawBasePointerActions = false;
            base.OnEnable();

            hideCursorWhenInactive = serializedObject.FindProperty("hideCursorWhenInactive");
            movementThresholdToUnHide = serializedObject.FindProperty("movementThresholdToUnHide");
            hideTimeout = serializedObject.FindProperty("hideTimeout");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            mousePointerFoldout = EditorGUILayout.Foldout(mousePointerFoldout, "Mouse Pointer Settings", true);

            if (mousePointerFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hideCursorWhenInactive);

                if (hideCursorWhenInactive.boolValue)
                {
                    EditorGUILayout.PropertyField(hideTimeout);
                    EditorGUILayout.PropertyField(movementThresholdToUnHide);
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}