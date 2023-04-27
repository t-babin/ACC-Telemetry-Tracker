import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { MessagingService } from '../messaging.service';
import { Audit } from '../_models/audit';
import { User } from '../_models/user';
import { GameVersion } from '../_models/version';
import { ApiService } from '../_services/api.service';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { Track } from '../_models/track';
import { Car } from '../_models/car';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {

  loading = false;
  users: User[] = [];
  audit: Audit[] = [];
  gameVersions: GameVersion[] = [];
  filteredGameVersions: GameVersion[] = [];
  tracks: Track[] = [];
  cars: Car[] = [];
  filteredUsers: User[] = [];
  original: any[] = [];
  auditMinimized = true;
  usersMinimized = true;
  versionMinimized = false;
  auditCount = 0;
  auditPageSize = 20;
  auditCurrentPage = 0;
  
  versionPageSize = 10;
  versionCurrentPage = 0;
  versionToEdit!: GameVersion | null;
  trackToEdit!: Track | null;
  carToEdit!: Car | null;
  
  constructor(private apiService: ApiService, private messagingService: MessagingService) { }

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading = true;
    this.original = [];
    forkJoin({
      users: this.apiService.getUsers(),
      audit: this.apiService.getAuditLog(this.auditPageSize, this.auditCurrentPage),
      versions: this.apiService.getGameVersions(),
      tracks: this.apiService.getAdminTracks(),
      cars: this.apiService.getAdminCars()
    })
      .subscribe(res => {
        this.users = res.users;
        this.filteredUsers = res.users;
        res.users.forEach(r => this.original.push({ id: r.id, username: r.username, serverName: r.serverName, isValid: r.isValid, role: r.role,
          signupDate: r.signupDate, fileUploadCount: r.fileUploadCount }));
        
        this.audit = res.audit.auditEvents;
        this.auditCount = res.audit.auditCount;
        this.gameVersions = res.versions;
        this.tracks = res.tracks;
        this.cars = res.cars;
        this.filteredGameVersions = this.gameVersions.slice(0, this.versionPageSize);
        this.versionToEdit = null;
        this.loading = false;
      });
  }

  versionPage(value: { currentPage: number, pageSize: number }): void {
    this.versionPageSize = value.pageSize;
    this.versionCurrentPage = value.currentPage;
    console.log(this.versionPageSize);
    console.log(this.versionCurrentPage);
    this.filteredGameVersions = this.gameVersions.slice(this.versionCurrentPage, this.versionCurrentPage + this.versionPageSize);
  }
  
  page(value: { currentPage: number, pageSize: number }): void {
    this.auditPageSize = value.pageSize;
    this.auditCurrentPage = value.currentPage;
    this.apiService.getAuditLog(this.auditPageSize, this.auditCurrentPage)
      .subscribe(res => {
        this.audit = res.auditEvents;
        this.auditCount = res.auditCount;
      });
  }

  filter(event: any): void {
    this.filteredUsers = this.users.filter(u => u.serverName.toLowerCase().indexOf(event.target.value.toLowerCase()) >= 0);
  }

  submitChanges(): void {
    this.apiService.updateUsers(this.users)
      .subscribe(_ => {
        this.reload();
        this.messagingService.pushMessage({ message: 'Successfully updated users', type: 'success'});
      });
  }

  hasUpdates(): boolean {
    return JSON.stringify(this.users) !== JSON.stringify(this.original);
  }

  edit(row: GameVersion): void {
    this.versionToEdit = { editing: true, startDate: row.startDate, endDate: row.endDate, versionNumber: row.versionNumber, id: row.id };
    row.editing = true;
  }

  cancel(row: GameVersion): void {
    if (row.id === null) {
      this.gameVersions = this.gameVersions.slice(1, this.gameVersions.length);
      this.filteredGameVersions = this.gameVersions.slice(this.versionCurrentPage, this.versionCurrentPage + this.versionPageSize);
    }
    this.versionToEdit = null;
    row.editing = false;
  }

  saveVersion(row: GameVersion): void {
    if (this.versionToEdit !== null) {
      this.apiService.updateGameVersion(this.versionToEdit)
        .subscribe(res => {
          this.versionToEdit = null;
          row.editing = false;
          this.reload();
        });
    }
  }

  editTrack(row: Track): void {
    this.trackToEdit = { editing: true, name: row.name, motecName: row.motecName, minLapTime: row.minLapTime, maxLapTime: row.maxLapTime, id: row.id };
    row.editing = true;
  }

  editCar(row: Car): void {
    this.carToEdit = { editing: true, name: row.name, motecName: row.motecName, class: row.class, id: row.id };
    row.editing = true;
  }

  cancelTrack(row: Track): void {
    if (row.id === null) {
      this.tracks = this.tracks.slice(1, this.tracks.length);
    }
    this.trackToEdit = null;
    row.editing = false;
  }

  cancelCar(row: Car): void {
    if (row.id === null) {
      this.cars = this.cars.slice(1, this.cars.length);
    }
    this.carToEdit = null;
    row.editing = false;
  }

  saveTrack(row: Track): void {
    if (this.trackToEdit !== null) {
      this.apiService.updateAdminTrack(this.trackToEdit)
        .subscribe(res => {
          this.trackToEdit = null;
          row.editing = false;
          this.reload();
        });
    }
  }

  saveCar(row: Car): void {
    if (this.carToEdit !== null) {
      this.apiService.updateAdminCar(this.carToEdit)
        .subscribe(res => {
          this.carToEdit = null;
          row.editing = false;
          this.reload();
        });
    }
  }

  addVersion(): void {
    this.gameVersions = [{ startDate: new Date(), endDate: null, versionNumber: '', editing: true, id: null }, ...this.gameVersions]
    this.versionToEdit = { editing: true, startDate: this.gameVersions[0].startDate, endDate: this.gameVersions[0].endDate, versionNumber: this.gameVersions[0].versionNumber,
      id: this.gameVersions[0].id };
    this.filteredGameVersions = this.gameVersions.slice(this.versionCurrentPage, this.versionCurrentPage + this.versionPageSize);
  }

  addTrack(): void {
    this.tracks = [{ name: '', motecName: '', minLapTime: 0, maxLapTime: 0, editing: true, id: null }, ...this.tracks]
    this.trackToEdit = { editing: true, name: this.tracks[0].name, motecName: this.tracks[0].motecName, minLapTime: this.tracks[0].minLapTime,
      maxLapTime: this.tracks[0].maxLapTime, id: this.tracks[0].id };
  }

  addCar(): void {
    this.cars = [{ name: '', motecName: '', class: '', editing: true, id: null }, ...this.cars]
    this.carToEdit = { editing: true, name: this.cars[0].name, motecName: this.cars[0].motecName, class: this.cars[0].class, id: this.cars[0].id };
  }

  delete(row: GameVersion): void {
    if (row.id === null) {
      return;
    }
    this.apiService.deleteGameVersion(row.id)
      .subscribe(_ => this.reload());
  }

  deleteTrack(row: Track): void {
    if (row.id === null) {
      return;
    }
    this.apiService.deleteTrack(row.id)
      .subscribe(_ => this.reload());
  }

  deleteCar(row: Car): void {
    if (row.id === null) {
      return;
    }
    this.apiService.deleteCar(row.id)
      .subscribe(_ => this.reload());
  }
}
