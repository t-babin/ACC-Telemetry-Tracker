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

- Discord notifications on file upload

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

**Server**

| Variable              | Description                                                                                                                             | Required? |
|-----------------------|-----------------------------------------------------------------------------------------------------------------------------------------|-----------|
| FRONTEND_URL          | Used for authentication callback - the full URL of the frontend                                                                         | Yes       |
| DATABASE_HOST         | Hostname of the database server                                                                                                         | No        |
| DATABASE_PORT         | Port that the database server is running on                                                                                             | No        |
| DATABASE_NAME         | Name of the database being connected to                                                                                                 | No        |
| DATABASE_USER         | Username to connect to the database with                                                                                                | No        |
| DATABASE_PASSWORD     | Password to connect to the database with                                                                                                | No        |
| SQLITE_DATABASE       | If at least one of the above DATABASE_* variables are missing, the application falls back to using SQLite with this connection string   | No        |
| ADMIN_USERS           | Comma-separated list of discord IDs that will be the admin users of the application                                                     | Yes       |
| STORAGE_PATH          | The absolute path of a directory where the Motec files will be saved. Defaults to the directory `files` in the backend source directory | No        |
| DISCORD_GUILD_ID      | A discord guild (server) ID that users will be required to be members of, otherwise they won't be authenticated                         | No        |
| DISCORD_CLIENT_ID     | The Discord OAuth application client ID                                                                                                 | Yes       |
| DISCORD_CLIENT_SECRET | The Discord OAuth application client secret                                                                                             | Yes       |
| DISCORD_WEBHOOK_URL   | A Discord webhook URL. If provided, the webhook will be triggered whenever a user uploads a new file                                    | No        |

**Frontend**

| Variable          | Description                                                                               | Required? |
|-------------------|-------------------------------------------------------------------------------------------|-----------|
| API_URL           | The URL of the backend API. If using docker, define it but keep it blank (see the sample) | No        |
| DISCORD_CLIENT_ID | The Discord OAuth application client ID                                                   | Yes       |

**Database (look [here](https://hub.docker.com/_/mysql) for reference)**
```
MYSQL_ROOT_PASSWORD

MYSQL_DATABASE

MYSQL_USER

MYSQL_PASSWORD
```

## Contributing

PRs are welcome.