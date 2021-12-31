import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../_services/authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(private authService: AuthenticationService) {

  }

  ngOnInit(): void {
  }
  
  login(): void {
    console.log('clicked');
    this.authService.authorize();
  }
}
