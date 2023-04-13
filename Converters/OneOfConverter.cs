using System.Reflection;
using Newtonsoft.Json;
using OneOf;

namespace TNRD.Zeepkist.GTR.Backend.Converters;

internal class OneOfConverter : JsonConverter
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        IOneOf instance = (IOneOf)value!;
        serializer.Serialize(writer, instance.Value);
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    )
    {
        List<Type> genericArguments = objectType.GetGenericArguments().ToList();

        Type idType = genericArguments.First(x => x.IsValueType);
        Type objType = genericArguments.First(x => !x.IsValueType);
        int idTypeIndex = genericArguments.IndexOf(idType);
        int objTypeIndex = genericArguments.IndexOf(objType);

        object?[] parameters = new object?[3];

        if (reader.TokenType == JsonToken.StartObject)
        {
            parameters[0] = objTypeIndex;
            parameters[idTypeIndex + 1] = Activator.CreateInstance(idType);
            parameters[objTypeIndex + 1] = serializer.Deserialize(reader, objType);
        }
        else
        {
            parameters[0] = idTypeIndex;
            parameters[idTypeIndex + 1] = serializer.Deserialize(reader, idType)!;
            parameters[objTypeIndex + 1] = null;
        }

        ConstructorInfo constructor =
            objectType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();

        object instance = constructor.Invoke(parameters);
        return instance;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        if (!typeof(IOneOf).IsAssignableFrom(objectType))
            return false;

        Type[] arguments = objectType.GetGenericArguments();
        if (arguments.Length != 2)
            return false;

        return (arguments[0].IsValueType && !arguments[1].IsValueType) ||
               (!arguments[0].IsValueType && arguments[1].IsValueType);
    }
}
