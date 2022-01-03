export interface MotecFile {
    id: number;
    carName: string;
    carClass: string;
    trackName: string;
    username: string;
    dateInserted: Date;
    sessionDate: Date;
    numberOfLaps: number;
    fastestLap: number;
    laps: MotecLap[];
}

export interface MotecLap {
    lapNumber: number;
    lapTime: number;
    sessionTime: number;
}

export interface MotecLaps {
    carTrackAverageLap: number;
    classAverageLap: number;
    classBestLap: number;
    laps: MotecLap[];
}