#if AI
using AIChara;
#endif

namespace KKAPI.Chara
{
    internal sealed class TestCharaCustomFunctionController : CharaCustomFunctionController
    {
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            KoikatuAPI.Logger.LogWarning("CharaController - OnCardBeingSaved - currentGameMode:" + currentGameMode);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnReload - currentGameMode:{currentGameMode}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingLoaded - coordinate:{coordinate?.coordinateFileName}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingSaved - coordinate:{coordinate?.coordinateFileName}");
        }
    }
}
