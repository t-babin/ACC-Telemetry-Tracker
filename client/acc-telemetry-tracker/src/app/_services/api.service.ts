import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { Car } from "../_models/car";
import { MotecFile } from "../_models/motecFile";
import { Track } from "../_models/track";
import { User } from "../_models/user";

@Injectable({ providedIn: 'root' })
export class ApiService {
    API_BASE_URL: string;

    constructor(private httpClient: HttpClient) {
        this.API_BASE_URL = environment.apiUrl;
    }

    getMotecFiles(carIds?: number[], trackIds?: number[], userIds?: number[], take?: number, skip?: number): Observable<MotecFile[]> {
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
        return this.httpClient.get<MotecFile[]>(`${this.API_BASE_URL}/api/motec`, { params: params });
    }

    getLaps(id: number): Observable<MotecFile> {
        return this.httpClient.get<MotecFile>(`${this.API_BASE_URL}/api/motec/${id}`);
    }

    getCars(): Observable<Car[]> {
        return this.httpClient.get<Car[]>(`${this.API_BASE_URL}/api/motec/cars`);
    }

    getTracks(): Observable<Track[]> {
        return this.httpClient.get<Track[]>(`${this.API_BASE_URL}/api/motec/tracks`);
    }

    getUsers(): Observable<User[]> {
        return this.httpClient.get<User[]>(`${this.API_BASE_URL}/api/user`);
    }

    uploadFile(formData: FormData): Observable<any> {
        return this.httpClient.post<any>(`${this.API_BASE_URL}/api/motec`, formData);
    }

    downloadFile(id: number) {
        return this.httpClient.get(`${this.API_BASE_URL}/api/motec/download/${id}`, {
            reportProgress: true,
            responseType: 'blob'
        });
    }

    updateUsers(users: User[]): Observable<any> {
        return this.httpClient.put<any>(`${this.API_BASE_URL}/api/user`, { users: users });
    }

    getFileCount(): Observable<number> {
        return this.httpClient.get<number>(`${this.API_BASE_URL}/api/motec/count`);
    }

    authenticateWithCode(code: string): Observable<any> {
        let params = new HttpParams();
        params = params.append('code', code);
        return this.httpClient.get<any>(`${this.API_BASE_URL}/auth/callback`, { params: params });
    }
}