#!/usr/bin/env bash
rm -f OnlineLeagueBackend/EventStoreReadContext.db
rm -f OnlineLeagueBackend/EventStoreWriteContext.db

cd Adapters.Framework.EventStores/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context EventStoreReadContext
dotnet ef migrations add InitialMigration -s ../OnlineLeagueBackend/ --context EventStoreReadContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context EventStoreReadContext

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context EventStoreWriteContext
dotnet ef migrations add InitialMigration -s ../OnlineLeagueBackend/ --context EventStoreWriteContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context EventStoreWriteContext

cd ..