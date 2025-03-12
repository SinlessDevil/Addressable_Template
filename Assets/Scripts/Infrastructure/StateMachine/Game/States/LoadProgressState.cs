using Cysharp.Threading.Tasks;
using Services.PersistenceProgress;
using Services.PersistenceProgress.Player;
using Services.SaveLoad;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadProgressState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IPersistenceProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;

        public LoadProgressState(
            IStateMachine<IGameState> stateMachine, 
            IPersistenceProgressService progressService, 
            ISaveLoadService saveLoadService)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _saveLoadService = saveLoadService;
        }

        public async UniTaskVoid Enter()
        {
            LoadOrCreatePlayerData();
            InitResourecesLoading();
            
            _stateMachine.Enter<BootstrapAnalyticState>();
        }

        public async UniTaskVoid Exit()
        {
            
        }

        private PlayerData LoadOrCreatePlayerData()
        {
            var playerData = _progressService.PlayerData =
                _saveLoadService.LoadProgress() != null ? _saveLoadService.LoadProgress() : CreatePlayerData();
            return playerData;
        }
        
        private PlayerData CreatePlayerData()
        {
            PlayerData playerData = new PlayerData();

            return playerData;
        }
        
        private void InitResourecesLoading()
        {
            string version = UnityEngine.Application.version;
            if (_progressService.PlayerData.Loading.Version != version)
                _progressService.PlayerData.Loading.Reset(version);
        }

    }
}