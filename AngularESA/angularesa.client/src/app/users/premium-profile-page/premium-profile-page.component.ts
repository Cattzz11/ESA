import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UsersService } from '../../../app/services/UsersService';
import { Router } from '@angular/router';
import { Trip } from '../../Models/Trip';
import { DataService } from '../../services/DataService';

@Component({
  selector: 'app-premium-profile-page',
  templateUrl: './premium-profile-page.component.html',
  styleUrl: './premium-profile-page.component.css'
})
export class PremiumProfilePageComponent {
  public user!: User;
  public users: User[] = [];
  public isLoggedInWGoogle: boolean = false;
  public searchTerm: string = '';
  filteredUsers: User[] = [];
  allUsers: User[] = [];
  public history: Trip[] = [];
  public historyLoading: boolean = true;
  //statistics: any;
  //loginCount: number = 0;
  //selectedDate: string = '';
  
  constructor(
    private auth: AuthorizeService,
    private formBuilder: FormBuilder,
    private userService: UsersService,
    private router: Router,
    private dataService: DataService,
    private cdr: ChangeDetectorRef
  ) {

  }

  ngOnInit() {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
    }
    else {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
          this.cdr.detectChanges();
          if (this.user && this.user.role === 1) {
            this.loadHistory();
          }
        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }

    if (this.isLoggedInWGoogle) {
      this.isLoggedInWGoogle = true;
    }

    this.loadUsers();

    //if (this.user?.role === 2) {
    //  this.userService.getStatistics().subscribe(data => {
    //    this.statistics = data;
    //  });
    //}

  }

  //onDateChange(): void {
  //  this.userService.getLoginsByDate(this.selectedDate).subscribe(count => {
  //    this.loginCount = count;
  //  }, error => {
  //    console.error('Erro ao recuperar o número de logins:', error);
  //  });
  //}

  private loadUsers(): void {
    this.userService.getUsers().subscribe(
      (users) => {
        this.allUsers = users;
        console.log('Users:', users);
        console.log('filtered users', this.filteredUsers.length);
        if (this.filteredUsers.length !== 0) {
          this.users = this.filteredUsers;
        }
        else {
          this.users = users;
        }

      },
      (error) => {
        console.error('Error fetching users:', error);
      }
    );
  }

  deleteUser(email: string): void {
    this.userService.deleteUser(email).subscribe(
      () => {
        // Success
        console.log('User deleted successfully', email);
        // Optionally, you can reload the user list after deletion
        this.loadUsers();
      },
      (error) => {
        // Handle error
        console.error('Error deleting user:', error);
      }
    );
  }

  async applyFilter(): Promise<void> {
    if (this.searchTerm !== '') {
      this.filteredUsers = [];
      this.filteredUsers = await this.allUsers.filter(user =>
        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
      console.log("filtered users", this.filteredUsers);
    }
    else {
      this.filteredUsers = [];
    }
    this.loadUsers();
  }

  loadHistory() {
    if (this.user) {
      this.dataService.getFlightsByUser(this.user.id).subscribe({
        next: (response) => {
          this.history = response;
          this.historyLoading = false;
        },
        error: (error) => {
          console.error('Error fetching flights:', error);
          this.historyLoading = false;
        }
      });
    } else {
      console.error('User ID is undefined');
      this.historyLoading = false;
    }
  }

}
