import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject, Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { User } from "../_models/user";

@Injectable({ providedIn: 'root' })
export class AuthenticationService {

    DISCORD_CLIENT_ID = environment.discordClient;
    API_URL = environment.apiUrl;
    isAuthorized = false;
    private userSubject: BehaviorSubject<User | null>;
    public user: Observable<User | null>;

    constructor(private router: Router) {
        const match = document.cookie.match(new RegExp('(^| )user=([^;]+)'));

        this.userSubject = new BehaviorSubject<User | null>(match ? new User(JSON.parse(decodeURIComponent(match![2]))) : null);
        this.user = this.userSubject.asObservable();
    }

    public get userValue(): User | null {
        return this.userSubject.value;
    }

    authorize(): void {
        const url =
            'https://discord.com/api/oauth2/authorize?'
            + 'response_type=code'
            + `&client_id=${this.DISCORD_CLIENT_ID}`
            + `&redirect_uri=${this.API_URL}/auth/callback`
            + '&scope=identify guilds guilds.members.read';

        window.location.href = url;
    }

    authorizedCallback(): void {
        const match = document.cookie.match(new RegExp('(^| )user=([^;]+)'));
        if (!match) {
            this.isAuthorized = false;
            this.router.navigate(['/unauthorized']);
        } else {
            const user = JSON.parse(decodeURIComponent(match![2]));
            this.userSubject.next(new User(user));
            this.isAuthorized = true;
        }
    }

    logout(): void {
        document.cookie = 'user=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        this.userSubject.next(null);
        this.isAuthorized = false;
        this.router.navigate(['/login']);
    }
}