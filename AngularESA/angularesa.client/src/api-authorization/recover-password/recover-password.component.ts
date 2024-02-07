import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-recover-password-component',
  templateUrl: './recover-password.component.html',
  styleUrls: ['./recover-password.component.css']
})
export class RecoverPasswordComponent implements OnInit {
  recoverForm!: FormGroup;
  recoverSucceeded: boolean = false;
  userEmail: string = '';
  confirmationMessage: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthorizeService) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.userEmail = params['email'] || '';
    });

    this.recoverForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  public recoverPassword() {
    if (!this.recoverForm.valid) {
      return;
    }

    const email = this.recoverForm.get('email')?.value;
    this.confirmationMessage = "Se o e-mail existir, irá receber um código de recuperação de password.";

    this.authService.recoverPassword(email).subscribe({
      next: (response) => {
        setTimeout(() => this.router.navigate(['/recovery-code/', email]), 2000); // espera 2 segundos
      },
      error: (error) => {
        setTimeout(() => this.router.navigate(['/recovery-code/', email]), 2000); // espera 2 segundos
      }
    });
  }
}
