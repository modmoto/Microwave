#!/usr/bin/env bash
cd Adapters.Framework.EventStores/

dotnet ef migrations add $1 -s ../OnlineLeagueBackend/ --context EventStoreContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context EventStoreContext

cd ..