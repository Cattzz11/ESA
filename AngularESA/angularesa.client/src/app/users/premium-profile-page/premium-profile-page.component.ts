import { Component } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { Trip } from '../../Models/Trip';
import { SkyscannerService } from '../../services/skyscannerService';

@Component({
  selector: 'app-premium-profile-page',
  templateUrl: './premium-profile-page.component.html',
  styleUrl: './premium-profile-page.component.css'
})
export class PremiumProfilePageComponent {
  public user: User | null = null;
  public history: Trip[] = [];
  public historyLoading: boolean = true;

  constructor(private auth: AuthorizeService, private skyscannerService: SkyscannerService) { }

  ngOnInit() {
    this.auth.getUserInfo().subscribe({
      next: (userInfo: User) => { 
        this.user = userInfo;

        if (this.user && this.user.role === 1) {
          console.log('É premium');
          this.loadHistory();
        }
      },
      error: (error) => {
        console.error('Error fetching user info', error);
      }
    });
  }

  edit() {
    console.log(this.user?.name);
    console.log(this.user?.email);
    console.log(this.user?.role);
    console.log(this.user?.occupation);
    console.log(this.user?.nationality);
    console.log(this.user?.age);

    this.history.forEach((trip, index) => {
      console.log(`Viagem ${index + 1}:`, trip);
      console.log('Voos da viagem:');

      if (trip.flights && trip.flights.length > 0) {
        trip.flights.forEach((flight, flightIndex) => {
          console.log(`  Voo ${flightIndex + 1}:`, flight);
          console.log('  Segmentos do voo:');

          if (flight.segments && flight.segments.length > 0) {
            flight.segments.forEach((segment, segmentIndex) => {
              console.log(`    Segmento ${segmentIndex + 1}:`, segment);

              if (segment.carrier) {
                console.log(`      Carrier do Segmento ${segmentIndex + 1}:`, segment.carrier);
                console.log(`        Nome do Carrier: ${segment.carrier.name}`);
                console.log(`        LogoUrl do Carrier: ${segment.carrier.logoURL}`);
              } else {
                console.log(`      O Segmento ${segmentIndex + 1} não possui dados de carrier.`);
              }
            });
          } else {
            console.log('      Este voo não possui segmentos.');
          }
        });
      } else {
        console.log('    Esta viagem não possui voos.');
      }
    });

  }

  loadHistory() {
    if (this.user) {
      this.skyscannerService.getFlightsByUser(this.user.id).subscribe({
        next: (response) => {
          this.history = response;
          this.historyLoading = false;
        },
        error: (error) => {
          console.error('Error fetching flights:', error);
          this.historyLoading = false;
        }
      });
    } else {
      console.error('User ID is undefined');
      this.historyLoading = false;
    }
  }

}
