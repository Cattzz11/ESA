import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-register-component',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  errors: string | undefined;
  registerForm!: FormGroup;
  confirmationMessage: string = '';
  registerFailed: boolean = false;
  registerSucceeded: boolean = false;
  signedIn: boolean = false;

  constructor(private authService: AuthorizeService, private router : Router,
    private formBuilder: FormBuilder) {
    this.authService.isSignedIn().forEach(
      isSignedIn => {
        this.signedIn = isSignedIn;
      });
  }

  ngOnInit(): void {
    this.registerFailed = false;
    this.registerSucceeded = false;
    this.errors = "";
    this.registerForm = this.formBuilder.group(
      {
        name: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required]],
        confirmPassword: ['', [Validators.required]]
      });
  }

  public register(_: any) {
    if (!this.registerForm.valid) {
      return;
    }

    this.registerFailed = false;
    this.errors = "";
    const name = this.registerForm.get('name')?.value;
    const userName = this.registerForm.get('email')?.value;
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = this.registerForm.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      this.registerFailed = true;
      this.errors = 'Passwords do not match';
      return;
    }

    this.authService.registerCustom(name, userName, password).forEach(
      response => {
        if (response) {
          this.registerSucceeded = true;
          this.confirmationEmail();
        }
      }).catch(
        error => {
          this.registerFailed = true;
          const match = /"description":"([^"]+)"/.exec(error.error);
          if (match) {
            this.errors = match[1];

          } else {
            this.errors = "Ocorreu um erro desconhecido.";
          }
        });
  }

  public confirmationEmail() {
    const email = this.registerForm.get('email')?.value;
    this.confirmationMessage = "Se o e-mail existir, irá receber um código de recuperação de password.";

    this.authService.confirmAccount(email).subscribe({
      next: (response) => {
        setTimeout(() => this.router.navigate(['/confirmation-account/', email]), 1000);
      },
      error: (error) => {
        this.errors = "Não foi possivel concluir o pedido.";
      }
    });
  }
}
