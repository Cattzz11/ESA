import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-confirmation-account-component',
  templateUrl: './confirmation-account.component.html',
  styleUrls: ['./confirmation-account.component.css']
})

export class ConfirmationAccountComponent implements OnInit {
  confirmationForm!: FormGroup;
  confirmationSucceeded: boolean = false;
  code: string = '';
  confirmationMessage: string = '';
  userEmail: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthorizeService) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.userEmail = this.route.snapshot.params['email'];
    });

    this.confirmationForm = this.formBuilder.group({
      code: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  public confirmationCode() {
    if (!this.confirmationForm.valid) {
      this.confirmationMessage = "PIN errado ou inválido";
      return;
    }

    const code = this.confirmationForm.get('code')?.value;

    this.authService.codeValidationEmail(code, this.userEmail).subscribe({
      next: (response) => {
        // Call the new API endpoint to update confirmedEmail status
        this.authService.updateConfirmedEmail(this.userEmail).subscribe({
          next: (updateResponse) => {
            this.sendConfirmationEmail();
          },
          error: (updateError) => {
            this.confirmationMessage = "Erro ao confirmar o email";
          }
        });
      },
      error: (error) => {
        this.confirmationMessage = "Tente novamente";
      }
    });
  }

  sendConfirmationEmail() {
    this.authService.confirmationAccountEmail(this.userEmail).subscribe({
      next: (updateResponse) => {
        this.router.navigate(['/success']);
      }
    });
  }

  public resendConfirmationCode() {
    // Implement logic to resend the confirmation code
    this.authService.resendConfirmationCode(this.userEmail).subscribe({
      next: (response) => {
        this.confirmationMessage = "O código de confirmação foi enviado.";
        console.log('Confirmation code resent successfully');
      },
      error: (error) => {
        this.confirmationMessage = "Ocorreu um erro a enviar o código de confirmação.";
        console.error('Error resending confirmation code', error);
      }
    });
  }

}
