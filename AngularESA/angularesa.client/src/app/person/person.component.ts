import { Component, Input, OnInit } from '@angular/core';
import { Person } from '../services/people.service';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html',
  styleUrl: './person.component.css'
})
export class PersonComponent implements OnInit {
  @Input() person: Person | undefined;

  constructor() { }

  ngOnInit(): void {
  }
}

