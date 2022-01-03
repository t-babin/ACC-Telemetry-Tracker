import { Injectable } from '@angular/core';
import { map, takeWhile, timer } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MessagingService {

  messages: { message: string, type: string, progress: number }[] = [];

  pushMessage(message: { message: string, type: string }): void {
    const alert = { ...message, progress: 100 };
    this.messages.push(alert);

    // timer(0, 100)
    // .pipe(
    //   takeWhile(_ => alert.progress > 0),
    //   map(() => {
    //     alert.progress -= 2;
    //     return alert.progress;
    //   })
    // ).subscribe();

    setTimeout(() => {
      this.messages.shift();
    }, 4000);
  }
}
