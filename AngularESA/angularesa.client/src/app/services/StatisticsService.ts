import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Chart, ChartType, registerables } from 'chart.js';
import { Observable } from 'rxjs';
Chart.register(...registerables);

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  constructor(private http: HttpClient) { }

  getStatistics(): Observable<any> {
    return this.http.get<any>("api/statistics"); // Certifique-se de que a URL está correta conforme sua configuração
  }

  getLoginsByDate(date: Date): Observable<any> {
    //?date=${date}
    const isoDate = date.toString().slice(0, 10); // Extract YYYY-MM-DD
    return this.http.get(`api/logins`, { params: { date: isoDate } });
  }

  getMaxLogins(): Observable<any> {
    return this.http.get(`api/maxLogins`);
  }

  getMaxRegistrations(): Observable<any> {
    return this.http.get(`api/maxRegistrations`)
  }

  doubleChart(graphTitle: string, primaryDatasetKey: string, secondaryDatasetKey: string,
    labels: any, primaryDatasetValue: number,
    secondaryDatasetValue: number, context: string, chartType: any) {
    var chart = new Chart(context, {
      type: chartType,
      data: {
        labels: labels, // This should be an array with two labels, e.g., ['Total Users', 'Premium Users']
        datasets: [{
          label: primaryDatasetKey,
          backgroundColor: 'rgb(0, 105, 148)',
          borderColor: 'rgb(0, 105, 148)',
          borderWidth: 1,
          data: [primaryDatasetValue] // Wrap the single value in an array
        },
        {
          label: secondaryDatasetKey,
          backgroundColor: 'rgb(135, 206, 235)',
          borderColor: 'rgb(135, 206, 235)',
          borderWidth: 1,
          data: [secondaryDatasetValue] // Wrap the single value in an array
        }]
      },
      options: {
        plugins: {
          title: {
            display: true,
            text: graphTitle
          },
        },
        responsive: true,
        scales: {
          x: { stacked: false },
          y: { stacked: false, beginAtZero: true, max:10 } // Adjusted to remove fixed min and max
        }
      }
    });
  }


  singleChart(graphTitle: string, primaryDatesetKey: string, labels: any, primaryDataset: any, context: string, charttype: any) {
    var chart = new Chart(context, {
      type: charttype,
      data: {
        labels: labels,
        datasets: [{
          label: primaryDatesetKey,
          backgroundColor: ['rgb(0, 105, 148)'],
          data: primaryDataset
        }]
      },
      options: {
        plugins: {
          title: {
            display: true,
            text: graphTitle
          },
        },
        responsive: true,
        scales: {
          x: { stacked: false },
          y: { stacked: false, min: 0, max: 20 }
        }
      }
    });
  }
}
