using Cysharp.Threading.Tasks;
using Infrastructure.Loading;
using Services.AssetProvider;
using Services.Factories.UIFactory;
using Services.PreloaderConductor;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadMenuState : IPayloadedState<string>, IGameState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IUIFactory _uiFactory;
        private readonly IAssetPreloaderConductor _preloaderConductor;
        private readonly IAssetProvider _assetProvider;

        public LoadMenuState(
            IStateMachine<IGameState> gameStateMachine,
            ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain,
            IUIFactory uiFactory,
            IAssetPreloaderConductor preloaderConductor,
            IAssetProvider assetProvider)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiFactory = uiFactory;
            _preloaderConductor = preloaderConductor;
            _assetProvider = assetProvider;
        }
        
        public void Enter(string payload)
        {
            _loadingCurtain.Show();
            
            _assetProvider.CleanUp();
            
            _sceneLoader.LoadForce(payload, () => OnMenuLoad(), _loadingCurtain);
        }

        public void Exit()
        {
            
        }

        private async UniTask OnMenuLoad()
        {
            _uiFactory.CreateUiRoot();

            var menuHud = _uiFactory.CreateMenuHud();
            menuHud.Initialize();
            
           await _preloaderConductor.TryPreload();
           
           _loadingCurtain.Hide();
        }
    }
}