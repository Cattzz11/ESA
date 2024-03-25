import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { StatisticsService } from '../../services/StatisticsService';
import { Chart, ChartType} from 'chart.js';
import { formatDate } from '@angular/common';
import { options } from '@fullcalendar/core/preact';
import { forkJoin } from 'rxjs';


@Component({
  selector: 'app-statistics-page',
  templateUrl: './statistics-page.component.html',
  styleUrl: './statistics-page.component.css'
})
export class StatisticsPageComponent implements OnInit{
  //statistics: any;
  loginCount: number = 0;
  selectedDate!: Date;
  totalUsers: number = 0;
  totalPremiumUsers: number = 0;
  totalAdmins: number = 0;
  maxLogins: number = 0;
  maxRegistrations: number = 0;
  labels1: string[] = ['User Normal | User Premium | Adminstradores'];
  labels2: string[] = ['Logins | Registos'];
  chartNameUsers: string = "Gráfico de Barras UserNormal/UserPremium/Administradores";
  chartNameRegistL: string = "Gráfico de Barras Máximo Logins/Registos"
  constructor(private statisticsService: StatisticsService, private chart: StatisticsService) {}

  ngOnInit(): void {
    this.statisticsService.getStatistics().subscribe(data => {
      this.totalUsers = data.totalUsersStats;
      this.totalPremiumUsers = data.totalPremiumStats;
      this.totalAdmins = data.totalAdmins;
      this.chart.thirdChart(this.chartNameUsers, 'Total Users Normais', 'Total Users Premium', 'Total Administradores', this.labels1, this.totalUsers,
        this.totalPremiumUsers, this.totalAdmins, 'barchart', 'bar');
      
    });

    this.statisticsService.getMaxLogins().subscribe(data => {
      this.maxLogins = data;
    });

    this.statisticsService.getMaxRegistrations().subscribe(data => {
      this.maxRegistrations = data;
    });

    this.onDateChange();

    this.fetchDataAndRenderChart();
  }

  fetchDataAndRenderChart(): void {
    forkJoin([
      this.statisticsService.getMaxLogins(),
      this.statisticsService.getMaxRegistrations()
    ]).subscribe({
      next: ([maxLogins, maxRegistrations]) => {
        this.chart.doubleChart(this.chartNameRegistL, 'Máximo Logins', 'Máximo Registos', this.labels2, this.maxLogins,
          this.maxRegistrations, 'barchart2', 'bar');
      },
      error: (error) => {
        console.error('Error fetching data for chart', error);
      }
    });
  }

  onDateChange(): void {
    if (this.selectedDate) {
      this.statisticsService.getLoginsByDate(this.selectedDate).subscribe(count => {
        this.loginCount = count;
      }, error => {
        console.error('Error fetching login count:', error);
      });
    }
  }

   
}
