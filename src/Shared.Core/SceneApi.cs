namespace KKAPI
{
    /// <summary>
    /// Game-agnostic version of Manager.Scene. It allows using the same code in all games without any #if directives.
    /// </summary>
    public static class SceneApi
    {
        /// <summary>
        /// Get name of the currently loaded overlay scene (eg. exit game box, config, confirmation dialogs).
        /// </summary>
        public static string GetAddSceneName()
        {
#if HS2 || KKS
            return Manager.Scene.AddSceneName;
#else
            return Manager.Scene.Instance.AddSceneName;
#endif
        }

        /// <summary>
        /// Get name of the currently loaded game scene (eg. maker, h, adv).
        /// </summary>
        public static string GetLoadSceneName()
        {
#if HS2 || KKS
            return Manager.Scene.LoadSceneName;
#else
            return Manager.Scene.Instance.LoadSceneName;
#endif
        }

        /// <summary>
        /// True if loading screen is being displayed, or if screen is currently fading in or out.
        /// </summary>
        public static bool GetIsNowLoadingFade()
        {
#if HS2 || KKS
            return Manager.Scene.IsNowLoadingFade;
#else
            return Manager.Scene.Instance.IsNowLoadingFade;
#endif
        }

        /// <summary>
        /// True if loading screen is being displayed.
        /// </summary>
        public static bool GetIsNowLoading()
        {
#if HS2 || KKS
            return Manager.Scene.IsNowLoading;
#else
            return Manager.Scene.Instance.IsNowLoading;
#endif
        }

        /// <summary>
        /// True if screen is currently fading in or out.
        /// </summary>
        public static bool GetIsFadeNow()
        {
#if HS2 || KKS
            return Manager.Scene.IsFadeNow;
#else
            return Manager.Scene.Instance.IsFadeNow;
#endif
        }

        /// <summary>
        /// True if a dialog box or some other overlapping menu is shown (e.g. exit dialog after pressing esc).
        /// </summary>
        public static bool GetIsOverlap()
        {
#if HS2 || KKS
            return Manager.Scene.IsOverlap;
#else
            return Manager.Scene.Instance.IsOverlap;
#endif
        }
    }
}