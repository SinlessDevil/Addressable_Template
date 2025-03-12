using Cysharp.Threading.Tasks;
using Infrastructure.Loading;
using Services.AssetProvider;
using Services.Factories.UIFactory;
using Services.Levels;
using Services.PreloaderConductor;
using Services.Provides.Widgets;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadLevelState : IState, IGameState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IUIFactory _uiFactory;
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly IWidgetProvider _widgetProvider;
        private readonly ILevelService _levelService;
        private readonly IAssetPreloaderConductor _preloaderConductor;
        private readonly IAssetProvider _assetProvider;

        public LoadLevelState(
            IStateMachine<IGameState> gameStateMachine, 
            ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain, 
            IUIFactory uiFactory,
            IWidgetProvider widgetProvider,
            ILevelService levelService,
            IAssetPreloaderConductor preloaderConductor,
            IAssetProvider assetProvider)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiFactory = uiFactory;
            _widgetProvider = widgetProvider;
            _levelService = levelService;
            _preloaderConductor = preloaderConductor;
            _assetProvider = assetProvider;
        }

        public void Enter()
        {
            _assetProvider.CleanUp();
            
            var chapter = _levelService.GetCurrentChapterStaticData();
            var nameScene = chapter.NameScene;
            
            _loadingCurtain.Show();
            _sceneLoader.LoadForce(nameScene, () => OnLevelLoad(), _loadingCurtain);
        }
        
        public void Exit()
        {
            _loadingCurtain.Hide();
        }
        
        protected virtual void OnLevelLoad()
        {
            InitGameWorld();

            _gameStateMachine.Enter<GameLoopState>();
        }

        private async UniTask InitGameWorld()
        {
            _uiFactory.CreateUiRoot();
            
            InitHud();
            
            InitProviders();
            
            await _preloaderConductor.TryPreload();
        }
        
        private void InitProviders()
        {
            _widgetProvider.CreatePoolWidgets();
        }
        
        private void InitHud()
        {
            var gameHud = _uiFactory.CreateGameHud();
            gameHud.Initialize();
        }
    }
}