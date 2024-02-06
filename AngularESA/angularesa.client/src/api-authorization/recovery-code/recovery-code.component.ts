import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthorizeService } from "../authorize.service";

@Component({
  selector: 'app-recovery-code-component',
  templateUrl: './recovery-code.component.html'
})
export class RecoveryCodeComponent implements OnInit {
  recoveryForm!: FormGroup;
  recoverSucceeded: boolean = false;
  code: string = '';
  errors: string[] = [];
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

    this.recoveryForm = this.formBuilder.group({
      code: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  public recoveryCode() {
    if (!this.recoveryForm.valid) {
      return;
    }

    const code = this.recoveryForm.get('code')?.value;

    this.authService.codeValidation(code, this.userEmail).subscribe({
      next: (response) => {
        this.router.navigate(['/new-password/', this.userEmail]);
      },
      error: (error) => {
        this.errors = ['Erro ao alterar a senha. Tente novamente.'];
      }
    });
  }
}
