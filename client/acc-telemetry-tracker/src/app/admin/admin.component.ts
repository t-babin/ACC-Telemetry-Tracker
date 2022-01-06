import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { MessagingService } from '../messaging.service';
import { Audit } from '../_models/audit';
import { User } from '../_models/user';
import { ApiService } from '../_services/api.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css'],
  animations: [
    trigger('shown', [
      state('true', style({ height: '0px', minHeight: '0' })),
      state('false', style({ height: '*' })),
      transition('true <=> false', animate('400ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class AdminComponent implements OnInit {

  loading = false;
  users: User[] = [];
  audit: Audit[] = [];
  filteredUsers: User[] = [];
  original: any[] = [];
  auditMinimized = true;

  constructor(private apiService: ApiService, private messagingService: MessagingService) { }

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading = true;
    this.original = [];
    forkJoin({
      users: this.apiService.getUsers(),
      audit: this.apiService.getAuditLog()
    })
      .subscribe(res => {
        this.users = res.users;
        this.filteredUsers = res.users;
        res.users.forEach(r => this.original.push({ id: r.id, username: r.username, serverName: r.serverName, isValid: r.isValid, role: r.role,
          signupDate: r.signupDate, fileUploadCount: r.fileUploadCount }));
        
        this.audit = res.audit;
        this.loading = false;
      });
  }

  filter(event: any): void {
    this.filteredUsers = this.users.filter(u => u.serverName.toLowerCase().indexOf(event.target.value.toLowerCase()) >= 0);
  }

  submitChanges(): void {
    this.apiService.updateUsers(this.users)
      .subscribe(_ => {
        this.reload();
        this.messagingService.pushMessage({ message: 'Successfully updated users', type: 'success'});
      });
  }

  hasUpdates(): boolean {
    return JSON.stringify(this.users) !== JSON.stringify(this.original);
  }
}
