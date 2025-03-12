using Cysharp.Threading.Tasks;

namespace Infrastructure.StateMachine.Game.States
{
    public interface IState : IExitable
    {
        UniTaskVoid Enter();
    }
}