using System.Collections.Generic;
using System.IO;
using ADV;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Utility methods for use with TalkScenes in main game roaming mode
    /// </summary>
    public static class TalkSceneUtils
    {
        /// <summary>
        /// Get scenario data for a specified girl. The data is inside abdata\adv\scenario.
        /// </summary>
        /// <example>
        /// var senarioData = TalkSceneUtils.GetSenarioData(talkScene.targetHeroine, "42"); // 42 - angry event
        /// </example>
        /// <param name="girl">Girl to get the scenario data for</param>
        /// <param name="asset">Scenario name as seen inside the bundles</param>
        public static List<ScenarioData.Param> GetSenarioData(SaveData.Heroine girl, string asset)
        {
            var files = Directory.GetFiles(Path.Combine(Paths.GameRootPath, "abdata\\adv\\scenario\\" + girl.ChaName), "??.unity3d");
            foreach (var path in files)
            {
                var assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(
                    "adv/scenario/" + girl.ChaName + "/" + Path.GetFileName(path), asset, typeof(ScenarioData));
                if (assetBundleLoadAssetOperation != null)
                {
                    var asset2 = assetBundleLoadAssetOperation.GetAsset<ScenarioData>();
                    if (!(asset2 == null) && asset2.list != null && asset2.list.Count != 0) return asset2.list;
                }
            }

            return null;
        }

        /// <summary>
        /// Where to touch. Same as clicking on the character with your mouse.
        /// </summary>
        public enum TouchLocation
        {
            /// <summary>
            /// Works in both Touch kinds
            /// </summary>
            Head, 
            /// <summary>
            /// Only works with Touch kind
            /// </summary>
            Cheek,
            /// <summary>
            /// Only works with Touch kind
            /// </summary>
            HandL,
            /// <summary>
            /// Only works with Touch kind
            /// </summary>
            HandR,
            /// <summary>
            /// Works in both Touch kinds
            /// </summary>
            MuneL, 
            /// <summary>
            /// Works in both Touch kinds
            /// </summary>
            MuneR
        }

        /// <summary>
        /// How to touch. Represents the eye and hand buttons at top right corner.
        /// </summary>
        public enum TouchKind
        {
            /// <summary>
            /// Mild reaction, doesn't work with some TouchLocations
            /// </summary>
            Look = 0,
            /// <summary>
            /// Major reaction, works with all TouchLocations
            /// </summary>
            Touch = 1
        }

        /// <summary>
        /// Simulate touching the character in TalkScene with mouse.
        /// </summary>
        /// <param name="talkScene">Talk scene</param>
        /// <param name="touchLocation">Where to touch</param>
        /// <param name="touchKind">How to touch</param>
        /// <param name="touchPosition">Optional position at which the touch happened (essentially mouse position)</param>
        public static void Touch(this TalkScene talkScene, TouchLocation touchLocation, TouchKind touchKind, Vector3 touchPosition = default)
        {
            var tv = Traverse.Create(talkScene);

            var tmf = tv.Field<int>("m_touchMode");
            var prevKind = tmf.Value;
            tmf.Value = (int)touchKind;

            tv.Method("TouchFunc", new[] { typeof(string), typeof(Vector3) }).GetValue(touchLocation.ToString(), touchPosition);

            tmf.Value = prevKind;
        }
    }
}