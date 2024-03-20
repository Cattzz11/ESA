import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { FlightItineraryService } from "../../services/FlightItineraryService";
import { FlightsItinerary } from "../../Models/FlightsItinerary";
import { Trip } from "../../Models/Trip";
import { City } from "../../Models/City";
import { TripDetails } from "../../Models/TripDetails";
import { Router } from "@angular/router";
import { User } from "../../Models/users";
import { AuthorizeService } from "../../../api-authorization/authorize.service";
import { PriceOptions } from "../../Models/PriceOptions";

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrl: './map.component.css'
})
export class MapComponent implements OnInit, AfterViewInit {
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  user: User | null = null;

  map!: google.maps.Map;

  flightsList: FlightsItinerary[] = [];
  tripList: Trip[] = [];
  tripListPremium: TripDetails[] = [];

  display: any;
  center: google.maps.LatLngLiteral = {
    lat: 38.52217424327734,
    lng: -8.838720917701721
  };
  zoom = 2;

  markers: google.maps.marker.AdvancedMarkerElement[] = [];
  polylines: google.maps.PolylineOptions[] = [];

  originMarker: google.maps.Marker | null = null;
  destinationMarker: google.maps.Marker | null = null;
  markersPlaced: boolean = false;

  flightsLoaded = false;
  isLoading = false;

  constructor(
    private flights: FlightItineraryService,
    private router: Router,
    private auth: AuthorizeService
  ) { }

  ngOnInit(): void {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
    }
    else {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }

    this.flights.getFlights().subscribe({
      next: (response) => {
        this.flightsList = response;
        this.flightsLoaded = true;
        if (this.map) {
          this.addFlightMarkers();
        }
      },
    });
  }

  ngAfterViewInit(): void {
    this.map = new google.maps.Map(
      this.mapContainer.nativeElement,
      {
        zoom: this.zoom,
        center: this.center,
        mapTypeId: "terrain",
      }
    );

    this.map.addListener('click', (event: google.maps.MapMouseEvent) => {
      const elevationService = new google.maps.ElevationService();

      elevationService.getElevationForLocations({
        'locations': [event.latLng!]
      }, (results, status) => {
        if (status === 'OK') {
          if (results && results[0].elevation <= 1) {
            console.log('Clicou no oceano, não adicione um marcador.');
          } else {
            if (this.markersPlaced) {
              this.clearMarkers();
              this.originMarker = this.createMarker(event.latLng!, 'green', 'Origem');
              this.markersPlaced = false;
            } else {
              if (!this.originMarker) {
                this.originMarker = this.createMarker(event.latLng!, 'green', 'Origem');
              }
              else if (!this.destinationMarker) {
                this.destinationMarker = this.createMarker(event.latLng!, 'red', 'Destino');
                this.markersPlaced = true;
                this.performCustomAction(this.originMarker, this.destinationMarker);
              }
            }
          }
        }
      });
    });

    if (this.flightsLoaded) {
      this.addFlightMarkers();
    }
  }

  createMarker(position: google.maps.LatLng, color: string, title: string): google.maps.Marker {
    const iconUrl = color === 'green' ? 'http://maps.google.com/mapfiles/ms/icons/yellow-dot.png' :
      'http://maps.google.com/mapfiles/ms/icons/red-dot.png';

    return new google.maps.Marker({
      map: this.map,
      position: position,
      icon: { url: iconUrl },
      title: title
    });
  }

  clearMarkers(): void {
    if (this.originMarker) {
      this.originMarker.setMap(null);
      this.originMarker = null;
    }
    if (this.destinationMarker) {
      this.destinationMarker.setMap(null);
      this.destinationMarker = null;
    }
  }

  performCustomAction(originMarker: google.maps.Marker, destinationMarker: google.maps.Marker): void {
    const geocoder = new google.maps.Geocoder();
    const originPosition = originMarker.getPosition();
    const destinationPosition = destinationMarker.getPosition();
    this.isLoading = true;

    const getAddressComponents = (position: google.maps.LatLng | null | undefined): Promise<AddressComponents> => {
      return new Promise((resolve, reject) => {
        if (position) {
          // Especificando o idioma dos resultados como inglês
          geocoder.geocode({ location: position, language: 'en' }, (results, status) => {
            if (status === 'OK' && results && results[0]) {
              const addressComponents = results[0].address_components;
              const cityComponent = addressComponents.find(component => component.types.includes("administrative_area_level_2"));
              const countryComponent = addressComponents.find(component => component.types.includes("country"));
              resolve({
                city: cityComponent ? cityComponent.long_name : "",
                country: countryComponent ? countryComponent.long_name : "",
                latitude: position.lat().toString(),
                longitude: position.lng().toString()
              });
            } else {
              reject(new Error('Geocoder failed due to: ' + status));
            }
          });
        } else {
          reject(new Error('No position provided'));
        }
      });
    }; 

    Promise.all([
      getAddressComponents(originPosition),
      getAddressComponents(destinationPosition)
    ]).then(([originAddress, destinationAddress]: [AddressComponents, AddressComponents]) => {
      if (this.user && this.user.role === 1) {
        this.flights.getTripsPremium(originAddress, destinationAddress).subscribe({
          next: (response) => {
            this.tripListPremium = response;
            this.isLoading = false;
          },
          error: (error) => {
            this.isLoading = false;
            console.error('Error fetching flights:', error);
          }
        });
      } else {
        this.flights.getTrips(originAddress, destinationAddress).subscribe({
          next: (response) => {
            this.tripList = response;
            this.isLoading = false;
          },
          error: (error) => {
            this.isLoading = false;
            console.error('Error fetching flights:', error);
          }
        });
      };
    }).catch(error => {
      console.error(error);
      this.isLoading = false;
    });
  }

  moveMap(event: google.maps.MapMouseEvent) {
    if (event.latLng != null) this.center = (event.latLng.toJSON());
  }

  move(event: google.maps.MapMouseEvent) {
    if (event.latLng != null) this.display = event.latLng.toJSON();
  }

  addFlightMarkers(): void {
    this.markers = [];
    this.polylines = [];

    this.flightsList.forEach(flight => {
      let departureCoordinates;
      let arrivalCoordinates;

      if (flight.departureLocation.coordinates) {
        const [depLat, depLng] = flight.departureLocation.coordinates.split(';').map(Number);
        departureCoordinates = { lat: depLat, lng: depLng };
        this.addMarker(depLat, depLng, 'departure');
      }

      if (flight.arrivalLocation.coordinates) {
        const [arrLat, arrLng] = flight.arrivalLocation.coordinates.split(';').map(Number);
        arrivalCoordinates = { lat: arrLat, lng: arrLng };
        this.addMarker(arrLat, arrLng, 'arrival');
      }

      if (departureCoordinates && arrivalCoordinates) {
        const polyline: google.maps.PolylineOptions = {
          path: [departureCoordinates, arrivalCoordinates],
          geodesic: true,
          strokeColor: '#FF0000',
          strokeOpacity: 1.0,
          strokeWeight: 2,
        };
        const flightPath = new google.maps.Polyline(polyline);
        flightPath.setMap(this.map);

        const randomPoint = this.getRandomPoint(flightPath);

        const planeSymbol = {
          path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
          scale: 3,
          rotation: randomPoint.heading,
        };

        const marker = new google.maps.Marker({
          map: this.map,
          icon: planeSymbol,
          position: randomPoint.position
        });

        // Aqui é a ação quando de clica num avião
        marker.addListener('click', function () {
          console.log(flight.departureLocation.name);
          console.log(flight.arrivalLocation.name);
        });
      }
    });
  }

  getRandomPoint(polyline: google.maps.Polyline): { position: google.maps.LatLng, heading: number } {
    const path = polyline.getPath();
    const start = path.getAt(0);
    const end = path.getAt(1);

    const fraction = Math.random();

    const point = google.maps.geometry.spherical.interpolate(start, end, fraction);

    const heading = google.maps.geometry.spherical.computeHeading(point, end);

    return { position: point, heading };
  }



  addMarker(lat: number, lng: number, type: 'departure' | 'arrival'): void {
    let icon;
    if (type === 'departure') {
      icon = 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png';
    } else {
      icon = 'https://maps.google.com/mapfiles/ms/icons/green-dot.png';
    }

    new google.maps.Marker({
      map: this.map,
      icon: {
        url: icon,
        scaledSize: new google.maps.Size(36, 36),
        anchor: new google.maps.Point(18, 18),
      },
      position: {
        lat: lat,
        lng: lng,
      }
    });
  }

  selectTrip(trip: any) {
    this.router.navigate(['/flight-data'], { state: { data: trip } });
  }

  calculatePrices(inputField: 'max' | 'med' | 'min', options: PriceOptions[]) {
    const prices = options.map(option => option.totalPrice);

    switch (inputField) {
      case 'max':
        return Math.max(...prices);
      case 'med':
        const averagePrice = prices.reduce((a, b) => a + b, 0) / prices.length;

        const closestToAveragePrice = prices.reduce((prev, curr) => {
          return (Math.abs(curr - averagePrice) < Math.abs(prev - averagePrice) ? curr : prev);
        });

        return closestToAveragePrice;
      case 'min':
        return Math.min(...prices);;
    }
  }
}

export interface AddressComponents {
  city: string;
  country: string;
  latitude: string;
  longitude: string;
}
