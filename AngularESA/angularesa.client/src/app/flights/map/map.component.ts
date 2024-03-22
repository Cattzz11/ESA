import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { FlightItineraryService } from "../../services/FlightItineraryService";
import { FlightsItinerary } from "../../Models/FlightsItinerary";
import { Trip } from "../../Models/Trip";
import { TripDetails } from "../../Models/TripDetails";
import { Router } from "@angular/router";
import { User } from "../../Models/users";
import { AuthorizeService } from "../../../api-authorization/authorize.service";
import { PriceOptions } from "../../Models/PriceOptions";
import { AeroDataBoxService } from "../../services/AeroDataBoxService";
import { AircraftData } from "../../Models/AircraftData";

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
  aircraftData: AircraftData | undefined;

  display: any;
  center: google.maps.LatLngLiteral = {
    lat: 38.52217424327734,
    lng: -8.838720917701721
  };
  zoom = 3;

  markers: google.maps.marker.AdvancedMarkerElement[] = [];
  polylines: google.maps.PolylineOptions[] = [];

  originMarker: google.maps.Marker | null = null;
  destinationMarker: google.maps.Marker | null = null;
  markersPlaced: boolean = false;

  flightsLoaded = false;
  isLoading = false;

  constructor(
    private flights: FlightItineraryService,
    private aircraft: AeroDataBoxService,
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

        marker.addListener('click', () => {
          this.aircraft.getAirplaneData(flight.flightIATA).subscribe({
            next: (data: AircraftData) => {
              this.aircraftData = data;

              function formatarDataHora(date: Date, type: 'arrival' | 'firstFlight'): string {
                const data = new Date(date);
                const dia = data.getDate().toString().padStart(2, '0');
                const mes = (data.getMonth() + 1).toString().padStart(2, '0');
                const ano = data.getFullYear();
                const horas = data.getHours().toString().padStart(2, '0');
                const minutos = data.getMinutes().toString().padStart(2, '0');

                if (type === 'arrival') {
                  return `${dia}/${mes}/${ano} ${horas}:${minutos}`;
                } else {
                  return `${dia}/${mes}/${ano}`;
                }
              }

              const infoWindowMain = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Voo nº ${flight.flightIATA} com origem em ${flight.departureLocation.name} e destino a ${flight.arrivalLocation.name}</h3>
                  <button id="btnNextFlights" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Informações sobre próximos voos</button>
                  <button id="btnBuyTickets" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Comprar Bilhetes</button>
                  <button id="btnRequestTrip" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Pedir Nova Viagem</button>
                  <div id="mainArea" style="font-family: Arial, sans-serif; display:flex; flex-direction: row; align-content: center; align-items: center; width: 100%;">
                    <div id="ballonButtons" style="display: flex; flex-direction: column; align-content: center; align-items: center; width: 50%;">
                      <button id="btnAverageFlightTime" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Média de tempo de viagem</button>
                      <button id="btnAverageLate" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Média de atrasos</button>
                      <button id="btnCountriesRoutes" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Países e rotas</button>
                      <label>Data do 1º vôo: ${formatarDataHora(this.aircraftData.firstFlightDate, 'firstFlight') } </label>
                    </div>
                    <div id="airplaneData" style="display: flex; flex-direction: column; align-content: center; align-items: center; width: 50%;">
                      <img src="${this.aircraftData?.photo}" style="height: 150px; width: 150px; border-radius: 10px; margin-top:20px; margin-bottom:10px;"/>
                      <button id="btnSpecifications" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px;">Especificações</button>
                      <label style="margin-top:10px;">Tempo decorrido: 2H</label>
                      <label>Estimativa de chegada:</label>
                      <label>${formatarDataHora(flight.arrivalSchedule, 'arrival')}H</label>
                    </div>
                  </div>
                </div>
              `;

              const infoNextFlights = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Informação dos proximos voos</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoBuyTickets = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Compra de Bilhetes</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoRequestTrip = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Pedir uma nova viagem</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoAverageFlightTime = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Informação sobre a média de tempo de viagem</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoAverageLate = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Informação sobre a média de atrasos</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoCountriesRoutes = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; align-content: center; align-items: center; width: 100%;">
                  <h3 style="color: #006994;">Informação sobre os países e rotas do avião</h3>
                  <p>Página ainda em construção</p>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%;">Voltar</button>
                </div>
              `;

              const infoSpecifications = `
                <div id="infoWindow" style="font-family: Arial, sans-serif; display: flex; flex-direction: column; width: 100%; max-height: 400px; overflow-y: auto;">
                  <h3 style="color: #006994; align-self: center;">Informação sobre o avião</h3>
                  <p style="margin-left: 20px;">Código do modelo: ${this.aircraftData.modelCode}</p>
                  <p style="margin-left: 20px;">Modelo: ${this.aircraftData.model}</p>
                  <p style="margin-left: 20px;">Matricula: ${this.aircraftData.registration}</p>
                  <p style="margin-left: 20px;">Companhia aérea: ${this.aircraftData.airline}</p>
                  <p style="margin-left: 20px;">ICAO: ${this.aircraftData.icao}</p>
                  <p style="margin-left: 20px;">Número de assentos: ${this.aircraftData.seatsNumber}</p>
                  <p style="margin-left: 20px;">Data de entrega à companhia: ${formatarDataHora(this.aircraftData.rolloutDate, 'firstFlight') }</p>
                  <p style="margin-left: 20px;">Data do 1º Vôo: ${formatarDataHora(this.aircraftData.firstFlightDate, 'firstFlight') }</p>
                  <p style="margin-left: 20px;">Data do 1º registro: ${formatarDataHora(this.aircraftData.registrationDate, 'firstFlight') }</p>
                  <p style="margin-left: 20px;">Número de motores: ${this.aircraftData.enginesNumber}</p>
                  <p style="margin-left: 20px;">Tipo de motores: ${this.aircraftData.enginesType}</p>
                  <p style="margin-left: 20px;">É avião de carga: ${this.aircraftData.isFreighter ? 'Sim' : 'Não'}</p>
                  <p style="margin-left: 20px;">Linha de produção: ${this.aircraftData.productionLine}</p>
                  <p style="margin-left: 20px;">Idade: ${this.aircraftData?.photo}</p>
                  <img src="${this.aircraftData?.photo}" style="height: 200px; width: 200px; border-radius: 10px; margin-top:20px; margin-bottom:10px; align-self: center;"/>
                  <button id="btnBackToMain" style="background-color: #87CEEB; color: white; margin: 4px; padding: 8px 16px; border: none; cursor: pointer; border-radius: 20px; width: 90%; align-self: center;">Voltar</button>
                </div>
              `;

              const infoWindow = new google.maps.InfoWindow({
                content: infoWindowMain
              });

              infoWindow.open(this.map, marker);

              function addListenersToInfoWindowButtons() {
                const btnNextFlights = document.getElementById('btnNextFlights');
                const btnBackToMain = document.getElementById('btnBackToMain');
                const btnBuyTickets = document.getElementById('btnBuyTickets');
                const btnRequestTrip = document.getElementById('btnRequestTrip');
                const btnAverageFlightTime = document.getElementById('btnAverageFlightTime');
                const btnAverageLate = document.getElementById('btnAverageLate');
                const btnCountriesRoutes = document.getElementById('btnCountriesRoutes');
                const btnSpecifications = document.getElementById('btnSpecifications');

                if (btnNextFlights) {
                  btnNextFlights.addEventListener('click', () => {
                    infoWindow.setContent(infoNextFlights);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnBackToMain) {
                  btnBackToMain.addEventListener('click', () => {
                    infoWindow.setContent(infoWindowMain);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnBuyTickets) {
                  btnBuyTickets.addEventListener('click', () => {
                    infoWindow.setContent(infoBuyTickets);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnRequestTrip) {
                  btnRequestTrip.addEventListener('click', () => {
                    infoWindow.setContent(infoRequestTrip);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnAverageFlightTime) {
                  btnAverageFlightTime.addEventListener('click', () => {
                    infoWindow.setContent(infoAverageFlightTime);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnAverageLate) {
                  btnAverageLate.addEventListener('click', () => {
                    infoWindow.setContent(infoAverageLate);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnCountriesRoutes) {
                  btnCountriesRoutes.addEventListener('click', () => {
                    infoWindow.setContent(infoCountriesRoutes);
                    addListenersToInfoWindowButtons();
                  });
                }


                if (btnSpecifications) {
                  btnSpecifications.addEventListener('click', () => {
                    infoWindow.setContent(infoSpecifications);
                    addListenersToInfoWindowButtons();
                  });
                }
              }

              google.maps.event.addListenerOnce(infoWindow, 'domready', () => {
                addListenersToInfoWindowButtons();
              });
            },
            error: (error) => {
              console.error('Error fetching user info', error);
            }
          });
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
