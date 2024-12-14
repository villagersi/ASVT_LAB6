using Lab6.Http.Common;

internal abstract class StorageBase<TModel>
{
    private readonly string filePath;
    private readonly IDataSerializer<TModel[]> dataSerializer;

    public StorageBase(string filePath, IDataSerializer<TModel[]> dataSerializer)
    {
        this.filePath = filePath;
        this.dataSerializer = dataSerializer;

        var storageDirectory = Path.GetDirectoryName(filePath) ?? "Data";

        if (!Directory.Exists(storageDirectory))
        {
            Directory.CreateDirectory(storageDirectory);
        }
    }


    public async Task<TModel[]> ReadAsync()
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<TModel>();
        }

        var dataText = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrEmpty(dataText)) 
        {
            return Array.Empty<TModel>();
        }

        var data = dataSerializer.Deserialize(dataText);
        return data ?? Array.Empty<TModel>();
    }

    public async Task<bool> WriteAsync(TModel[] models)
    {
        var dataText = dataSerializer.Serialize(models);
        await File.WriteAllTextAsync(filePath, dataText);

        return true;
    }
}
