import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AuthGuard } from '../api-authorization/authorize.guard';
import { AboutComponent } from './about/about.component';
import { RegisterComponent } from '../api-authorization/register/register.component';
import { SignInComponent } from '../api-authorization/signin/signin.component';
import { NewPasswordComponent } from "../api-authorization/new-password/new-password.component";
import { RecoverPasswordComponent } from '../api-authorization/recover-password/recover-password.component';
import { RecoveryCodeComponent } from '../api-authorization/recovery-code/recovery-code.component';
import { PremiumProfilePageComponent } from './users/premium-profile-page/premium-profile-page.component';
import { FilterByAirlineComponent } from './flights/filter-by-airline/filter-by-airline.component';


const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'about', component: AboutComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: SignInComponent },
  { path: 'new-password/:email', component: NewPasswordComponent },
  { path: 'recover-password', component: RecoverPasswordComponent },
  { path: 'recovery-code/:email', component: RecoveryCodeComponent },
  { path: 'premium-profile-page', component: PremiumProfilePageComponent },
  { path: 'filter-by-airline/:id/:logoURL', component: FilterByAirlineComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
