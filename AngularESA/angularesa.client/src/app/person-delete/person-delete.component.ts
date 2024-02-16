import { Component, OnInit } from '@angular/core';
import { PeopleService, Person } from '../services/people.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-person-delete',
  templateUrl: './person-delete.component.html',
  styleUrl: './person-delete.component.css'
})
export class PersonDeleteComponent implements OnInit {
  person: Person = { personId: 0, name: '', age: 0 };

  constructor(
    private peopleService: PeopleService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    const personId = this.route.snapshot.params['id'];
    this.peopleService.getPerson(personId).subscribe(person => {
      this.person = person;
    });
  }

  onDelete() {
    this.peopleService.deletePerson(this.person.personId).subscribe(() => {
      this.router.navigate(['/people']);
    });
  }

  onCancel() {
    this.router.navigate(['/people']);
  }
}
