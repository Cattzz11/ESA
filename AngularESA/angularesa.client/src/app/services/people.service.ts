// Implementação 4: Serviço Assincrono para obter a lista das pessoas (valor obtido através da API da aplicação Server)
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PeopleService {

  constructor(private http: HttpClient) { }

  getPeople(): Observable<Person[]> {
    return this.http.get<Person[]>('api/people');
  }

  getPerson(id: number): Observable<Person> {
    return this.http.get<Person>('api/people/' + id);
  }

  createPerson(person: Person): Observable<Person> {
    return this.http.post<Person>('api/people', person);
  }

  updatePerson(person: Person): Observable<Person> {
    return this.http.put<Person>('api/people/' + person.personId, person);
  }

  deletePerson(id: number): Observable<Person> {
    return this.http.delete<Person>('api/people/' + id);
  }
}

export interface Person {
  personId: number;
  name: string;
  age: number;
}



//// Implementação 3: Serviço Assincrono para obter a lista das pessoas (valor guardado na contante Pessoas)

//import { Injectable } from '@angular/core';
//import { Observable, of } from 'rxjs';

//@Injectable({
//  providedIn: 'root'
//})
//export class PeopleService {

//  constructor() { }

//  getPeople(): Observable<Person[]> {
//    return of(PEOPLE);
//  }
//}

//export interface Person {
//  personId: number;
//  name: string;
//  age: number;
//}

//export const PEOPLE: Person[] = [
//  { personId: 1, name: 'José', age: 21 },
//  { personId: 2, name: 'Ana', age: 18 },
//  { personId: 3, name: 'Ivo', age: 20 }
//]


//// Implementação 2: Serviço Sincrono para obter a lista das pessoas (valor guardado na contante PEOPLE)
//import { Injectable } from '@angular/core';

//@Injectable({
//  providedIn: 'root'
//})

//export class PeopleService {

//  constructor() { }

//  getPeople(): Person[] {
//    return PEOPLE;
//  }
//}

//export interface Person {
//  personId: number;
//  name: string;
//  age: number;
//}

//export const PEOPLE: Person[] = [
//  { personId: 1, name: 'José', age: 21 },
//  { personId: 2, name: 'Ana', age: 18 },
//  { personId: 3, name: 'Ivo', age: 20 }
//]

//// Implementação 1: Sem código: o serviço ainda não foi criado
