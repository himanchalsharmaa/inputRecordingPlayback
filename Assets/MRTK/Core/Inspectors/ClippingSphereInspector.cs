﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Editor
{
    /// <summary>
    /// A custom editor for the ClippingSphere to allow for specification of the framing bounds.
    /// </summary>
    [CustomEditor(typeof(ClippingSphere))]
    [CanEditMultipleObjects]
    public class ClippingSphereEditor : ClippingPrimitiveEditor
    {
        /// <inheritdoc/>
        protected override bool HasFrameBounds()
        {
            return true;
        }

        /// <inheritdoc/>
        protected override Bounds OnGetFrameBounds()
        {
            var primitive = target as ClippingSphere;
            Debug.Assert(primitive != null);
            return new Bounds(primitive.transform.position, primitive.Radii);
        }
    }
}
