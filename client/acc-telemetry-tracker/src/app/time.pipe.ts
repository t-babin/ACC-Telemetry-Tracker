import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'time'
})
export class TimePipe implements PipeTransform {

  // 113.849
  transform(value: number, ...args: unknown[]): string {
    // 1
    const minutes = Math.floor(value / 60);
    // 113 - 60
    const seconds = Math.floor(value - (minutes * 60));

    // 0.849
    const milliseconds = ((value - (minutes * 60) - seconds) * 1000).toFixed(0);

    // 1:53.849
    return `${minutes}:${seconds.toString().padStart(2, '0')}.${milliseconds.toString().padEnd(3, '0')}`;
  }

}
