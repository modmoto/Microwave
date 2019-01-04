<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**

- [Microwave](#microwave)
  - [Overview](#overview)
  - [EventStore](#eventstore)
    - [DomainEvents](#domainevents)
    - [Entities](#entities)
  - [Distributed Event Handling](#distributed-event-handling)
    - [EventHandlers](#eventhandlers)
    - [ReadModels](#readmodels)
  - [WebApi](#webapi)
    - [EventStreams](#eventstreams)
  - [Quality of Live Tools](#quality-of-live-tools)
    - [Exceptions](#exceptions)
    - [Result Objects](#result-objects)
    - [WebApi Filters](#webapi-filters)
    - [Identity Handling in WebApi](#identity-handling-in-webapi)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Microwave
Microwave is a Framework for Eventsourcing and CQRS that lets you implement your domain and not have you worried about distributing your domain events or storing them.

[![Build status](https://ci.appveyor.com/api/projects/status/sc8x8uaakosryu7c?svg=true)](https://ci.appveyor.com/project/Lauchi/microwave)
[![sonarCubeCoverage](https://sonarcloud.io/api/project_badges/measure?project=Lauchi_Microwave&metric=coverage)](https://sonarcloud.io/dashboard?id=Lauchi_Microwave)


## Overview

### Installation

## EventStore

The `EventStore` is your persistance layer that offers saving DomainEvents and restoring Entities by EventSourcing. There is a SnapShot function for performance reasons, if you need it. Inject the `IEventStore` into your desired class and use the functions to save and load entities. The Eventstore uses optimistic concurrency so you have to give him the version of the entity that you are trying to save.

### DomainEvents

To append DomainEvents to the `EventStore` you have to implement the `IDomainEvent` interface on your DomainEvents. The interface forces you to implement the property `EntityId` so the eventstore can assign the events to the entity. Everything else is up to your choice. The `EventStore` also generates upcounting versions for the DomainEvents.

### IApply/Entities

To Load an entity the Entity has to implement the Interface `IApply` wich takes a list of DomainEvents and forces you to apply them to your entity. There is a class `Entity` that implements the `IApply` method in a way, so the entity applies the DomainEvent to private or public methods that take a single DomainEvent. Reflection is used so you might want to do it on your own, if you run into performance issues. Example:

```
public class User : Entity
{
    public void Apply(UserCreatedEvent domainEvent)
    {
        Id = domainEvent.EntityId;
    }

    private void Apply(UserChangedNameEvent domainEvent)
    {
        Name = domainEvent.Name;
    }

    public Identity Id { get; private set; }
    public string Name { get; private set; }
}

// OR with IApply

public class User2 : IApply
{
    public void Apply(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case UserCreatedEvent ev: Apply(ev);
                case UserChangedNameEvent ev: Apply(ev);

            }
        }
    }

    public void Apply(UserCreatedEvent domainEvent)
    {
        Id = domainEvent.EntityId;
    }

    public void Apply(UserChangedNameEvent domainEvent)
    {
        Name = domainEvent.Name;
    }

    public Identity Id { get; private set; }
    public string Name { get; private set; }
}
```

### Snapshots

## Distributed Event Handling

### EventHandlers

### ReadModels

## WebApi

### EventStreams

## Quality of Live Tools

### Exceptions

### Result Objects

### WebApi Filters

### Identity Handling in WebApi

## Known Issues

### String and GuidIdentity in Parameters