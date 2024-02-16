import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { PeopleService, Person } from '../services/people.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-person-edit',
  templateUrl: './person-edit.component.html',
  styleUrl: './person-edit.component.css'
})
export class PersonEditComponent implements OnInit {
  id: number = 0;
  person: Person = { personId: 0, name: '', age: 0 };

  personForm = new FormGroup({
    personId: new FormControl(),
    name: new FormControl('', [Validators.required, Validators.minLength(2)]),
    age: new FormControl()
  });

  constructor(
    private service: PeopleService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.id = this.route.snapshot.params['id'];
    this.service.getPerson(this.id).subscribe((person: Person) => {
      this.person = person;
    });
  }

  get name() {
    return this.personForm.get('name');
  }

  onSubmit() {
    console.log(this.personForm.value);

    this.service.updatePerson(this.personForm.value as Person).subscribe(res => {
      console.log('Person updated successfully!');
      this.router.navigateByUrl('people');
    });
  }
}
