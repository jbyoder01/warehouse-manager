## Commands

`open -a Docker` - Run after a reboot to start Docker Desktop
`docker compose up -d db` - Create the DB container
`docker compose start db` - Start the existing container
`docker compose down` - Removes the stopped container, keeps the data volume
`docker compose ps` - Show running containers

`dotnet run --project src/Api/Api.csproj` - Run the project

## Notes
- DB will migrate automatically on app startup in development(configured in `Program.cs`).

## TODO
- Ensure Locations and Items exist when writing to `Transactions` table - should be handling in API and DB.
