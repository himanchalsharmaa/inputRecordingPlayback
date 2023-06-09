﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface to implement to react to focus changed events.
    /// </summary>
    /// <remarks>
    /// <para>The events on this interface are related to those of <see cref="IMixedRealityFocusHandler"/>, whose event have
    /// a known ordering with this interface:</para>
    /// <para>IMixedRealityFocusChangedHandler::OnBeforeFocusChange</para>
    /// <para>IMixedRealityFocusHandler::OnFocusEnter</para>
    /// <para>IMixedRealityFocusHandler::OnFocusExit</para>
    /// <para>IMixedRealityFocusChangedHandler::OnFocusChanged</para>
    /// <para>Because these two interfaces are different, consumers must be wary about having nested
    /// hierarchies where some game objects will implement both interfaces, and more deeply nested
    /// object within the same parent-child chain that implement a single one of these - such
    /// a presence can lead to scenarios where one interface is invoked on the child object, and then
    /// the other interface is invoked on the parent object (thus, the parent would "miss" getting
    /// the event that the child had already processed).</para>
    /// </remarks>
    public interface IMixedRealityFocusChangedHandler : IEventSystemHandler
    {
        /// <summary>
        /// Focus event that is raised before the focus is actually changed.
        /// </summary>
        /// <remarks>Useful for logic that needs to take place before focus changes.</remarks>
        void OnBeforeFocusChange(FocusEventData eventData);

        /// <summary>
        /// Focus event that is raised when the focused object is changed.
        /// </summary>
        void OnFocusChanged(FocusEventData eventData);
    }
}
