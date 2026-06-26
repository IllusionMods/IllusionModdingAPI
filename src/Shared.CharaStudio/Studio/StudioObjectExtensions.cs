using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if AI || HS2
using AIChara;
#endif

namespace KKAPI.Studio
{
    /// <summary>
    /// Useful extensions for studio metaobjects
    /// </summary>
    public static class StudioObjectExtensions
    {
        /// <summary>
        /// Get the ObjectCtrlInfo object that uses this ObjectInfo.
        /// If the object was not found in current scene, null is returned.
        /// </summary>
        public static ObjectCtrlInfo GetObjectCtrlInfo(this ObjectInfo obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (StudioAPI.StudioInstance == null) throw new InvalidOperationException("Studio is not initialized yet!");

            return StudioAPI.StudioInstance.dicObjectCtrl.Values.FirstOrDefault(x => x.objectInfo == obj);
        }

        /// <summary>
        /// Get the ID of this object as used in the currently loaded scene.
        /// If the object was not found in current scene, -1 is returned.
        /// </summary>
        public static int GetSceneId(this ObjectCtrlInfo obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (StudioAPI.StudioInstance == null) throw new InvalidOperationException("Studio is not initialized yet!");

            if (StudioAPI.StudioInstance.dicObjectCtrl.TryGetValue(obj.objectInfo.dicKey, out var oci) && oci == obj)
                return obj.objectInfo.dicKey;

            return -1;
        }

        /// <summary>
        /// Get the ID of this object as used in the currently loaded scene.
        /// If the object was not found in current scene, -1 is returned.
        /// </summary>
        public static int GetSceneId(this ObjectInfo obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (StudioAPI.StudioInstance == null) throw new InvalidOperationException("Studio is not initialized yet!");
            
            if (StudioAPI.StudioInstance.dicObjectCtrl.TryGetValue(obj.dicKey, out var oci) && oci.objectInfo == obj)
                return obj.dicKey;

            return -1;
        }

        /// <summary>
        /// Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere.
        /// </summary>
        public static OCIChar GetOCIChar(this ChaControl chaControl)
        {
            if (!StudioAPI.InsideStudio) return null;
            var infos = StudioAPI.StudioInstance.dicInfo;
            var charas = infos.Values.OfType<OCIChar>();
            return charas.FirstOrDefault(x => x.charInfo == chaControl);
        }

        /// <summary>
        /// Get character component for this studio object
        /// </summary>
#if PH
        public static Human GetChaControl(this OCIChar ociChar) => ociChar.charInfo.human;
#else
        public static ChaControl GetChaControl(this OCIChar ociChar) => ociChar.charInfo;
#endif

#if PH
        internal static List<Transform> Children(this Transform self)
        {
            List<Transform> list = new List<Transform>();
            for (int i = 0; i < self.childCount; i++)
                list.Add(self.GetChild(i));
            return list;
        }
#endif

        /// <summary>
        /// Try to get the ObjectCtrlInfo controlled by this TreeNodeObject. Returns null if the object is not found or if called outside of studio.
        /// </summary>
        public static bool TryGetObjectCtrlInfo(this TreeNodeObject tno, out ObjectCtrlInfo objectCtrlInfo)
        {
            if (!StudioAPI.StudioLoaded)
            {
                objectCtrlInfo = null;
                return false;
            }

            return StudioAPI.StudioInstance.dicInfo.TryGetValue(tno, out objectCtrlInfo);
        } 

        /// <summary>
        /// Recursively flatten this TreeNodeObject and all of its children into a single collection of TreeNodeObjects.
        /// </summary>
        public static IEnumerable<TreeNodeObject> Flatten(this TreeNodeObject treeNodeObject)
        {
            yield return treeNodeObject;
            foreach (var child in treeNodeObject.child)
            {
                foreach (var descendant in child.Flatten())
                {
                    yield return descendant;
                }
            }
        }
    }
}
