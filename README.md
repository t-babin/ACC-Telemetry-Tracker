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

## Running Locally (not Docker)

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [node.js (LTS version)](https://nodejs.org/en/download/)
- You will need a [Discord Application](https://discord.com/developers/applications) with an OAuth client for the authentication to work.

### Running the Backend Application

Either update the `server/AccTelemetryTracker.Api/appsettings.json` file with your environment variables, or add them as startup arguments when debugging/running from the cli. See below for the list of mandatory and optional environment variables. Note that the default frontend URL value is `http://localhost:4200`.

Open a terminal and run:
```bash
cd server/AccTelemetryTracker.Api/
dotnet build
dotnet run
```

If running successfully, you should see logs similar to this either in `server/AccTelemetryTracker.Api/logs/log-yyyymmdd.txt` or in the console:
```
[10:07:16 INF] Starting up application
[10:07:16 INF] Read variable FRONTEND_URL: [http://localhost:4200]
[10:07:16 INF] Read variable DATABASE_HOST: []
[10:07:16 INF] Read variable DATABASE_PORT: []
[10:07:16 INF] Read variable DATABASE_NAME: []
[10:07:16 INF] Read variable DATABASE_USER: []
[10:07:16 INF] Read variable DATABASE_PASSWORD: []
[10:07:16 INF] Read variable SQLITE_DATABASE: [C:\Code\ACC-Telemetry-Tracker\acc.db]
[10:07:16 INF] Read variable ADMIN_USERS: [***]
[10:07:16 INF] Connecting to a local sqlite database
[10:07:18 INF] Found [1] admin users. Ensuring their roles are 'admin' and they are activated...
[10:07:18 INF] Updating admin [***]
[10:07:18 INF] Now listening on: https://localhost:7112
[10:07:18 INF] Now listening on: http://localhost:5195
[10:07:18 INF] Application started. Press Ctrl+C to shut down.
[10:07:18 INF] Hosting environment: Development
[10:07:18 INF] Content root path: C:\Code\ACC-Telemetry-Tracker\server\AccTelemetryTracker.Api\
```

### Running the Frontend Application
Update the file `client/acc-telemetry-tracker/src/assets/env.js` with your environment variables. The default value for the API URL is `https://localhost:7112`.

Open another terminal and run:
```bash
cd client/acc-telemetry-tracker
npm install --save
ng serve
```

If running successfully, you should see logs similar to this in the console:
```
✔ Browser application bundle generation complete.

Initial Chunk Files   | Names         |  Raw Size
vendor.js             | vendor        |   6.31 MB | 
polyfills.js          | polyfills     | 339.32 kB | 
styles.css, styles.js | styles        | 299.73 kB | 
main.js               | main          | 272.02 kB | 
runtime.js            | runtime       |   6.88 kB | 

                      | Initial Total |   7.21 MB

Build at: 2022-03-28T19:09:05.826Z - Hash: 36ce93b9a03e0ca7 - Time: 9831ms

** Angular Live Development Server is listening on localhost:4200, open your browser on http://localhost:4200/ **


√ Compiled successfully.
```

Navigate to http://localhost:4200/login in your browser and you should see the login screen.

## Running With Docker (preferred)
A sample docker-compose file is provided. Update with your environment variables and then run the following commands. Note that this has only been tested on a linux environment, YMMV on Windows.

```bash
docker-compose build
docker-compose up -d
```

## Environment Variables

### Server

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

### Frontend

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