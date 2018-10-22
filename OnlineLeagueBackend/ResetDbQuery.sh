#!/usr/bin/env bash
rm -f OnlineLeagueBackend/QueryContext.db
cd Adapters.Framework.EventStores/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context QueryContext
dotnet ef migrations add InitialMigrationQuery -s ../OnlineLeagueBackend/ --context QueryContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context QueryContext

cd ..