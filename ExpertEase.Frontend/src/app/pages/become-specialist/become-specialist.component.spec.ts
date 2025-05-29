import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BecomeSpecialistComponent } from './become-specialist.component';

describe('BecomeSpecialistComponent', () => {
  let component: BecomeSpecialistComponent;
  let fixture: ComponentFixture<BecomeSpecialistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BecomeSpecialistComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BecomeSpecialistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
