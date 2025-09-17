using ActionGame;
using BepInEx.Logging;
using UnityEngine;

namespace KKAPI.MainGame
{
    internal sealed class TestGameFunctionController : GameCustomFunctionController
    {
        protected internal override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnEndH - FreeH:" + hFlag.isFreeH);
        }

        protected internal override void OnEnterNightMenu()
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnEnterNightMenu");
        }

        protected internal override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnGameLoad - Path:" + args.FullFilename);
        }

        protected internal override void OnGameSave(GameSaveLoadEventArgs args)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnGameSave - Path:" + args.FullFilename);
        }

        protected internal override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnStartH - FreeH:" + hFlag.isFreeH);
        }

        protected internal override void OnDayChange(Cycle.Week day)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnDayChange - day:" + day);
        }

        protected internal override void OnPeriodChange(Cycle.Type period)
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnPeriodChange - period:" + period);
        }

        protected internal override void OnNewGame()
        {
            KoikatuAPI.Logger.Log(LogLevel.Warning | LogLevel.Message, "GameController - OnNewGame");
        }
    }
}
