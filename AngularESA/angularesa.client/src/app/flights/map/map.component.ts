import { Component } from '@angular/core';
import { FlightItineraryService } from '../../services/FlightItineraryService';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrl: './map.component.css'
})
export class MapComponent {
  mapUrl: SafeUrl | undefined;

  constructor(private mapService: FlightItineraryService, public sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.mapService.getMap().subscribe({
      next: (response) => {
        this.mapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(response);
        console.log(response);
      },
      error: (error) => {
        console.error('Error fetching map data:', error);
      }
    });
  }
}
