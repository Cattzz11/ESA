import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { AuthorizeService } from "../authorize.service";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-new-password-component',
  templateUrl: './new-password.component.html'
})
export class NewPasswordComponent implements OnInit {
  errors: string[] = [];
  changePasswordForm!: FormGroup;
  changeFailed: boolean = false;
  changeSucceeded: boolean = false;
  userEmail: string = '';

  constructor(
    private authService: AuthorizeService,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.userEmail = this.route.snapshot.params['email'];
    });

    this.changePasswordForm = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    });
  }

  public changePassword() {
    if (!this.changePasswordForm.valid) {
      this.changeFailed = true;
      this.errors = ['Formulário inválido.'];
      return;
    }

    const newPassword = this.changePasswordForm.get('newPassword')?.value;
    const confirmNewPassword = this.changePasswordForm.get('confirmNewPassword')?.value;

    if (newPassword !== confirmNewPassword) {
      this.changeFailed = true;
      this.errors = ['As novas senhas não coincidem.'];
      return;
    }

    this.authService.changePassword(newPassword, this.userEmail).subscribe({
      next: (response) => {
        this.changeSucceeded = true;
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.changeFailed = true;
        this.errors = ['Erro ao alterar a senha. Tente novamente.'];
      }
    });
  }
}
