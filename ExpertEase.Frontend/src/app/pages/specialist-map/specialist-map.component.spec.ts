import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecialistMapComponent } from './specialist-map.component';

describe('SpecialistMapComponent', () => {
  let component: SpecialistMapComponent;
  let fixture: ComponentFixture<SpecialistMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialistMapComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SpecialistMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
