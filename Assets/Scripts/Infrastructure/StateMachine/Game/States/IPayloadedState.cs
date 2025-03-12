using Cysharp.Threading.Tasks;

namespace Infrastructure.StateMachine.Game.States
{
    public interface IPayloadedState<TPayload> : IExitable
    {
        UniTaskVoid Enter(TPayload payload);
    }
}