import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

    constructor(private http: HttpClient) {}

    ngOnInit() {
      this.getForecasts();
    }

    getForecasts() {
      this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
        (result) => {
          this.forecasts = result;
        },
        (error) => {
          console.error(error);
        }
      );
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

// Código Original
//import { HttpClient } from '@angular/common/http';
//import { Component, OnInit } from '@angular/core';



//@Component({
//  selector: 'app-root',
//  templateUrl: './app.component.html',
//  styleUrl: './app.component.css'
//})
//export class AppComponent implements OnInit {
//  public forecasts: WeatherForecast[] = [];

//  constructor(private http: HttpClient) {}

//  ngOnInit() {
//    this.getForecasts();
//  }

//  getForecasts() {
//    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
//      (result) => {
//        this.forecasts = result;
//      },
//      (error) => {
//        console.error(error);
//      }
//    );
//  }

//  title = 'peopleangular.client';
//}
