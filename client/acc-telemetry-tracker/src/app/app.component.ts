import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { User } from './_models/user';
import { AuthenticationService } from './_services/authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'acc-telemetry-tracker';
  user!: User | null;

  constructor(public authService: AuthenticationService, public router: Router) {
    this.authService.user.subscribe(u => this.user = u);
    // this.router.
  }

  getUserImage(): string {
    return `https://cdn.discordapp.com/avatars/${this.user?.id}/${this.user?.avatar}.png`;
  }

  logout(): void {
    this.authService.logout(true);
  }
}
