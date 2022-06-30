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
  lapStats: MotecLapStat[] = [];
  counts: MotecStat[] = [];
  overallBreakdownData: MultiSeries[] = [];
  averageLapData: MultiSeries[] = [];
  selectedTrackIndex = 0;

  colourScheme = {
    domain: ['#DA4D1A', '#EE8F6D', '#3F599F', '#6D85C5', '#10152C', '#314087', '#7CB4B8', '#E8DB7D']
  };

  yAxisTickFormatting = (value: any) => this.timePipe.transform(value);

  constructor(private apiService: ApiService, private timePipe: TimePipe) { }

  ngOnInit(): void {
  }

  getChartData(): void {
    this.loadingStats = true;
    this.loadingBreakdownChart = true;
    this.overallBreakdownData = [];
    forkJoin({
      counts: this.apiService.getMotecStats(),
      stats: this.apiService.getMotecTrackStats()
    }).subscribe({
      next: (res) => {
        this.counts = res.counts;
        this.lapStats = res.stats;
        this.counts.forEach(s => {
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
      }
    });
  }

  getTracks(): string[] {
    return this.counts.map(s => s.track).filter((value, index, self) => self.indexOf(value) === index);
  }

  changeMetricsVisibility(): void {
    this.metricsMinimized = !this.metricsMinimized;
  }

  tabChanged(event: MatTabChangeEvent): void {
    if (event.tab.textLabel === 'Track Breakdown' || this.getTracks().indexOf(event.tab.textLabel) >= 0) {
      this.averageLapData = [];
      this.loadingTrackChart = true;
      const trackId = this.counts.filter(s => s.track === this.getTracks()[this.selectedTrackIndex])[0].trackId;
      const trackStats = this.lapStats.filter(l => l.trackId === trackId);
      console.log(trackStats);
      const conditions = [...new Set(trackStats.map(t => t.trackCondition))];
      console.log(conditions);
      conditions.forEach(c => {
        this.averageLapData.push({ name: `${c} - Average Fastest Lap`, series: [] });
        this.averageLapData.push({ name: `${c} - Overall Fastest Lap`, series: [] });
      });
      this.lapStats.filter(l => l.trackId === trackId).forEach(l => {
        const avgBucket = this.averageLapData.filter(a => a.name === `${l.trackCondition} - Average Fastest Lap`)[0];
        const fastBucket = this.averageLapData.filter(a => a.name === `${l.trackCondition} - Overall Fastest Lap`)[0];

        avgBucket.series.push({ name: l.car, value: l.averageFastestLap });
        fastBucket.series.push({ name: l.car, value: l.fastestLap });
      });
      setTimeout(() => this.loadingTrackChart = false, 500);
    }
  }
}
