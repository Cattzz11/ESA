import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { PeopleComponent } from './People/people.component';
import { PersonDetailsComponent } from './person-details/person-details.component';
import { PersonCreateComponent } from './person-create/person-create.component';
import { PersonEditComponent } from './person-edit/person-edit.component';
import { PersonDeleteComponent } from './person-delete/person-delete.component';
import { AuthGuard } from '../api-authorization/authorize.guard';
import { AboutComponent } from './about/about.component';
import { RegisterComponent } from '../api-authorization/register/register.component';
import { SignInComponent } from '../api-authorization/signin/signin.component';
import { NewPasswordComponent } from "../api-authorization/new-password/new-password.component";
import { RecoverPasswordComponent } from '../api-authorization/recover-password/recover-password.component';
import { RecoveryCodeComponent } from '../api-authorization/recovery-code/recovery-code.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'counter', component: CounterComponent },
  { path: 'fetch-data', component: FetchDataComponent, canActivate: [AuthGuard] },
  { path: 'people', component: PeopleComponent },
  { path: 'about', component: AboutComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: SignInComponent },
  { path: 'person/details/:id', component: PersonDetailsComponent },
  { path: 'person/create', component: PersonCreateComponent },
  { path: 'person/edit/:id', component: PersonEditComponent },
  { path: 'person/delete/:id', component: PersonDeleteComponent },
  { path: 'new-password/:email', component: NewPasswordComponent },
  { path: 'recover-password', component: RecoverPasswordComponent },
  { path: 'recovery-code/:email', component: RecoveryCodeComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
