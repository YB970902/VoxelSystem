using System;
using System.Collections;
using System.Collections.Generic;
using Bean.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bean.Addressable
{
    /// <summary>
    /// 어드레서블로 애셋을 로드해주는 것을 도와주는 매니저
    /// </summary>
    public class AddressableManager : MonoSingleton<AddressableManager>
    {
        /// <summary>
        /// 한 번 어드레서블로 로드한 애셋을 캐싱하기 위한 데이터
        /// </summary>
        public class AssetCache
        {
            /// <summary> 애셋의 주소 </summary>
            public string Address { get; private set; }
            /// <summary> 애셋 제거를 위해 관리하는 키 </summary>
            public string Key { get; private set; }
            /// <summary> 애셋 데이터 </summary>
            public UnityEngine.Object Asset { get; set; }
            /// <summary> 비동기 로드 시 콜백을 추가하기 위한 핸들 </summary>
            public AsyncOperationHandle Handle { get; set; }

            public AssetCache(string address, string key, UnityEngine.Object asset)
            {
                Address = address;
                Key = key;
                Asset = asset;
            }
        }
        
        Dictionary<string, AssetCache> dictAssetCache = new Dictionary<string, AssetCache>();

        protected override void Init()
        {
            dictAssetCache = new Dictionary<string, AssetCache>();
            
        }

        /// <summary>
        /// 애셋을 동기로 로드한다.
        /// </summary>
        public T LoadAssetSync<T>(string address, string key) where T : UnityEngine.Object
        {
            if (dictAssetCache.ContainsKey(address))
            {
                return dictAssetCache[address].Asset as T;
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            handle.WaitForCompletion();
            T result = handle.Result;
            dictAssetCache[address] = new AssetCache(address, key, result);
            return result;
        }

        /// <summary>
        /// 애셋을 비동기로 로드한다.
        /// </summary>
        public void LoadAssetAsync<T>(string address, string key, Action<T> cbOnLoad) where T : UnityEngine.Object
        {
            if (dictAssetCache.ContainsKey(address))
            {
                if (dictAssetCache[address].Asset != null)
                {
                    cbOnLoad?.Invoke(dictAssetCache[address].Asset as T);
                    return;
                }
                
                // 캐싱된 데이터가 있지만, 애셋이 아직 없다면 아직 로드가 안되었다는 의미이므로 핸들에 콜백함수를 추가한다.
                dictAssetCache[address].Handle.Completed += _ => cbOnLoad?.Invoke(_.Result as T);
                return;
            }
            
            AssetCache cache = new AssetCache(address, key, null);
            cache.Handle = Addressables.LoadAssetAsync<T>(address);;
            dictAssetCache[address] = cache;
            cache.Handle.Completed += OnLoadDone;
            
            void OnLoadDone(AsyncOperationHandle handle)
            {
                T asset = handle.Result as T;
                cache.Asset = asset;
                cbOnLoad?.Invoke(asset);
            }
        }
    }
}