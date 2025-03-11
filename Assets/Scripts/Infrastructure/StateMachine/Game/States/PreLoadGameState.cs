using Services.PersistenceProgress;
using Services.PreloaderConductor;
using Services.StaticData;
using UnityEngine.SceneManagement;

namespace Infrastructure.StateMachine.Game.States
{
    public class PreLoadGameState : IPayloadedState<TypeLoad>, IGameState
    {
        private string _firstSceneName;
        
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly IStaticDataService _staticData;
        private readonly IAssetPreloaderConductor _assetPreloaderConductor;
        private readonly IAssetPreloaderConductor _preloaderConductor;

        public PreLoadGameState(
            IStateMachine<IGameState> stateMachine,
            IPersistenceProgressService persistenceProgressService,
            IStaticDataService staticData,
            IAssetPreloaderConductor assetPreloaderConductor)
        {
            _stateMachine = stateMachine;
            _persistenceProgressService = persistenceProgressService;
            _staticData = staticData;
            _assetPreloaderConductor = assetPreloaderConductor;
            _preloaderConductor = assetPreloaderConductor;
        }
        
        public void Enter(TypeLoad payload)
        {
            _preloaderConductor.TryPreload();
            
            if(TypeLoad.MenuLoading == payload)
            {
                _stateMachine.Enter<LoadLevelState>();
                return;
            }
            
            var hasCompletedFirstLevel = _persistenceProgressService.PlayerData.PlayerTutorialData.HasFirstCompleteLevel;
            _firstSceneName = FirstSceneName(hasCompletedFirstLevel);

            if (hasCompletedFirstLevel)
            {
                _stateMachine.Enter<LoadMenuState, string>(_firstSceneName);
            }
            else
            {
                _stateMachine.Enter<LoadLevelState>();
            }
        }

        public void Exit()
        {
                
        }
        
        private string FirstSceneName(bool hasCompletedFirstLevel)
        {
            var nameScene = string.Empty;
            nameScene = hasCompletedFirstLevel ? _staticData.GameConfig.MenuScene : _staticData.GameConfig.GameScene;
            
#if UNITY_EDITOR
            if (_staticData.GameConfig.CanLoadCurrentOpenedScene)
                nameScene = SceneManager.GetActiveScene().name;        
#endif
            return nameScene;
        }
    }
}