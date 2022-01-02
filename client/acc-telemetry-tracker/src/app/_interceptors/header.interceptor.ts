import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";

@Injectable()
export class HeaderInterceptor implements HttpInterceptor {

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        request = request.clone({
            withCredentials: true
        });
        if (request.method === "POST" && request.url.endsWith("/api/motec")) {
            return next.handle(request);
        } else {
            request = request.clone({
                setHeaders: {
                    'Content-Type':  'application/json',
                    // 'Access-Control-Allow-Origin': `${environment.apiUrl}`,
                    // 'Access-Control-Allow-Headers': 'Origin, X-Requested-With, Content-Type, Accept'
                },
            });
            return next.handle(request);
        }

    }
}