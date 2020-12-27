using ExtensibleSaveFormat;
using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Base type for custom scene/studio extensions.
    /// It provides many useful methods that abstract away the nasty hooks needed to figure out when
    /// a scene is loaded or imported, or how to save and load your custom data to the scene file.
    /// 
    /// This controller is a MonoBehaviour that is created upon registration in <see cref="StudioSaveLoadApi.RegisterExtraBehaviour{T}"/>. 
    /// The controller is created only once. If it's created too late it might miss some scene load events.
    /// It's recommended to register controllers in your Start method.
    /// </summary>
    public abstract class SceneCustomFunctionController : MonoBehaviour
    {
        /// <summary>
        /// Fired when a scene is successfully changed, either by loading, importing or resetting.
        /// </summary>
        /// <param name="operation">Operation that caused this event</param>
        /// <param name="loadedItems">A dictionary of items loaded by this operation and their original IDs.
        /// The IDs are identical to the IDs at the time of saving the scene, even during import.
        /// Warning: The IDs here might not be the same as IDs of the objects in the scene!
        /// To get current scene ID of the object call <see cref="StudioObjectExtensions.GetSceneId(ObjectCtrlInfo)"/></param>
        protected internal abstract void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems);

        /// <summary>
        /// Fired when a scene is about to be saved and any exteneded data needs to be written.
        /// </summary>
        protected internal abstract void OnSceneSave();

        /// <summary>
        /// Fired when objects are copied.
        /// </summary>
        /// <param name="copiedItems">A dictionary of items loaded by this operation and their original IDs.
        /// The IDs match the original object in the scene.
        /// To get current scene ID of the object call <see cref="StudioObjectExtensions.GetSceneId(ObjectCtrlInfo)"/></param>
        protected internal virtual void OnObjectsCopied(ReadOnlyDictionary<int, ObjectCtrlInfo> copiedItems) { }

        /// <summary>
        /// Fired when objects are deleted.
        /// </summary>
        /// <param name="objectCtrlInfo">ObjectCtrlInfo associated with the item being deleted</param>
        protected internal virtual void OnObjectDeleted(ObjectCtrlInfo objectCtrlInfo) { }

        /// <summary>
        /// Fired when objects have their visibility toggled.
        /// </summary>
        /// <param name="objectCtrlInfo">ObjectCtrlInfo associated with the item being toggled</param>
        /// <param name="visible">Visibility of the object</param>
        protected internal virtual void OnObjectVisibilityToggled(ObjectCtrlInfo objectCtrlInfo, bool visible) { }

        /// <summary>
        /// Fired when objects are selected.
        /// </summary>
        /// <param name="objectCtrlInfo">ObjectCtrlInfo associated with the item being selected</param>
        protected internal virtual void OnObjectsSelected(List<ObjectCtrlInfo> objectCtrlInfo) { }

        /// <summary>
        /// ID used for extended data by this controller. It's set when registering the controller
        /// with <see cref="StudioSaveLoadApi.RegisterExtraBehaviour{T}(string)"/>
        /// </summary>
        public string ExtendedDataId { get; internal set; }

        /// <summary>
        /// Get extended data of the last loaded scene by using the ID you specified when registering this controller.
        /// </summary>
        public PluginData GetExtendedData()
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            return ExtendedSave.GetSceneExtendedDataById(ExtendedDataId);
        }

        /// <summary>
        /// Save your custom data to the scene under the ID you specified when registering this controller.
        /// </summary>
        /// <param name="data">Your custom data to be written to the scene. Can be null to remove the data.</param>
        public void SetExtendedData(PluginData data)
        {
            if (ExtendedDataId == null) throw new ArgumentException(nameof(ExtendedDataId));
            ExtendedSave.SetSceneExtendedDataById(ExtendedDataId, data);
        }

        /// <summary>
        /// Get the instance of the Studio game manager object.
        /// </summary>
        public global::Studio.Studio GetStudio() => global::Studio.Studio.Instance;
    }
}
