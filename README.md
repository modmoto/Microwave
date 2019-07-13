# Microwave
Microwave is a Framework to create event driven microservices. It helps you implement Eventsourcing and CQRS and lets 
you implement your domain and not have you worried about distributing your domain events or storing them. The 
generated service network is able to transfer the domain events by itself and is not dependant on a central unit.

[![Build status](https://dev.azure.com/simonheiss87/Microwave/_apis/build/status/Microwave-DEV)](https://dev.azure.com/simonheiss87/Microwave/_build/latest?definitionId=5)
[![sonarCubeCoverage](https://sonarcloud.io/api/project_badges/measure?project=Lauchi_Microwave&metric=coverage)](https://sonarcloud.io/dashboard?id=Lauchi_Microwave)
[![nuget](https://img.shields.io/nuget/v/Microwave.svg)](https://www.nuget.org/packages/Microwave/)

## General Architecture
Keeping Microservices independant from each other is key if you want to build a system that aims to not become the 
"Distributed Monolith". Microwave is a framework that supports the idea of "Self Contained Systems" by delivering a 
infrastructure that helps you transfer changes in your domain through your system without any headache. The key 
element is a event driven architecture that evolves around domain events. If something of value happens in your 
domain, you persist a domain event and microwave distributes the event to all services that are subscribed to this 
event asynchronously. Therefore Microwave uses the `IEventstore` that persists the domain events of a service. To 
subscribe to events, you just implement a `IHandleAsync<T>` Interface where `T` is the type of a domain event and 
microwave transfers the events to this handler as soon as they happen in your domain. With this mechanism Microwave 
also implements readmodels if you want to implement CQRS over domain events.

![alt text](https://github.com/Lauchi/Microwave/blob/master/MicrowaveOverview.svg "Overview")

Take a look at the wiki for further details!

