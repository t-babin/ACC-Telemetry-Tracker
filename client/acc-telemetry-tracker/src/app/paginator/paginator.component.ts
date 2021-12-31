import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.css']
})
export class PaginatorComponent implements OnInit {

  @Input() currentPage = 0;

  @Input() dataLength = 0;

  @Input() pageSize = 20;

  @Input() pageSizes = [20, 50, 100];

  @Output() pageEvent: EventEmitter<{currentPage: number, pageSize: number}> = new EventEmitter();
  
  constructor() { }

  ngOnInit(): void {
  }

  firstDataPage(): void {
    this.currentPage = 0;
    this.emit();
  }

  lastDataPage(): void {
    this.currentPage = this.dataLength - this.pageSize;
    this.emit();
  }

  previousDataPage(): void {
    if (this.currentPage - this.pageSize < 0) {
      this.currentPage -= this.currentPage;
    } else {
      this.currentPage -= this.pageSize;
    }
    this.emit();
  }

  nextDataPage(): void {
    this.currentPage += this.pageSize;
    this.emit();
  }
  emit(): void {
    this.pageEvent.emit({ currentPage: this.currentPage, pageSize: this.pageSize });
  }

  pageSizeChange(): void {
    this.firstDataPage();
  }
}
