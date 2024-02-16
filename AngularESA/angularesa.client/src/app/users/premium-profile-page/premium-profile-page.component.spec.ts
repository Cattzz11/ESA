import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PremiumProfilePageComponent } from './premium-profile-page.component';

describe('PremiumProfilePageComponent', () => {
  let component: PremiumProfilePageComponent;
  let fixture: ComponentFixture<PremiumProfilePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PremiumProfilePageComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PremiumProfilePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
