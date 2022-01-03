import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Car } from '../_models/car';
import { MotecFile, MotecLap } from '../_models/motecFile';
import { Track } from '../_models/track';
import { User } from '../_models/user';
import { ApiService } from '../_services/api.service';
import { AuthenticationService } from '../_services/authentication.service';
import { saveAs } from 'file-saver';

import * as moment from 'moment';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class DashboardComponent implements OnInit {
  motecFiles: MotecFile[] = [];
  cars: Car[] = [];
  selectedCars = new FormControl();
  tracks: Track[] = [];
  selectedTracks = new FormControl();
  users: User[] = [];
  selectedUsers = new FormControl();
  selectedFile: MotecFile | null = null;
  laps: MotecLap[] = [];
  lapData: any[] = [];
  loadingFiles = false;
  loadingLaps = false;
  loadingLapChart = false;
  fileCount = 0;
  pageSize = 20;
  currentPage = 0;
  carTrackAverage = 0;
  classAverage = 0;
  classBest = 0;

  colorScheme = {
    domain: [ '#DA4D1A', '#3F599F', '#10152C' ]
  };

  constructor(public authService: AuthenticationService, private apiService: ApiService) { }
  
  ngOnInit(): void {
    if (!this.authService.isAuthorized) {
      this.authService.authorizedCallback();
    }
    
    this.reload();
  }

  reload(): void {
    this.loadingFiles = true;
    forkJoin({
      files: this.apiService.getMotecFiles(this.selectedCars.value, this.selectedTracks.value,
        this.selectedUsers.value, this.pageSize, this.currentPage),
      cars: this.apiService.getCars(),
      tracks: this.apiService.getTracks(),
      users: this.apiService.getUsers(),
      count: this.apiService.getFileCount()
    }).subscribe({
      next: (value) => {
        this.motecFiles = value.files;
        this.cars = value.cars;
        this.tracks = value.tracks;
        this.users = value.users;
        this.fileCount = value.count;
        this.loadingFiles = false;
      },
      error: (e) => console.log(e)
    });
  }

  showLaps(file: MotecFile): void {
    this.lapData = [];
    if (this.selectedFile === file) {
      this.selectedFile = null;
      this.laps = [];
      return;
    }
    this.loadingLaps = true;
    this.loadingLapChart = true;
    this.selectedFile = file;
    this.apiService.getLaps(file.id)
      .subscribe(res => {
        this.laps = res.laps;
        this.carTrackAverage = res.carTrackAverageLap;
        this.classAverage = res.classAverageLap;
        this.classBest = res.classBestLap;
        this.lapData.push({
          name: 'Lap Time', series: []
        });
        this.laps.forEach(l => this.lapData[0].series.push({ name: `Lap ${l.lapNumber + 1}`, value: l.lapTime, tooltipText: 'test tooltip' }));
        this.loadingLaps = false;
        // this.minLapTime = Math.min(...this.laps.map(l => l.lapTime)) - 5;
        // this.maxLapTime = Math.max(...this.laps.map(l => l.lapTime)) + 5;
        setTimeout(() => this.loadingLapChart = false, 500);
      });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];

    if (file) {
      const formData = new FormData();
      formData.append("file", file);

      this.apiService.uploadFile(formData)
        .subscribe({
          next: (v) => {
            console.log(v);
          },
          error: (e) => {
            // TODO add error handling
            console.log('error', e.error.message);
          },
          complete: () => {
            this.reload();
          }
        });
    }
  }

  downloadFile(file: MotecFile): void {
    this.apiService.downloadFile(file.id)
      .subscribe({
        next: (n) => {
          console.log(`${file.carName.replace(' ', '-')}_${file.trackName.replace(' ', '-')}_${moment(file.sessionDate).format('yyyy-MM-DD')}.zip`);
          const blob = new Blob([n], { type: 'application/x-zip-compressed' });
          saveAs(blob, `${file.carName.replaceAll(' ', '-')}_${file.trackName.replace(' ', '-')}_${moment(file.sessionDate).format('yyyy-MM-DD')}.zip`);
        },
      })
  }

  onFilterChange(event: any): void {
    this.reload();
  }

  fastest(lap: MotecLap): boolean {
    return Math.min(...this.laps.map(l => l.lapTime)) === lap.lapTime;
  }

  fastestLap(): number {
    return Math.min(...this.laps.map(l => l.lapTime));
  }

  page(value: { currentPage: number, pageSize: number }): void {
    this.pageSize = value.pageSize;
    this.currentPage = value.currentPage;
    this.reload();
  }
}
