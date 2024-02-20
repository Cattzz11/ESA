import { Component, OnInit } from '@angular/core';
import { AuthorizeService } from '../../../api-authorization/authorize.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { User } from '../../Models/users';
import { Router } from '@angular/router';

@Component({
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.css'
})
export class EditProfileComponent implements OnInit {
  public user: User | null = null;
  public editForm!: FormGroup;

  constructor(private auth: AuthorizeService, private formBuilder: FormBuilder, private router: Router) {
    this.editForm = this.formBuilder.group({
      name: ['', Validators.required],
      birthDate: [''],
      age: [''],
      nationality: [''],
      occupation: ['']
    });

    this.editForm.get('birthDate')?.valueChanges.subscribe(() => {
      this.calcularIdade();
    });
  }
    ngOnInit(): void {
      this.auth.getUserInfo().subscribe({
        next: (userInfo: User) => {
          this.user = userInfo;
          this.editForm.patchValue({
            name: userInfo.name,
            birthDate: userInfo.age,
            nationality: userInfo.nationality,
            occupation: userInfo.occupation
          });
        },
        error: (error) => {
          console.error('Error fetching user info', error);
        }
      });
    }

  edit() {
    if (this.editForm.valid) {
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
