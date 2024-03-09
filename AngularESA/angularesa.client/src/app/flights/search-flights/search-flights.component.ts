import { Component, OnInit } from '@angular/core';
import { City } from '../../Models/City';
import { DataService } from '../../services/DataService';
import { Calendar } from '../../Models/Calendar';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';
import dayGridPlugin from '@fullcalendar/daygrid';
import { CalendarOptions } from '@fullcalendar/core';
import { Flight } from '../../Models/Flight';
import { Trip } from '../../Models/Trip';
import { Router } from '@angular/router';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrl: './search-flights.component.css'
})
export class SearchFlightsComponent implements OnInit {
  cities: City[] = [];
  calendar: Calendar[] | undefined;
  filteredCities: City[] = [];
  flights: Trip[] = [];

  selectedCityFrom = '';
  selectedCityTo = '';
  departureDate = '';
  arrivalDate = '';

  showDropdownFrom = false;
  showDropdownTo = false;
  interactingWithDropdownFrom = false;
  fromValid = false;
  toValid = false;
  canSearch = false;
  departureEnabled = false;
  arrivalEnabled = false;
  isLoading = false;

  constructor(private dataService: DataService, private skyscannerService: SkyscannerService, private router: Router) { }

  ngOnInit(): void {
    this.dataService.getAllCities().subscribe({
      next: (response) => {
        this.cities = response;
      },
      error: (error) => {
        console.error('Error fetching flights:', error);
      }
    });
  }

  loadCalendar(from: string, to: string) {
    let data: FlightData = {
      fromEntityId: this.findCityApiKeyByName(from)!,
      toEntityId: this.findCityApiKeyByName(to)
    }

    this.skyscannerService.getCalendarFlights(data).subscribe({
      next: (response) => {
        this.calendar = response;
      },
      error: (error) => {
        console.error('Error fetching flights:', error);
      }
    });
  }

  showAllCities(inputField: 'from' | 'to') {
    this.filteredCities = this.cities;

    if (inputField === 'from') {
      this.showDropdownFrom = true;
    } else {
      this.showDropdownTo = true;
    }
  }

  filterCitiesByText(inputField: 'from' | 'to') {
    const searchText = inputField === 'from' ? this.selectedCityFrom.toLowerCase() : this.selectedCityTo.toLowerCase();
    const filteredCities = this.cities.filter(city =>
      city.name.toLowerCase().includes(searchText) ||
      city.country.name.toLowerCase().includes(searchText)
    );

    this.filteredCities = filteredCities;

    if (inputField === 'from') {
      this.showDropdownFrom = this.filteredCities.length > 0;
    } else {
      this.showDropdownTo = this.filteredCities.length > 0;
    }
  }

  selectCity(city: City, inputField: 'from' | 'to') {
    if (inputField === 'from') {
      this.selectedCityFrom = city.name;
      this.showDropdownFrom = false;
      this.fromValid = this.isCityValid(this.selectedCityFrom);
      this.toValid = false;
    } else {
      this.selectedCityTo = city.name;
      this.showDropdownTo = false;
      this.toValid = this.isCityValid(this.selectedCityTo);
      this.departureEnabled = this.toValid;
    }
    this.validateForm();
  }

  isCityValid(cityName: string): boolean {
    return this.cities.some(city => cityName.includes(city.name));
  }

  validateForm() {
    this.fromValid = this.isCityValid(this.selectedCityFrom);
    this.toValid = this.isCityValid(this.selectedCityTo);

    if (this.fromValid && this.toValid && this.calendar === undefined) {
      this.loadCalendar(this.selectedCityFrom, this.selectedCityTo);
    }

    this.canSearch = this.fromValid && this.toValid && !!this.departureDate && !!this.arrivalDate;
  }

  hideDropdown(inputField: 'from' | 'to') {
    if (inputField === 'from') {
      if (!this.interactingWithDropdownFrom) {
        this.showDropdownFrom = false;
      }
    } else {
      if (!this.interactingWithDropdownFrom) {
        this.showDropdownTo = false;
      }
    }
  }

  searchFlights() {
    this.isLoading = true;

    let data: FlightData = {
      fromEntityId: this.findCityApiKeyByName(this.selectedCityFrom)!,
      toEntityId: this.findCityApiKeyByName(this.selectedCityTo),
      departDate: this.departureDate,
      returnDate: this.arrivalDate
    }

    this.skyscannerService.getRoundtripFlights(data).subscribe({
      next: (response) => {
        this.flights = response;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Error fetching flights:', error);
      }
    });
  }

  printData() {
    console.log(this.selectedCityFrom);
    console.log(this.selectedCityTo);
    console.log(this.departureDate);
    console.log(this.arrivalDate);
  }

  selectTrip(trip: Trip) {
    this.router.navigate(['/flight-data'], { state: { data: trip } });
  }

  private findCityApiKeyByName(cityName: string) {
    var city = this.cities.find(c => c.name.toLowerCase() === cityName.toLowerCase());
    return city?.apiKey;
  }

  
}
