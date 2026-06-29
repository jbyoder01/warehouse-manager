## Commands

`open -a Docker` - Run after a reboot to start Docker Desktop
`docker compose up -d db` - Create the DB container
`docker compose start db` - Start the existing container
`docker compose down` - Removes the stopped container, keeps the data volume
`docker compose ps` - Show running containers

## Notes
- DB will migrate automatically on app startup in development(configured in `Program.cs`).
