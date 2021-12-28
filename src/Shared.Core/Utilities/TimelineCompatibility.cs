#if !EC // No studio
using Studio;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using BepInEx.Bootstrap;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// API for adding Timeline support to other plugins. Support is done by registering interpolable models that appear in the interpolables list in Timeline.
    /// </summary>
    /// <remarks>
    /// Always call IsTimelineAvailable first and only call other methods in this class if it returns <c>true</c>.
    /// <example>
    /// Example usage (assuming using a KKAPI chara function controller):
    /// <code>
    /// if (TimelineCompatibility.IsTimelineAvailable())
    /// {
    ///     TimelineCompatibility.AddCharaFunctionInterpolable&lt;int, PregnancyCharaController&gt;(
    ///         GUID, "week", "Pregnancy week",
    ///         (oci, parameter, leftValue, rightValue, factor) => parameter.Data.Week = Mathf.RoundToInt(Mathf.LerpUnclamped(leftValue, rightValue, factor)),
    ///         null,
    ///         (oci, parameter) => parameter.Data.Week);
    /// }
    /// </code>
    /// You can use other methods in this class if you are not working with function controllers.
    /// </example>
    /// This API probably won't work if called from a thread.
    /// </remarks>
    public static class TimelineCompatibility
    {
        /// <inheritdoc cref="InterpolableDelegate{TValue,TParameter}"/>
        public delegate void InterpolableCharaDelegate<in TValue, in TParameter>(OCIChar oci, TParameter parameter, TValue leftValue, TValue rightValue, float factor);

        /// <inheritdoc cref="InterpolableDelegate{TValue,TParameter}"/>
        public delegate void InterpolableDelegate(ObjectCtrlInfo oci, object parameter, object leftValue, object rightValue, float factor);

        /// <summary>
        /// Action that applies a value interpolated by Timeline.
        /// </summary>
        /// <remarks>
        /// <example>Example callback that changes a transform's position:<code>((Transform)parameter).localPosition = Vector3.LerpUnclamped((Vector3)leftValue, (Vector3)rightValue, factor);</code></example>
        /// If <typeparamref name="TValue"/> is a <c>bool</c> then simply use the <paramref name="leftValue"/> directly.
        /// </remarks>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TParameter">The type of the parameter.</typeparam>
        /// <param name="oci">Target studio object.</param>
        /// <param name="parameter">Parameter that's being worked on.</param>
        /// <param name="leftValue">Value of the interpolable on the left side of current playback position.</param>
        /// <param name="rightValue">Value of the interpolable on the right side of current playback position.</param>
        /// <param name="factor">
        /// Point between <paramref name="leftValue"/> and <paramref name="rightValue"/>we are currently at (percentage in 0-1 range).
        /// Use with <see cref="Mathf.LerpUnclamped"/> and similar methods to get the final value. Make sure to use Unclamped lerp methods unless you need the percentage to be in the 0-1 range.
        /// </param>
        public delegate void InterpolableDelegate<in TValue, in TParameter>(ObjectCtrlInfo oci, TParameter parameter, TValue leftValue, TValue rightValue, float factor);

        private static Func<float> _getPlaybackTime;
        private static Func<float> _getDuration;
        private static Func<bool> _getIsPlaying;
        private static Action _play;
        private static MethodInfo _addInterpolableModelStatic;
        private static MethodInfo _addInterpolableModelDynamic;
        private static Action _refreshInterpolablesList;
        private static Type _interpolableDelegate;

        private static bool _initialized;
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Check if Timeline is loaded and available to be used. If false, other methods in this class will throw.
        /// This must be called after all plugins finish loadeding (in Start/Main instead of constructors/Awake).
        /// It always returns false outside of studio.
        /// </summary>
        public static bool IsTimelineAvailable()
        {
            lock (_lockObj) //todo unnecessary since other methods can't be used from a bg thread anyways?
            {
                if (_initialized) return _interpolableDelegate != null;

                // Ensure that Timeline dll was loaded by now
                if (!(bool)AccessTools.Field(typeof(Chainloader), "_loaded").GetValue(null))
                    throw new InvalidOperationException("This API can't be used before all plugins finish loadeding (in constructors/Awake). Use it inside Start or Main instead.");
                
                if (!StudioAPI.InsideStudio)
                {
                    _initialized = true;
                    return false;
                }

                try
                {
                    Type timelineType = Type.GetType("Timeline.Timeline,Timeline", false);
                    if (timelineType != null)
                    {
                        _getPlaybackTime = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), timelineType.GetProperty("playbackTime", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
                        _getDuration = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), timelineType.GetProperty("duration", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
                        _getIsPlaying = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), timelineType.GetProperty("isPlaying", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
                        _play = (Action)Delegate.CreateDelegate(typeof(Action), timelineType.GetMethod("Play", BindingFlags.Public | BindingFlags.Static));
                        _addInterpolableModelStatic = timelineType.GetMethod("AddInterpolableModelStatic", BindingFlags.Public | BindingFlags.Static);
                        _addInterpolableModelDynamic = timelineType.GetMethod("AddInterpolableModelDynamic", BindingFlags.Public | BindingFlags.Static);
                        _refreshInterpolablesList = (Action)Delegate.CreateDelegate(typeof(Action), timelineType.GetMethod("RefreshInterpolablesList", BindingFlags.Public | BindingFlags.Static));
                        _interpolableDelegate = Type.GetType("Timeline.InterpolableDelegate,Timeline");
                        _initialized = true;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Exception caught when trying to find Timeline: " + e);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets current playback location in seconds.
        /// </summary>
        public static float GetPlaybackTime()
        {
            InitCheck();
            return _getPlaybackTime();
        }

        /// <summary>
        /// Gets the total duration in seconds.
        /// </summary>
        public static float GetDuration()
        {
            InitCheck();
            return _getDuration();
        }

        /// <summary>
        /// Check if timeline is currently playing.
        /// </summary>
        public static bool GetIsPlaying()
        {
            InitCheck();
            return _getIsPlaying();
        }

        /// <summary>
        /// Play/Pause timeline playback (toggles between the two).
        /// </summary>
        public static void Play()
        {
            InitCheck();
            _play();
        }

        /// <summary>
        /// Adds an interpolableModel with a dynamic parameter to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.
        /// The dynamic parameter means that every time user creates this interpolable, you can provide a different parameter - this allows having multiple instances of this interpolable for a single studio object. The instances are differentiated by the parameter, which must be unique and have a unique hash code (if the parameters have identical hashes they are treated as the same thing).
        /// Examples where this is useful: editing individual character bone positions/scales, changing color of individual accessories.
        /// If you want to make a global interpolable you can use AddInterpolableModelStatic instead.
        /// Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks). If you need to find some object to later use in the callbacks, consider using it as the parameter (that way it will only be computed once and you can use it later for free, just make sure the parameter is always the same since if you have different parameters will create separate instances).
        /// </summary>
        /// <remarks>
        /// Use cases:
        /// <list type="bullet">
        /// <item>To make a global interpolable (affects overall studio state) set <paramref name="useOciInHash"/> to false and don't provide unique parameters (or any parameters at all).</item>
        /// <item>To make a single interpolable per studio object (e.g. changing a character's hair color) set <paramref name="useOciInHash"/> to true and don't provide unique parameters (or any parameters at all).</item>
        /// <item>To make multiple interpolables per studio object (e.g. changing color of different clothing items) set <paramref name="useOciInHash"/> to true and provide unique parameters that represent the exact thing that is being changed.
        /// How this works: When user adds a new interpolable then</item>
        /// </list>
        /// Execution order:
        /// <list type="number">
        /// <item>InterpolableModel is created and added</item>
        /// <item><paramref name="isCompatibleWithTarget"/> is called when any studio object is selected (null if nothing is selected)</item>
        /// <item>When user adds a new keyframe for this interpolable, <paramref name="isCompatibleWithTarget"/> is called to determine if it can be created.</item>
        /// <item><paramref name="getParameter"/> and <paramref name="getValue"/> are called in that order. The new interpolable's hash is computed based on <paramref name="useOciInHash"/> and the returned parameter.</item>
        /// <item><paramref name="shouldShow"/> and <paramref name="getFinalName"/> are called after creating the interpolable and every time the user does something that should update the UI.</item>
        /// <item>If playing, <paramref name="interpolateBefore"/> and <paramref name="interpolateAfter"/> are called in that order. <paramref name="checkIntegrity"/> called before each of them to determine if the callbacks should be ran. Put logic that affects the studio object here (e.g. changing bone position).</item>
        /// <item>When saving the studio scene or just the timeline data, <paramref name="writeParameterToXml"/> and <paramref name="writeValueToXml"/> are called as necessary.</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TParameter">The type of the parameter.</typeparam>
        /// <param name="owner">Display name of the owner of this interpolable. It's used both in the UI and for serialization. Must not change between game restarts.</param>
        /// <param name="id">ID of this interpolable used for serialization. Must be unique and not change between game restarts.</param>
        /// <param name="name">Display name shown in the available interpolable list. It's also used for names of the created interpolable instances if <paramref name="getFinalName"/> is not provided.</param>
        /// <param name="interpolateBefore">
        /// Action to do around the start of the frame (inside Update).
        /// Make changes to the object state here (e.g. change transform position).
        /// <example><code>((Transform)parameter).localPosition = Vector3.LerpUnclamped((Vector3)leftValue, (Vector3)rightValue, factor);</code></example>
        /// </param>
        /// <param name="interpolateAfter">
        /// Action to do around the end of the frame (inside LateUpdate).
        /// Make changes to the object state here (e.g. change transform position) if you need to do them inside LateUpdate.
        /// <example><code>((Transform)parameter).localPosition = Vector3.LerpUnclamped((Vector3)leftValue, (Vector3)rightValue, factor);</code></example>
        /// </param>
        /// <param name="isCompatibleWithTarget">
        /// Func that should return true if this interpolable can be used on the given studio object.
        /// This is called before all other callbacks and before the interpolable is actually created.
        /// <example><code>oci is OCIChar</code></example>
        /// </param>
        /// <param name="getValue">
        /// Get the current value of this interpolable.
        /// If loading timeline data, <paramref name="readValueFromXml"/> is used instead.
        /// <example><code>((Transform)parameter).localPosition</code></example>
        /// </param>
        /// <param name="readValueFromXml">
        /// Deserialize a value of this interpolable from XML.
        /// It's optional as long as you use a common simple value type like <see cref="float"/> or <see cref="Vector3"/> that has a deserializer available.
        /// <example><code>node.ReadVector3("value")</code></example>
        /// </param>
        /// <param name="writeValueToXml">
        /// Serialize a value of this interpolable to XML.
        /// It's optional as long as you use a common simple value type like <see cref="float"/> or <see cref="Vector3"/> that has a serializer available.
        /// <example><code>writer.WriteValue("value", (Vector3)obj)</code></example>
        /// </param>
        /// <param name="getParameter">
        /// Get object/data to apply changes to.
        /// The main use of parameters is when you can have multiple instances of this interpolable on a single studio object - you can then use parameters to differentiate individual parts, e.g. bones.
        /// For example you can use the transform that this interporable will affect (set its position etc.) as a parameter - that way you can have separate interpolables for different transforms of the same studio object.
        /// This parameter will be passed to other callbacks, and it will be serialized and deserialized when saving/loading timeline data (serializing it is optional but without serializing the parameter you will lose it after scene reload).
        /// If you are using parameters, make sure to implement <paramref name="getFinalName"/> as well - it's used to give different names to interpolables based on the parameter (e.g. transform name).
        /// Note: Everything related to parameters is optional and can be skipped if you don't need multiple instances of the interpolable on a single studio object.
        /// Parameters should be hashable (hash should be stable during the game session).
        /// <example><code>((OCIChar)oci).GetChaControl().transform.Find("someBone")</code></example>
        /// </param>
        /// <param name="readParameterFromXml">
        /// Deserialize a parameter previosuly saved with <paramref name="writeParameterToXml"/>.
        /// <example><code>return oci.guideObject.transformTarget.transform.Find(node.Attributes["parameter"].Value));</code></example>
        /// </param>
        /// <param name="writeParameterToXml">
        /// Serialize the parameter to store it inside timeline data.
        /// It will be later deserialized with <paramref name="readParameterFromXml"/>.
        /// For example if you use a <see cref="UnityEngine.Transform"/> for your parameter, you can write its path or name if its unique, and then search for it when deserializing.
        /// <example><code>writer.WriteAttributeString("parameter", ((Transform)parameter).name);</code></example>
        /// </param>
        /// <param name="checkIntegrity">
        /// Check if the parameter and data are valid and can be interpolated.
        /// If false is returned, <paramref name="interpolateBefore"/> and <paramref name="interpolateAfter"/> are not called (everything else is unaffected).
        /// </param>
        /// <param name="useOciInHash">If true, assign this interpolable to a specific studio object. If false, the interpolable is global and most callbacks will be given <c>null</c> as the <see cref="ObjectCtrlInfo"/> argument.</param>
        /// <param name="getFinalName">Get a name for a newly created interpolable instance. Use to differentiate different instances of the same interpolable. If <c>null</c>, <paramref name="name"/> is used.</param>
        /// <param name="shouldShow">Check if this interpolable <b>instance</b> should apper in the list given the current state. This is called on every list refresh <b>after</b> the interpolable is created.</param>
        public static void AddInterpolableModelDynamic<TValue, TParameter>(string owner,
            string id,
            string name,
            InterpolableDelegate<TValue, TParameter> interpolateBefore,
            InterpolableDelegate<TValue, TParameter> interpolateAfter,
            Func<ObjectCtrlInfo, bool> isCompatibleWithTarget,
            Func<ObjectCtrlInfo, TParameter, TValue> getValue,
            Func<TParameter, XmlNode, TValue> readValueFromXml = null,
            Action<TParameter, XmlTextWriter, TValue> writeValueToXml = null,
            Func<ObjectCtrlInfo, TParameter> getParameter = null,
            Func<ObjectCtrlInfo, XmlNode, TParameter> readParameterFromXml = null,
            Action<ObjectCtrlInfo, XmlTextWriter, TParameter> writeParameterToXml = null,
            Func<ObjectCtrlInfo, TParameter, TValue, TValue, bool> checkIntegrity = null,
            bool useOciInHash = true,
            Func<string, ObjectCtrlInfo, TParameter, string> getFinalName = null,
            Func<ObjectCtrlInfo, TParameter, bool> shouldShow = null)
        {
            var valueType = typeof(TValue);
            if (readValueFromXml == null)
            {
                if (XmlConvertHelper.IsTypeSupported(valueType))
                    readValueFromXml = (parameter, node) => (TValue)XmlConvertHelper.Deserialize(node.Attributes["value"].Value, valueType);
                else
                    throw new ArgumentNullException(nameof(readValueFromXml), $"Can not convert type {valueType} automatically, you have to provide custom readValueFromXml");
            }
            if (writeValueToXml == null)
            {
                if (XmlConvertHelper.IsTypeSupported(valueType))
                    writeValueToXml = (parameter, writer, val) => writer.WriteAttributeString("value", XmlConvertHelper.Serialize(val, valueType));
                else
                    throw new ArgumentNullException(nameof(writeValueToXml), $"Can not convert type {valueType} automatically, you have to provide custom writeValueToXml");
            }

            AddInterpolableModelDynamic(owner, id, name,
                interpolateBefore != null ? new InterpolableDelegate((oci, parameter, value, rightValue, factor) => interpolateBefore(oci, (TParameter)parameter, (TValue)value, (TValue)rightValue, factor)) : null,
                interpolateAfter != null ? new InterpolableDelegate((oci, parameter, value, rightValue, factor) => interpolateAfter(oci, (TParameter)parameter, (TValue)value, (TValue)rightValue, factor)) : null,
                isCompatibleWithTarget,
                getValue != null ? (info, param) => getValue(info, (TParameter)param) : (Func<ObjectCtrlInfo, object, object>)null,
                (param, node) => readValueFromXml((TParameter)param, node),
                (param, writer, val) => writeValueToXml((TParameter)param, writer, (TValue)val),
                getParameter != null ? info => getParameter(info) : (Func<ObjectCtrlInfo, object>)null,
                readParameterFromXml != null ? (info, node) => readParameterFromXml(info, node) : (Func<ObjectCtrlInfo, XmlNode, object>)null,
                writeParameterToXml != null ? (info, writer, param) => writeParameterToXml(info, writer, (TParameter)param) : (Action<ObjectCtrlInfo, XmlTextWriter, object>)null,
                checkIntegrity != null ? (info, parameter, left, right) => checkIntegrity(info, (TParameter)parameter, (TValue)left, (TValue)right) : (Func<ObjectCtrlInfo, object, object, object, bool>)null,
                useOciInHash,
                getFinalName != null ? (s, info, param) => getFinalName(s, info, (TParameter)param) : (Func<string, ObjectCtrlInfo, object, string>)null,
                shouldShow != null ? (info, parameter) => shouldShow(info, (TParameter)parameter) : (Func<ObjectCtrlInfo, object, bool>)null);
        }

        ///<inheritdoc cref="AddInterpolableModelDynamic{TValue,TParameter}"/>
        public static void AddInterpolableModelDynamic(string owner,
                                                       string id,
                                                       string name,
                                                       InterpolableDelegate interpolateBefore,
                                                       InterpolableDelegate interpolateAfter,
                                                       Func<ObjectCtrlInfo, bool> isCompatibleWithTarget,
                                                       Func<ObjectCtrlInfo, object, object> getValue,
                                                       Func<object, XmlNode, object> readValueFromXml,
                                                       Action<object, XmlTextWriter, object> writeValueToXml,
                                                       Func<ObjectCtrlInfo, object> getParameter,
                                                       Func<ObjectCtrlInfo, XmlNode, object> readParameterFromXml = null,
                                                       Action<ObjectCtrlInfo, XmlTextWriter, object> writeParameterToXml = null,
                                                       Func<ObjectCtrlInfo, object, object, object, bool> checkIntegrity = null,
                                                       bool useOciInHash = true,
                                                       Func<string, ObjectCtrlInfo, object, string> getFinalName = null,
                                                       Func<ObjectCtrlInfo, object, bool> shouldShow = null)
        {
            InitCheck();
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (isCompatibleWithTarget == null) throw new ArgumentNullException(nameof(isCompatibleWithTarget));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));

            if (readValueFromXml == null) throw new ArgumentNullException(nameof(readValueFromXml));
            if (writeValueToXml == null) throw new ArgumentNullException(nameof(writeValueToXml));

            Delegate ib = null;
            if (interpolateBefore != null)
                ib = Delegate.CreateDelegate(_interpolableDelegate, interpolateBefore.Target, interpolateBefore.Method);
            Delegate ia = null;
            if (interpolateAfter != null)
                ia = Delegate.CreateDelegate(_interpolableDelegate, interpolateAfter.Target, interpolateAfter.Method);
            _addInterpolableModelDynamic.Invoke(null, new object[]
            {
                owner,
                id,
                name,
                ib,
                ia,
                isCompatibleWithTarget,
                getValue,
                readValueFromXml,
                writeValueToXml,
                getParameter,
                readParameterFromXml,
                writeParameterToXml,
                checkIntegrity,
                useOciInHash,
                getFinalName,
                shouldShow
            });
        }


        /// <summary>
        /// Adds an interpolableModel to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.
        /// User can create a single instance of this interpolable for each studio object (or single instance globally if <paramref name="useOciInHash"/> is <c>false</c>). If you want to have multiple instances of this interpolable for a single studio object use AddInterpolableModelDynamic instead.
        /// Try to keep the callbacks as light and self-contained as possible since they can be called on every frame during playback or UI usage (except the xml callbacks).
        /// </summary>
        /// <param name="parameter">Optional parameter for this interpolable. Mostly useful for global interpolables where required objects might be expensive to get.</param>
        /// <inheritdoc cref="AddInterpolableModelDynamic{TValue,TParameter}"/>
#pragma warning disable 1573
        public static void AddInterpolableModelStatic<TValue, TParameter>(string owner,
            string id,
            TParameter parameter,
            string name,
            InterpolableDelegate<TValue, TParameter> interpolateBefore,
            InterpolableDelegate<TValue, TParameter> interpolateAfter,
            Func<ObjectCtrlInfo, bool> isCompatibleWithTarget,
            Func<ObjectCtrlInfo, TParameter, TValue> getValue,
            Func<TParameter, XmlNode, TValue> readValueFromXml = null,
            Action<TParameter, XmlTextWriter, TValue> writeValueToXml = null,
            Func<ObjectCtrlInfo, XmlNode, TParameter> readParameterFromXml = null,
            Action<ObjectCtrlInfo, XmlTextWriter, TParameter> writeParameterToXml = null,
            Func<ObjectCtrlInfo, TParameter, TValue, TValue, bool> checkIntegrity = null,
            bool useOciInHash = true,
            Func<string, ObjectCtrlInfo, TParameter, string> getFinalName = null,
            Func<ObjectCtrlInfo, TParameter, bool> shouldShow = null)
#pragma warning restore 1573
        {
            var valueType = typeof(TValue);
            if (readValueFromXml == null)
            {
                if (XmlConvertHelper.IsTypeSupported(valueType))
                    readValueFromXml = (param, node) => (TValue)XmlConvertHelper.Deserialize(node.Attributes["value"].Value, valueType);
                else
                    throw new ArgumentNullException(nameof(readValueFromXml), $"Can not convert type {valueType} automatically, you have to provide custom readValueFromXml");
            }
            if (writeValueToXml == null)
            {
                if (XmlConvertHelper.IsTypeSupported(valueType))
                    writeValueToXml = (param, writer, val) => writer.WriteAttributeString("value", XmlConvertHelper.Serialize(val, valueType));
                else
                    throw new ArgumentNullException(nameof(writeValueToXml), $"Can not convert type {valueType} automatically, you have to provide custom writeValueToXml");
            }

            AddInterpolableModelStatic(owner, id, parameter, name,
                interpolateBefore != null ? new InterpolableDelegate((oci, param, value, rightValue, factor) => interpolateBefore(oci, (TParameter)param, (TValue)value, (TValue)rightValue, factor)) : null,
                interpolateAfter != null ? new InterpolableDelegate((oci, param, value, rightValue, factor) => interpolateAfter(oci, (TParameter)param, (TValue)value, (TValue)rightValue, factor)) : null,
                isCompatibleWithTarget,
                getValue != null ? (info, param) => getValue(info, (TParameter)param) : (Func<ObjectCtrlInfo, object, object>)null,
                (param, node) => readValueFromXml((TParameter)param, node),
                (param, writer, val) => writeValueToXml((TParameter)param, writer, (TValue)val),
                readParameterFromXml != null ? (info, node) => readParameterFromXml(info, node) : (Func<ObjectCtrlInfo, XmlNode, object>)null,
                writeParameterToXml != null ? (info, writer, param) => writeParameterToXml(info, writer, (TParameter)param) : (Action<ObjectCtrlInfo, XmlTextWriter, object>)null,
                checkIntegrity != null ? (info, param, left, right) => checkIntegrity(info, (TParameter)param, (TValue)left, (TValue)right) : (Func<ObjectCtrlInfo, object, object, object, bool>)null,
                useOciInHash,
                getFinalName != null ? (s, info, param) => getFinalName(s, info, (TParameter)param) : (Func<string, ObjectCtrlInfo, object, string>)null,
                shouldShow != null ? (info, param) => shouldShow(info, (TParameter)param) : (Func<ObjectCtrlInfo, object, bool>)null);
        }

        /// <inheritdoc cref="AddInterpolableModelStatic{TValue,TParameter}"/>
        public static void AddInterpolableModelStatic(string owner,
                                                      string id,
                                                      object parameter,
                                                      string name,
                                                      InterpolableDelegate interpolateBefore,
                                                      InterpolableDelegate interpolateAfter,
                                                      Func<ObjectCtrlInfo, bool> isCompatibleWithTarget,
                                                      Func<ObjectCtrlInfo, object, object> getValue,
                                                      Func<object, XmlNode, object> readValueFromXml,
                                                      Action<object, XmlTextWriter, object> writeValueToXml,
                                                      Func<ObjectCtrlInfo, XmlNode, object> readParameterFromXml = null,
                                                      Action<ObjectCtrlInfo, XmlTextWriter, object> writeParameterToXml = null,
                                                      Func<ObjectCtrlInfo, object, object, object, bool> checkIntegrity = null,
                                                      bool useOciInHash = true,
                                                      Func<string, ObjectCtrlInfo, object, string> getFinalName = null,
                                                      Func<ObjectCtrlInfo, object, bool> shouldShow = null)
        {
            InitCheck();
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (isCompatibleWithTarget == null) throw new ArgumentNullException(nameof(isCompatibleWithTarget));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));
            if (readValueFromXml == null) throw new ArgumentNullException(nameof(readValueFromXml));
            if (writeValueToXml == null) throw new ArgumentNullException(nameof(writeValueToXml));

            Delegate ib = null;
            if (interpolateBefore != null)
                ib = Delegate.CreateDelegate(_interpolableDelegate, interpolateBefore.Target, interpolateBefore.Method);
            Delegate ia = null;
            if (interpolateAfter != null)
                ia = Delegate.CreateDelegate(_interpolableDelegate, interpolateAfter.Target, interpolateAfter.Method);
            _addInterpolableModelStatic.Invoke(null, new object[]
            {
                owner,
                id,
                parameter,
                name,
                ib,
                ia,
                isCompatibleWithTarget,
                getValue,
                readValueFromXml,
                writeValueToXml,
                readParameterFromXml,
                writeParameterToXml,
                checkIntegrity,
                useOciInHash,
                getFinalName,
                shouldShow
            });
        }

        /// <summary>
        /// Adds an interpolableModel that targets CharaCustomFunctionControllers on characters to the available interpolables list. This interpolable can then be instantiated by the user by adding a keyframe.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TController">Type of a plugin's <see cref="CharaCustomFunctionController"/>.</typeparam>
        /// <inheritdoc cref="AddInterpolableModelDynamic{TValue,TParameter}"/>
        public static void AddCharaFunctionInterpolable<TValue, TController>(string owner,
            string id,
            string name,
            InterpolableCharaDelegate<TValue, TController> interpolateBefore,
            InterpolableCharaDelegate<TValue, TController> interpolateAfter,
            Func<OCIChar, TController, TValue> getValue,
            Func<TController, XmlNode, TValue> readValueFromXml = null,
            Action<TController, XmlTextWriter, TValue> writeValueToXml = null,
            Func<OCIChar, TController, TValue, TValue, bool> checkIntegrity = null,
            bool useOciInHash = true,
            Func<string, OCIChar, TController, string> getFinalName = null,
            Func<OCIChar, TController, bool> shouldShow = null) where TController : CharaCustomFunctionController
        {
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));
            AddInterpolableModelDynamic<TValue, TController>(
                owner: owner,
                id: id,
                name: name,
                interpolateBefore: interpolateBefore != null
                    ? (oci, parameter, value, rightValue, factor) => interpolateBefore((OCIChar)oci, parameter, value, rightValue, factor)
                    : (InterpolableDelegate<TValue, TController>)null,
                interpolateAfter: interpolateAfter != null
                    ? (oci, parameter, value, rightValue, factor) => interpolateAfter((OCIChar)oci, parameter, value, rightValue, factor)
                    : (InterpolableDelegate<TValue, TController>)null,
                isCompatibleWithTarget: oci => oci is OCIChar,
                getValue: (oci, parameter) => getValue((OCIChar)oci, parameter),
                readValueFromXml: readValueFromXml,
                writeValueToXml: writeValueToXml,
                getParameter: oci => ((OCIChar)oci).GetChaControl().GetComponent<TController>(),
                checkIntegrity: checkIntegrity != null
                    ? (info, controller, leftValue, rightValue) => checkIntegrity((OCIChar)info, controller, leftValue, rightValue)
                    : (Func<ObjectCtrlInfo, TController, TValue, TValue, bool>)null,
                useOciInHash: useOciInHash,
                getFinalName: getFinalName != null
                    ? (string s, ObjectCtrlInfo info, TController param) => getFinalName(s, (OCIChar)info, param)
                    : (Func<string, ObjectCtrlInfo, TController, string>)null,
                shouldShow: shouldShow != null
                    ? (info, controller) => shouldShow((OCIChar)info, controller)
                    : (Func<ObjectCtrlInfo, TController, bool>)null
            );
        }

        /// <summary>
        /// Refreshes the list of displayed interpolables. This function is quite heavy as it must go through each InterpolableModel and check if it's compatible with the current target.
        /// It is called automatically by Timeline when selecting another Workspace object or GuideObject.
        /// This triggers visibility checks of all interpolables and can be used to force update all instances of your interpolables if something important changes and Timeline doesn't notice it.
        /// </summary>
        public static void RefreshInterpolablesList()
        {
            InitCheck();
            if (_refreshInterpolablesList != null)
                _refreshInterpolablesList();
        }

        private static void InitCheck()
        {
            if (!_initialized)
                throw new InvalidOperationException("TimelineCompatibility was not initialized. Use IsTimelineAvailable() first");
            if (_interpolableDelegate == null)
            {
                if (StudioAPI.InsideStudio)
                    throw new InvalidOperationException("Timeline is not installed or an incompatible version is installed. Use IsTimelineAvailable() to check if it's available before using this API");
                else
                    throw new InvalidOperationException("Timeline is only available inside studio. Use IsTimelineAvailable() to check if it's available before using this API");
            }
        }

        private static class XmlConvertHelper
        {
            //WriteLine(string.Join(", ",typeof(XmlConvert).GetMethods().Where(x=>x.Name.StartsWith("To") && x.Name != "ToString").Select(x=>x.Name.Substring(2)).Distinct().OrderBy(x=>x).Select(x=>$"typeof({x})")))
            private static HashSet<Type> _supportedTypes = new HashSet<Type>
            {
                typeof(Boolean), typeof(Byte), typeof(Char), typeof(DateTime), typeof(DateTimeOffset), typeof(Decimal),
                typeof(Double), typeof(Guid), typeof(Int16), typeof(Int32), typeof(Int64), typeof(SByte), typeof(Single),
                typeof(TimeSpan), typeof(UInt16), typeof(UInt32), typeof(UInt64),
                typeof(string),
                typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Quaternion), typeof(Color), typeof(Color32), typeof(Rect)
            };

            public static bool IsTypeSupported(Type t)
            {
                return _supportedTypes.Contains(t);
            }

            public static object Deserialize(string s, Type targetType)
            {
                if (targetType == typeof(string)) return XmlConvert.DecodeName(s);

                // WriteLine(string.Join("\r\n",typeof(XmlConvert).GetMethods().Where(x=>x.Name.StartsWith("To") && x.Name != "ToString").Select(x=>x.Name.Substring(2)).Distinct().OrderBy(x=>x).Select(x=>$"if (targetType == typeof({x})) return XmlConvert.To{x}(s);")))
                if (targetType == typeof(Boolean)) return XmlConvert.ToBoolean(s);
                if (targetType == typeof(Byte)) return XmlConvert.ToByte(s);
                if (targetType == typeof(Char)) return XmlConvert.ToChar(s);
                if (targetType == typeof(DateTime)) return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Utc);
                if (targetType == typeof(DateTimeOffset)) return XmlConvert.ToDateTimeOffset(s);
                if (targetType == typeof(Decimal)) return XmlConvert.ToDecimal(s);
                if (targetType == typeof(Double)) return XmlConvert.ToDouble(s);
                if (targetType == typeof(Guid)) return XmlConvert.ToGuid(s);
                if (targetType == typeof(Int16)) return XmlConvert.ToInt16(s);
                if (targetType == typeof(Int32)) return XmlConvert.ToInt32(s);
                if (targetType == typeof(Int64)) return XmlConvert.ToInt64(s);
                if (targetType == typeof(SByte)) return XmlConvert.ToSByte(s);
                if (targetType == typeof(Single)) return XmlConvert.ToSingle(s);
                if (targetType == typeof(TimeSpan)) return XmlConvert.ToTimeSpan(s);
                if (targetType == typeof(UInt16)) return XmlConvert.ToUInt16(s);
                if (targetType == typeof(UInt32)) return XmlConvert.ToUInt32(s);
                if (targetType == typeof(UInt64)) return XmlConvert.ToUInt64(s);

                if (targetType == typeof(Vector2))
                {
                    var parts = s.Split(';');
                    return new Vector2(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]));
                }
                if (targetType == typeof(Vector3))
                {
                    var parts = s.Split(';');
                    return new Vector3(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]), XmlConvert.ToSingle(parts[2]));
                }
                if (targetType == typeof(Vector4))
                {
                    var parts = s.Split(';');
                    return new Vector4(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]), XmlConvert.ToSingle(parts[2]), XmlConvert.ToSingle(parts[3]));
                }
                if (targetType == typeof(Quaternion))
                {
                    var parts = s.Split(';');
                    return new Quaternion(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]), XmlConvert.ToSingle(parts[2]), XmlConvert.ToSingle(parts[3]));
                }
                if (targetType == typeof(Color))
                {
                    var parts = s.Split(';');
                    return new Color(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]), XmlConvert.ToSingle(parts[2]), XmlConvert.ToSingle(parts[3]));
                }
                if (targetType == typeof(Color32))
                {
                    var parts = s.Split(';');
                    return new Color32(XmlConvert.ToByte(parts[0]), XmlConvert.ToByte(parts[1]), XmlConvert.ToByte(parts[2]), XmlConvert.ToByte(parts[3]));
                }
                if (targetType == typeof(Rect))
                {
                    var parts = s.Split(';');
                    return new Rect(XmlConvert.ToSingle(parts[0]), XmlConvert.ToSingle(parts[1]), XmlConvert.ToSingle(parts[2]), XmlConvert.ToSingle(parts[3]));
                }

                throw new NotSupportedException($"Could not convert type {targetType.FullName} with XmlConvert, you have to convert it manually");
            }

            public static string Serialize(object o, Type targetType)
            {
                if (targetType == typeof(string)) return XmlConvert.EncodeName((string)o);

                //WriteLine(string.Join("\r\n",typeof(XmlConvert).GetMethods().Where(x=>x.Name.StartsWith("To") && x.Name != "ToString").Select(x=>x.Name.Substring(2)).Distinct().OrderBy(x=>x).Select(x=>$"if (targetType == typeof({x})) return XmlConvert.ToString(({x})o);")))
                if (targetType == typeof(Boolean)) return XmlConvert.ToString((Boolean)o);
                if (targetType == typeof(Byte)) return XmlConvert.ToString((Byte)o);
                if (targetType == typeof(Char)) return XmlConvert.ToString((Char)o);
                if (targetType == typeof(DateTime)) return XmlConvert.ToString((DateTime)o, XmlDateTimeSerializationMode.Utc);
                if (targetType == typeof(DateTimeOffset)) return XmlConvert.ToString((DateTimeOffset)o);
                if (targetType == typeof(Decimal)) return XmlConvert.ToString((Decimal)o);
                if (targetType == typeof(Double)) return XmlConvert.ToString((Double)o);
                if (targetType == typeof(Guid)) return XmlConvert.ToString((Guid)o);
                if (targetType == typeof(Int16)) return XmlConvert.ToString((Int16)o);
                if (targetType == typeof(Int32)) return XmlConvert.ToString((Int32)o);
                if (targetType == typeof(Int64)) return XmlConvert.ToString((Int64)o);
                if (targetType == typeof(SByte)) return XmlConvert.ToString((SByte)o);
                if (targetType == typeof(Single)) return XmlConvert.ToString((Single)o);
                if (targetType == typeof(TimeSpan)) return XmlConvert.ToString((TimeSpan)o);
                if (targetType == typeof(UInt16)) return XmlConvert.ToString((UInt16)o);
                if (targetType == typeof(UInt32)) return XmlConvert.ToString((UInt32)o);
                if (targetType == typeof(UInt64)) return XmlConvert.ToString((UInt64)o);

                if (targetType == typeof(Vector2))
                {
                    var ot = (Vector2)o;
                    return XmlConvert.ToString(ot.x) + ";" + XmlConvert.ToString(ot.y);
                }
                if (targetType == typeof(Vector3))
                {
                    var ot = (Vector3)o;
                    return XmlConvert.ToString(ot.x) + ";" + XmlConvert.ToString(ot.y) + ";" + XmlConvert.ToString(ot.z);
                }
                if (targetType == typeof(Vector4))
                {
                    var ot = (Vector4)o;
                    return XmlConvert.ToString(ot.x) + ";" + XmlConvert.ToString(ot.y) + ";" + XmlConvert.ToString(ot.z) + ";" + XmlConvert.ToString(ot.w);
                }
                if (targetType == typeof(Quaternion))
                {
                    var ot = (Quaternion)o;
                    return XmlConvert.ToString(ot.x) + ";" + XmlConvert.ToString(ot.y) + ";" + XmlConvert.ToString(ot.z) + ";" + XmlConvert.ToString(ot.w);
                }
                if (targetType == typeof(Color))
                {
                    var ot = (Color)o;
                    return XmlConvert.ToString(ot.r) + ";" + XmlConvert.ToString(ot.g) + ";" + XmlConvert.ToString(ot.b) + ";" + XmlConvert.ToString(ot.a);
                }
                if (targetType == typeof(Color32))
                {
                    var ot = (Color32)o;
                    return XmlConvert.ToString(ot.r) + ";" + XmlConvert.ToString(ot.g) + ";" + XmlConvert.ToString(ot.b) + ";" + XmlConvert.ToString(ot.a);
                }
                if (targetType == typeof(Rect))
                {
                    var ot = (Rect)o;
                    return XmlConvert.ToString(ot.x) + ";" + XmlConvert.ToString(ot.y) + ";" + XmlConvert.ToString(ot.width) + ";" + XmlConvert.ToString(ot.height);
                }

                throw new NotSupportedException($"Could not convert type {targetType.FullName} with XmlConvert, you have to convert it manually");
            }
        }
    }
}
#endif