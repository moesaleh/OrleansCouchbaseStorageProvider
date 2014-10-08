OrleansCouchbaseStorageProvider
===============================

An Orleans grain state provider which persists state in JSON format in Couchbase Database Storage

The `master` branch targets the September (0.9) Orleans SDK release and Couchbase .NET Client Library 1.2.5

## Installation

The Orleans SDK must be installed first.

Using NuGet, with the GrainClasses implementation (or Silo Host) project as your target:

```
Install-Package Orleans.StorageProvider.Blob
```
Then install Couchbase .NET Client Library 1.2.5 using NuGet:

```
Install-Package CouchbaseNetClient
```

Then register the provider in your Silo Configuration:

```xml
<?xml version="1.0" encoding="utf-8"?>
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
    <StorageProviders>
      <Provider Type="Orleans.StorageProvider.Couchbase.CouchbaseStorageProvider" Name="CouchbaseStore" />
    </StorageProviders>
    ...
```

Then register the Couchbase nodes and data buckets configuration in the app.config or web.config within the Silo host project as the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configSections>
    <section name="couchbase" type="Couchbase.Configuration.CouchbaseClientSection, Couchbase"/>
</configSections>
<couchbase>
    <servers bucket="beer-sample">
      <add uri="http://127.0.0.1:8091/pools"/>
    </servers>
</couchbase>
    ...
```

Then from your grain code configure grain storage:

```cs
// define a state interface
public interface IDeviceGrainState : IGrainState
{
    string UniqueID { get; set; }
}

// Select the CouchbaseStore as the storage provider for the grain
[StorageProvider(ProviderName="CouchbaseStore")]
public class DeviceGrain : Orleans.Grain<IDeviceGrainState>, DeviceGrain
{
    public Task Test(string value)
    {
    	// set the state and save it
        this.State.UniqueID = value;
        return this.State.WriteStateAsync();
    }

}
```

Grains are stored in json format with the following naming convention within the Couchbase Data Bucket:

```
{GrainType}-{GrainId}.json
```

i.e.

```
IoTDevice.Grain1-0.json
```

## License

MIT
