#if EXTENJECT_INCLUDE_ADDRESSABLE_BINDINGS
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Assert = NUnit.Framework.Assert;

public class TestAddressable : ZenjectIntegrationTestFixture
{
    private AssetReferenceT<GameObject> addressablePrefabReference;

    [TearDown]
    public void Teardown()
    {
        addressablePrefabReference = null;
        Resources.UnloadUnusedAssets();
    }
    
    [Test]
    public async UniTask TestAddressableAsyncLoad()
    {
        await ValidateTestDependency();
        
        PreInstall();
        AsyncOperationHandle<GameObject> handle = default;
        Container.BindAsync<GameObject>().FromMethod(async () =>
        {
            try
            {
                var locationsHandle = Addressables.LoadResourceLocationsAsync("TestAddressablePrefab");
                await locationsHandle.ToUniTask();
                Assert.Greater(locationsHandle.Result.Count, 0, "Key required for test is not configured. Check Readme.txt in addressable test folder");

                IResourceLocation location = locationsHandle.Result[0];
                handle = Addressables.LoadAssetAsync<GameObject>(location);
                await handle.ToUniTask();
                return handle.Result;
            }
            catch (InvalidKeyException)
            {
            }
            return null;
        }).AsCached();
        PostInstall();

        await UniTask.Yield();
        
        AsyncInject<GameObject> asyncFoo = Container.Resolve<AsyncInject<GameObject>>();

        int frameCounter = 0;
        while (!asyncFoo.HasResult && !asyncFoo.IsFaulted)
        {
            frameCounter++;
            if (frameCounter > 10000)
            {
                Addressables.Release(handle);
                Assert.Fail();
            }
            await UniTask.Yield();    
        }
        
        Addressables.Release(handle);
        Assert.Pass();
    }
    
    [Test]
    public async UniTask TestAssetReferenceTMethod()
    {
        await ValidateTestDependency();

        PreInstall();

        Container.BindAsync<GameObject>()
            .FromAssetReferenceT(addressablePrefabReference)
            .AsCached();
        PostInstall();

        AddressableInject<GameObject> asyncPrefab = Container.Resolve<AddressableInject<GameObject>>();

        int frameCounter = 0;
        while (!asyncPrefab.HasResult && !asyncPrefab.IsFaulted)
        {
            frameCounter++;
            if (frameCounter > 10000)
            {
                Assert.Fail();
            }
            await UniTask.Yield();    
        }
        
        Addressables.Release(asyncPrefab.AssetReferenceHandle);
        Assert.Pass();
    }
    
    [Test]
    public async UniTask TestFailedLoad()
    {
        PreInstall();
        
        Container.BindAsync<GameObject>().FromMethod(async () =>
        {
            FailedOperation failingOperation = new FailedOperation();
            var customHandle = Addressables.ResourceManager.StartOperation(failingOperation, default(AsyncOperationHandle));   
            await customHandle.ToUniTask();

            if (customHandle.Status == AsyncOperationStatus.Failed)
            {
                throw new Exception("Async operation failed", customHandle.OperationException);
            }
            
            return customHandle.Result;
        }).AsCached();
        PostInstall();

        await UniTask.Yield();
        
        AsyncInject<GameObject> asyncGameObj = Container.Resolve<AsyncInject<GameObject>>();

        Assert.IsFalse(asyncGameObj.HasResult);
        Assert.IsTrue(asyncGameObj.IsCompleted);
        Assert.IsTrue(asyncGameObj.IsFaulted);
    }

    private class FailedOperation : AsyncOperationBase<GameObject>
    {
        protected override void Execute()
        {
            Complete(null, false, "Intentionally failed message");
        }
    }

    private async UniTask ValidateTestDependency()
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationsHandle;
        try
        {
            locationsHandle = Addressables.LoadResourceLocationsAsync("TestAddressablePrefab");
            await locationsHandle.ToUniTask();
        }
        catch (Exception)
        {
            Assert.Inconclusive("You need to set TestAddressablePrefab key to run this test");
            return;
        }

        var locations = locationsHandle.Result;
        if (locations == null || locations.Count == 0)
        {
            Assert.Inconclusive("You need to set TestAddressablePrefab key to run this test");
        }

        var resourceLocation = locations[0];

        if (resourceLocation.ResourceType != typeof(GameObject))
        {
            Assert.Inconclusive("TestAddressablePrefab should be a GameObject");
        }

        addressablePrefabReference = new AssetReferenceT<GameObject>(resourceLocation.PrimaryKey);
    }
}
#endif
