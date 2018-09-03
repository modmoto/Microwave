using Application.Framework;

namespace Adapters.Json.ObjectPersistences
{
    public class QuerryPersister<T> : JsonFileObjectPersister<T> where T : Querry
    {
    }
}