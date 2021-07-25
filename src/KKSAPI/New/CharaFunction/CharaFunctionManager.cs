using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KKAPI;
using UniRx;

namespace ModdingAPI
{
    public class CharaFunctionManager : KKAPI.Chara.CharaCustomFunctionController
    {
        private readonly List<CharaFunctionControllerBase> _controllers = new List<CharaFunctionControllerBase>(API.Chara.RegisteredControllers.Count);
        private ReadOnlyCollection<CharaFunctionControllerBase> _controllersReadonly;
        public ReadOnlyCollection<CharaFunctionControllerBase> Controllers => _controllersReadonly ?? (_controllersReadonly = _controllers.AsReadOnly());

        //public new ChaControlWrapper ChaControl => base.ChaControl.Wrap();
        //public new ChaFileControlWrapper ChaFileControl => base.ChaFileControl.Wrap();

        //protected override void Awake()
        //{
        //    base.Awake();
        //    OnInitialize();  //todo put in start?
        //}

        protected override void Start()
        {
            OnInitialize();
            base.Start();
        }

        public readonly SafeEvent<NewValueEventArgs<int>> EvtCurrentCoordinateChanged = new SafeEvent<NewValueEventArgs<int>>();

        private void OnInitialize()
        {
            CurrentCoordinate.Subscribe(t => EvtCurrentCoordinateChanged.SafeInvoke(this, new NewValueEventArgs<int>((int)t)));

            // Sort by priority so that added events end up in the correct order since they are all supposed to be added in Initialize
            // Create all controllers first before initializing in case any of the controllers wants to touch another controller before its Initialize
            // Randomize load order to help uncover bugs at dev time/testing
            foreach (var reg in API.Chara.RegisteredControllers.Values.OrderBy(x => x.Priority).ThenBy(x => UnityEngine.Random.value))
            {
                try
                {
                    var controller = (CharaFunctionControllerBase)Activator.CreateInstance(reg.ControllerType, reg, this);
                    _controllers.Add(controller);
                }
                catch (Exception e)
                {
                    //todo rollback code for events? keep last count and remove until count is equal
                    KKAPI.KoikatuAPI.Logger.LogError($"Failed to create controller {reg.ControllerType.FullName} - {e}");
                }
            }

            foreach (var ctrl in _controllers)
            {
                try
                {
                    ctrl.OnInitialize();
                }
                catch (Exception e)
                {
                    //todo rollback code for events? keep last count and remove until count is equal
                    KKAPI.KoikatuAPI.Logger.LogError($"Failed to initialize controller {ctrl.Source.ControllerType.FullName} - {e}");
                }
            }
        }

        public readonly SafeEvent<SaveEventArgs<ChaFileControl>> EvtCardSave = new SafeEvent<SaveEventArgs<ChaFileControl>>();
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (_destroyed) return;
            EvtCardSave.SafeInvoke(this, new SaveEventArgs<ChaFileControl>(ChaFileControl));
        }

        public readonly SafeEvent<LoadEventArgs<ChaFileControl>> EvtReload = new SafeEvent<LoadEventArgs<ChaFileControl>>();
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (_destroyed) return;
            base.OnReload(currentGameMode, maintainState);
            EvtReload.SafeInvoke(this, new LoadEventArgs<ChaFileControl>(ChaFileControl, maintainState));
        }

        public readonly SafeEvent<SaveEventArgs<ChaFileCoordinate>> EvtCoordinateSave = new SafeEvent<SaveEventArgs<ChaFileCoordinate>>();
        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            if (_destroyed) return;
            base.OnCoordinateBeingSaved(coordinate);
            EvtCoordinateSave.SafeInvoke(this, new SaveEventArgs<ChaFileCoordinate>(coordinate));
        }

        public readonly SafeEvent<LoadEventArgs<ChaFileCoordinate>> EvtCoordinateLoad = new SafeEvent<LoadEventArgs<ChaFileCoordinate>>();
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (_destroyed) return;
            base.OnCoordinateBeingLoaded(coordinate, maintainState);
            EvtCoordinateLoad.SafeInvoke(this, new LoadEventArgs<ChaFileCoordinate>(coordinate, maintainState));
        }

        public readonly SafeEvent EvtUpdate = new SafeEvent();
        protected override void Update()
        {
            base.Update();
            EvtUpdate.SafeInvoke(this);
        }

        private bool _destroyed;
        public readonly SafeEvent EvtOnDestroy = new SafeEvent();
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EvtOnDestroy.SafeInvoke(this);
            _destroyed = true;
        }

        public readonly SafeEvent EvtOnEnable = new SafeEvent();
        protected override void OnEnable()
        {
            base.OnEnable();
            EvtOnEnable.SafeInvoke(this);
        }
    }
}