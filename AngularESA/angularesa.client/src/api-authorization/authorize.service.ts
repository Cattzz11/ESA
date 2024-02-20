import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject, catchError, map, of, throwError } from 'rxjs';
import { UserInfo } from './authorize.dto';
import { jwtDecode } from 'jwt-decode';
import { Router } from '@angular/router';
declare const google: any;


@Injectable({
  providedIn: 'root'
})
export class AuthorizeService {
  constructor(private http: HttpClient, private router: Router) { }

  private _authStateChanged: Subject<boolean> = new BehaviorSubject<boolean>(false);

  public onStateChanged() {
    return this._authStateChanged.asObservable();
  }


  // cookie-based login
  public signIn(email: string, password: string) {
    return this.http.post('/login?useCookies=true', {
      email: email,
      password: password
    }, {
      observe: 'response',
      responseType: 'text'
    })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        this._authStateChanged.next(res.ok);
        return res.ok;
      }));
  }


  // register new user
  public registerCustom(name: string, email: string, password: string) {
    return this.http.post('api/register', {
      name: name,
      email: email,
      password: password
    }, {
      observe: 'response',
      responseType: 'text'
    })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        return res.ok;
      }));
  }

  public changePassword(newPassword: string, email: string) {
    return this.http.post('api/change-password', {
      email: email,
      password: newPassword
    }, {
      observe: 'response',
      responseType: 'text'
    })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        return res.ok;
      }));
  }

  public codeValidation(code: string, email: string) {
    return this.http.post<any>('api/validate-recovery-code', {
      userEmail: email,
      code: code
    }).pipe(
      map(res => {
        return true;
      }),
      catchError(error => {
        return of(false);
      })
    );
  }

  // register new user - não funciona para um novo tipo de IdentityUser com propriedades extra
  //public register(email: string, password: string) {
  //  return this.http.post('/register', {
  //    email: email,
  //    password: password
  //  }, {
  //    observe: 'response',
  //    responseType: 'text'
  //  })
  //    .pipe<boolean>(map((res: HttpResponse<string>) => {
  //      return res.ok;
  //    }));
  //}

  // sign out - não aparece como um serviço
  public signOut() {
    console.log("gg");
    return this.http.post('/api/logout', {}, {
      withCredentials: true,
      observe: 'response',
      responseType: 'text'
    }).pipe<boolean>(map((res: HttpResponse<string>) => {
      if (res.ok) {
        this._authStateChanged.next(false);
      }
      return res.ok;
    }));
  }

  initializeGoogleOnTap() {
    (window as any).onGoogleLibraryLoad = () => {
      console.log('Google\'s One-tap sign in script loaded!');

      //@ts-ignore
      google.accounts.id.initialize({
        // Ref: https://developers.google.com/identity/gsi/web/reference/js-reference#IdConfiguration
        client_id: '712855861147-lt433p2k7stok4g5h6hvba6qmt7iktld.apps.googleusercontent.com',
        callback: this.googleResponse.bind(this),
        auto_select: true,
        cancel_on_tap_outside: false
      });

      //@ts-ignore
      google.accounts!.id.renderButton(document!.getElementById('login-googleBTN')!, { theme: 'outline', size: 'large', width: 200 })
      //@ts-ignore
      google.accounts.id.prompt();
    };

  }

  async googleResponse(response: any) {
    if (response && response.credential) {
      //this.isLoggedIn = true;
      this._authStateChanged.next(true);
      console.log("Google login successfull");
      const decoded = jwtDecode(response.credential);
      console.log('Decoded JWT:', decoded);
      this.commonAuthenticationProcedure(decoded);
      sessionStorage.setItem('user', response);
    }

    console.log('RESPONSE :>> ', response);
    console.log("deu3");
  }

  commonAuthenticationProcedure(userDetails: any) {
    // Aqui, você configura o usuário como logado, armazena o token JWT se necessário, etc.
    console.log('User details:', userDetails);

    this._authStateChanged.next(true); // Por exemplo, atualizando o estado de autenticação
    // Redirecionar para a página inicial ou dashboard
    this.router.navigateByUrl("/");
  }

  // logout
  public signOutGoogle() {
    google.accounts.id.cancel();
    sessionStorage.removeItem('user');
    this._authStateChanged.next(false);  // Update your local signed-in state
    console.log('User signed out successfully');
  }

  // check if the user is authenticated. the endpoint is protected so 401 if not.
  public user() {
    return this.http.get<UserInfo>('/manage/info', {
      withCredentials: true
    }).pipe(
      catchError((_: HttpErrorResponse, __: Observable<UserInfo>) => {
        return of({} as UserInfo);
      }));
  }

  // is signed in when the call completes without error and the user has an email
  public isSignedIn(): Observable<boolean> {
    return this.user().pipe(
      map((userInfo) => {
        const valid = !!(userInfo && userInfo.email && userInfo.email.length > 0);
        return valid;
      }),
      catchError((_) => {
        return of(false);
      }));
  }

  public recoverPassword(email: string) {
    return this.http.post('api/send-recovery-code', {
      email: email
    }, {
      observe: 'response',
      responseType: 'text'
    })
      .pipe<boolean>(map((res: HttpResponse<string>) => {
        return res.ok;
      }));
  }

  public getUserInfo(): Observable<any> {
    return this.http.get<any>('api/userInfo');
  }
}
