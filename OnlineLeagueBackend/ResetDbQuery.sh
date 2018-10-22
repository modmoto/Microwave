#!/usr/bin/env bash
rm -f OnlineLeagueBackend/Eventstore.db
cd Adapters.Framework.EventStores/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context QueryContext
dotnet ef migrations add InitialMigration -s ../OnlineLeagueBackend/ --context QueryContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context QueryContext

cd ..