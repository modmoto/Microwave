if (Test-Path OnlineLeagueBackendt/QueryStorageContext.db) {
	Remove-Item -Force OnlineLeagueBackendt/QueryStorageContext.db
}

cd Adapters.Framework.Queries/

dotnet ef migrations remove -s ../OnlineLeagueBackend/ --context QueryStorageContext
dotnet ef migrations add InitialMigrationQuery -s ../OnlineLeagueBackend/ --context QueryStorageContext
dotnet ef database update -s ../OnlineLeagueBackend/ --context QueryStorageContext

cd ..