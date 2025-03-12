using Cysharp.Threading.Tasks;

namespace Services.PreloaderConductor
{
    public interface IAssetPreloaderConductor
    {
        UniTask TryPreload();
    }
}