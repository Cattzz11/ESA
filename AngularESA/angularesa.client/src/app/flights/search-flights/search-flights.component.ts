import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { City } from '../../Models/City';
import { DataService } from '../../services/DataService';
import { Calendar } from '../../Models/Calendar';
import { SkyscannerService } from '../../services/skyscannerService';
import { FlightData } from '../../Models/flight-data';
import { Trip } from '../../Models/Trip';
import { Router } from '@angular/router';
import { ViewEncapsulation } from '@angular/core';
import { MatCalendarCellClassFunction, MatDatepickerInputEvent, MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { provideNativeDateAdapter } from '@angular/material/core';
import { DateAdapter } from '@angular/material/core';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrl: './search-flights.component.css',
  encapsulation: ViewEncapsulation.None,
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

  constructor(
    private dataService: DataService,
    private skyscannerService: SkyscannerService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private datePipe: DatePipe,
  )
  {}

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
    // Verifica se ambas as cidades são válidas antes de permitir a busca
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

  printData() {
    console.log(this.selectedCityFrom);
    console.log(this.selectedCityTo);
    console.log(this.departureDate);
    console.log(this.arrivalDate);
    console.log(this.calendar?.length);
  }

  selectTrip(trip: Trip) {
    this.router.navigate(['/flight-data'], { state: { data: trip } });
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
    var city = this.cities.find(c => c.name.toLowerCase() === cityName.toLowerCase());
    return city?.apiKey;
  }
}
