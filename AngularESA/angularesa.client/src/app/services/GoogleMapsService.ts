import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

declare global {
  interface Window {
    initMap: () => void;
  }
}

@Injectable({
  providedIn: 'root'
})
export class GoogleMapsService {
  private googleMapsUrl = 'https://maps.googleapis.com/maps/api/js?key=AIzaSyD58yFxevJ8McI8Wc1WxUfx9EhVl-6D4gQ&callback=initMap';

  constructor() { }

  public loadGoogleMaps(): Observable<void> {
    return new Observable(observer => {
      // Adiciona o script de forma dinâmica
      const script = document.createElement('script');
      script.src = this.googleMapsUrl;
      script.async = true;
      script.defer = true;
      window.initMap = () => {
        observer.next();
        observer.complete();
      };
      document.body.appendChild(script);
    });
  }
}
