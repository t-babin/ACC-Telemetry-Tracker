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
    comment: string;
    changedComment: string;
    editingComment: boolean;
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

export interface MotecStat {
    car: string;
    carId: number;
    track: string;
    trackId: number;
}

export interface MotecLapStat extends MotecStat {
    fastestLap: number;
}