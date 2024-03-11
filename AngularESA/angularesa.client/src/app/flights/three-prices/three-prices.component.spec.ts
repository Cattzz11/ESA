import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThreePricesComponent } from './three-prices.component';

describe('ThreePricesComponent', () => {
  let component: ThreePricesComponent;
  let fixture: ComponentFixture<ThreePricesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ThreePricesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ThreePricesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
