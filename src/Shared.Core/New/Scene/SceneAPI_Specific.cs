namespace KKAPI.Maker
{
    public sealed class SceneAPI_Specific : SceneAPI
    {
        public override string GetAddSceneName()
        {
#if HS2 || KKS
            return Manager.Scene.AddSceneName;
#else
            return Manager.Scene.Instance.AddSceneName;
#endif
        }

        public override string GetLoadSceneName()
        {
#if HS2 || KKS
            return Manager.Scene.LoadSceneName;
#else
            return Manager.Scene.Instance.LoadSceneName;
#endif
        }

        public override bool GetIsNowLoadingFade()
        {
#if HS2 || KKS
            return Manager.Scene.IsNowLoadingFade;
#else
            return Manager.Scene.Instance.IsNowLoadingFade;
#endif
        }

        public override bool GetIsNowLoading()
        {
#if HS2 || KKS
            return Manager.Scene.IsNowLoading;
#else
            return Manager.Scene.Instance.IsNowLoading;
#endif
        }

        public override bool GetIsFadeNow()
        {
#if HS2 || KKS
            return Manager.Scene.IsFadeNow;
#else
            return Manager.Scene.Instance.IsFadeNow;
#endif
        }
    }
}