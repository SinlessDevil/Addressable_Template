using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.AssetProvider
{
    public class AssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new();
        private readonly Dictionary<string, AsyncOperationHandle> _completedPersistenceHandles = new();
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handles = new();
        private readonly Dictionary<string, List<AsyncOperationHandle>> _persistenceHandles = new();

        public string Progress { get; private set; }
        public void Initialize()
        {
            Addressables.InitializeAsync();
        }
        
        public async UniTask<T> Load<T>(AssetReference assetReference) where T : class
        {
            //Debug.Log(assetReference);
            string cacheKey = assetReference.AssetGUID + assetReference.SubObjectName;
            if (_completedHandles.TryGetValue(cacheKey, out AsyncOperationHandle completedHandle) ||
                _completedPersistenceHandles.TryGetValue(cacheKey, out completedHandle))
                return completedHandle.Result as T;
            
            return await RunWithCacheOnComplete(
                Addressables.LoadAssetAsync<T>(assetReference),
                cacheKey: cacheKey);
        }

        public async UniTask<T> Load<T>(string address) where T : class
        {
            //Debug.Log(address);
            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle completedHandle) ||
                _completedPersistenceHandles.TryGetValue(address, out completedHandle))
                return completedHandle.Result as T;
            
            return await RunWithCacheOnComplete(
                Addressables.LoadAssetAsync<T>(address),
                cacheKey: address);
        }
        
        public async UniTask<T> LoadPersistence<T>(AssetReference assetReference) where T : class
        {
            //Debug.Log(assetReference);
            string cacheKey = assetReference.AssetGUID + assetReference.SubObjectName;
            if (_completedHandles.TryGetValue(cacheKey, out AsyncOperationHandle completedHandle) ||
                _completedPersistenceHandles.TryGetValue(cacheKey, out completedHandle))
                return completedHandle.Result as T;

            return await RunWithPersistenceCacheOnComplete(
                Addressables.LoadAssetAsync<T>(assetReference),
                cacheKey: cacheKey);
        }

        public async UniTask<T> LoadPersistence<T>(string address) where T : class
        {
            //Debug.Log(address);
            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle completedHandle) ||
                _completedPersistenceHandles.TryGetValue(address, out completedHandle))
                return completedHandle.Result as T;
            
            return await RunWithPersistenceCacheOnComplete(
                Addressables.LoadAssetAsync<T>(address),
                cacheKey: address);
        }

        public void MakeTemp(AssetReference assetReference)
        {
            string cacheKey = assetReference.AssetGUID + assetReference.SubObjectName;

            if (_persistenceHandles.TryGetValue(cacheKey, out List<AsyncOperationHandle> handles))
            {
                _handles[cacheKey] = handles;
                _persistenceHandles.Remove(cacheKey);

                var nonCompletedHandles = handles.Where(x => !x.IsDone);

                foreach (AsyncOperationHandle handle in nonCompletedHandles)
                {
                    handle.Completed += completed => MakeTemp(assetReference);
                }
            }

            if (_completedPersistenceHandles.TryGetValue(cacheKey, out AsyncOperationHandle completedHandle))
            {
                _completedHandles[cacheKey] = completedHandle;
                _completedPersistenceHandles.Remove(cacheKey);
            }
        }
        
        public void MakePersistence(AssetReference assetReference)
        {
            string cacheKey = assetReference.AssetGUID + assetReference.SubObjectName;

            if (_handles.TryGetValue(cacheKey, out List<AsyncOperationHandle> handles))
            {
                _persistenceHandles[cacheKey] = handles;
                _handles.Remove(cacheKey);

                var nonCompletedHandles = handles.Where(x => !x.IsDone);

                foreach (AsyncOperationHandle handle in nonCompletedHandles)
                {
                    handle.Completed += completed => MakePersistence(assetReference);
                }
            }

            if (_completedHandles.TryGetValue(cacheKey, out AsyncOperationHandle completedHandle))
            {
                _completedPersistenceHandles[cacheKey] = completedHandle;
                _completedHandles.Remove(cacheKey);
            }
        }

        public void CleanUp()
        {
            foreach (List<AsyncOperationHandle> handles in _handles.Values)
            foreach (AsyncOperationHandle handle in handles)
                Addressables.Release(handle);
            
            _handles.Clear();
            _completedHandles.Clear();
        }

        private async UniTask<T> RunWithCacheOnComplete<T>(AsyncOperationHandle<T> handle, string cacheKey) where T : class
        {
            handle.Completed += h =>
            {
                _completedHandles[cacheKey] = h;
            };

            AddTempHandle(cacheKey, handle);
            while (!handle.IsDone)
            {
                Progress = $"{handle.PercentComplete}";
                await UniTask.Yield();
            }

            return await handle.Task;
        }

        private async UniTask<T> RunWithPersistenceCacheOnComplete<T>(AsyncOperationHandle<T> handle, string cacheKey) where T : class
        {
            handle.Completed += h =>
            {
                _completedPersistenceHandles[cacheKey] = h;
            };

            AddPersistenceHandle(cacheKey, handle);
            while (!handle.IsDone)
            {
                Progress = $"{handle.PercentComplete}";
                await UniTask.Yield();
            }
            return await handle.Task;
        }
        
        private void AddTempHandle<T>(string key, AsyncOperationHandle<T> handle) where T : class =>
            AddHandle(key, handle, _handles);

        private void AddPersistenceHandle<T>(string key, AsyncOperationHandle<T> handle) where T : class =>
            AddHandle(key, handle, _persistenceHandles);
        
        private static void AddHandle<T>(string key, AsyncOperationHandle<T> handle, Dictionary<string, List<AsyncOperationHandle>> handleCollection) where T : class
        {
            if (!handleCollection.TryGetValue(key, out List<AsyncOperationHandle> handles))
            {
                handles = new List<AsyncOperationHandle>();
                handleCollection[key] = handles;
            }

            handles.Add(handle);
        }
    }
}