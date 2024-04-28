using System;
using System.Diagnostics;
using System.Text;

namespace KKAPI.Utilities
{
    internal class ApiEventExecutionLogger
    {
        [ThreadStatic]
        private static ApiEventExecutionLogger _eventLogger;
        public static ApiEventExecutionLogger GetEventLogger() => _eventLogger != null && !_eventLogger._running ? _eventLogger : _eventLogger = new ApiEventExecutionLogger();

        private StringBuilder _sb;
        private readonly Stopwatch _swPlug, _swTotal;
        private int _pluginCounter, _handlerCounter;
        private bool _running;
        private bool _addedPluginHeader;

        private ApiEventExecutionLogger()
        {
            _swPlug = new Stopwatch();
            _swTotal = new Stopwatch();
        }

        public void Begin(string eventName, string targetName)
        {
            if (!KoikatuAPI.EnableDebugLogging)
            {
                _sb = null;
                return;
            }
            else if (_sb == null)
            {
                _sb = new StringBuilder();
            }
            else
            {
                _sb.Length = 0;
            }

            _running = true;
            _addedPluginHeader = false;

            _sb.Append("Finished raising event: ").Append(eventName);
            if (targetName != null) _sb.Append(" | Target: ").Append(targetName);

            _pluginCounter = 0;
            _handlerCounter = 0;
            _swTotal.Reset();
            _swTotal.Start();
        }

        public void PluginStart()
        {
            if (_sb == null) return;

            if (!_addedPluginHeader)
            {
                _sb.AppendLine().Append("___/ ").Append("Controllers:");
                _addedPluginHeader = true;
            }

            _swPlug.Start();
        }

        public void PluginEnd(UnityEngine.MonoBehaviour pluginBehaviour)
        {
            if (_sb == null) return;

            PluginEnd(pluginBehaviour.GetType().FullName);
        }
        public void PluginEnd(string pluginName)
        {
            if (_sb == null) return;

            _swPlug.Stop();

            LogEnd(++_pluginCounter, pluginName ?? "NULL", _swPlug.ElapsedMilliseconds);

            _swPlug.Reset();
        }

        public void EventHandlerBegin(string eventName)
        {
            if (_sb == null) return;

            _sb.AppendLine().Append("___/ ").Append(eventName).Append(" event handlers:");
            _handlerCounter = 0;
        }

        public void EventHandlerStart()
        {
            if (_sb == null) return;

            _swPlug.Start();
        }

        public void EventHandlerEnd(System.Reflection.MethodInfo handler)
        {
            if (_sb == null) return;

            _swPlug.Stop();

            var declaringTypeFullName = handler.DeclaringType?.FullName;

            const string spam = "KKAPI.Chara.CharacterExtensions+<>c__DisplayClass4_0`2[[";
            if (declaringTypeFullName != null && declaringTypeFullName.StartsWith(spam))
                declaringTypeFullName = declaringTypeFullName.Substring(spam.Length, declaringTypeFullName.IndexOfAny(new char[] { ',', ']' }, spam.Length) - spam.Length);

            LogEnd(++_handlerCounter, (declaringTypeFullName ?? "NULL") + "::" + handler.Name, _swPlug.ElapsedMilliseconds);

            _swPlug.Reset();
        }

        private void LogEnd(int count, string fullName, long milliseconds)
        {
            _sb.AppendLine()
               .Append("#").Append(count.ToString("D2")).Append(" ")
               .Append(fullName.PadRight(62)).Append("\t - ")
               .Append(milliseconds).Append("ms ")
               .Append(new string('<', (int)(milliseconds / 20)));
        }

        public void End()
        {
            _running = false;
            if (_sb == null) return;

            _sb.AppendLine().Append("____ ").Append("Total: ").Append(_swTotal.ElapsedMilliseconds).Append("ms");
            KoikatuAPI.Logger.LogDebug(_sb.ToString());
        }
    }
}
