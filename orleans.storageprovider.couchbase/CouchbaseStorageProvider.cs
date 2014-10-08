using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Storage;
using Orleans.Runtime;
using Newtonsoft.Json;
using Couchbase;
using Couchbase.Extensions;
using Enyim.Caching.Memcached;


namespace Orleans.StorageProvider.Couchbase
{
    public class CouchbaseStorageProvider : IStorageProvider
    {
        public string Name  { get; set;}
        public OrleansLogger Log { get; set; }

        public Task Init(string name, Orleans.Providers.IProviderRuntime providerRuntime, Orleans.Providers.IProviderConfiguration config)
        {
            this.Name = name;
            return TaskDone.Done;
        }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public async Task ReadStateAsync(string grainType, Orleans.Runtime.GrainReference grainReference, IGrainState grainState)
        {

            var GrainKey = GetGrainKey(grainType, grainReference);
            try
            {
                var Client = new CouchbaseClient();
                if (Client.KeyExists(GrainKey))
                {
                    var json = "";
                    await Task.Run(() => { json = Client.Get(GrainKey).ToString(); });
                    var data = JsonConvert.DeserializeObject<Dictionary<String, Object>>(json);
                    grainState.SetAll(data);
                }
            }
            catch (Exception ex)
            {

                Log.Error(0, "Error in ReadStateAsync", ex);
            }
        }

        public async Task WriteStateAsync(string grainType, Orleans.Runtime.GrainReference grainReference, IGrainState grainState)
        {
            try
            {
                var Client = new CouchbaseClient();
                var GrainKey = GetGrainKey(grainType, grainReference);
                var json = JsonConvert.SerializeObject(grainState.AsDictionary());
                await Task.Run(() =>
                {
                    Client.Store(StoreMode.Set, GrainKey, json);
                });

            }
            catch (Exception ex)
            {

                Log.Error(0, "Error in WriteStateAsync", ex);
            }
        }

        public async Task ClearStateAsync(string grainType, Orleans.Runtime.GrainReference grainReference, IGrainState grainState)
        {
            try
            {
                var Client = new CouchbaseClient();
                await Task.Run(() => Client.Remove(GetGrainKey(grainType, grainReference)));
            }
            catch (Exception ex)
            {

                Log.Error(0, "Error in ClearStateAsync", ex);
            }
        }
        
        private string GetGrainKey(string grainType, GrainReference grainReference)
        {
            return string.Format("{0}-{1}.json", grainType, grainReference.ToKeyString());
        }

    }
}
