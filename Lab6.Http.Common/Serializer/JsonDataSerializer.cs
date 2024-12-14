
using System.Text.Json;
using Lab6.Http.Common;

public class JsonDataSerializer<TModel> : IDataSerializer<TModel>
{
    private JsonSerializerOptions options;

    public JsonDataSerializer()
    {
        options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public string SerializerType => "json";

    public TModel? Deserialize(string stringData)
    {
        try {
            var model = JsonSerializer.Deserialize<TModel>(stringData);

            return model!;
        }
        catch
        {
            return default;
        }
    }

    public string Serialize(TModel model)
    {
        var jsonString = JsonSerializer.Serialize(model, options);
        return jsonString;
    }
}
