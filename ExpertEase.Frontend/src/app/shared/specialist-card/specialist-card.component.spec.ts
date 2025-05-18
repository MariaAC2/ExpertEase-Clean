import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecialistCardComponent } from './specialist-card.component';

describe('SpecialistCardComponent', () => {
  let component: SpecialistCardComponent;
  let fixture: ComponentFixture<SpecialistCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialistCardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SpecialistCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
