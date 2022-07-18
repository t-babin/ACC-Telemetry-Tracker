import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { forkJoin } from 'rxjs';
import { TimePipe } from '../time.pipe';
import { MotecLapStat, MotecStat } from '../_models/motecFile';
import { ApiService } from '../_services/api.service';
import { AuthenticationService } from '../_services/authentication.service';

export interface MultiSeries {
  name: string;
  series: SeriesData[];
}

export interface SeriesData {
  name: string;
  value: number;
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css'],
  animations: [
    trigger('shown', [
      state('true', style({ height: '0px', minHeight: '0' })),
      state('false', style({ height: '*' })),
      transition('true <=> false', animate('400ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class ReportsComponent implements OnInit {

  metricsMinimized = true;
  loadingStats = false;
  loadingBreakdownChart = true;
  loadingTrackChart = true;
  lapStats: MotecLapStat[] = [];
  userLapStats: MotecLapStat[] = [];
  counts: MotecStat[] = [];
  trackCarMetricsStacked: MultiSeries[] = [];
  carTrackMetricsStacked: MultiSeries[] = [];
  userTrackMetricsStacked: MultiSeries[] = [];
  userCarMetricsStacked: MultiSeries[] = [];
  trackMetrics: SeriesData[] = [];
  carMetrics: SeriesData[] = [];
  userMetrics: SeriesData[] = [];
  averageLapData: MultiSeries[] = [];
  userLapData: MultiSeries[] = [];
  selectedTrackIndex = 0;
  selectedTrack: string = '';
  selectedUser: string[] = [];
  includeCars = false;
  includeTracks = false;
  includeUserCars = false;
  includeUserTracks = false;

  colourScheme = {
    domain: ['#DA4D1A', '#E76536', '#8A5043', '#802D0F', '#495984', '#EE8F6D', '#947276', '#A7B6DC', '#3F599F', '#293966', '#6D85C5', '#10152C', '#314087', '#7CB4B8', '#E8DB7D', '#BFAC22']
  };

  yAxisTickFormatting = (value: any) => this.timePipe.transform(value);

  constructor(private apiService: ApiService, private timePipe: TimePipe, private authService: AuthenticationService) { }

  ngOnInit(): void {
  }

  getChartData(): void {
    this.loadingStats = true;
    this.loadingBreakdownChart = true;
    this.trackCarMetricsStacked = [];
    this.carTrackMetricsStacked = [];
    this.userTrackMetricsStacked = [];
    this.userCarMetricsStacked = [];
    this.trackMetrics = [];
    this.carMetrics = [];
    this.userMetrics = [];
    this.includeCars = false;
    this.includeTracks = false;
    this.includeUserCars = false;
    this.includeUserTracks = false;
    forkJoin({
      counts: this.apiService.getMotecStats(),
      lapStats: this.apiService.getMotecLapStats(),
      userStats: this.apiService.getMotecUserStats(),
      userLapStats: this.apiService.getUserLapStats()
    }).subscribe({
      next: (res) => {
        this.counts = res.counts;
        this.lapStats = res.lapStats;
        this.userLapStats = res.userLapStats;
        this.selectedTrack = res.counts[0].track;
        this.counts.forEach(s => {
          // load the car metrics
          if (this.carMetrics.filter(c => c.name === s.car).length > 0) {
            const carData = this.carMetrics.filter(c => c.name === s.car)[0];
            carData.value += s.count;
          } else {
            this.carMetrics.push({ name: s.car, value: s.count });
          }

          // load the track metrics
          if (this.trackMetrics.filter(c => c.name === s.track).length > 0) {
            const trackData = this.trackMetrics.filter(c => c.name === s.track)[0];
            trackData.value += s.count;
          } else {
            this.trackMetrics.push({ name: s.track, value: s.count });
          }

          // load the track + car metrics
          if (this.trackCarMetricsStacked.filter(o => o.name === s.track).length > 0) {
            const trackData = this.trackCarMetricsStacked.filter(o => o.name === s.track)[0];
            const series: SeriesData = { name: s.car, value: s.count };
            trackData.series.push(series);
          } else {
            const series: SeriesData[] = [];
            series.push({ name: s.car, value: s.count });
            this.trackCarMetricsStacked.push({ name: s.track, series: series });
          }

          // load the car + track metrics
          if (this.carTrackMetricsStacked.filter(o => o.name === s.car).length > 0) {
            const carData = this.carTrackMetricsStacked.filter(o => o.name === s.car)[0];
            const series: SeriesData = { name: s.track, value: s.count };
            carData.series.push(series);
          } else {
            const series: SeriesData[] = [];
            series.push({ name: s.track, value: s.count });
            this.carTrackMetricsStacked.push({ name: s.car, series: series });
          }
        });

        // set up the user statistics charts
        res.userStats.forEach(u => {
          // load the single user metrics
          if (this.userMetrics.filter(c => c.name === u.user).length > 0) {
            const userData = this.userMetrics.filter(c => c.name === u.user)[0];
            userData.value += u.count;
          } else {
            this.userMetrics.push({ name: u.user, value: u.count });
          }

          // load the user + track metrics
          if (this.userTrackMetricsStacked.filter(o => o.name === u.user).length > 0) {
            const userData = this.userTrackMetricsStacked.filter(o => o.name === u.user)[0];
            if (userData.series.filter(s => s.name === u.track).length > 0) {
              const trackData = userData.series.filter(s => s.name === u.track)[0];
              trackData.value += u.count;
            } else {
              const series: SeriesData = { name: u.track, value: u.count };
              userData.series.push(series);
            }
          } else {
            const series: SeriesData[] = [];
            series.push({ name: u.track, value: u.count });
            this.userTrackMetricsStacked.push({ name: u.user, series: series });
          }

          // load the user + car metrics
          if (this.userCarMetricsStacked.filter(o => o.name === u.user).length > 0) {
            const userData = this.userCarMetricsStacked.filter(o => o.name === u.user)[0];
            if (userData.series.filter(s => s.name === u.car).length > 0) {
              const carData = userData.series.filter(s => s.name === u.car)[0];
              carData.value += u.count;
            } else {
              const series: SeriesData = { name: u.car, value: u.count };
              userData.series.push(series);
            }
          } else {
            const series: SeriesData[] = [];
            series.push({ name: u.car, value: u.count });
            this.userCarMetricsStacked.push({ name: u.user, series: series });
          }
        });

        this.loadingStats = false;
        this.loadingBreakdownChart = false;
      }
    });
  }

  getTracks(order?: boolean): string[] {
    const tracks = this.counts.map(s => s.track).filter((value, index, self) => self.indexOf(value) === index);
    return order ? tracks.sort() : tracks;
  }

  getUsers(): string[] {
    return this.userLapStats.map(s => s.user).filter((value, index, self) => self.indexOf(value) === index);
  }

  changeMetricsVisibility(): void {
    this.metricsMinimized = !this.metricsMinimized;
    if (!this.metricsMinimized) {
      setTimeout(() => {
        this.loadingBreakdownChart = false;
        document.getElementById('scroll')!.scrollIntoView({ behavior: 'smooth' });
      }, 500);
    }
  }

  tabChanged(event: MatTabChangeEvent): void {
    this.selectedTrack = this.getTracks(true)[0];
    this.selectedUser = [this.authService.userValue!.username]
    if (event.tab.textLabel === 'Laptime Statistics' || event.tab.textLabel === 'Overall') {
      this.selectedTrackChanged();
    } else if (event.tab.textLabel === 'Driver Fastest Laps') {
      this.selectedUserChanged();
    }
  }

  userChartSelectionChanged(type: 'cars' | 'tracks', checked: boolean): void {
    if (type === 'cars') {
      if (checked && this.includeUserTracks) {
        this.includeUserTracks = false;
      }
      this.includeUserCars = checked;
    } else {
      if (checked && this.includeUserCars) {
        this.includeUserCars = false;
      }
      this.includeUserTracks = checked;
    }
  }

  selectedUserChanged(): void {
    if (this.selectedUser.length === 0) {
      this.selectedUser = [this.authService.userValue!.username];
    }
    this.userLapData = [];
    this.loadingTrackChart = true;
    this.userLapStats.filter(l => this.selectedUser.indexOf(l.user) >= 0).forEach(l => {
      if (this.selectedUser.length > 1) {
        const thisItem = this.userLapData.filter(f => f.name === `${l.user} - ${l.car} - ${l.trackCondition}`)[0];
        if (thisItem) {
          thisItem.series.push({ name: l.track, value: l.laptime });
        } else {
          this.userLapData.push({ name: `${l.user} - ${l.car} - ${l.trackCondition}`, series: [{ name: l.track, value: l.laptime }] });
        }
      } else {
        const thisItem = this.userLapData.filter(f => f.name === `${l.car} - ${l.trackCondition}`)[0];
        if (thisItem) {
          thisItem.series.push({ name: l.track, value: l.laptime });
        } else {
          this.userLapData.push({ name: `${l.car} - ${l.trackCondition}`, series: [{ name: l.track, value: l.laptime }] });
        }
      }
    });
    setTimeout(() => {
      this.loadingTrackChart = false;
      document.getElementById('scroll')!.scrollIntoView({ behavior: 'smooth' });
    }, 500);
  }

  selectedTrackChanged(): void {
    this.averageLapData = [];
    this.loadingTrackChart = true;
    const conditions = [...new Set(this.lapStats.filter(l => l.track === this.selectedTrack).map(t => t.trackCondition))];
    conditions.forEach(c => {
      this.averageLapData.push({ name: `${c} - Average Fastest Lap`, series: [] });
      this.averageLapData.push({ name: `${c} - Overall Fastest Lap`, series: [] });
    });
    this.lapStats.filter(l => l.track === this.selectedTrack).forEach(l => {
      const avgBucket = this.averageLapData.filter(a => a.name === `${l.trackCondition} - Average Fastest Lap`)[0];
      const fastBucket = this.averageLapData.filter(a => a.name === `${l.trackCondition} - Overall Fastest Lap`)[0];

      avgBucket.series.push({ name: l.car, value: l.averageFastestLap });
      fastBucket.series.push({ name: l.car, value: l.fastestLap });
    });
    setTimeout(() => {
      this.loadingTrackChart = false;
      document.getElementById('scroll')!.scrollIntoView({ behavior: 'smooth' });
    }, 500);
  }
}
