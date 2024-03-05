import { Component } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { User } from '../../Models/users';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UsersService } from '../../../app/services/UsersService';
import { Router } from '@angular/router';

@Component({
  selector: 'app-premium-profile-page',
  templateUrl: './premium-profile-page.component.html',
  styleUrl: './premium-profile-page.component.css'
})
export class PremiumProfilePageComponent {
  public user: User | null = null;
  public users: User[] = [];
  public isLoggedInWGoogle: boolean = false;
  public searchTerm: string = '';
  filteredUsers: User[] = [];
  allUsers: User[] = [];

  constructor(private auth: AuthorizeService, private formBuilder: FormBuilder, private userService: UsersService, private router: Router) {

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
          //this.editForm.patchValue({
          //  name: userInfo.name,
          //  birthDate: userInfo.age,
          //  nationality: userInfo.nationality,
          //  occupation: userInfo.occupation
          //});
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

  }

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

  deleteUserProfile(email: string): void {
    console.log(email);
    this.userService.deleteUser(email).subscribe(
      () => {
        // Success
        console.log('User deleted successfully', email);
        // Optionally, you can reload the user list after deletion
        this.router.navigateByUrl("/");
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
}
