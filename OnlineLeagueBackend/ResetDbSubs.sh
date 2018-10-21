#!/usr/bin/env bash
rm -f OnlineLeagueBackend/SubscriptionContext.db
cd Adapters.Framework.Subscriptions/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context SubscriptionContext
dotnet ef migrations add InitialMigration -s ../OnlineLeagueBackend/ --context SubscriptionContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context SubscriptionContext

cd ..