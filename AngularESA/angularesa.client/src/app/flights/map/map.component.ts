import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { FlightItineraryService } from "../../services/FlightItineraryService";
import { FlightsItinerary } from "../../Models/FlightsItinerary";

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrl: './map.component.css'
})
export class MapComponent implements OnInit, AfterViewInit {
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  map!: google.maps.Map;

  flightsList: FlightsItinerary[] = [];

  display: any;
  center: google.maps.LatLngLiteral = {
    lat: 38.52217424327734,
    lng: -8.838720917701721
  };
  zoom = 2;

  markers: google.maps.marker.AdvancedMarkerElement[] = [];
  polylines: google.maps.PolylineOptions[] = [];

 constructor(private flights: FlightItineraryService) { }

  flightsLoaded = false;

  ngOnInit(): void {
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
    if (this.flightsLoaded) {
      this.addFlightMarkers();
    }
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
}
