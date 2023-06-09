﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Automatically loads a given Mixed Reality Toolkit configuration profile when loading up the scene. 
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/LoadProfilesOnStartup")]
    public class LoadProfilesOnStartup : MonoBehaviour
    {
        [Tooltip("Mixed Reality Toolkit profile to load when starting up this scene.")]
        [SerializeField]
        private MixedRealityToolkitConfigurationProfile configProfile = null;

        private void Update()
        {
            if ((configProfile != null) && (MixedRealityToolkit.Instance != null))
            {
                MixedRealityToolkit.Instance.ActiveProfile = configProfile;
                Debug.Log($"Loading new MRTK configuration profile: {configProfile.name}");
                configProfile = null;
            }
        }
    }
}
