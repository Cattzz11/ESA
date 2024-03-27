import { Component, OnInit } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { User } from '../../Models/users';
import { Router } from '@angular/router';
import { PhotoUploadService } from '../../services/photoUploadService.service';
import { UsersService } from '../../services/UsersService';
import Swal from 'sweetalert2';

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
    });

    this.editForm.get('age')?.valueChanges.subscribe(() => {
      this.calcularIdade();
    });
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
    // Show confirmation dialog
    Swal.fire({
      title: "Tem a certeza que quer apagar?",
      text: "Esta operação não é possível reverter",
      icon: "warning",
      showCancelButton: true,
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Sim, apagar!"
    }).then((result: { isConfirmed: any; }) => {
      if (result.isConfirmed) {
        // Proceed with deletion
        this.confirmDeleteUser(email);
      }
    });
  }

  confirmDeleteUser(email: string): void {
    this.userService.deleteUser(email).subscribe(
      () => {
        // Success
        console.log('User deleted successfully', email);
        Swal.fire({
          title: "Conta apagada!",
          text: "A sua conta foi apagada com sucesso.",
          icon: "success"
        });
        // Optionally, you can reload the user list or redirect as needed
        this.router.navigateByUrl("/");
      },
      (error) => {
        // Handle error
        console.error('Error deleting user:', error);
        // Optionally, show an error message to the user
        Swal.fire({
          title: "Erro!",
          text: "Ocorreu um erro ao tentar apagar a sua conta. Por favor tente novamente.",
          icon: "error"
        });
      }
    );
  }
 

  updateUserInformation() {
    const editedData = this.editForm.value;
    console.log('Dados Editados:', editedData);
    this.auth.updateUserInfo(editedData).subscribe(
      () => {
        console.log('Perfil atualizado com sucesso.');
        // Show success notification
        Swal.fire({
          position: 'center',
          icon: 'success',
          title: 'O seu perfil foi atualizado com sucesso!',
          showConfirmButton: false,
          timer: 1500
        }).then(() => {
          // Navigate after the toast disappears or user closes it
          this.router.navigate(['/premium-profile-page']);
        });
      },
      error => {
        console.error('Erro ao atualizar o perfil:', error);
        // Show error notification
        Swal.fire({
          position: 'center',
          icon: 'error',
          title: 'Erro a atualizar o perfil!',
          text: 'Ocorreu o erro ao atualizar o perfil, por favor tente novamente.',
          showConfirmButton: true, // You might want the user to acknowledge the error
        });
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


