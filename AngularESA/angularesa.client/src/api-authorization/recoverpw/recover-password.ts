import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-recover-password',
  templateUrl: './recover-password.component.html'
})

export class PasswordRecoveryComponent implements OnInit {
  recoveryForm!: FormGroup;
  recoveryFailed: boolean = false;
  recoverySuccess: boolean = false;
  errorMessage: string = '';

  constructor(
    private authService: AuthorizeService,
    private formBuilder: FormBuilder
  ) { }

  ngOnInit(): void {
    this.recoveryFailed = false;
    this.recoverySuccess = false;
    this.errorMessage = '';
    this.recoveryForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  recoverPassword(): void {
    if (!this.recoveryForm.valid) {
      return;
    }

    const email = this.recoveryForm.get('email')?.value;

    this.authService.recoverPassword(email).subscribe(
      () => {
        // Assuming the response is successful
        this.recoveryFailed = false;
        this.recoverySuccess = true;
        this.errorMessage = '';
        // Clear the form or reset other relevant state
        this.recoveryForm.reset();
      },
      (error) => {
        // Handle recovery failure
        this.recoveryFailed = true;

        if (error.status === 404) {
          this.errorMessage = 'User not found. Please check your email.';
        } else {
          this.errorMessage = 'Password recovery failed. Please try again later.';
        }
      }
    );
  }
}
