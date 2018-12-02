﻿using Microwave.Application;
using Microwave.Application.Ports;
using Newtonsoft.Json;

namespace Microwave.ObjectPersistences
{
    public class ObjectConverter : IObjectConverter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public string Serialize<T>(T objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, _settings);
        }

        public T Deserialize<T>(string payLoad)
        {
            var deserializeObject = JsonConvert.DeserializeObject<T>(payLoad, _settings);
            return deserializeObject;
        }
    }
}