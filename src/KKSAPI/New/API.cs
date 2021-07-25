using System.Text;
using System.Threading.Tasks;
using BepInEx;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using ModdingAPI.CharaMaker;
using UnityEngine;

#pragma warning disable 1591 //todo

namespace ModdingAPI
{
    public static class API
    {
        /// <summary>
        /// Version of this assembly/plugin.
        /// WARNING: This is a const field, therefore it will be copied to your assembly!
        /// Use this field to check if the installed version of the plugin is up to date by adding this attribute to your plugin class:
        /// <code>[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]</code>
        /// THIS VALUE WILL NOT BE READ FROM THE INSTALLED VERSION, YOU WILL READ THE VALUE FROM THIS VERSION THAT YOU COMPILE YOUR PLUGIN AGAINST!
        /// More info: https://stackoverflow.com/questions/55984/what-is-the-difference-between-const-and-readonly
        /// </summary>
        public const string VersionConst = KoikatuAPI.VersionConst;

        /// <summary>
        /// GUID of this plugin, use for checking dependancies with <see cref="BepInDependency"/>."/>
        /// </summary>
        public const string GUID = KoikatuAPI.GUID;


        public static CharaFunctionAPI Chara => CharaGameSpecific;
        public static CharaMakerAPI Maker => MakerGameSpecific;
        public static CharaStudioAPI Studio => StudioGameSpecific;
        public static SceneAPI Scene => SceneGameSpecific;

        public static CharaFunctionAPI_Specific CharaGameSpecific { get; } = new CharaFunctionAPI_Specific();
        public static CharaMakerAPI_Specific MakerGameSpecific { get; } = new CharaMakerAPI_Specific();
        public static CharaStudioAPI_KKS StudioGameSpecific { get; } = new CharaStudioAPI_KKS();
        public static SceneAPI_Specific SceneGameSpecific { get; } = new SceneAPI_Specific();
    }

    //public class ChaFileWrapper
    //{
    //    public ChaFileWrapper(ChaFile chaFile)
    //    {
    //        ChaFile = chaFile;
    //    }
    //
    //    public ChaFile ChaFile { get; }
    //}
    //
    //public class ChaFileControlWrapper
    //{
    //    public ChaFileControlWrapper(ChaFileControl chaFileControl)
    //    {
    //        ChaFileControl = chaFileControl;
    //    }
    //
    //    public ChaFileControl ChaFileControl { get; }
    //}
    //
    //public class ChaControlWrapper
    //{
    //    public ChaControl ChaControl { get; }
    //
    //    public ChaControlWrapper(ChaControl chaControl)
    //    {
    //        ChaControl = chaControl;
    //    }
    //
    //    public GameObject[] GetAccessoryObjects() //todo AccessoryWrapper ?
    //    {
    //        return ChaControl.GetAccessoryObjects();
    //    }
    //}
    //
    //public class AccessoryWrapper
    //{
    //
    //}
    //
    //public class ChaFileCoordinateWrapper
    //{
    //    public ChaFileCoordinateWrapper(ChaFileCoordinate chaFileCoordinate)
    //    {
    //        ChaFileCoordinate = chaFileCoordinate;
    //    }
    //
    //    public ChaFileCoordinate ChaFileCoordinate { get; }
    //}
    //
    //public static class WrapperExtensions
    //{
    //    public static ChaFileControlWrapper Wrap(this ChaFileControl chaFileControl) => new ChaFileControlWrapper(chaFileControl);
    //    public static ChaFileWrapper Wrap(this ChaFile chaFile) => new ChaFileWrapper(chaFile);
    //    public static ChaControlWrapper Wrap(this ChaControl chaControl) => new ChaControlWrapper(chaControl);
    //    public static ChaFileCoordinateWrapper Wrap(this ChaFileCoordinate chaFileCoordinate) => new ChaFileCoordinateWrapper(chaFileCoordinate);
    //}
}
