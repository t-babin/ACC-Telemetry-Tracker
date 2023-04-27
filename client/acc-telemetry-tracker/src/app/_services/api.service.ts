import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { AuditLog } from "../_models/audit";
import { Car } from "../_models/car";
import { MotecFile, MotecLaps, MotecLapStat, MotecStat, UserMetric } from "../_models/motecFile";
import { Track } from "../_models/track";
import { User } from "../_models/user";
import { GameVersion } from "../_models/version";

@Injectable({ providedIn: 'root' })
export class ApiService {
    API_BASE_URL: string;

    constructor(private httpClient: HttpClient) {
        this.API_BASE_URL = environment.apiUrl;
    }

    getMotecFiles(carIds?: number[], trackIds?: number[], userIds?: number[], take?: number, skip?: number, sortOn?: string, includeId?: number | null): Observable<MotecFile[]> {
        let params = new HttpParams();
        if (carIds) {
            carIds.forEach(c => params = params.append('carIds', c));
        }
        if (trackIds) {
            trackIds.forEach(c => params = params.append('trackIds', c));
        }
        if (userIds) {
            userIds.forEach(c => params = params.append('userIds', c));
        }
        if (take) {
            params = params.append('take', take);
        }
        if (skip) {
            params = params.append('skip', skip);
        }
        if (sortOn && sortOn !== '') {
            params = params.append('sortOn', sortOn);
        }
        if (includeId) {
            params = params.append('includeId', includeId);
        }
        return this.httpClient.get<MotecFile[]>(`${this.API_BASE_URL}/api/motec`, { params: params });
    }

    getSingleFile(id: number): Observable<MotecFile> {
        return this.httpClient.get<MotecFile>(`${this.API_BASE_URL}/api/motec/${id}`);
    }

    getLaps(id: number): Observable<MotecLaps> {
        return this.httpClient.get<MotecLaps>(`${this.API_BASE_URL}/api/motec/laps/${id}`);
    }

    uploadFile(formData: FormData): Observable<MotecFile> {
        return this.httpClient.post<MotecFile>(`${this.API_BASE_URL}/api/motec`, formData);
    }

    downloadFile(id: number) {
        return this.httpClient.get(`${this.API_BASE_URL}/api/motec/download/${id}`, {
            reportProgress: true,
            responseType: 'blob'
        });
    }

    deleteFile(id: number): Observable<any> {
        return this.httpClient.delete(`${this.API_BASE_URL}/api/motec/delete/${id}`);
    }

    updateFileComment(updatedFile: MotecFile): Observable<any> {
        const body = {
            id: updatedFile.id,
            comment: updatedFile.comment
        };
        return this.httpClient.put<any>(`${this.API_BASE_URL}/api/motec/${updatedFile.id}/comment`, body);
    }

    updateFileConditions(updatedFile: MotecFile): Observable<any> {
        const body = {
            id: updatedFile.id,
            trackConditions: updatedFile.trackConditions
        };
        return this.httpClient.put<any>(`${this.API_BASE_URL}/api/motec/${updatedFile.id}/conditions`, body);
    }

    updateCarAverageTime(): Observable<any> {
        return this.httpClient.post<any>(`${this.API_BASE_URL}/api/motec/average`, null);
    }

    getCars(): Observable<Car[]> {
        return this.httpClient.get<Car[]>(`${this.API_BASE_URL}/api/motec/cars`);
    }

    getTracks(): Observable<Track[]> {
        return this.httpClient.get<Track[]>(`${this.API_BASE_URL}/api/motec/tracks`);
    }

    getMotecStats(): Observable<MotecStat[]> {
        return this.httpClient.get<MotecStat[]>(`${this.API_BASE_URL}/api/motec/stats`);
    }

    getMotecUserStats(): Observable<MotecStat[]> {
        return this.httpClient.get<MotecStat[]>(`${this.API_BASE_URL}/api/motec/stats/users`);
    }

    getMotecLapStats(): Observable<MotecLapStat[]> {
        return this.httpClient.get<MotecLapStat[]>(`${this.API_BASE_URL}/api/motec/stats/laps`);
    }

    getUserLapStats(): Observable<MotecLapStat[]> {
        return this.httpClient.get<MotecLapStat[]>(`${this.API_BASE_URL}/api/motec/stats/users/laptimes`);
    }

    getUserMetrics(): Observable<UserMetric[]> {
        return this.httpClient.get<UserMetric[]>(`${this.API_BASE_URL}/api/motec/stats/user/metrics`);
    }

    getUsers(): Observable<User[]> {
        return this.httpClient.get<User[]>(`${this.API_BASE_URL}/api/user`);
    }

    updateUsers(users: User[]): Observable<any> {
        return this.httpClient.put<any>(`${this.API_BASE_URL}/api/user`, { users: users });
    }

    getFileCount(carIds?: number[], trackIds?: number[], userIds?: number[]): Observable<number> {
        let params = new HttpParams();
        if (carIds) {
            carIds.forEach(c => params = params.append('carIds', c));
        }
        if (trackIds) {
            trackIds.forEach(c => params = params.append('trackIds', c));
        }
        if (userIds) {
            userIds.forEach(c => params = params.append('userIds', c));
        }
        return this.httpClient.get<number>(`${this.API_BASE_URL}/api/motec/count`, { params: params });
    }

    getAuditLog(take: number, skip: number): Observable<AuditLog> {
        let params = new HttpParams();
        params = params.append('take', take);
        params = params.append('skip', skip);
        return this.httpClient.get<AuditLog>(`${this.API_BASE_URL}/api/audit`, { params: params });
    }

    authenticateWithCode(code: string): Observable<any> {
        let params = new HttpParams();
        params = params.append('code', code);
        return this.httpClient.get<any>(`${this.API_BASE_URL}/api/auth/callback`, { params: params });
    }

    notify(id: number): Observable<any> {
        return this.httpClient.get<any>(`${this.API_BASE_URL}/api/motec/${id}/notify`);
    }

    getGameVersions(): Observable<GameVersion[]> {
        return this.httpClient.get<GameVersion[]>(`${this.API_BASE_URL}/api/admin/gameversions`);
    }

    updateGameVersion(data: GameVersion): Observable<any> {
        if (data.id === null) {
            data.id = 0;
        }
        return this.httpClient.post<any>(`${this.API_BASE_URL}/api/admin/gameversion`, data);
    }

    getAdminTracks(): Observable<Track[]> {
        return this.httpClient.get<Track[]>(`${this.API_BASE_URL}/api/admin/tracks`);
    }

    updateAdminTrack(data: Track): Observable<any> {
        if (data.id === null) {
            data.id = 0;
        }
        return this.httpClient.post<any>(`${this.API_BASE_URL}/api/admin/track`, data);
    }

    getAdminCars(): Observable<Car[]> {
        return this.httpClient.get<Car[]>(`${this.API_BASE_URL}/api/admin/cars`);
    }

    updateAdminCar(data: Car): Observable<any> {
        if (data.id === null) {
            data.id = 0;
        }
        return this.httpClient.post<any>(`${this.API_BASE_URL}/api/admin/car`, data);
    }

    deleteGameVersion(id: number): Observable<any> {
        return this.httpClient.delete(`${this.API_BASE_URL}/api/admin/gameversion/${id}`);
    }

    deleteTrack(id: number): Observable<any> {
        return this.httpClient.delete(`${this.API_BASE_URL}/api/admin/track/${id}`);
    }

    deleteCar(id: number): Observable<any> {
        return this.httpClient.delete(`${this.API_BASE_URL}/api/admin/car/${id}`);
    }
}