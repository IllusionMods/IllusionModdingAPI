using BepInEx.Logging;
using KKAPI.Utilities;
using Studio;

namespace KKAPI.Studio.SaveLoad
{
    internal class TestSceneFunctionController : SceneCustomFunctionController
    {
        protected internal override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, $"OnSceneLoad {operation} - {loadedItems.Count}");
        }

        protected internal override void OnSceneSave()
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "OnSceneSave");
        }

        protected internal override void OnObjectsCopied(ReadOnlyDictionary<int, ObjectCtrlInfo> copiedItems)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "OnObjectsCopied");
        }
    }
}
