// using ActionGame;
using BepInEx.Logging;
using AIProject;
using Manager;

namespace KKAPI.MainGame
{
    internal sealed class TestGameFunctionController : GameCustomFunctionController
    {
        protected internal override void OnEndH(HSceneManager proc, bool freeH)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnEndH - FreeH:" + freeH);
        }

        protected internal override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnGameLoad - Path:" + args.FullFilename);
        }

        protected internal override void OnGameSave(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnGameSave - Path:" + args.FullFilename);
        }

        protected internal override void OnStartH(HSceneManager proc, bool freeH)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnStartH - FreeH:" + freeH);
        }

        protected internal override void OnDayChange(int day)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnDayChange - day:" + day);
        }

        protected internal override void OnPeriodChange(AIProject.TimeZone period)
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnPeriodChange - period:" + period);
        }

        protected internal override void OnNewGame()
        {
            KoikatuAPI.Logger.LogWarning("GameController - OnNewGame");
        }
    }
}
