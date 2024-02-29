import { Component, OnInit } from '@angular/core';
import { City } from '../../Models/City';
import { DataService } from '../../services/DataService';
import { Calendar } from '../../Models/Calendar';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrl: './search-flights.component.css'
})
export class SearchFlightsComponent implements OnInit {
  cities: City[] = [];
  filteredCities: City[] = [];
  showDropdown = false;
  selectedCity = '';
  interactingWithDropdown = false;
  selectedCityTo = '';
  showDropdownTo = false;
  filteredCitiesTo: City[] = [];
  calendar: Calendar[] | undefined;
  fromValid = false;
  toValid = false;
  canSearch = false;
  departureEnabled = false;
  arrivalEnabled = false;
  departureDate: string = '';
  arrivalDate: string = '';

  constructor(private dataService: DataService, private skyscannerService: SkyscannerService) { }

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

  findCityApiKeyByName(cityName: string): string | undefined {
    const city = this.cities.find(c => c.name.toLowerCase() === cityName.toLowerCase());
    return city?.apiKey;
  }

  showAllCities(inputField: 'from' | 'to') {
    if (inputField === 'from') {
      this.filteredCities = this.cities;
      this.showDropdown = true;
    } else {
      this.filteredCitiesTo = this.cities;
      this.showDropdownTo = true;
    }
  }

  filterCitiesByText(inputField: 'from' | 'to') {
    const searchText = inputField === 'from' ? this.selectedCity.toLowerCase() : this.selectedCityTo.toLowerCase();
    const filteredCities = this.cities.filter(city =>
      city.name.toLowerCase().includes(searchText) ||
      city.country.name.toLowerCase().includes(searchText)
    );

    if (inputField === 'from') {
      this.filteredCities = filteredCities;
      this.showDropdown = this.filteredCities.length > 0;
    } else {
      this.filteredCitiesTo = filteredCities;
      this.showDropdownTo = this.filteredCitiesTo.length > 0;
    }
  }

  selectCity(city: City, inputField: 'from' | 'to') {
    if (inputField === 'from') {
      this.selectedCity = city.country.name + ' -> ' + city.name;
      this.showDropdown = false;
      this.fromValid = this.isCityValid(this.selectedCity);
      this.toValid = false;
    } else {
      this.selectedCityTo = city.country.name + ' -> ' + city.name;
      this.showDropdownTo = false;
      this.toValid = this.isCityValid(this.selectedCityTo);
      this.departureEnabled = this.toValid;
    }
    this.validateForm();
  }

  isCityValid(cityName: string): boolean {
    return this.cities.some(city => cityName.includes(city.name) && cityName.includes(city.country.name));
  }

  validateForm() {
    this.fromValid = this.isCityValid(this.selectedCity);
    this.toValid = this.isCityValid(this.selectedCityTo);

    if (this.fromValid && this.toValid && this.calendar === undefined) {
      this.loadCalendar(this.selectedCity, this.selectedCityTo);
    }

    this.canSearch = this.fromValid && this.toValid && !!this.departureDate && !!this.arrivalDate;
  }

  hideDropdown(inputField: 'from' | 'to') {
    if (inputField === 'from') {
      if (!this.interactingWithDropdown) {
        this.showDropdown = false;
      }
    } else {
      if (!this.interactingWithDropdown) {
        this.showDropdownTo = false;
      }
    }
  }
}
