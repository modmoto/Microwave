using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Domain.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adapters.Framework.EventStores
{
    public class EventSourcingAtributeStrategy : IEventSourcingStrategy
    {
        public T Apply<T>(T entity, DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var eventProperties = eventType.GetProperties().Where(eventProp => eventProp.Name != nameof(DomainEvent.Id));

            var eventJson = JObject.Parse(JsonConvert.SerializeObject(domainEvent));
            var entityJson = JObject.Parse(JsonConvert.SerializeObject(entity));

            foreach (var eventProperty in eventProperties)
            {
                var customAttributes = eventProperty.GetCustomAttributes(typeof(ActualPropertyName));
                var attributes = customAttributes.ToList();

                if (attributes.Any())
                {
                    var pathSplitted = ((ActualPropertyName) attributes.First()).Path;
                    var eventValue = eventProperty.GetValue(domainEvent);
                    var dynamicObjectRenamed = CreateDynamicObject(pathSplitted, new Dictionary<string, object>(), eventValue);
                    MergeOnto(eventJson, dynamicObjectRenamed);

                    var eventPropertyName = eventProperty.Name;
                    var entityValueProperty = entity.GetType().GetProperty(eventPropertyName);
                    if (entityValueProperty != null)
                    {
                        var entityValue = entityValueProperty.GetValue(entity);
                        var dynamicObjectOld = CreateDynamicObject(new [] { eventPropertyName }, new Dictionary<string, object>(), entityValue);
                        MergeOnto(eventJson, dynamicObjectOld);
                    }
                }
            }

            MergeOnto(entityJson, eventJson);
            var deserializeObject = entityJson.ToObject<T>();
            return deserializeObject;
        }

        private void MergeOnto(JObject target, object source)
        {
            var dynamicObjectOldJson = JObject.Parse(JsonConvert.SerializeObject(source));
            target.Merge(dynamicObjectOldJson, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge
            });
        }

        private IDictionary<string, object> CreateDynamicObject(string[] pathSplitted, Dictionary<string, object> dictionary, object eventValue)
        {
            if (pathSplitted.Length == 1)
            {
                var dic = new Dictionary<string, object>();
                dic.Add(pathSplitted[0], eventValue);
                return dic;
            }

            var dynamicObject = CreateDynamicObject(pathSplitted.Skip(1).ToArray(), dictionary, eventValue);
            dictionary.Add(pathSplitted[0], dynamicObject);
            return dictionary;
        }
    }
}