export interface Track {
    id: number | null;
    name: string;
    motecName: string;
    minLapTime: number;
    maxLapTime: number;
    editing: boolean;
}