import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider'; 
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthenticationService } from './_services/authentication.service';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PageNotFoundComponent } from './pagenotfound/pagenotfound.component';
import { TimePipe } from './time.pipe';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HeaderInterceptor } from './_interceptors/header.interceptor';
import { LoaderComponent } from './loader/loader.component';
import { AdminComponent } from './admin/admin.component';
import { ErrorInterceptor } from './_interceptors/error.interceptor';
import { MessagingComponent } from './messaging/messaging.component';
import { PaginatorComponent } from './paginator/paginator.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { AuthComponent } from './auth/auth.component';
import { ReportsComponent } from './reports/reports.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    DashboardComponent,
    PageNotFoundComponent,
    TimePipe,
    LoaderComponent,
    AdminComponent,
    MessagingComponent,
    PaginatorComponent,
    UnauthorizedComponent,
    AuthComponent,
    ReportsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FlexLayoutModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatIconModule,
    MatTableModule,
    MatProgressBarModule,
    MatCheckboxModule,
    MatButtonModule,
    MatInputModule,
    MatTabsModule,
    MatDividerModule,
    MatTooltipModule,
    FormsModule,
    ReactiveFormsModule,
    NgxChartsModule
  ],
  providers: [
    AuthenticationService,
    { provide: HTTP_INTERCEPTORS, useClass: HeaderInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    TimePipe
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
