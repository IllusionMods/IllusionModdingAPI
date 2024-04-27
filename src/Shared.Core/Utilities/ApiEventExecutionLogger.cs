using System;
using System.Diagnostics;
using System.Text;

namespace KKAPI.Chara
{
    internal class ApiEventExecutionLogger
    {
        [ThreadStatic]
        private static ApiEventExecutionLogger _eventLogger;
        public static ApiEventExecutionLogger GetEventLogger() => _eventLogger ?? (_eventLogger = new ApiEventExecutionLogger());

        private readonly StringBuilder _sb;
        private readonly Stopwatch _swPlug, _swTotal;
        private int _pluginCounter, _handlerCounter;

        private ApiEventExecutionLogger()
        {
            //todo if(!KoikatuAPI.EnableDebugLogging) return;
            _sb = new StringBuilder();
            _swPlug = new Stopwatch();
            _swTotal = new Stopwatch();
        }

        public void Begin(string eventName, string targetName)
        {
            if (_sb == null) return;

            _sb.Length = 0;
            _sb.Append("Finished running ").Append(eventName).Append(" events for: ").Append(targetName);

            _pluginCounter = 0;
            _handlerCounter = 0;
            _swTotal.Reset();
            _swTotal.Start();
        }

        public void PluginStart()
        {
            if (_sb == null) return;

            _swPlug.Start();
        }

        public void PluginEnd(CharaCustomFunctionController pluginBehaviour)
        {
            if (_sb == null) return;

            _swPlug.Stop();

            LogEnd(++_pluginCounter, pluginBehaviour.GetType().FullName ?? "NULL", _swPlug.ElapsedMilliseconds);

            _swPlug.Reset();
        }

        public void EventHandlerBegin(string eventName)
        {
            if (_sb == null) return;

            _sb.AppendLine().Append(eventName).Append(" event handlers:");
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

            LogEnd(++_handlerCounter, (handler.DeclaringType?.FullName ?? "NULL") + "::" + handler.Name, _swPlug.ElapsedMilliseconds);

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
            if (_sb == null) return;

            _sb.AppendLine().Append("## Total: ").Append(_swTotal.ElapsedMilliseconds).Append("ms");
            KoikatuAPI.Logger.LogDebug(_sb.ToString());
        }
    }
}
