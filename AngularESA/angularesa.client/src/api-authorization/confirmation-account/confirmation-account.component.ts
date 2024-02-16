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

    this.authService.codeValidation(code, this.userEmail).subscribe({
      next: (response) => {
        this.router.navigate(['/success']);
      },
      error: (error) => {
        this.confirmationMessage = "Tente novamente";
      }
    });
  }
}
