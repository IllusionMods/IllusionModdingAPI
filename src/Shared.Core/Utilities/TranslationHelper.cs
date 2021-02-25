using System;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Class that abstracts away AutoTranslator. It lets you translate text to current language.
    /// </summary>
    public static class TranslationHelper
    {
        private static readonly Action<string, Action<string>> _translatorCallback;
        private static readonly Func<string, string> _tryTranslateCallback;

        /// <summary>
        /// True if a reasonably recent version of AutoTranslator is installed.
        /// It might return false for some very old versions that don't have the necessary APIs to make this class work.
        /// </summary>
        public static bool AutoTranslatorInstalled { get; }

        static TranslationHelper()
        {
            var xua = Type.GetType("XUnity.AutoTranslator.Plugin.Core.ITranslator, XUnity.AutoTranslator.Plugin.Core", false);
            if (xua != null && xua.GetMethods().Any(x => x.Name == "TranslateAsync"))
            {
                // The lambdas don't get their types resolved until called so this doesn't crash here if the type doesn't exist
                _translatorCallback = (s, action) => AutoTranslator.Default.TranslateAsync(s, result => { if (result.Succeeded) action(result.TranslatedText); });
                _tryTranslateCallback = s => AutoTranslator.Default.TryTranslate(s, out s) ? s : null;
                AutoTranslatorInstalled = true;
            }
            else
            {
                KoikatuAPI.Logger.LogWarning("Could not find method AutoTranslator.Default.TranslateAsync, translations will be limited or unavailable");
                _translatorCallback = null;
            }
        }

        /// <summary>
        /// Queries AutoTranslator to provide a translated text for the untranslated text.
        /// If the translation cannot be found in the cache, it will make a request to the translator selected by the user.
        /// If AutoTranslator is not installed, this will do nothing.
        /// </summary>
        /// <param name="untranslatedText">The untranslated text to provide a translation for.</param>
        /// <param name="onCompleted">Callback with the completed translation. It can return immediately or at some later point.</param>
        public static void TranslateAsync(string untranslatedText, Action<string> onCompleted)
        {
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
            if (string.IsNullOrEmpty(untranslatedText)) return;
            _translatorCallback?.Invoke(untranslatedText, onCompleted);
        }

        /// <summary>
        /// Queries the plugin to provide a translated text for the untranslated text.
        /// If the translation cannot be found in the cache, the method returns false
        /// and returns null as the untranslated text.
        /// </summary>
        /// <param name="untranslatedText">The untranslated text to provide a translation for.</param>
        /// <param name="translatedText">The translated text.</param>
        public static bool TryTranslate(string untranslatedText, out string translatedText)
        {
            if (string.IsNullOrEmpty(untranslatedText) || _tryTranslateCallback == null)
            {
                translatedText = null;
                return false;
            }

            translatedText = _tryTranslateCallback(untranslatedText);
            return translatedText != null;
        }
    }
}