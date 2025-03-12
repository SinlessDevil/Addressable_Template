using Cysharp.Threading.Tasks;

namespace Infrastructure.StateMachine.Game.States
{
    public interface IExitable
    {
        UniTaskVoid Exit();
    }
}