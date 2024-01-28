import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";

@Component({
  selector: 'app-recover-password-component',
  templateUrl: './recover-password.component.html'
})
export class RecoverPasswordComponent implements OnInit {
  recoverForm!: FormGroup;
  recoverSucceeded: boolean = false;

  constructor(private formBuilder: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.recoverForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (this.recoverForm.valid) {
      const email = this.recoverForm.get('email')?.value;

      //Enviar email

      this.recoverSucceeded = true;
      this.router.navigate(['/new-password'], { queryParams: { email: email } });
    }
  }
}
