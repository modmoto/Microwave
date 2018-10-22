#!/usr/bin/env bash
cd Adapters.Framework.Subscriptions/

dotnet ef migrations add $1 -s ../OnlineLeagueBackend/ --context SubscriptionContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context SubscriptionContext

cd ..