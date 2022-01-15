import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
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
import { MessagingService } from '../messaging.service';
import { TimePipe } from '../time.pipe';
import { ReportsComponent } from '../reports/reports.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class DashboardComponent implements OnInit, AfterViewInit {
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
  downloadId = 0;
  processingUpload = false;
  referenceLines: any[] = [];

  @ViewChild(ReportsComponent)
  reports!: ReportsComponent;

  colorScheme = {
    domain: ['#DA4D1A', '#3F599F', '#10152C']
  };

  yAxisTickFormatting = (value: any) => this.timePipe.transform(value);

  constructor(public authService: AuthenticationService, private apiService: ApiService, private messagingService: MessagingService,
    private timePipe: TimePipe) { }

  ngOnInit(): void {
    if (!this.authService.isAuthorized) {
      this.authService.authorizedCallback();
    }
  }

  ngAfterViewInit(): void {
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
        this.reports.getChartData();
        this.loadingFiles = false;
      },
      error: (e) => console.log(e)
    });
  }

  showLaps(file: MotecFile): void {
    this.lapData = [];
    this.referenceLines = [];
    if (this.selectedFile === file) {
      this.selectedFile = null;
      this.laps = [];
      return;
    }
    this.loadingLaps = true;
    this.loadingLapChart = true;
    this.selectedFile = file;
    this.apiService.getLaps(file.id)
      .subscribe({
        next: (res) => {
          this.laps = res.laps;
          this.carTrackAverage = res.carTrackAverageLap;
          this.classAverage = res.classAverageLap;
          this.classBest = res.classBestLap;
          this.lapData.push({
            name: 'Lap Time', series: []
          });
          this.laps.forEach(l => this.lapData[0].series.push({ name: `Lap ${l.lapNumber + 1}`, value: l.lapTime }));
          this.referenceLines.push({ name: `Car/Track Average Fastest Lap: ${this.timePipe.transform(this.carTrackAverage)}`, value: this.carTrackAverage });
          if (this.classAverage !== this.carTrackAverage) {
            this.referenceLines.push({ name: `Class Average Fastest Lap: ${this.timePipe.transform(this.classAverage)}`, value: this.classAverage });
          }
          this.loadingLaps = false;
          // this.minLapTime = Math.min(...this.laps.map(l => l.lapTime)) - 5;
          // this.maxLapTime = Math.max(...this.laps.map(l => l.lapTime)) + 5;
          setTimeout(() => this.loadingLapChart = false, 500);
        },
        error: (e) => {
          this.loadingLaps = false;
          this.selectedFile = null;
          this.laps = [];
        }
      });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];

    if (file) {
      const formData = new FormData();
      formData.append("file", file);
      this.processingUpload = true;

      this.apiService.uploadFile(formData)
        .subscribe({
          next: (v) => {
            this.messagingService.pushMessage({ message: `Successfully uploaded ${v.car} / ${v.track} MoTeC data`, type: 'success' });
            this.processingUpload = false;
            this.reload();
          },
          error: (e) => {
            this.processingUpload = false;
          }
        });
    }
  }

  downloadFile(file: MotecFile): void {
    this.downloadId = file.id;
    this.apiService.downloadFile(file.id)
      .subscribe({
        next: (n) => {
          const blob = new Blob([n], { type: 'application/x-zip-compressed' });
          saveAs(blob, `${file.carName.replaceAll(' ', '-')}_${file.trackName.replace(' ', '-')}_${moment(file.sessionDate).format('yyyy-MM-DD')}.zip`);
          this.downloadId = 0;
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

  slowestLap(): number {
    let values = this.laps.map(l => l.lapTime);
    values.push(...[this.classAverage, this.carTrackAverage]);
    return Math.max(...values);
  }

  page(value: { currentPage: number, pageSize: number }): void {
    this.pageSize = value.pageSize;
    this.currentPage = value.currentPage;
    this.reload();
  }
}
