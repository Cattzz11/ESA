import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AuthGuard } from '../api-authorization/authorize.guard';
import { AboutComponent } from './about/about.component';
import { RegisterComponent } from '../api-authorization/register/register.component';
import { SignInComponent } from '../api-authorization/signin/signin.component';
import { LogoutComponent } from '../api-authorization/logout/logout.component';
import { NewPasswordComponent } from "../api-authorization/new-password/new-password.component";
import { RecoverPasswordComponent } from '../api-authorization/recover-password/recover-password.component';
import { RecoveryCodeComponent } from '../api-authorization/recovery-code/recovery-code.component';
import { PremiumProfilePageComponent } from './users/premium-profile-page/premium-profile-page.component';
import { FilterByAirlineComponent } from './flights/filter-by-airline/filter-by-airline.component';
import { SearchFlightsComponent } from './flights/search-flights/search-flights.component';
import { ConfirmationAccountComponent } from '../api-authorization/confirmation-account/confirmation-account.component';
import { SuccessComponent } from '../api-authorization/success/success.component';
import { PremiumComponent } from './users/premium/premium.component';
import { SubscriptionPageComponent } from './users/subscription-page/subscription-page.component';
import { EditProfileComponent } from './users/edit-profile/edit-profile.component';
import { FlightDataComponent } from './flights/flight-data/flight-data.component';
import { PaymentComponentComponent } from './users/payment-component/payment-component.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'about', component: AboutComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: SignInComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'new-password/:email', component: NewPasswordComponent },
  { path: 'recover-password', component: RecoverPasswordComponent },
  { path: 'recovery-code/:email', component: RecoveryCodeComponent },
  { path: 'premium-profile-page', component: PremiumProfilePageComponent },
  { path: 'search-flights-page', component: SearchFlightsComponent },
  { path: 'confirmation-account/:email', component: ConfirmationAccountComponent },
  { path: 'success', component: SuccessComponent },
  { path: 'edit-profile', component: EditProfileComponent },
  { path: 'premium-component', component: PremiumComponent },
  { path: 'subscription-component', component: SubscriptionPageComponent },
  { path: 'flight-data', component: FlightDataComponent },
  { path: 'filter-by-airline', component: FilterByAirlineComponent },
  { path: 'search-flights', component: SearchFlightsComponent },
  { path: 'payment-component', component: PaymentComponentComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
