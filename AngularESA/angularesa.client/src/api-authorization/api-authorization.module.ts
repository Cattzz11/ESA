import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginMenuComponent } from './login-menu/login-menu.component';
import { SignInComponent } from './signin/signin.component';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { RegisterComponent } from './register/register.component';
import { ReactiveFormsModule } from '@angular/forms';
import { RecoverPasswordComponent } from './recover-password/recover-password.component';
import { NewPasswordComponent } from "./new-password/new-password.component";
import { RecoveryCodeComponent } from "./recovery-code/recovery-code.component";

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule,    
    ReactiveFormsModule,
    RouterModule.forChild(
      [
        { path: 'sigin', component: SignInComponent },
        { path: 'new', component: RegisterComponent },
        { path: 'new-password', component: NewPasswordComponent },
        { path: 'recover-password', component: RecoverPasswordComponent },
        { path: 'recovery-code', component: RecoveryCodeComponent },
      ]
    )
  ],
  declarations: [LoginMenuComponent, SignInComponent, RegisterComponent, RecoverPasswordComponent, NewPasswordComponent, RecoveryCodeComponent],
  exports: [LoginMenuComponent, SignInComponent, RegisterComponent, RecoverPasswordComponent, NewPasswordComponent, RecoveryCodeComponent]
})
export class ApiAuthorizationModule { }


