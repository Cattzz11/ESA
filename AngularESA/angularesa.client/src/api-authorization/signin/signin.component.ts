import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";
import { jwtDecode } from "jwt-decode";
import { User } from "oidc-client";

@Component({
  selector: 'app-signin-component',
  templateUrl: './signin.component.html',
  styleUrl:'./signin.component.css'
})
export class SignInComponent implements OnInit {
  loginForm!: FormGroup;
  authFailed: boolean = false;
  signedIn: boolean = false;
  isLoggedIn: boolean = false;

  constructor(private authService: AuthorizeService,
    private formBuilder: FormBuilder,
    private router: Router) {
    this.authService.isSignedIn().forEach(
      isSignedIn => {
        console.log("deu4");
        this.signedIn = isSignedIn;
        this.isLoggedIn = isSignedIn;
      });
  }

  ngOnInit(): void {
    this.authFailed = false;
    this.loginForm = this.formBuilder.group(
      {
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required]]
      });
    console.log("deu5");
    this.initializeGoogleOnTap();

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
      this.isLoggedIn = true;
      this.signedIn = true;
      console.log("Google login successfull");
      const decoded = jwtDecode(response.credential);
      console.log('Decoded JWT:', decoded);
      this.commonAuthenticationProcedure(decoded);
      sessionStorage.setItem('user', response);
    }

    console.log('RESPONSE :>> ', response);
    console.log("deu3");
  }


  public signIn(_: any) {
    if (!this.loginForm.valid || !this.isLoggedIn) {
      return;
    }
    const userName = this.loginForm.get('email')?.value;
    const password = this.loginForm.get('password')?.value;
    this.authService.signIn(userName, password).forEach(
      response => {
        if (response) {
          this.commonAuthenticationProcedure(response);
        }
      }).catch(
        _ => {
          this.authFailed = true;
        });
  }

  commonAuthenticationProcedure(userDetails: any) {
    // Aqui, você configura o usuário como logado, armazena o token JWT se necessário, etc.
    console.log('User details:', userDetails);
    
    this.signedIn = true; // Por exemplo, atualizando o estado de autenticação
    // Redirecionar para a página inicial ou dashboard
    this.router.navigateByUrl("/");
  }
}
