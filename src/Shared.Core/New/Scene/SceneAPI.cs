namespace KKAPI.Maker
{
    public abstract class SceneAPI
    {
        public abstract string GetAddSceneName();
        public abstract string GetLoadSceneName();

        public abstract bool GetIsNowLoadingFade();
        public abstract bool GetIsNowLoading();
        public abstract bool GetIsFadeNow();
    }
}