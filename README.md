<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**

- [Overview](#overview)
  - [Installation](#installation)
  - [Database](#database)
- [EventStore](#eventstore)
  - [DomainEvents and Identities](#domainevents-and-identities)
  - [IApply/Entities](#iapplyentities)
  - [Snapshots](#snapshots)
- [Distributed Event Handling](#distributed-event-handling)
  - [Reacting to Events happening in the Domain](#reacting-to-events-happening-in-the-domain)
  - [ReadModels and Querries](#readmodels-and-querries)
    - [Loading Querries and Readmodels](#loading-querries-and-readmodels)
  - [Setting up the event source](#setting-up-the-event-source)
- [Quality of Live Tools](#quality-of-live-tools)
  - [Exceptions and Filters](#exceptions-and-filters)
  - [Result Objects](#result-objects)
  - [Identity Handling in WebApi](#identity-handling-in-webapi)
- [Known Issues](#known-issues)
  - [StringIdentity and GuidIdentity in Constructor of DomainEvents](#stringidentity-and-guididentity-in-constructor-of-domainevents)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Microwave
Microwave is a Framework for Eventsourcing and CQRS that lets you implement your domain and not have you worried about distributing your domain events or storing them.

[![Build status](https://ci.appveyor.com/api/projects/status/sc8x8uaakosryu7c?svg=true)](https://ci.appveyor.com/project/Lauchi/microwave)
[![sonarCubeCoverage](https://sonarcloud.io/api/project_badges/measure?project=Lauchi_Microwave&metric=coverage)](https://sonarcloud.io/dashboard?id=Lauchi_Microwave)


# Overview

## Installation

There is a release nuget feed where you can get the release version of Microwave : https://ci.appveyor.com/nuget/microwave-p8dt2iyv8g0c

To install all Microwave dependencies, use this in the startup:

```
services.AddMicrowave(typeof(UserCreated).Assembly, Configuration);
```

You have to tell Microwave the Assembly with the DomainEvents, so it can set up all the handling. Also you have to pass an `IConfiguration` to tell Microwave your DBConnectionstring.

On the readside, you have to add:

```
services.AddMicrowaveReadModels(typeof(UserReadModel).Assembly, Configuration);
```
and the assembly containing the ReadModels/Querries. Also you have to start Microwave with this in the builder section:

```
app.RunMicrowaveQueries();
```

Now you should be able to start the services and the read side will update when something on the write side happens, provided you defined some handlers. Of course you can implement the read side on the write side, too.

The Packages are also available in smaller chunks, like Microwave.Eventstores, Microwave.Domain, etc. if you want them to be used in other projects, that are not in the Host.

## Database

Microwave uses mongodb as the database and you have to define the connectionstring in the `appsettings.json` for the service to work. The definition goes like this:

```
{
  "WriteModelDatabase": {
    "ConnectionString" : "mongodb://localhost:815/",
    "DatabaseName" : "MyWriteDb"                        --> optional, default is "MicrowaveWriteModelDatabase"
  },
  "ReadModelDatabase": {
      "ConnectionString" : "mongodb://localhost:815/",
      "DatabaseName" : "MyReadDb"                       --> optional, default is "MicrowaveReadModelDatabase"
    }
}
```

# EventStore

The `EventStore` is your persistance layer that offers saving DomainEvents and restoring Entities by EventSourcing. There is a SnapShot function for performance reasons, if you need it. Inject the `IEventStore` into your desired class and use the functions to save and load entities. The Eventstore uses optimistic concurrency so you have to give him the version of the entity that you are trying to save.

## DomainEvents and Identities

To append DomainEvents to the `EventStore` you have to implement the `IDomainEvent` interface on your DomainEvents. The interface forces you to implement the property `EntityId` so the eventstore can assign the events to the entity. Everything else is up to your choice. The `EventStore` also generates upcounting versions for the DomainEvents. The Identity class is a base class that has two implementations: `StringIdentity` and `GuidIdentity`. You can create them with a string or an guid on the static create method. Equals and == are overwritten so you can use them just like a string or a guid and the eventstore can handle both of them. As Microwave defines Modelbinders and Formatters for those types, they feel like strings/guid regarding the webapi. For more see the chapters below. I found myself using the `StringIdentity` to give an id in a specific scenario more context.

## IApply/Entities

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
    public void Apply(IEnumerable<IDomainEvent> domainEvents)  // this here is basically what is done in the Entity class with reflection
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

## Snapshots

The EventStore supports snapshots that lets you save the state of an entity after a certain times of loading it, so the eventstore does not need to apply too much events at once. The eventstore first loads the snapshot and then applies the remaining events on it. The snapshots only get created when the entity is being loaded, so if you never load the entity, the snapshot is not created on the given threshold. To setup an Entity for Snapshots, just add the Attribute over the class like this:

```
[SnapShotAfter(50)] // entity is being snapshotted after 50 events
public class User : Entity
{
    ...
}
```

# Distributed Event Handling

Microwave supports distributed systems and you can subscribe to events from any service with just implementing an interface. Microwave will get the events, put them in the handling classes and saves the location where you left of, in case a service is unavailable or an error occurs during the event handling on the client side.

## Reacting to Events happening in the Domain

If you want to react to DomainEvents in your Domain, to trigger other processes, you can do this by implementing the `IHandleAsync` interface in a class. You will have to implement a method that will receive the event as soon as it happens in the Domain. This all happens asynchronouse, so the emitting thread is not blocked by your handling. A handler could look like this:

```
public class WelcomeMailHandler : IHandleAsync<UserCreatedEvent>
{
    private readonly IMailRepository _mailRepository;

    public WelcomeMailHandler(IMailRepository mailRepository)
    {
        _mailRepository = mailRepository;
    }

    public Task HandleAsync(UserCreatedEvent domainEvent)
    {
        await _mailRepository.SendWelcomeMail(domainEvent.UserId);
    }
}
```

Using the `IHandleAsync` is usually useful when updating your write or read model when something happens in another entity that you do not want to reference directly.

## ReadModels and Querries

Microwave has ReadModels and Querries that can be used to create a querry service that is independent from the write side and therefore can be run in parallel. There are two main differences between Querries and ReadModels: ReadModels can be retrieved with and Identity and Querries are retrieved by their classname. For example you can have a ReadModel that represents a UserReadModel that can be retrieved by the UserId and a Querry could be the Top 10 Most Active Users in the Domain.

To add a Querry, inherit from `Querry` and implement the `IHandle` Interface to register the Querry to the specific DomainEvent. A Querry could look like this:

```
public class UserCounterQuerry : Querry, IHandle<UserCreatedEvent>
{
    public int Count { get; private set; }

    public void Handle(UserCreatedEvent domainEvent)
    {
        Count++;
    }
}
```

To add a ReadModel inherit from `ReadModel` and also implement `IHandle` accordingly. The only difference is that you have to define when Microwave should create this ReadModel by overriding the `GetsCreatedOn` method. After the creation Mircowave tracks the ReadModel and updates it when new events emerge. The Readmodel also has a Version that is being updated alongside with the write side, so you can call the write side with the eventual consistent version. The version is also updated, when the event is not porcessed by the ReadModel. A ReadModel could look like this:

```
public class UserReadModel : ReadModel, IHandle<UserCreatedEvent>, IHandle<UserChangedNameEvent>
{
    public void Handle(UserCreatedEvent domainEvent)
    {
        Id = domainEvent.EntityId;
    }

    public void Apply(UserChangedNameEvent domainEvent)
    {
        Name = domainEvent.Name;
    }

    public Identity Id { get; private set; }
    public string Name { get; private set; }
    public override Type GetsCreatedOn => typeof(UserCreatedEvent);
}
```

### Loading Querries and Readmodels
To load the Querries and ReadModels there are two Repositories `IQuerryRepository` and `IReadModelRepository` that offer functionality to load and update Querries/Readmodels. You can use them to update Querries or Readmodels inside a IHandleAsync by yourself, if you need to.

## Setting up the event source

The location for Events can be defined in the appsettings.json, so you can quickly move adjust the source for your events if you move entities aroung. For Microwave it does not matter where the events are stored, it always gets the from the webapi provided and applies them to the local handlers or ReadModels. The appsettings has to contain the following definitions:

```
{
  "DefaultDomainEventLocation": "http://localhost:5000/",  --> this is mandatory (will throw exception when not present)
  "DomainEventLocations": {
    "UserCreatedEvent" : "http://localhost:123/"           --> tells Mircowave to get the event from this location for handlers and querries
  },
  "DomainEventReadModelLocations": {
    "UserReadModel" : "http://localhost:123/"              --> tells Mircowave to get the all events for the readmodel from this location
  }
}
```

The Locations are optional, if they are not provided, Microwave will try to get the events from the `DefaultDomainEventLocation`

# Quality of Live Tools

## Exceptions and Filters

There a some Exceptions provided, that can return nice Problem Documents when an error occurs. Just throw them, when you see fit and get a propper response. Exceptions are:
* DomainValidationException
* NotFoundException
* ConcurrencyViolatedException

The DomainValidaitionException wants some errors that are being transferred through the wep api

## Result Objects

There are result objects, that can be used for repositories or methods on entities to return if something worked or not. The results all have static creation methods and should be self explanatory. Use them to not throw exceptions and have a cleaner programmflow. Resuls are:

* DomainValidationResult (cotains created DomainEvents or Errors)
* Ok
* NotFound
* ConcurrencyViolated

All Microwave classes return ResulObjects that can be used for detecting what worked and what not. There is also a `Is<T>` that tells you the state of the current result, if you need to know.

## Identity Handling in WebApi

The Identity classes are handled like basic types such as int or string in webApi. Deserializing an Identity will result in a plain string and not an object with an inner Id. Likewise, if you post it to web api, you do not need to construct a object, just send a plain string. Also, you can use it as a parameter in the url, like you would with a guid. This makes the api much more clean. Examples:

```
[HttpGet("{id}")]
public async Task<ActionResult> GetUser(StringIdentity id)         // call the get with .../api/MyUserId1
{
    var querry = await _queryRepository.Load<UserReadModel>(id);
    return Ok(querry.Value);
}
```

or

```
[HttpPost("create")]
public async Task<ActionResult> CreateUser([FromBody] CreateUserCommand command)
{
    var teamGuid = await _commandHandler.CreateUser(command);
    return Created();
}

public class CreateUserCommand
{
    public StringIdentity UserId { get; set; }
    public string Name { get; set; }
}

// the json object for the command is simply just this. Likewise it looks like this when returning an object containing an Identity
// {
//  "UserId": "MyUserId1",
//  "Name" : 7
// }
```

# Known Issues

## StringIdentity and GuidIdentity in Constructor of DomainEvents

StringIdentity and GuidIdentity can not be passed as concrete Parameter in a DomainEvent, as bson from mongodb does not support it (or I have not found a solution yet). As a workaround, you can use the Identity class itself or make a static create function, to call the constructor type safely.

This works:
```
public class UserCreatedEvent : IDomainEvent
{
    public UserCreatedEvent(Identity newGuid)
    {
        EntityId = newGuid;
    }

    public Identity EntityId { get; }
}
```

This does not:

```
public class UserCreatedEvent : IDomainEvent
{
    public UserCreatedEvent(GuidIdentity newGuid)
    {
        EntityId = newGuid;
    }

    public Identity EntityId { get; }
}
```
