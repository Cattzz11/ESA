import { Component } from '@angular/core';
import { PeopleService, Person } from '../services/people.service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-person-create',
  templateUrl: './person-create.component.html',
  styleUrl: './person-create.component.css'
})
export class PersonCreateComponent {
  constructor(private peopleService: PeopleService, private router: Router) { }

  onSubmit(form: NgForm) {
    if (form.valid) {
      this.peopleService.createPerson(form.value as Person).subscribe(res => {
        console.log('Person created successfully!');
        this.router.navigateByUrl('people');
      });
    }
  }
}
