export interface GameVersion {
    id: number | null;
    startDate: Date;
    endDate: Date | null;
    versionNumber: string;
    editing: boolean;
}
