using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UniRx;

namespace KKAPI
{
    [BepInPlugin(GUID, "Card Author Data", KoikatuAPI.VersionConst)]
    [BepInDependency(KoikatuAPI.GUID)]
    [Browsable(false)]
    [JetBrains.Annotations.UsedImplicitly]
    internal class CardAuthorData : BaseUnityPlugin
    {
        private const string DefaultNickname = "Anonymous";
        private const string GUID = "marco.authordata";

        private static ManualLogSource _logger;
        private static ConfigEntry<string> _nickname;
        private MakerText _authorText;
        private static readonly HashSet<char> _invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars().Concat(new[] { '.' }));

        private static string CurrentNickname
        {
            get
            {
                var n = _nickname.Value;
                return string.IsNullOrEmpty(n) ? DefaultNickname : n;
            }
        }

        private void Start()
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio) return;

            _logger = Logger;

            MakerCardSave.RegisterNewCardSavePathModifier(null, FilenameModifier);

            _nickname = Config.Bind("", "Nickname", DefaultNickname, "Your nickname that will be saved to your cards and used in the card filenames.");

            CharacterApi.RegisterExtraBehaviour<CardAuthorDataController>(GUID);
            MakerAPI.RegisterCustomSubCategories += MakerAPI_RegisterCustomSubCategories;
            MakerAPI.ReloadCustomInterface += MakerApiOnReloadCustomInterface;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
        }

        private static string FilenameModifier(string currentCardName)
        {
            var nameBackup = currentCardName;
            try
            {
                if (currentCardName.EndsWith(".png")) currentCardName = currentCardName.Substring(0, currentCardName.Length - 4);

                var param = MakerAPI.GetCharacterControl().fileParam;
                var charaName = param.fullname.Trim();
#if KK || KKS
                if (charaName.Length == 0) charaName = param.nickname.Trim();
#endif
                var addStr = $"_{charaName}";

                if (CurrentNickname != DefaultNickname)
                    addStr = $"{addStr}_{CurrentNickname}";

                addStr = new string(addStr.Select(c => _invalidChars.Contains(c) ? '_' : c).ToArray());

                return currentCardName + addStr;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return nameBackup;
            }
        }

        private static string GetAuthorsText()
        {
            var authors = string.Join(" > ", MakerAPI.GetCharacterControl().GetComponent<CardAuthorDataController>().Authors.ToArray());
            var text = "Author history: " + (authors.Length == 0 ? "[Empty]" : authors);
            return text;
        }

        private void MakerAPI_MakerExiting(object sender, EventArgs e)
        {
            _authorText = null;
        }

        private void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
#if AI || HS2
            // AI maker needs a separate category, using an existing cagetory makes a UI mess
            var makerCategory = new MakerCategory(MakerConstants.Parameter.CategoryName, "Card author data");
            e.AddSubCategory(makerCategory);
#else
            var makerCategory = MakerConstants.Parameter.Character;
#endif

            _authorText = e.AddControl(new MakerText("Not loaded yet...", makerCategory, this));

            var tb = e.AddControl(new MakerTextbox(makerCategory, "Your nickname", DefaultNickname, this));
            tb.Value = CurrentNickname;
            tb.ValueChanged.Subscribe(
                s =>
                {
                    if (string.IsNullOrEmpty(s)) tb.Value = DefaultNickname;
                    else _nickname.Value = s;
                });

            e.AddControl(new MakerText("Your nickname will be saved to the card and used in the card's filename. This setting is global.", makerCategory, this) { TextColor = MakerText.ExplanationGray });
        }

        private void MakerApiOnReloadCustomInterface(object sender, EventArgs eventArgs)
        {
            if (_authorText != null)
            {
                var text = GetAuthorsText();
                _authorText.Text = text;
            }
        }

        private sealed class CardAuthorDataController : CharaCustomFunctionController
        {
            private const string AuthorsKey = "Authors";
            private string[] _previousAuthors;

            public IEnumerable<string> Authors => _previousAuthors ?? Enumerable.Empty<string>();

            protected override void OnCardBeingSaved(GameMode currentGameMode)
            {
                var authorList = Authors.ToList();

                if (MakerAPI.InsideMaker)
                {
                    if (authorList.LastOrDefault() != CurrentNickname)
                        authorList.Add(CurrentNickname);
                }

                if (authorList.Any())
                {
                    SetExtendedData(new PluginData
                    {
                        version = 1,
                        data = { { AuthorsKey, MessagePack.LZ4MessagePackSerializer.Serialize(authorList.ToArray()) } }
                    });
                }
                else
                    SetExtendedData(null);
            }

            protected override void OnReload(GameMode currentGameMode)
            {
                var flags = MakerAPI.GetCharacterLoadFlags();
                // If majority of the parts were loaded then use authors of the other card, else keep current
                if (ReplacedParts(flags) > 3)
                {
                    // Flags is null when starting maker and loading chika, our otside maker. Do NOT add the unknown author tag in these cases or cards based on chika would have it too
                    _previousAuthors = flags == null ? null : new[] { "[Unknown]" };
                }

                var data = GetExtendedData();
                if (data != null)
                {
                    if (data.data.TryGetValue(AuthorsKey, out var arr) && arr is byte[] strArr)
                        _previousAuthors = MessagePack.LZ4MessagePackSerializer.Deserialize<string[]>(strArr);
                }
            }
        }

        private static int ReplacedParts(CharacterLoadFlags flags)
        {
            if (flags == null) return 5;
            var bools = new[] { flags.Body, flags.Clothes, flags.Face, flags.Hair, flags.Parameters };
            return bools.Sum(b => b ? 1 : 0);
        }
    }
}
