import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminSpecialistsComponent } from './admin.specialists.component';

describe('AdminUsersComponent', () => {
  let component: AdminSpecialistsComponent;
  let fixture: ComponentFixture<AdminSpecialistsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminSpecialistsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminSpecialistsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
