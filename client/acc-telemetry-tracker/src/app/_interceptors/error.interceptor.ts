import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, throwError } from "rxjs";
import { catchError } from "rxjs/operators";
import { MessagingService } from "../messaging.service";
import { AuthenticationService } from "../_services/authentication.service";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

    constructor(private authService: AuthenticationService, private messagingService: MessagingService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            catchError((requestError) => {
              if (requestError.status !== 401) {
                const { error } = requestError;
                console.log('error from interceptor', error);
                this.messagingService.pushMessage({ message: error.message, type: 'error'});
              } else {
                this.authService.logout(true);
              }
              return throwError(() => new Error(requestError));
            })
          );
    }
}