using System;
using Cysharp.Threading.Tasks;

namespace Infrastructure
{
    public interface ISceneLoader
    {
        UniTask Load(string name, Action onLevelLoad, bool isAddressable, ILoadingCurtain loadingCurtain = null);
        UniTask LoadForce(string constantServiceFirstScene, Action action, ILoadingCurtain loadingCurtain);
    }
}