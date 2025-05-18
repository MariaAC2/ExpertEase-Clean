import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecialistDetailsComponent } from './specialist-details.component';

describe('SpecialistDetailsComponent', () => {
  let component: SpecialistDetailsComponent;
  let fixture: ComponentFixture<SpecialistDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialistDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SpecialistDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
