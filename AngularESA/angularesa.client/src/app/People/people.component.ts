// Implementações 3 e 4: Acesso assincrono ao serviço para obter a lista das pessoas
import { Component, OnInit } from '@angular/core';
import { PeopleService, Person } from '../services/people.service';

@Component({
  selector: 'app-people',
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.css'
  ]
})
export class PeopleComponent implements OnInit {
  public people: Person[] = [];
  selectedPerson: Person | undefined;
  imagepath:string = "/assets/img/plus-18.png";

  constructor(private service: PeopleService) { }

  ngOnInit() {
    this.getPeople();
  }

  getPeople() {
    this.service.getPeople().subscribe(
      (result) => {
        this.people = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  onSelectPerson(person: Person) {
    this.selectedPerson = person;
  }
}

//// Implementação 2: Acesso sincrono ao serviço para obter a lista das pessoas (valor guardado na contante Pessoas)
//import { Component } from '@angular/core';
//import { PeopleService, Person } from '../services/people.service';


//@Component({
//  selector: 'app-people',
//  templateUrl: './people.component.html'
//})
//export class PeopleComponent {
//  public people: Person[] = [];

//  constructor(private service: PeopleService) {
//  }

//  ngOnInit() {
//    this.getPeople();
//  }

//  getPeople(): void {
//    this.people = this.service.getPeople();
//  }
//}



//// Implementação 1: Acesso direto à API para obter a lista das pessoas
//import { Component, Inject } from '@angular/core';
//import { HttpClient } from '@angular/common/http';

//@Component({
//  selector: 'app-people',
//  templateUrl: './people.component.html'
//})
//export class PeopleComponent {
//  public people: Person[] = [];

//  constructor(http: HttpClient) {
//    http.get<Person[]>('api/people').subscribe(result => {
//      this.people = result;
//    }, error => console.error(error));
//  }
//}

//interface Person {
//  personId: number;
//  name: string;
//  age: number;
//}


