# ACC Telemetry Tracker

.NET 6 backend

Angular 13 frontend

MySQL Database

## Purpose

Sharing ACC Telemetry in larger discords can get a bit unwieldy. Hopefully this application makes it easier.

Users are authenticated via Discord, and optionally need to belong to a specific server.

## Features

- Stores Motec files on disk and parses car/track/lap times into a database.

- Users can filter for and download the files

- Charts that show how the lap times in each file compare to the other files from the same track

- See how your lap times compare to other cars on the same track

- A user cannot upload a file until they have been activated by one of the admin users

A single file can be uploaded at a time. Each file needs to be a zip archive (not exceeding 150MB) containing exactly two files: the .ld and .ldx Motec files.

Screenshots can be found [here](screenshots/)

## Running

### If running locally:
Frontend
```bash
cd client/acc-telemetry-tracker
npm install --save
ng serve
```
Backend
Either update appsettings.json with your environment variables, or add them as startup arguments when debugging/running from the cli
```bash
cd server
dotnet run
```

A sample docker-compose file is provided.

You will need a [Discord Application](https://discord.com/developers/applications) with an OAuth client for the authentication to work.

### Environment Variables

Server
```
FRONTEND_URL: (required) - used for the authentication callback - the full URL of the frontend

DATABASE_HOST: (optional) - hostname of the database server

DATABASE_NAME: (optional) - name of the database connecting to

DATABASE_USER: (optional) - username to connect to the database

DATABASE_PASSWORD: (optional) - password to connect to the database

SQLITE_DATABASE: (optional) - if at least one of the above DATABASE_* variables are missing, the application falls back to using SQLite

ADMIN_USERS: (required) - Comma-separated list of discord IDs that will be the admin users of the application

DISCORD_GUILD_ID: (optional) - A Discord Guild (server) ID that users will be required to be members of, otherwise they won't be authenticated

STORAGE_PATH: (optional) - The absolute path of a directory where the Motec files will be saved. Defaults to the directory 'files' in the backend source directory

DISCORD_CLIENT_ID: (required) - The Discord OAuth application client ID

DISCORD_CLIENT_SECRET: (required) - The Discord OAuth application client secret
```

Frontend
```
API_URL: (optional) - The URL of the backend API. If using docker, define it but keep it blank (see the sample)

DISCORD_CLIENT_ID: (required) - The Discord OAuth application client ID
```

Database (look [here](https://hub.docker.com/_/mysql) for reference)
```
MYSQL_ROOT_PASSWORD

MYSQL_DATABASE

MYSQL_USER

MYSQL_PASSWORD
```

## Contributing

PRs are welcome.