# ACC Telemetry Tracker

## User
| Name       | Type           |
| ---        | ---            |
| steamid    | base 64 string |
| lastsignin | date           |


## Car
| Name  | Type   |
| ---   | ---    |
| id    | int    |
| name  | string |
| class | string |


## Track
| Name | Type   |
| ---  | ---    |
| id   | int    |
| name | string |


## MotecFile
| Name         | Type           |
| ---          | ---            |
| id           | int            |
| userid       | base 64 string |
| carid        | int            |
| trackid      | int            |
| dateinserted | date           |
| numberoflaps | int            |
| fastestlap   | float          |
| filelocation | string         |