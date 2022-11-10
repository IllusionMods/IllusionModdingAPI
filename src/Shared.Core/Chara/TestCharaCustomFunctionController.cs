using System;
using System.Collections.Generic;
using ExtensibleSaveFormat;
#if AI || HS2
using AIChara;
#endif

namespace KKAPI.Chara
{
    internal sealed class TestCharaCustomFunctionController : CharaCustomFunctionController
    {
        public string id;
        public static int No;
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SetParameterExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
            SetBodyExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
            SetFaceExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
            SetExtendedData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
            // todo test if ext data gets carried over when saving inside talk scene and h
            Console.WriteLine($"Save ID to ext data - {id}  |  Chara - {ChaControl.name}");

            KoikatuAPI.Logger.LogWarning($"event:OnCardBeingSaved  chara:{ChaControl.name}  currentGameMode:{currentGameMode}");
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            var a = GetExtendedData();
            var b = GetBodyExtData();
            var c = GetParameterExtData();
            var d = GetFaceExtData();
            KoikatuAPI.Assert((a == null && b == null && c == null && d == null) || (a != null && b != null && c != null && d != null), "ext data not lining up");
            if (a != null)
            {
                KoikatuAPI.Assert(b != null, "b != null");
                KoikatuAPI.Assert(c != null, "c != null");
                KoikatuAPI.Assert(d != null, "d != null");
                var newId = a.data["id"] as string;
                KoikatuAPI.Assert(newId == b?.data["id"] as string, "a.data[\"id\"] == b.data[\"id\"]");
                KoikatuAPI.Assert(newId == c?.data["id"] as string, "a.data[\"id\"] == c.data[\"id\"]");
                KoikatuAPI.Assert(newId == d?.data["id"] as string, "a.data[\"id\"] == d.data[\"id\"]");
                Console.WriteLine($"ID get from ext data - {newId}  |  Old ID - {id}  |  Chara - {ChaControl.name}");
                id = newId;
            }
            if (id == null)
            {
                id = $"{No++} - {SceneApi.GetLoadSceneName()} - {SceneApi.GetAddSceneName()}";
                Console.WriteLine($"New ID assigned - {id}  |  Chara - {ChaControl.name} | {ChaFileControl.GetFancyCharacterName()}");
                SetParameterExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
                SetBodyExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
                SetFaceExtData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
                SetExtendedData(new PluginData() { data = new Dictionary<string, object> { { "id", id } } });
            }

            KoikatuAPI.Logger.LogWarning($"event:OnReload  chara:{ChaControl.name}  currentGameMode:{currentGameMode}  maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            KoikatuAPI.Logger.LogWarning($"event:OnCoordinateBeingLoaded  chara:{ChaControl.name}  coordinate:{coordinate?.coordinateFileName}; maintainState:{maintainState}");
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            KoikatuAPI.Logger.LogWarning($"event:OnCoordinateBeingSaved  chara:{ChaControl.name}  coordinate:{coordinate?.coordinateFileName}");
        }
    }
}
