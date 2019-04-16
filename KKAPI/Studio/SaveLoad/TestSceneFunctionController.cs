using BepInEx.Logging;
using KKAPI.Utilities;
using Studio;
using Logger = BepInEx.Logger;

namespace KKAPI.Studio.SaveLoad
{
    internal class TestSceneFunctionController : SceneCustomFunctionController
    {
        protected internal override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            Logger.Log(LogLevel.Warning | LogLevel.Message, $"OnSceneLoad {operation} - {loadedItems.Count}");
        }

        protected internal override void OnSceneSave()
        {
            Logger.Log(LogLevel.Warning | LogLevel.Message, $"OnSceneSave");
        }
    }
}
