using Cysharp.Threading.Tasks;

namespace Infrastructure.StateMachine.Game.States
{
    public class BootstrapAnalyticState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;

        public BootstrapAnalyticState(IStateMachine<IGameState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public async UniTaskVoid Enter()
        {
            _stateMachine.Enter<PreLoadGameState, TypeLoad>(TypeLoad.InitialLoading);
        }

        public async UniTaskVoid Exit()
        {

        }
    }
}