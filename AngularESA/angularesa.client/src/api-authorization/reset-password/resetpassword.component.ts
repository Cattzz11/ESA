import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-resetpassword',
  templateUrl: './resetpassword.component.html',
})
export class ResetPasswordComponent implements OnInit {
  resetForm!: FormGroup;
  resetFailed: boolean = false;
  successMessage: string = '';

  constructor(
    private authService: AuthorizeService,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.resetFailed = false;
    this.successMessage = '';

    this.resetForm = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  resetPassword(): void {
    if (!this.resetForm.valid) {
      return;
    }

    const newPassword = this.resetForm.get('newPassword')?.value;
    const confirmPassword = this.resetForm.get('confirmPassword')?.value;

    if (newPassword !== confirmPassword) {
      this.resetFailed = true;
      this.successMessage = '';
      return;
    }

    // Call your service to reset the password
    this.authService.resetPassword(newPassword).subscribe(
      response => {
        this.resetFailed = false;
        this.successMessage = 'Password reset successfully!';
        // Optionally, navigate to a different page after successful password reset
        // this.router.navigateByUrl("/login");
      },
      error => {
        this.resetFailed = true;
        this.successMessage = '';
        // Handle password reset failure
      }
    );
  }
}
