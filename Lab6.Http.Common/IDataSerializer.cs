namespace Lab6.Http.Common;

public interface IDataSerializer<TModel>
{
    string Serialize(TModel model);

    TModel? Deserialize(string stringData);

    public string SerializerType { get; }
}
