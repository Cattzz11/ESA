import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SearchStateService {
  private searchState: any = null;

  saveSearchState(state: any) {
    this.searchState = state;
  }

  getSearchState() {
    return this.searchState;
  }

  clearSearchState() {
    this.searchState = null;
  }
}
