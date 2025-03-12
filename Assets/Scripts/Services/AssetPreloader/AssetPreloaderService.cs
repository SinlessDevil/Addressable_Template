using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Services.StaticData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.AssetPreloader
{
    public class AssetPreloaderService : IAssetPreloaderService
    {
        private readonly IStaticDataService _staticData;
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handlers = new();
        private readonly Dictionary<string, bool> _loadingOperations = new();
        
        public AssetPreloaderService(IStaticDataService staticData)
        {
            _staticData = staticData;
        }

        public async UniTask<bool> NeedLoadAssetsFor(string address)
        {
            Debug.Log($"[NeedLoadAssetsFor] Checking download size for address: {address}");
    
            long size = await Addressables.GetDownloadSizeAsync(address).Task;
    
            Debug.Log($"[NeedLoadAssetsFor] Address: {address}, Download Size: {size}");
    
            return size != 0;
        }
        
        public async UniTask<bool> NeedLoadAssetsFor(AssetReference assetReference)
        {
            long size = await Addressables.GetDownloadSizeAsync(assetReference).Task;
            return size != 0;
        }

        public async UniTask<AsyncOperationStatus> LoadAssetsFor(string address)
        {
            if (_loadingOperations.ContainsKey(address))
            {
                Debug.LogWarning($"[LoadAssetsFor] {address} It's already downloading!");
                return AsyncOperationStatus.None;
            }
            
            _loadingOperations[address] = true;
    
            var status = await Load(address, cacheKey: address);
    
            _loadingOperations.Remove(address);
    
            return status;
        }
        
        public async UniTask<AsyncOperationStatus> LoadAssetsFor(AssetReference assetReference) => 
            await Load(assetReference, cacheKey: assetReference.AssetGUID);

        public bool IsLoading(string address) =>
            _handlers.TryGetValue(address, out var handles) && handles.Any(x => !x.IsDone);
        
        public bool IsLoading(AssetReference assetReference) =>
            _handlers.TryGetValue(assetReference.AssetGUID, out var handles) && handles.Any(x => !x.IsDone);

        public DownloadStatus GetLoadingProgress(string address)
        {
            if (_handlers.TryGetValue(address, out var handles))
                return handles.First(x => !x.IsDone).GetDownloadStatus();
            
            return default;
        }

        public DownloadStatus GetLoadingProgress(AssetReference assetReference)
        {
            if (_handlers.TryGetValue(assetReference.AssetGUID, out var handles))
                return handles.First(x => !x.IsDone).GetDownloadStatus();
            
            return default;
        }

        private async UniTask<AsyncOperationStatus> Load(string address, string cacheKey)
        {
            Debug.Log($"Addr: Start loading {address} cached by {cacheKey}.");

            if (!await NeedLoadAssetsWithWarningFor(address))
            {
                Debug.LogWarning($"Addr: No need to load {address}. Exiting.");
                return AsyncOperationStatus.None;
            }

            if (IsLoading(cacheKey))
            {
                Debug.Log($"Addr: {address} is already loading. Waiting...");
                await WaitCompletion(cacheKey);
            }

            Debug.Log($"Addr: Downloading {address}...");
    
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<object>(address);
    
            await handle.Task;

            if (!handle.IsValid()) 
            {
                Debug.LogError($"[Load] Invalid handle for {address}");
                return AsyncOperationStatus.Failed;
            }
    
            if (handle.Status == AsyncOperationStatus.Failed) 
            {
                Debug.LogError($"[Load] Failed to load {address}. Error: {handle.Status}, Exception: {handle.OperationException}");
                return AsyncOperationStatus.Failed;
            }
    
            Debug.Log($"[Load] Successfully loaded {address}");
    
            return handle.Status;
        }

        private async UniTask<AsyncOperationStatus> Load(AssetReference assetReference, string cacheKey)
        {
            Debug.Log($"Addr: Start loading dependency for {assetReference} cached by {cacheKey}.");
            
            if (!await NeedLoadAssetsWithWarningFor(assetReference))
                return AsyncOperationStatus.None;
            
            if (IsLoading(cacheKey)) 
                await WaitCompletion(cacheKey);

            if (_handlers.ContainsKey(cacheKey))
            {
                Debug.Log($"Addr: Has cache for {assetReference} cached by {cacheKey}");
                if (!await NeedLoadAssetsWithWarningFor(assetReference))
                    return AsyncOperationStatus.None;
            }

            Debug.Log($"Addr: Loading dependency for {assetReference} cached by {cacheKey}...");
            var handle = Addressables.DownloadDependenciesAsync(assetReference, true);
            await RunWithCache(handle, cacheKey);
            Debug.Log($"Addr: Loaded dependency for {assetReference} cached by {cacheKey}. Result {handle.Status}");

            return handle.Status;
        }
        
        private async UniTask RunWithCache(AsyncOperationHandle handle, string cacheKey)
        {
            if (!_handlers.TryGetValue(cacheKey, out var handlers))
                _handlers[cacheKey] = new List<AsyncOperationHandle>();

            _handlers[cacheKey].Add(handle);
            await handle.Task;
        }

        private async Task WaitCompletion(string cacheKey)
        {
            Debug.Log($"Addr: Waiting completion of another loading for {cacheKey}...");
            await Task.WhenAll(_handlers[cacheKey].Where(x => x.IsValid()).Select(x => x.Task));
            await Task.Run(() => Thread.Sleep(1000));
            Debug.Log($"Addr: Another loading is complete. Continue load {cacheKey}");
        }

        
        private async UniTask<bool> NeedLoadAssetsWithWarningFor(string address)
        {
            bool needLoad = await NeedLoadAssetsFor(address);
            if (!needLoad)
                Debug.LogWarning($"Addr: Didn't need load dependency for {address}. Exit");

            return needLoad;
        }
        
        private async UniTask<bool> NeedLoadAssetsWithWarningFor(AssetReference assetReference)
        {
            bool needLoad = await NeedLoadAssetsFor(assetReference);
            if (!needLoad)
                Debug.LogWarning($"Addr: Didn't need load dependency for {assetReference}. Exit");

            return needLoad;
        }
    }
}
