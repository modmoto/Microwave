using System.Collections.Generic;
using Application.Framework;
using Domain.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Adapters.Json.ObjectPersistences
{
    public class DomainEventPersister : ObjectPersister<IEnumerable<DomainEvent>>, IDomainEventPersister
    {
        protected override void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            base.HandleDeserializationError(sender, errorArgs);
            if (errorArgs.ErrorContext.Error is JsonSerializationException)
            {
                errorArgs.ErrorContext.Handled = true;
            }
        }
    }
}