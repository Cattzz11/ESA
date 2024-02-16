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
import { ProfileComponent } from './profile/profile.component';
import { LogoutComponent } from '../api-authorization/logout/logout.component';

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
    LogoutComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule, FormsModule,
    ReactiveFormsModule, ApiAuthorizationModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    AuthGuard,
    AuthorizeService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
