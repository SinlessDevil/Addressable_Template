using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Services.AssetPreloader;
using Services.PersistenceProgress;
using Services.SaveLoad;
using Services.StaticData;
using StaticData;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.PreloaderConductor
{
    public class AssetPreloaderConductor : IAssetPreloaderConductor
    {
        private const int UpdateInterval = 3;
        
        private readonly IAssetPreloaderService _assetPreloaderService;
        private readonly IStaticDataService _staticDataService;
        private readonly IPersistenceProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;

        public AssetPreloaderConductor(IAssetPreloaderService assetPreloaderService, 
            IStaticDataService staticDataService,
            IPersistenceProgressService progressService,
            ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
            _progressService = progressService;
            _assetPreloaderService = assetPreloaderService;
            _staticDataService = staticDataService;
        }
        
        public async UniTask TryPreload()
        {
            await TryPreloadByLevel();
        }

        private async UniTask TryPreloadByLevel()
        {
            int currentLevel = _progressService.PlayerData.PlayerLevelData.CurrentProgress.LevelId;
            
            await PreloadLevelDependency(currentLevel);
        }

        private async UniTask PreloadLevelDependency(int level)
        {
            var preloadTasks = LevelConfigsForPreload(level)
                .Select(config => PreloadDependency(config))
                .ToArray();
            
            await UniTask.WhenAll(preloadTasks);
        }

        private async UniTask PreloadDependency(PreloadGroup config)
        {
            if (await _assetPreloaderService.NeedLoadAssetsFor(config.AssetGroupName))
            {
                Debug.Log($"Start Preload: {config.AssetGroupName}");
                AsyncOperationStatus status = await _assetPreloaderService.LoadAssetsFor(config.AssetGroupName);

                if (status == AsyncOperationStatus.Succeeded)
                    RegisterAsPreloaded(config);
            }
            else
            {
                RegisterAsPreloaded(config);
            }
        }

        private void RegisterAsPreloaded(PreloadGroup configOrNull)
        {
            _progressService.PlayerData.Loading.Version = Application.version;
            _progressService.PlayerData.Loading.LoadedKeys.Add(configOrNull.AssetGroupName);
            
            _saveLoadService.SaveProgress();
        }

        private IEnumerable<PreloadGroup> LevelConfigsForPreload(int level) =>
            _staticDataService.PreloadConfig.LevelGroups.Where(x =>
            {
                bool isAlreadyPreloaded = _progressService.PlayerData.Loading.LoadedKeys.Contains(x.AssetGroupName);
                return x.LoadAfterUnlocked <= level && !isAlreadyPreloaded;
            });
    }
}