import { Component, OnInit } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { User } from '../../Models/users';
import { Router } from '@angular/router';
import { PhotoUploadService } from '../../services/photoUploadService.service';
import { UsersService } from '../../services/UsersService';

@Component({
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.css'
})
export class EditProfileComponent implements OnInit {
  public user!: User;
  public editForm!: FormGroup;
  photoPreview: string | ArrayBuffer | null = null;
  selectedFile: File | null = null;
  private photoUploadService!: PhotoUploadService;

  constructor(
    private auth: AuthorizeService,
    private formBuilder: FormBuilder,
    private router: Router,
    private _photoUploadService: PhotoUploadService,
    private userService: UsersService
  ) {
    this.photoUploadService = _photoUploadService;

    this.editForm = this.formBuilder.group({
      name: ['', Validators.required],
      email: ['', Validators.required],
      birthDate: [''],
      age: [''],
      occupation: [''],
      nationality: [''],
      profilePicture: [''],
      gender:[''],
    });

    this.editForm.get('birthDate')?.valueChanges.subscribe(() => {
      this.calcularIdade();
    });

    this.editForm.get('gender')?.valueChanges.subscribe(() => {
      this.user?.gender;
    })
  }
    ngOnInit(): void {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
          this.editForm.patchValue({
            name: userInfo.name,
            email: userInfo.email,
            birthDate: userInfo.age,
            occupation: userInfo.occupation,
            nationality: userInfo.nationality,
            profilePicture: userInfo.profilePicture,
            gender: userInfo.gender,
          });
          console.log("idade", userInfo.age);
          this.photoPreview = userInfo.profilePicture
            ? `assets/Images/${userInfo.profilePicture}`
            : 'assets/Images/user.png';
        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }

  edit() {
    if (this.editForm.valid) {
      this.updateUserInformation();

      //if (this.selectedFile) {
      //  this.uploadPhotoAndEdit();
      //} else {
      //  this.updateUserInformation();
      //}
    }
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

  updateUserInformation() {
    const editedData = this.editForm.value;
    console.log('Dados Editados:', editedData);
    this.auth.updateUserInfo(editedData).subscribe(
      () => {
        console.log('Perfil atualizado com sucesso.');
        this.router.navigate(['/premium-profile-page']);
      },
      error => {
        console.error('Erro ao atualizar o perfil:', error);
      }
    );
  }

  calcularIdade() {
    const birthDateValue = this.editForm.get('birthDate')?.value;

    if (birthDateValue) {
      // Converter para um objeto Date se não for uma instância válida
      const birthDate = birthDateValue instanceof Date ? birthDateValue : new Date(birthDateValue);

      const hoje = new Date();
      const age = hoje.getFullYear() - birthDate.getFullYear();

      // Verificar se o aniversário já ocorreu este ano
      if (
        hoje.getMonth() < birthDate.getMonth() ||
        (hoje.getMonth() === birthDate.getMonth() && hoje.getDate() < birthDate.getDate())
      ) {
        this.editForm.patchValue({ age: age - 1 });
      } else {
        this.editForm.patchValue({ age: age });
      }
    }
  }

}


