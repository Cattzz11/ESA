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
  errors: string[] = [];
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
    this.errors = [];
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
    this.errors = [];
    const name = this.registerForm.get('name')?.value;
    const userName = this.registerForm.get('email')?.value;
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = this.registerForm.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      this.registerFailed = true;
      this.errors.push('Passwords do not match.');
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
          if (error.error) {
            const errorObj = JSON.parse(error.error);
            if (errorObj && errorObj.errors) {
              //problem details { "field1": [ "error1", "error2" ], "field2": [ "error1", "error2" ]}
              const errorList = errorObj.errors;
              for (let field in errorList) {
                if (Object.hasOwn(errorList, field)) {
                  let list: string[] = errorList[field];
                  for (let idx = 0; idx < list.length; idx += 1) {
                    this.errors.push(`${field}: ${list[idx]}`);
                  }
                }
              }
            }
          }
        });
  }

  public confirmationEmail() {
    const email = this.registerForm.get('email')?.value;
    this.confirmationMessage = "Se o e-mail existir, irá receber um código de recuperação de password.";

    this.authService.confirmAccount(email).subscribe({
      next: (response) => {
        setTimeout(() => this.router.navigate(['/confirmation-account/', email]), 2000); // espera 2 segundos
      },
      error: (error) => {
        setTimeout(() => this.router.navigate(['/confirmation-account/', email]), 2000); // espera 2 segundos
      }
    });
  }
  


}
