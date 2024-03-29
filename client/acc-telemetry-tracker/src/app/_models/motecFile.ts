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
    editingConditions: boolean;
    trackConditions: string;
    gameVersion: string;
    changedTrackConditions: string;
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
    user: string;
    userId: string;
    count: number;
}

export interface MotecLapStat extends MotecStat {
    fastestLap: number;
    averageFastestLap: number;
    trackCondition: string;
    laptime: number;
}

export interface UserMetric extends MotecStat {
    favouriteCar: string;
    favouriteTrack: string;
    numberOfFastestLaps: number;
    numberOfLaps: number;
    numberOfUploads: number;
}