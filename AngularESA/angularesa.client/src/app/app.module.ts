import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { PeopleComponent } from './People/people.component';
import { PersonComponent } from './person/person.component';
import { PersonDetailsComponent } from './person-details/person-details.component';
import { PersonCreateComponent } from './person-create/person-create.component';
import { PersonEditComponent } from './person-edit/person-edit.component';
import { PersonDeleteComponent } from './person-delete/person-delete.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ShortnamePipe } from './shortname.pipe';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';
import { AuthInterceptor } from '../api-authorization/authorize.interceptor';
import { AuthGuard } from '../api-authorization/authorize.guard';
import { AuthorizeService } from '../api-authorization/authorize.service';
import { PremiumProfilePageComponent } from './users/premium-profile-page/premium-profile-page.component';
import { ProfileComponent } from './profile/profile.component';
import { LogoutComponent } from '../api-authorization/logout/logout.component';
import { FilterByAirlineComponent } from './flights/filter-by-airline/filter-by-airline.component';
import { SearchFlightsComponent } from './flights/search-flights/search-flights.component';
import { ConfirmationAccountComponent } from '../api-authorization/confirmation-account/confirmation-account.component';
import { SuccessComponent } from '../api-authorization/success/success.component';
import { PhotoUploadService } from './services/photoUploadService.service';
import { EditProfileComponent } from './users/edit-profile/edit-profile.component';
import { PremiumComponent } from './users/premium/premium.component';
import { SubscriptionPageComponent } from './users/subscription-page/subscription-page.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ShortnamePipe,
    PremiumProfilePageComponent,
    PersonDetailsComponent,
    PersonCreateComponent,
    PersonEditComponent,
    PersonDeleteComponent,
    ProfileComponent,
    LogoutComponent,
    CounterComponent,
    FetchDataComponent,
    PeopleComponent,
    PersonComponent,
    FilterByAirlineComponent,
    SearchFlightsComponent,
    ConfirmationAccountComponent,
    SuccessComponent,
    EditProfileComponent,
    PremiumComponent,
    SubscriptionPageComponent,
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule, FormsModule,
    ReactiveFormsModule, ApiAuthorizationModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    AuthGuard,
    AuthorizeService, PhotoUploadService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
