using BepInEx;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using ExtensibleSaveFormat;
using HarmonyLib;
using UniRx;
using ChaCustom;
using System.Reflection.Emit;
using System.Reflection;
using BepInEx.Harmony;
#if KK || EC
using UniRx;
#elif AI
using AIChara;
#endif

namespace KKAPI
{
    [BepInPlugin(GUID, "Card Author Data", KoikatuAPI.VersionConst)]
    [Browsable(false)]
    internal class CardAuthorData : BaseUnityPlugin
    {
        private const string DefaultNickname = "Anonymous";
        private const string GUID = "marco.authordata";

        private static ManualLogSource _logger;
        private static ConfigWrapper<string> _nickname;
        private MakerText _authorText;

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

            Hooks.Init();

            _nickname = Config.Wrap("", "Nickname", "Your nickname that will be saved to your cards and used in the card filenames.", DefaultNickname);

            CharacterApi.RegisterExtraBehaviour<CardAuthorDataController>(GUID);
            MakerAPI.RegisterCustomSubCategories += MakerAPI_RegisterCustomSubCategories;
            MakerAPI.ReloadCustomInterface += MakerApiOnReloadCustomInterface;
            MakerAPI.MakerExiting += MakerAPI_MakerExiting;
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
            _authorText = e.AddControl(new MakerText(GetAuthorsText(), MakerConstants.Parameter.Character, this));

            var tb = e.AddControl(new MakerTextbox(MakerConstants.Parameter.Character, "Your nickname", DefaultNickname, this));
            tb.Value = CurrentNickname;
            tb.ValueChanged.Subscribe(
                s =>
                {
                    if (string.IsNullOrEmpty(s)) tb.Value = DefaultNickname;
                    else _nickname.Value = s;
                });

            e.AddControl(new MakerText("Your nickname will be saved to the card and used in the card's filename. This setting is global.", MakerConstants.Parameter.Character, this) { TextColor = MakerText.ExplanationGray });
        }

        private void MakerApiOnReloadCustomInterface(object sender, EventArgs eventArgs)
        {
            if (_authorText != null)
            {
                var text = GetAuthorsText();
                _authorText.Text = text;
            }
        }

        private static class Hooks
        {
            private static Harmony harmony;

            public static void Init()
            {
                harmony = new Harmony("cardauthordata.harmony");
                HarmonyWrapper.PatchAll(typeof(Hooks), harmony);
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(CustomControl), "Start")]
            internal static IEnumerable<CodeInstruction> FindSaveMethod(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                bool buttonFound = false;

                for(int i = 0; i < codes.Count; i++)
                {
                    var code = codes[i];

                    if(!buttonFound && code.opcode == OpCodes.Ldfld && code.operand == AccessTools.Field(typeof(CustomControl), "btnSave"))
                        buttonFound = true;

                    if(buttonFound)
                    {
                        if(code.opcode == OpCodes.Ldftn)
                        {
                            if(code.operand is MethodInfo methodInfo)
                            {
                                var patchMethod = AccessTools.Method(typeof(Hooks), nameof(NamePatch));
                                harmony.Patch(methodInfo, null, null, new HarmonyMethod(patchMethod));
                                _logger.LogDebug("Save method found for patching");
                            }

                            break;
                        }
                    }
                }

                return codes;
            }

            private static IEnumerable<CodeInstruction> NamePatch(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                bool stringFound = false;

                for(int i = 0; i < codes.Count; i++)
                {
                    var code = codes[i];

                    if(!stringFound && code.opcode == OpCodes.Ldstr && code.operand != null && (string)code.operand == "Koikatu_F_")
                        stringFound = true;

                    if(stringFound)
                    {
                        if(code.opcode == OpCodes.Stloc_0)
                        {
                            var labels = codes[i + 1].labels.ToList();
                            codes[i + 1].labels.Clear();

                            codes.InsertRange(i + 1, new List<CodeInstruction>
                            {
                                new CodeInstruction(OpCodes.Ldloc_0){ labels = labels },
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Hooks), nameof(EditString))),
                                new CodeInstruction(OpCodes.Stloc_0),
                            });

                            _logger.LogDebug("Save method patched");
                            break;
                        }
                    }
                }

                return codes;
            }

            private static string EditString(string input)
            {
                try
                {
                    var dot = input.Length - Path.GetExtension(input).Length;
                    if(dot < 0) dot = input.Length;

                    var param = MakerAPI.GetCharacterControl().fileParam;
                    var name = param.fullname.Trim();
#if KK
                    if(name.Length == 0) name = param.nickname.Trim();
#endif
                    var addStr = $"_{name}";

                    if(CurrentNickname != DefaultNickname)
                        addStr = $"{addStr}_{CurrentNickname}";

                    var invalid = Path.GetInvalidFileNameChars();
                    addStr = new string(addStr.Select(c => invalid.Contains(c) ? '?' : c).ToArray());

                    return input.Insert(dot, addStr);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex);
                    return input;
                }
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
