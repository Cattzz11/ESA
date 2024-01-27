import { Component, OnInit } from '@angular/core';
import { PeopleService, Person } from '../services/people.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-person-details',
  templateUrl: './person-details.component.html',
  styleUrl: './person-details.component.css'
})
export class PersonDetailsComponent implements OnInit {
  person: Person | undefined;
  id: number = 0;

  constructor(private service: PeopleService, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.params['id'];
    this.getPerson();
  }

  getPerson(): void {
    this.service.getPerson(this.id)
      .subscribe((person: Person) => this.person = person);
  }
}

