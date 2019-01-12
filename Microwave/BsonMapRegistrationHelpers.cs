using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microwave.Domain;
using MongoDB.Bson.Serialization;

namespace Microwave
{
    public static class BsonMapRegistrationHelpers
    {
        public static void AddBsonMapsForMicrowave(Assembly assembly)
        {
            var domainEventTypes = assembly.GetTypes().Where(ev => ev.GetInterfaces().Contains(typeof(IDomainEvent)));
            var addBsonMapGeneric = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddBsonMapFor));

            foreach (var domainEventType in domainEventTypes)
            {
                if (addBsonMapGeneric != null)
                {
                    var addBsonMap = addBsonMapGeneric.MakeGenericMethod(domainEventType);
                    addBsonMap.Invoke(null, new object[] {});
                }
            }
        }

        public static void AddBsonMapFor<T>() where T : IDomainEvent
        {
            BsonClassMap.RegisterClassMap<T>(c =>
            {
                var eventType = typeof(T);
                var constructorInfo = GetConstructorWithMostMatchingParameters(eventType);

                foreach (var property in eventType.GetProperties())
                {
                    c.MapProperty(property.Name);
                }

                var lambdaParameter = Expression.Parameter(eventType, "domainEvent");
                var secondProp = Expression.Property(lambdaParameter, "Name");


                var firstProp = Expression.Property(lambdaParameter, nameof(IDomainEvent.EntityId));
                var idOfIdentity = Expression.Property(firstProp, nameof(Identity.Id));
                var identityCreate = typeof(Identity).GetMethod(nameof(Identity.Create), new []{ typeof(string) });
                var identity = Expression.Call(identityCreate, idOfIdentity);
                var idConverted = Expression.Convert(identity, typeof(GuidIdentity));

                var body = Expression.New(constructorInfo, idConverted, secondProp);

                var funcType = typeof(Func<,>).MakeGenericType(eventType, eventType);
                var lambda = Expression.Lambda(funcType, body, lambdaParameter);

                var expressionType = typeof(Expression<>).MakeGenericType(funcType);
                var genericClassMap = typeof(BsonClassMap<>).MakeGenericType(eventType);
                var mapCreatorFunction = genericClassMap.GetMethod(nameof(BsonClassMap.MapCreator), new []{
                expressionType });
                mapCreatorFunction.Invoke(c, new []{ lambda });
            });
        }

        private static ConstructorInfo GetConstructorWithMostMatchingParameters(Type eventType)
        {
            var constructors = eventType.GetConstructors();
            var maxParams = constructors.Max(c => c.GetParameters().Length);
            return constructors.FirstOrDefault(c => c.GetParameters().Length == maxParams);
        }
    }
}