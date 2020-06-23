using Character;

namespace KKAPI.Chara
{
    internal sealed class TestCharaCustomFunctionController : CharaCustomFunctionController
    {
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCardBeingSaved - name:{ChaControl.GetCharacterName()}; currentGameMode:{currentGameMode}");
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnReload - name:{ChaControl.GetCharacterName()}; currentGameMode:{currentGameMode}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingLoaded(CustomParameter coordinate, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingLoaded - name:{ChaControl.GetCharacterName()}; sex:{coordinate?.Sex}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingSaved(CustomParameter coordinate)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingSaved - name:{ChaControl.GetCharacterName()}; sex:{coordinate?.Sex}");
        }
    }
}
