import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditSpecialistInfoComponent } from './edit-specialist-info.component';

describe('EditSpecialistInfoComponent', () => {
  let component: EditSpecialistInfoComponent;
  let fixture: ComponentFixture<EditSpecialistInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditSpecialistInfoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditSpecialistInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
