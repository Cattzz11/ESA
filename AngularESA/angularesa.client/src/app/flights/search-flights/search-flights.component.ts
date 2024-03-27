import { Component, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { City } from '../../Models/City';
import { DataService } from '../../services/DataService';
import { Calendar } from '../../Models/Calendar';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';
import { Trip } from '../../Models/Trip';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { ViewEncapsulation } from '@angular/core';
import { MatCalendarCellClassFunction, MatDatepickerInputEvent } from '@angular/material/datepicker';
import { DatePipe } from '@angular/common';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { TripDetails } from '../../Models/TripDetails';
import { PriceOptions } from '../../Models/PriceOptions';
import { SearchStateService } from '../../services/SearchStateService';
import { filter } from 'rxjs';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrl: './search-flights.component.css',
  encapsulation: ViewEncapsulation.None,
})
export class SearchFlightsComponent implements OnInit {
  user: User | null = null;

  cities: City[] = [];
  calendar: Calendar[] | undefined;
  filteredCities: City[] = [];
  flights: Trip[] = [];
  flightsPremium: TripDetails[] = [];

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

  constructor(
    private dataService: DataService,
    private skyscannerService: SkyscannerService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private datePipe: DatePipe,
    private auth: AuthorizeService,
    private searchStateService: SearchStateService,
    private activatedRoute: ActivatedRoute
  )
  {}

  ngOnInit(): void {
    const savedState = this.searchStateService.getSearchState();
    if (savedState) {
      this.selectedCityFrom = savedState.selectedCityFrom;
      this.selectedCityTo = savedState.selectedCityTo;
      this.departureDate = savedState.departureDate;
      this.arrivalDate = savedState.arrivalDate;
      this.fromValid = savedState.fromValid;
      this.toValid = savedState.toValid;
      this.canSearch = savedState.canSearch;
      this.departureEnabled = savedState.departureEnabled;
      this.arrivalEnabled = savedState.arrivalEnabled;
      this.cities = savedState.cities;
      this.calendar = savedState.calendar;
      this.flights = savedState.flights;
      this.flightsPremium = savedState.flightsPremium;

      this.searchStateService.clearSearchState();
    }

    this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
        }
      });

    this.dataService.getAllCities().subscribe({
      next: (response) => {
        this.cities = response;
        this.activatedRoute.queryParams.subscribe(params => {
          this.selectedCityFrom = params['origin'];
          this.selectedCityTo = params['destination'];
          this.departureDate = params['departureDate'];
          this.arrivalDate = params['arrivalDate'];
        });
        if (this.selectedCityFrom.trim() && this.selectedCityTo.trim() && this.departureDate.trim() && this.arrivalDate.trim()) {
          this.isLoading = true;
          this.validateForm();
          this.loadCalendar(this.selectedCityFrom, this.selectedCityTo);
          this.departureEnabled = true;
          this.arrivalEnabled = true;
          this.searchFlights();
        }

        this.cdr.markForCheck();
      }
    });
  }

  saveStateAndNavigate(trip: Trip | TripDetails) {
    const currentState = {
      selectedCityFrom: this.selectedCityFrom,
      selectedCityTo: this.selectedCityTo,
      departureDate: this.departureDate,
      arrivalDate: this.arrivalDate,
      fromValid: this.fromValid,
      toValid: this.toValid,
      canSearch: this.canSearch,
      departureEnabled: this.departureEnabled,
      arrivalEnabled: this.arrivalEnabled,
      cities: this.cities,
      calendar: this.calendar,
      flights: this.flights,
      flightsPremium: this.flightsPremium
    };

    this.searchStateService.saveSearchState(currentState);
    this.router.navigate(['/flight-data'], { state: { data: trip } });
  }

  loadCalendar(from: string, to: string) {
    let data: FlightData = {
      fromEntityId: this.findCityApiKeyByName(from)!,
      toEntityId: this.findCityApiKeyByName(to)
    }

    this.skyscannerService.getCalendarFlights(data).subscribe({
      next: (response) => {
        this.calendar = response;
        this.cdr.markForCheck();
        this.departureEnabled = true;
      },
      error: (error) => {
        console.error('Error fetching flights:', error);
      }
    });
  }

  searchFlights() {
    this.isLoading = true;

    let data: FlightData = {
      fromEntityId: this.findCityApiKeyByName(this.selectedCityFrom)!,
      toEntityId: this.findCityApiKeyByName(this.selectedCityTo),
      departDate: this.departureDate,
      returnDate: this.arrivalDate
    }

    if (this.user && this.user.role === 1) {
      this.skyscannerService.getRoundtripFlightsPremium(data).subscribe({
        next: (response) => {
          this.flightsPremium = response;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Error fetching flights:', error);
        }
      });
    } else {
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

  dateClass: MatCalendarCellClassFunction<Date> = (cellDate, view) => {
    if (view === 'month' && this.calendar && this.calendar.length > 0) {
      const cellDateFormatted = new Date(cellDate.getFullYear(), cellDate.getMonth(), cellDate.getDate());

      const calendarEntry = this.calendar.find(entry => {
        const entryDate = new Date(entry.date);
        const entryDateFormatted = new Date(entryDate.getFullYear(), entryDate.getMonth(), entryDate.getDate());

        return entryDateFormatted.getTime() === cellDateFormatted.getTime();
      });

      if (calendarEntry) {
        switch (calendarEntry.category) {
          case 'high':
            return 'date-high';
          case 'medium':
            return 'date-medium';
          case 'low':
            return 'date-low';
          default:
            return '';
        }
      }
    }

    return '';
  };

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
      this.toValid = this.isCityValid(this.selectedCityTo);
    } else {
      this.selectedCityTo = city.name;
      this.showDropdownTo = false;
      this.toValid = this.isCityValid(this.selectedCityTo);
    }

    if (this.departureEnabled) {
      this.departureEnabled = false;
      this.arrivalEnabled = false;
      this.departureDate = '';
      this.arrivalDate = '';
      this.calendar = [];
    }

    // Se ambas as cidades forem válidas, carrega o calendário com as novas cidades
    if (this.fromValid && this.toValid) {
      this.loadCalendar(this.selectedCityFrom, this.selectedCityTo);
    }

    this.validateForm();
  }


  isCityValid(cityName: string): boolean {
    return this.cities.some(city => cityName.includes(city.name));
  }

  validateForm() {
    this.fromValid = this.isCityValid(this.selectedCityFrom);
    this.toValid = this.isCityValid(this.selectedCityTo);
    this.canSearch = this.fromValid && this.toValid && !!this.departureDate && !!this.arrivalDate;
  }


  hideDropdown(inputField: 'from' | 'to') {
    if (inputField === 'from') {
      if (!this.interactingWithDropdownFrom) {
        this.showDropdownFrom = false;
        this.fromValid = this.isCityValid(this.selectedCityFrom);
      }
    } else {
      if (!this.interactingWithDropdownFrom) {
        this.showDropdownTo = false;
        this.toValid = this.isCityValid(this.selectedCityTo);
      }
    }

    if (this.fromValid && this.toValid) {
      this.loadCalendar(this.selectedCityFrom, this.selectedCityTo);
    }

    this.validateForm();
  }

  onDepartureDateChange(event: MatDatepickerInputEvent<Date>) {
    const formatted = this.datePipe.transform(event.value, 'yyyy-MM-dd');
    if (formatted)
      this.departureDate = formatted;
    this.arrivalEnabled = !!this.departureDate;
  }

  onArrivalDateChange(event: MatDatepickerInputEvent<Date>) {
    const formatted = this.datePipe.transform(event.value, 'yyyy-MM-dd');
    if (formatted)
      this.arrivalDate = formatted;
    this.validateForm();
  }

  private findCityApiKeyByName(cityName: string) {
    let city = this.cities.find(c => c.name?.toLowerCase() === cityName?.toLowerCase());

    if (city !== undefined) {
      return city.apiKey;
    }

    let bestMatch = { city: null as City | null, score: 0 };
    const target = cityName.toLowerCase();

    this.cities.forEach(city => {
      const cityNameLower = city.name.toLowerCase();
      let score = this.similarityScore(target, cityNameLower);

      if (score > bestMatch.score) {
        bestMatch = { city, score };
      }
    });

    return bestMatch.city?.apiKey;
  }

  private similarityScore(s1: string, s2: string): number {
    const shorter = s1.length < s2.length ? s1 : s2;
    const longer = s1.length < s2.length ? s2 : s1;
    let score = 0;

    for (let i = 0; i < shorter.length; i++) {
      if (shorter[i] === longer[i]) {
        score++;
      }
    }

    return score;
  }
}
