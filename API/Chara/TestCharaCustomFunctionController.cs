using BepInEx.Logging;

namespace KKAPI.Chara
{
    internal sealed class TestCharaCustomFunctionController : CharaCustomFunctionController
    {
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            KoikatuAPI.Log(LogLevel.Warning | LogLevel.Message, "CharaController - OnCardBeingSaved - currentGameMode:" + currentGameMode);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            KoikatuAPI.Log(LogLevel.Warning | LogLevel.Message, $"CharaController - OnReload - currentGameMode:{currentGameMode}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            KoikatuAPI.Log(LogLevel.Warning | LogLevel.Message, $"CharaController - OnCoordinateBeingLoaded - coordinate:{coordinate?.coordinateFileName}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            KoikatuAPI.Log(LogLevel.Warning | LogLevel.Message, $"CharaController - OnCoordinateBeingSaved - coordinate:{coordinate?.coordinateFileName}");
        }
    }
}
