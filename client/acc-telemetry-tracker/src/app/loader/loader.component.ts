import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-loader',
  template: `
    <div style="text-align: center;" *ngIf="visible">
      <p style="margin-bottom: 2%; font-weight: bold;">Loading {{ component }}</p>
      <mat-progress-bar style="margin: 0 auto;" mode="indeterminate"></mat-progress-bar>
    </div>
  `
})
export class LoaderComponent implements OnInit {

  @Input() component: string = '';

  @Input() visible: boolean = true;

  constructor() { }

  ngOnInit(): void {
  }

}
