#!/usr/bin/env bash
rm -f OnlineLeagueBackend/Eventstore.db
cd Adapters.Framework.EventStores/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context EventStoreContext
dotnet ef migrations add InitialMigration -s ../OnlineLeagueBackend/ --context EventStoreContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context EventStoreContext

cd ..