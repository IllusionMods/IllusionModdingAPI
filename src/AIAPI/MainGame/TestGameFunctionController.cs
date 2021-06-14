namespace KKAPI.MainGame
{
    internal sealed class TestGameFunctionController : GameCustomFunctionController
    {
        protected internal override void OnEndH(HScene proc, bool freeH)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnEndH - FreeH:" + freeH);
        }

        protected internal override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnGameLoad - Path:" + args.FullFilename);
        }

        protected internal override void OnGameSave(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnGameSave - Path:" + args.FullFilename);
        }

        protected internal override void OnStartH(HScene proc, bool freeH)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnStartH - FreeH:" + freeH);
        }

        protected internal override void OnDayChange(int day)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnDayChange - day:" + day);
        }

        protected internal override void OnPeriodChange(AIProject.TimeZone period)
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnPeriodChange - period:" + period);
        }

        protected internal override void OnNewGame()
        {
            KoikatuAPI.Logger.LogDebug("GameController - OnNewGame");
        }
    }
}
