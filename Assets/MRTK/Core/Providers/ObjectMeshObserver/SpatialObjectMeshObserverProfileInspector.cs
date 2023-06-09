﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialObjectMeshObserver
{
    [CustomEditor(typeof(SpatialObjectMeshObserverProfile))]
    public class SpatialObjectMeshObserverProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private const string ProfileTitle = "Object Mesh Observer Settings";
        private const string ProfileDescription = "Configuration settings for how an object is used to simulate a spatial awareness mesh.";

        // Object settings
        private SerializedProperty spatialMeshObject;

        // General settings
        private SerializedProperty startupBehavior;
        private SerializedProperty observationExtents;
        private SerializedProperty observerVolumeType;
        private SerializedProperty isStationaryObserver;
        private SerializedProperty updateInterval;

        // Physics settings
        private SerializedProperty meshPhysicsLayer;
        private SerializedProperty recalculateNormals;

        // Level of Detail settings
        private SerializedProperty levelOfDetail;
        private SerializedProperty trianglesPerCubicMeter;

        // Display settings
        private SerializedProperty displayOption;
        private SerializedProperty visibleMaterial;
        private SerializedProperty occlusionMaterial;

        private SerializedProperty physicsMaterial;

        private readonly GUIContent displayOptionContent = new GUIContent("Display Option");
        private readonly GUIContent lodContent = new GUIContent("Level of Detail");
        private readonly GUIContent volumeTypeContent = new GUIContent("Observer Shape");
        private readonly GUIContent physicsLayerContent = new GUIContent("Physics Layer");
        private readonly GUIContent trianglesPerCubicMeterContent = new GUIContent("Triangles/Cubic Meter");

        protected override void OnEnable()
        {
            base.OnEnable();

            // Object mesh
            spatialMeshObject = serializedObject.FindProperty("spatialMeshObject");

            // General settings
            startupBehavior = serializedObject.FindProperty("startupBehavior");
            observationExtents = serializedObject.FindProperty("observationExtents");
            observerVolumeType = serializedObject.FindProperty("observerVolumeType");
            isStationaryObserver = serializedObject.FindProperty("isStationaryObserver");
            updateInterval = serializedObject.FindProperty("updateInterval");

            // Mesh settings
            meshPhysicsLayer = serializedObject.FindProperty("meshPhysicsLayer");
            recalculateNormals = serializedObject.FindProperty("recalculateNormals");
            levelOfDetail = serializedObject.FindProperty("levelOfDetail");
            trianglesPerCubicMeter = serializedObject.FindProperty("trianglesPerCubicMeter");
            displayOption = serializedObject.FindProperty("displayOption");
            visibleMaterial = serializedObject.FindProperty("visibleMaterial");
            occlusionMaterial = serializedObject.FindProperty("occlusionMaterial");
            physicsMaterial = serializedObject.FindProperty("physicsMaterial");
        }

        public override void OnInspectorGUI()
        {
            if (!RenderProfileHeader(ProfileTitle, ProfileDescription, target, true, BackProfileType.SpatialAwareness))
            {
                return;
            }

            using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(spatialMeshObject);
                GameObject parent = spatialMeshObject.objectReferenceValue as GameObject;
                MeshFilter[] filters = (parent != null) ? parent.GetComponentsInChildren<MeshFilter>() : null;
                if ((filters == null) ||
                    (filters.Length == 0))
                {
                    EditorGUILayout.HelpBox("The specified Spatial Mesh Object is not specified or does not appear to be a 3D model.", MessageType.Warning);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(startupBehavior);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(updateInterval);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(isStationaryObserver);
                    EditorGUILayout.PropertyField(observerVolumeType, volumeTypeContent);
                    string message = string.Empty;
                    if (observerVolumeType.intValue == (int)VolumeType.AxisAlignedCube)
                    {
                        message = "Observed meshes will be aligned to the world coordinate space.";
                    }
                    else if (observerVolumeType.intValue == (int)VolumeType.UserAlignedCube)
                    {
                        message = "Observed meshes will be aligned to the user's coordinate space.";
                    }
                    else if (observerVolumeType.intValue == (int)VolumeType.Sphere)
                    {
                        message = "The X value of the Observation Extents will be used as the sphere radius.";
                    }
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                    EditorGUILayout.PropertyField(observationExtents);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Physics Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(meshPhysicsLayer, physicsLayerContent);
                    EditorGUILayout.PropertyField(recalculateNormals);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Level of Detail Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(levelOfDetail, lodContent);
                    EditorGUILayout.PropertyField(trianglesPerCubicMeter, trianglesPerCubicMeterContent);
                    EditorGUILayout.HelpBox("The value of Triangles per Cubic Meter is ignored unless Level of Detail is set to Custom.", MessageType.Info);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(displayOption, displayOptionContent);
                    EditorGUILayout.PropertyField(visibleMaterial);
                    EditorGUILayout.PropertyField(occlusionMaterial);
                    EditorGUILayout.PropertyField(physicsMaterial);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;

            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile != null &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile.ObserverConfigurations != null &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile.ObserverConfigurations.Any(s => s.ObserverProfile == profile);
        }
    }
}
