import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MessagingService {

  messages: { message: string, type: 'success' | 'error', progress: number }[] = [];

  pushMessage(message: { message: string, type: 'success' | 'error' }): void {
    const alert = { ...message, progress: 100 };
    this.messages.push(alert);

    setTimeout(() => {
      this.messages.shift();
    }, 4000);
  }
}
