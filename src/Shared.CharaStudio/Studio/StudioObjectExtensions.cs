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
        internal static Dictionary<TreeNodeObject, ObjectCtrlInfo> dicTreeNodeOCI = new Dictionary<TreeNodeObject, ObjectCtrlInfo>();

        private static global::Studio.Studio Studio => global::Studio.Studio.Instance;

        /// <summary>
        /// Get the ObjectCtrlInfo object that uses this ObjectInfo.
        /// If the object was not found in current scene, null is returned.
        /// </summary>
        public static ObjectCtrlInfo GetObjectCtrlInfo(this ObjectInfo obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (Studio == null) throw new InvalidOperationException("Studio is not initialized yet!");

            return Studio.dicObjectCtrl.Values.FirstOrDefault(x => x.objectInfo == obj);
        }

        /// <summary>
        /// Get the ID of this object as used in the currently loaded scene.
        /// If the object was not found in current scene, -1 is returned.
        /// </summary>
        public static int GetSceneId(this ObjectCtrlInfo obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (Studio == null) throw new InvalidOperationException("Studio is not initialized yet!");

            if (Studio.dicObjectCtrl.TryGetValue(obj.objectInfo.dicKey, out var oci) && oci == obj)
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
            if (Studio == null) throw new InvalidOperationException("Studio is not initialized yet!");
            
            if (Studio.dicObjectCtrl.TryGetValue(obj.dicKey, out var oci) && oci.objectInfo == obj)
                return obj.dicKey;

            return -1;
        }

        /// <summary>
        /// Get GetOCIChar that is assigned to this character. Only works in CharaStudio, returns null elsewhere.
        /// </summary>
        public static OCIChar GetOCIChar(this ChaControl chaControl)
        {
            if (!StudioAPI.InsideStudio) return null;
            var infos = global::Studio.Studio.Instance.dicInfo;
            var charas = infos.Values.OfType<OCIChar>();
            return charas.FirstOrDefault(x => x.charInfo == chaControl);
        }

        /// <summary>
        /// Get the ObjectControlInfo corresponding to the TreeNodeObject.
        /// </summary>
        public static ObjectCtrlInfo GetOCI(this TreeNodeObject treeNodeObject)
        {
            if (treeNodeObject == null) throw new ArgumentNullException("TreeNodeObject reference was null!");
            bool rebuilt = false;
            if (!dicTreeNodeOCI.TryGetValue(treeNodeObject, out ObjectCtrlInfo oci))
            {
                RebuildDictionary();
                if (!dicTreeNodeOCI.TryGetValue(treeNodeObject, out oci)) return null;
            }
            if (oci.treeNodeObject != treeNodeObject)
            {
                if (!rebuilt) RebuildDictionary();
                else return null;
                if (!dicTreeNodeOCI.TryGetValue(treeNodeObject, out oci)) return null;
                if (oci.treeNodeObject != treeNodeObject) throw new ArgumentException("TreeNodeObject reference is unreliable! No corresponding OCI found.");
            }
            return oci;

            void RebuildDictionary()
            {
                rebuilt = true;
                dicTreeNodeOCI.Clear();
                foreach (var kvp in Studio.dicObjectCtrl)
                    dicTreeNodeOCI.Add(kvp.Value.treeNodeObject, kvp.Value);
            }
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
    }
}
