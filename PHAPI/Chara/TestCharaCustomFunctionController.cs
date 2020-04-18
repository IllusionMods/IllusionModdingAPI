using Character;

namespace KKAPI.Chara
{
    internal sealed class TestCharaCustomFunctionController : CharaCustomFunctionController
    {
        private string GetName() => ChaControl is Female f ? f.HeroineID.ToString() : ((Male)ChaControl).MaleID.ToString();

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCardBeingSaved - name:{GetName()}; currentGameMode:{currentGameMode}");
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnReload - name:{GetName()}; currentGameMode:{currentGameMode}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingLoaded(CustomParameter coordinate, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingLoaded - name:{GetName()}; sex:{coordinate?.Sex}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingSaved(CustomParameter coordinate)
        {
            KoikatuAPI.Logger.LogWarning($"CharaController - OnCoordinateBeingSaved - name:{GetName()}; sex:{coordinate?.Sex}");
        }
    }
}
