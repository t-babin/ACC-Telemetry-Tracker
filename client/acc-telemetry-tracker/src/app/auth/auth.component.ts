import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../_services/api.service';
import { AuthenticationService } from '../_services/authentication.service';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {

  loading = false;
  
  constructor(private apiService: ApiService, private router: Router, private authService: AuthenticationService) { }

  ngOnInit(): void {
    this.loading = true;
    if (location.search) {
      const code = location.search.substring(1).split('=')[1];
      this.apiService.authenticateWithCode(code)
        .subscribe({
          next: (v) => {
            this.loading = false;
            this.router.navigate(['/dashboard']);
          },
          error: (e) => {
            this.authService.logout();
            this.loading = false;
            this.router.navigate(['/unauthorized']);
          }
      });
    } else {
      this.router.navigate(['/login']);
    }
  }

}
