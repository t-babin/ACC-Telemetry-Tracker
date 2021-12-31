import { Component, OnInit } from '@angular/core';
import { User } from '../_models/user';
import { ApiService } from '../_services/api.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {

  loading = false;
  users: User[] = [];
  original: any[] = [];
  filterValue: string = '';

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    console.log('reloading');
    this.loading = true;
    this.original = [];
    this.apiService.getUsers()
      .subscribe(res => {
        this.users = res;
        res.forEach(r => this.original.push({ id: r.id, username: r.username, serverName: r.serverName, isValid: r.isValid, role: r.role,
          signupDate: r.signupDate, fileUploadCount: r.fileUploadCount }));
        this.loading = false;
      });
  }

  submitChanges(): void {
    this.apiService.updateUsers(this.users)
      .subscribe(_ => this.reload());
  }

  hasUpdates(): boolean {
    return JSON.stringify(this.users) !== JSON.stringify(this.original);
  }
}
