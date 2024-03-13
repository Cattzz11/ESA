import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ShortnamePipe } from './shortname.pipe';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';
import { AuthInterceptor } from '../api-authorization/authorize.interceptor';
import { AuthGuard } from '../api-authorization/authorize.guard';
import { AuthorizeService } from '../api-authorization/authorize.service';
import { PremiumProfilePageComponent } from './users/premium-profile-page/premium-profile-page.component';
import { LogoutComponent } from '../api-authorization/logout/logout.component';
import { FilterByAirlineComponent } from './flights/filter-by-airline/filter-by-airline.component';
import { SearchFlightsComponent } from './flights/search-flights/search-flights.component';
import { ConfirmationAccountComponent } from '../api-authorization/confirmation-account/confirmation-account.component';
import { SuccessComponent } from '../api-authorization/success/success.component';
import { PhotoUploadService } from './services/photoUploadService.service';
import { EditProfileComponent } from './users/edit-profile/edit-profile.component';
import { PremiumComponent } from './users/premium/premium.component';
import { SubscriptionPageComponent } from './users/subscription-page/subscription-page.component';
import { TripDetailsComponent } from './flights/trip-details/trip-details.component';
import { FlightDataComponent } from './flights/flight-data/flight-data.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { MatCalendarCellClassFunction, MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MAT_DATE_LOCALE, MAT_DATE_FORMATS } from '@angular/material/core';
import { DateAdapter } from '@angular/material/core';
import { DatePipe } from '@angular/common';
import { PaymentComponentComponent } from './users/payment-component/payment-component.component';

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ShortnamePipe,
    PremiumProfilePageComponent,
    LogoutComponent,
    FilterByAirlineComponent,
    SearchFlightsComponent,
    ConfirmationAccountComponent,
    SuccessComponent,
    EditProfileComponent,
    PremiumComponent,
    SubscriptionPageComponent,
    TripDetailsComponent,
    FlightDataComponent,
    PaymentComponentComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    ApiAuthorizationModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    AuthGuard,
    AuthorizeService,
    PhotoUploadService,
    provideAnimationsAsync(),
    provideNativeDateAdapter(),
    { provide: MAT_DATE_LOCALE, useValue: 'pt-PT' },
    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
    DatePipe
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
