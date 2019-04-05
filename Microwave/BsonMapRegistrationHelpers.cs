using System;
using System.Collections.Generic;
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
            var addBsonMapGeneric = typeof(BsonMapRegistrationHelpers).GetMethod(nameof(AddBsonMapFor),
                BindingFlags.Public | BindingFlags.Static);

            foreach (var domainEventType in domainEventTypes)
            {
                var addBsonMap = addBsonMapGeneric.MakeGenericMethod(domainEventType);
                addBsonMap.Invoke(null, new object[] { });
            }
        }

        public static void AddBsonMapFor<T>() where T : IDomainEvent
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T))) BsonClassMap.RegisterClassMap<T>(map =>
            {
                var eventType = typeof(T);
                var lambdaParameter = Expression.Parameter(eventType, "domainEvent");

                var constructorInfo = GetConstructorWithMostMatchingParameters(eventType);
                MapProperties(eventType.GetProperties(), map);

                var expressions = CreateExpressionsFromConstructorParameters<T>(constructorInfo, eventType, lambdaParameter);

                var body = Expression.New(constructorInfo, expressions);

                var funcType = typeof(Func<,>).MakeGenericType(eventType, eventType);
                var lambda = Expression.Lambda(funcType, body, lambdaParameter);

                var expressionType = typeof(Expression<>).MakeGenericType(funcType);
                var genericClassMap = typeof(BsonClassMap<>).MakeGenericType(eventType);
                var mapCreatorFunction = genericClassMap.GetMethod(nameof(BsonClassMap.MapCreator), new[]
                {
                    expressionType
                });
                mapCreatorFunction.Invoke(map, new[] {lambda});
            });
        }

        private static List<Expression> CreateExpressionsFromConstructorParameters<T>(
            ConstructorInfo constructorInfo,
            Type eventType,
            ParameterExpression lambdaParameter)
            where T : IDomainEvent
        {
            var expressions = new List<Expression>();
            var entityIdFound = false;
            foreach (var parameter in constructorInfo.GetParameters())
            {
                var parameterName = GetParameterNameForProperty(parameter.Name, eventType.GetProperties());
                if (parameterName == null)
                    throw new IllegalDomainEventContructorException(
                        $"Can not find identically named property for parameter: {parameter.Name} in DomainEvent {typeof(T).Name}. Rename the parameter to match a Property!");
                if (parameter.Name.ToLower() == nameof(IDomainEvent.EntityId).ToLower()) entityIdFound = true;

                expressions.Add(ExtractExpression(lambdaParameter, parameter));
            }

            if (!entityIdFound)
                throw new IllegalDomainEventContructorException(
                    $"Not parameter with entityId defined in constructor for DomainEvent {typeof(T).Name}. Can not initialize DomainEvent correctly");
            return expressions;
        }

        private static void MapProperties<T>(PropertyInfo[] propertyInfos, BsonClassMap<T> c) where T : IDomainEvent
        {
            foreach (var property in propertyInfos) c.MapProperty(property.Name);
        }

        private static Expression ExtractExpression(ParameterExpression lambdaParameter, ParameterInfo parameter)
        {
            if (IsIdentityParameter(parameter))
            {
                var firstProp = Expression.Property(lambdaParameter, parameter.Name);
                var idOfIdentity = Expression.Property(firstProp, nameof(Identity.Id));
                var identityCreate = typeof(Identity).GetMethod(nameof(Identity.Create), new[] {typeof(string)});
                var identity = Expression.Call(identityCreate, idOfIdentity);
                var idConverted = Expression.Convert(identity, parameter.ParameterType);
                return idConverted;
            }
            else
            {
                return Expression.Property(lambdaParameter, parameter.Name);
            }
        }

        private static bool IsIdentityParameter(ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            return parameterType == typeof(GuidIdentity) || parameterType == typeof(StringIdentity) || parameterType ==
                   typeof
                       (Identity);
        }

        private static string GetParameterNameForProperty(string parameterName, PropertyInfo[] properties)
        {
            return properties.FirstOrDefault(p => p.Name.ToLower() == parameterName.ToLower())?.Name;
        }

        private static ConstructorInfo GetConstructorWithMostMatchingParameters(Type eventType)
        {
            var constructors = eventType.GetConstructors();
            var maxParams = constructors.Max(c => c.GetParameters().Length);
            return constructors.FirstOrDefault(c => c.GetParameters().Length == maxParams);
        }
    }
}