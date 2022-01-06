import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { forkJoin } from 'rxjs';
import { TimePipe } from '../time.pipe';
import { MotecLapStat, MotecStat } from '../_models/motecFile';
import { ApiService } from '../_services/api.service';

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
  averageLaps: MotecLapStat[] = [];
  stats: MotecStat[] = [];
  overallBreakdownData: MultiSeries[] = [];
  averageLapData: MultiSeries[] = [];
  selectedTrackIndex = 0;

  colourScheme = {
    domain: ['#DA4D1A', '#3F599F', '#10152C']
  };

  yAxisTickFormatting = (value: any) => this.timePipe.transform(value);
  
  constructor(private apiService: ApiService, private timePipe: TimePipe) { }

  ngOnInit(): void {
  }

  getChartData(): void {
    this.loadingStats = true;
    this.loadingBreakdownChart = true;
    this.overallBreakdownData = [];
    this.apiService.getMotecStats()
      .subscribe(res => {
        this.stats = res;
        this.stats.forEach(s => {
          if (this.overallBreakdownData.filter(o => o.name === s.track).length > 0) {
            const trackData = this.overallBreakdownData.filter(o => o.name === s.track)[0];
            if (trackData.series.filter(v => v.name === s.car).length > 0) {
              const carData = trackData.series.filter(v => v.name === s.car)[0];
              carData.value += 1;
            } else {
              const series: SeriesData = { name: s.car, value: 1 };
              trackData.series.push(series);
            }
          } else {
            const series: SeriesData[] = [];
            series.push({ name: s.car, value: 1 });
            this.overallBreakdownData.push({ name: s.track, series: series });
          }
        });
        this.loadingStats = false;
        setTimeout(() => this.loadingBreakdownChart = false, 500);
      });
  }

  getTracks(): string[] {
    return this.stats.map(s => s.track).filter((value, index, self) => self.indexOf(value) === index);
  }

  changeMetricsVisibility(): void {
    this.metricsMinimized = !this.metricsMinimized;
  }

  tabChanged(event: MatTabChangeEvent): void {
    if (event.tab.textLabel === 'Track Breakdown' || this.getTracks().indexOf(event.tab.textLabel) >= 0) {
      this.averageLapData = [];
      this.loadingTrackChart = true;
      const trackId = this.stats.filter(s => s.track === this.getTracks()[this.selectedTrackIndex])[0].trackId;
      forkJoin({
        average: this.apiService.getMotecAverageTrackStats(trackId),
        fastest: this.apiService.getMotecFastestTrackStats(trackId),
      })
        .subscribe({
          next: (res) => {
          this.averageLaps = res.average;
          this.averageLapData.push({
            name: 'Average Fastest Lap Time', series: []
          });
          this.averageLapData.push({
            name: 'Overall Fastest Lap Time', series: []
          });
          res.average.forEach(r => this.averageLapData[0].series.push({ name: r.car, value: r.fastestLap }));
          res.fastest.forEach(r => this.averageLapData[1].series.push({ name: r.name, value: r.min }));
          setTimeout(() => this.loadingTrackChart = false, 500);
        }});
    }
  }

  fastestLap(): number {
    return Math.min(...this.averageLaps.map(l => l.fastestLap));
  }
}
